from variants import generate_variants


def test_master_is_first_combo_and_child_combo_not_repeated():
    combos = generate_variants("Shirt", ["Red", "Blue"], {"Red": ["S", "M"], "Blue": ["L"]})
    masters = [c for c in combos if c["is_master"]]
    assert len(masters) == 1
    assert masters[0] == {"color": "Red", "size": "S", "is_master": True, "position": 0}
    child_combos = {(c["color"], c["size"]) for c in combos if not c["is_master"]}
    assert ("Red", "S") not in child_combos
    assert ("Red", "M") in child_combos
    assert ("Blue", "L") in child_combos


def test_size_major_ordering():
    combos = generate_variants("Shirt", ["Red", "Blue"], {"Red": ["S", "M"], "Blue": ["S"]})
    assert [(c["color"], c["size"]) for c in combos] == [
        ("Red", "S"), ("Red", "M"), ("Blue", "S"),
    ]


def test_cap_at_ten_variants():
    colors = [f"C{i}" for i in range(4)]
    sizes_by_color = {c: [f"S{j}" for j in range(5)] for c in colors}
    combos = generate_variants("Product", colors, sizes_by_color)
    assert len(combos) == 10
    assert combos[0]["is_master"]
    assert combos[0]["position"] == 0
    assert [c["position"] for c in combos] == list(range(10))


def test_no_sizes_color_only_master_and_children():
    combos = generate_variants("Perfume", ["Gold", "Silver"], {"Gold": [], "Silver": []})
    assert [(c["color"], c["size"], c["is_master"]) for c in combos] == [
        ("Gold", None, True), ("Silver", None, False),
    ]


def test_no_color_no_sizes_master_only():
    combos = generate_variants("Mystery", [], {})
    assert combos == [{"color": None, "size": None, "is_master": True, "position": 0}]


def test_every_variant_has_at_most_one_value_per_type():
    combos = generate_variants("Shirt", ["Red", "Blue"], {"Red": ["S", "M", "L"], "Blue": ["S"]})
    for c in combos:
        assert c["color"] is None or c["size"] is None or c["color"] != c["size"]
        assert sum(v is not None for v in (c["color"], c["size"])) <= 2
