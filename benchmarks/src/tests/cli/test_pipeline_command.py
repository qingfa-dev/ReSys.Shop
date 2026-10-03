from typer.testing import CliRunner

from benchmark.cli.benchmark import app

runner = CliRunner()


def test_pipeline_command_exists():
    result = runner.invoke(app, ["pipeline", "--help"])
    assert result.exit_code == 0
    assert "production pipeline" in result.output.lower()
