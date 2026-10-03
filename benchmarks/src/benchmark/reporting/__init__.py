"""Report generation: JSON, CSV, Markdown, Typst tables, and charts.

Exports the primary report-writing functions for benchmark results.
Each sub-module handles a specific output format. All write functions
accept an ``output_dir`` parameter defaulting to ``outputs/`` subfolders.
"""

from benchmark.reporting.charts import generate_all_charts
from benchmark.reporting.csv import write_csv
from benchmark.reporting.json import write_comparison_json, write_model_json
from benchmark.reporting.markdown import write_markdown
from benchmark.reporting.pipeline import write_pipeline_json, write_pipeline_typst
from benchmark.reporting.typst import write_all_tables, write_thesis_tables

__all__ = [
    "write_model_json",
    "write_comparison_json",
    "write_csv",
    "write_markdown",
    "write_all_tables",
    "write_thesis_tables",
    "write_pipeline_typst",
    "write_pipeline_json",
    "generate_all_charts",
]
