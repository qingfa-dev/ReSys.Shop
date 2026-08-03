"""Variant combination generator: one value per option type, capped."""
from __future__ import annotations

import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))

MAX_VARIANTS_PER_PRODUCT = 10


def generate_variants(
    product_name: str,
    colors: list[str],
    sizes_by_color: dict[str, list[str]],
    max_variants: int = MAX_VARIANTS_PER_PRODUCT,
) -> list[dict]:
    """Generate color×size combinations in size-major order.

    The master variant is the first combination; no duplicate child is
    created for it. Returns at most ``max_variants`` entries.
    ``product_name`` is reserved for future deterministic ID coupling.
    """
    combos: list[tuple[str | None, str | None]] = []
    for color in colors:
        sizes = sizes_by_color.get(color, []) or []
        if sizes:
            for size in sizes:
                combos.append((color, size))
        else:
            combos.append((color, None))
    if not combos:
        combos.append((None, None))

    selected = combos[:max_variants]
    return [
        {
            "color": color,
            "size": size,
            "is_master": i == 0,
            "position": i,
        }
        for i, (color, size) in enumerate(selected)
    ]


def derive_sku(base: str, variant_index: int) -> str:
    safe = base.upper().replace(" ", "-").replace("'", "").replace("&", "AND")[:20]
    return f"{safe}-{variant_index:03d}"
