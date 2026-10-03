"""
Thread-safe singleton for tracking ONNX model export background process.
"""
import logging
import os
import sys
import threading
import time
from dataclasses import dataclass, field
from enum import Enum
from pathlib import Path
from typing import Dict, List, Optional

logger = logging.getLogger(__name__)


class ExportStatus(str, Enum):
    IDLE = "idle"
    RUNNING = "running"
    COMPLETED = "completed"
    FAILED = "failed"


class ModelExportStatus(str, Enum):
    PENDING = "pending"
    EXPORTING = "exporting"
    COMPLETED = "completed"
    FAILED = "failed"


@dataclass
class ModelExportReport:
    model_name: str
    status: ModelExportStatus = ModelExportStatus.PENDING
    duration_ms: Optional[float] = None
    error: Optional[str] = None


@dataclass
class ExportReport:
    overall_status: ExportStatus = ExportStatus.IDLE
    models: List[ModelExportReport] = field(default_factory=list)
    start_time: Optional[str] = None
    end_time: Optional[str] = None
    total_duration_ms: Optional[float] = None


# Models to export — order matches scripts/export_onnx.py
EXPORT_MODELS = [
    "efficientnet_b0",
    "clip_vit_b16",
    "fashion_clip",
    "dinov2_vits14",
]


class ExportState:
    """Singleton tracking the background ONNX export process.

    Thread-safe: all state mutations are protected by a lock.
    """

    _instance: Optional["ExportState"] = None
    _lock: threading.Lock = threading.Lock()
    _init_lock: threading.Lock = threading.Lock()

    def __new__(cls) -> "ExportState":
        if cls._instance is None:
            with cls._init_lock:
                if cls._instance is None:
                    cls._instance = super().__new__(cls)
                    cls._instance._initialized = False
        return cls._instance

    def __init__(self) -> None:
        if self._initialized:
            return
        self._status = ExportStatus.IDLE
        self._models: Dict[str, ModelExportReport] = {}
        self._start_time: Optional[float] = None
        self._end_time: Optional[float] = None
        self._total_duration_ms: Optional[float] = None
        self._thread: Optional[threading.Thread] = None
        self._data_lock = threading.Lock()
        self._initialized = True

    def is_running(self) -> bool:
        with self._data_lock:
            return self._status == ExportStatus.RUNNING

    def get_report(self) -> ExportReport:
        with self._data_lock:
            return self._build_report_locked()

    def _build_report_locked(self) -> ExportReport:
        """Build report from current state. Caller must hold _data_lock."""
        return ExportReport(
            overall_status=self._status,
            models=list(self._models.values()),
            start_time=(
                time.strftime(
                    "%Y-%m-%dT%H:%M:%S", time.localtime(self._start_time)
                )
                if self._start_time
                else None
            ),
            end_time=(
                time.strftime(
                    "%Y-%m-%dT%H:%M:%S", time.localtime(self._end_time)
                )
                if self._end_time
                else None
            ),
            total_duration_ms=self._total_duration_ms,
        )

    def start_export(self) -> ExportReport:
        """Start background export if not already running.

        Returns:
            Current export report (either newly started or existing).
        """
        with self._data_lock:
            if self._status == ExportStatus.RUNNING:
                logger.info("Export already in progress — returning current status")
                return self._build_report_locked()

            # Reset state for new export
            self._status = ExportStatus.RUNNING
            self._models = {
                name: ModelExportReport(model_name=name) for name in EXPORT_MODELS
            }
            self._start_time = time.time()
            self._end_time = None
            self._total_duration_ms = None

            self._thread = threading.Thread(
                target=self._run_export, daemon=True, name="onnx-export"
            )
            self._thread.start()

            return self._build_report_locked()

    def _run_export(self) -> None:
        """Execute export for all models in background thread."""
        try:
            self._run_export_inner()
        except Exception as exc:
            logger.error(f"Export thread crashed: {exc}", exc_info=True)
            with self._data_lock:
                self._status = ExportStatus.FAILED
                self._end_time = time.time()
                if self._start_time:
                    self._total_duration_ms = (
                        (self._end_time - self._start_time) * 1000
                    )

    def _run_export_inner(self) -> None:
        """Inner export logic — iterates models, exports each one."""
        # Propagate HF token for gated models (fashion_clip)
        from embedding.core.config import settings

        if settings.HUGGING_FACE_TOKEN:
            os.environ["HF_TOKEN"] = settings.HUGGING_FACE_TOKEN
            os.environ["HUGGING_FACE_HUB_TOKEN"] = settings.HUGGING_FACE_TOKEN

        # Import export functions — these live in scripts/ which is outside
        # the embedding package, so we add the project root to sys.path.
        export_funcs = self._load_export_functions()

        failed_count = 0
        for model_name in EXPORT_MODELS:
            with self._data_lock:
                self._models[model_name].status = ModelExportStatus.EXPORTING

            model_start = time.time()
            try:
                func = export_funcs[model_name]
                logger.info(f"Starting ONNX export for {model_name}")
                func()
                duration = (time.time() - model_start) * 1000

                with self._data_lock:
                    self._models[model_name].status = ModelExportStatus.COMPLETED
                    self._models[model_name].duration_ms = round(duration, 2)

                logger.info(
                    f"Completed ONNX export for {model_name} in {duration:.0f}ms"
                )
            except Exception as exc:
                duration = (time.time() - model_start) * 1000
                failed_count += 1
                logger.error(f"Failed ONNX export for {model_name}: {exc}")

                with self._data_lock:
                    self._models[model_name].status = ModelExportStatus.FAILED
                    self._models[model_name].duration_ms = round(duration, 2)
                    self._models[model_name].error = str(exc)

        # Finalize
        with self._data_lock:
            self._end_time = time.time()
            if self._start_time:
                self._total_duration_ms = round(
                    (self._end_time - self._start_time) * 1000, 2
                )
            self._status = (
                ExportStatus.FAILED if failed_count == len(EXPORT_MODELS)
                else ExportStatus.COMPLETED
            )

        logger.info(
            f"Export finished: status={self._status.value}, "
            f"failed={failed_count}/{len(EXPORT_MODELS)}"
        )

    @staticmethod
    def _load_export_functions() -> Dict[str, callable]:
        """Load export functions from scripts/export/vision.py.

        The scripts use bare imports (``from core.constants``) that require
        ``src/`` on sys.path, and ``from scripts.export.base`` that require
        the project root on sys.path. Both are added temporarily and removed
        after the import to avoid polluting the running process.

        Returns:
            Dict mapping model name to its export function.
        """
        # Find project root (service/Embedding/)
        current_dir = Path(__file__).resolve().parent
        # src/models/onnx/ → src/models/ → src/ → service/Embedding/
        project_root = current_dir.parent.parent.parent
        src_dir = project_root / "src"

        paths_to_add = []
        root_str = str(project_root)
        src_str = str(src_dir)

        if root_str not in sys.path:
            sys.path.insert(0, root_str)
            paths_to_add.append(root_str)
        if src_str not in sys.path:
            sys.path.insert(0, src_str)
            paths_to_add.append(src_str)

        try:
            from scripts.export.vision import (
                export_clip,
                export_dinov2,
                export_efficientnet,
                export_fashion_clip,
            )

            return {
                "efficientnet_b0": export_efficientnet,
                "clip_vit_b16": export_clip,
                "fashion_clip": export_fashion_clip,
                "dinov2_vits14": export_dinov2,
            }
        finally:
            for p in paths_to_add:
                if p in sys.path:
                    sys.path.remove(p)
