from metadata import (
    build_image_alt,
    build_product_seo,
    build_taxon_seo,
    cost_price,
    extract_tags,
    variant_dimensions,
)


def test_taxon_seo_fields():
    seo = build_taxon_seo("Jeans", "Categories")
    assert seo["pretty_name"] == "Jeans"
    assert seo["permalink"].endswith("jeans")
    assert "Jeans" in seo["meta_title"]
    assert seo["description"]


def test_product_seo_contains_brand_and_article():
    seo = build_product_seo("Slim Jeans", "Jeans", "Levis", "Apparel", ["Denim", "Slim"])
    assert "Levis" in seo["meta_keywords"]
    assert "Denim" in seo["meta_keywords"]
    assert seo["meta_description"]
    assert seo["meta_description"] != "Slim Jeans"


def test_variant_dimensions_defaults_by_article():
    assert variant_dimensions("Jeans")["weight"] == 0.3
    assert variant_dimensions("Casual Shoes")["weight"] == 1.0
    assert variant_dimensions("Unknown")["weight_unit"] == "Kg"


def test_cost_price_is_half_of_price():
    assert cost_price(24.98) == 12.49
    assert cost_price(0) == 0


def test_image_alt_mentions_product():
    alt = build_image_alt("Slim Jeans", "Default")
    assert "Slim Jeans" in alt
    assert "Default" in alt


def test_extract_tags_takes_top_three_values():
    attrs = {"fit": "Slim", "fabric": "Denim", "pattern": "Solid", "occasion": "Casual", "extra": "x"}
    assert sorted(extract_tags(attrs)) == ["Denim", "Slim", "Solid"]
