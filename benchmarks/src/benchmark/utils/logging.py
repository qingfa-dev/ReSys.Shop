"""Structured logging configuration for the benchmark.

Configures root logger with Rich console output (stderr) and optional
file sink for persistent records. All loggers in the project are children
of the ``benchmark`` namespace.

Edge cases:
- Log level defaults to INFO if the string is not a recognised level name.
- File handler creates parent directories if they do not exist.
"""

from __future__ import annotations

import logging
from pathlib import Path

from rich.console import Console
from rich.logging import RichHandler

_console = Console(stderr=True)


def setup_logging(level: str = "INFO", log_file: Path | None = None) -> logging.Logger:
    """Configure root logger with Rich console and optional file sink.

    Args:
        level: Log level string (``"DEBUG"``, ``"INFO"``, ``"WARNING"``, etc.).
        log_file: Optional path to a log file. Parent directories are created.

    Returns:
        The root ``benchmark`` logger instance.
    """
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
    """Return a child logger under the ``benchmark`` namespace.

    Args:
        name: Dot-separated sub-namespace, e.g. ``"evaluation.thesis"``.

    Returns:
        Logger instance for ``benchmark.{name}``.
    """
    return logging.getLogger(f"benchmark.{name}")
