"""Structured logging configuration for the benchmark."""

from __future__ import annotations

import logging
from pathlib import Path

from rich.console import Console
from rich.logging import RichHandler

_console = Console(stderr=True)


def setup_logging(level: str = "INFO", log_file: Path | None = None) -> logging.Logger:
    """Configure root logger with Rich console + optional file sink."""
    handlers: list[logging.Handler] = [
        RichHandler(console=_console, rich_tracebacks=True, markup=True, show_path=False)
    ]

    if log_file is not None:
        log_file.parent.mkdir(parents=True, exist_ok=True)
        fh = logging.FileHandler(log_file, encoding="utf-8")
        fh.setFormatter(
            logging.Formatter(
                "%(asctime)s %(levelname)-8s %(name)s — %(message)s",
                datefmt="%Y-%m-%dT%H:%M:%S",
            )
        )
        handlers.append(fh)

    logging.basicConfig(
        level=getattr(logging, level.upper(), logging.INFO),
        handlers=handlers,
        format="%(message)s",
        datefmt="[%X]",
        force=True,
    )
    return logging.getLogger("benchmark")


def get_logger(name: str) -> logging.Logger:
    """Return a child logger under the benchmark namespace."""
    return logging.getLogger(f"benchmark.{name}")
