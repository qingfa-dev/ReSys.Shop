import json
from pathlib import Path

from scripts._05_enrich_dataset import extract_pattern


def test_extract_pattern_solid():
    data = {"articleAttributes": {"Pattern": "Solid"}}
    assert extract_pattern(data) == "Solid"


def test_extract_pattern_missing():
    assert extract_pattern({}) == "Unknown"
    assert extract_pattern({"articleAttributes": {}}) == "Unknown"


def test_extract_pattern_empty_string():
    data = {"articleAttributes": {"Pattern": ""}}
    assert extract_pattern(data) == "Unknown"


def test_enrich_integration(tmp_path: Path):
    import subprocess

    csv_path = tmp_path / "mini.csv"
    csv_path.write_text("id,masterCategory,subCategory,articleType,baseColour,season,year,usage,gender,productDisplayName\n"
                        "1,Apparel,Topwear,Tshirts,Blue,Summer,2012,Casual,Men,Blue T-shirt\n"
                        "2,Apparel,Topwear,Shirts,Blue,Fall,2011,Casual,Men,Navy Blue Shirt\n")

    json_dir = tmp_path / "json_styles"
    json_dir.mkdir()
    (json_dir / "1.json").write_text(json.dumps({"data": {"id": 1, "articleAttributes": {"Pattern": "Solid"}}}))
    (json_dir / "2.json").write_text(json.dumps({"data": {"id": 2, "articleAttributes": {"Pattern": "Checked"}}}))

    out_dir = tmp_path / "enriched"
    result = subprocess.run([
        "uv", "run", "python", "scripts/_05_enrich_dataset.py",
        "--json-styles", str(json_dir),
        "--csv", str(csv_path),
        "--output", str(out_dir),
        "--subset", "2",
    ], capture_output=True, text=True)
    assert result.returncode == 0, result.stderr

    csv_out = out_dir / "styles.csv"
    assert csv_out.exists()

    split_file = out_dir / "splits" / "fold_0_test.json"
    samples = json.loads(split_file.read_text())
    assert len(samples) > 0
    assert "label" in samples[0]
    assert "label_pattern" in samples[0]
    if samples[0]["product_id"] == "1":
        assert samples[0]["label"] == "Topwear/Blue"
        assert samples[0]["label_pattern"] == "Topwear/Blue/Solid"
