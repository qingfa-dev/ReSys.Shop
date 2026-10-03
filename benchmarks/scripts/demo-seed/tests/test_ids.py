from ids import (
    master_variant_id,
    option_value_id,
    product_id,
    taxon_id,
    variant_id,
    variant_image_id,
)


def test_master_variant_id_is_stable():
    assert master_variant_id("Striped Shirt") == "11b775bd-cf45-5d6d-8361-4975a6e406ea"


def test_variant_id_is_deterministic():
    a = variant_id("Striped Shirt", "Red", "40")
    b = variant_id("Striped Shirt", "Red", "40")
    assert a == b
    assert a != variant_id("Striped Shirt", "Blue", "40")


def test_entity_ids_are_distinct_across_kinds():
    names = {product_id("X"), taxon_id("cat.X"), option_value_id("color", "X"), variant_image_id("X", "0.default")}
    assert len(names) == 4
