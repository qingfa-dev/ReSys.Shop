# 09 — Visual Similarity Attributes

How the benchmark defines "similarity" — why only 2 of 6 dataset attributes
capture visual resemblance, and why adding the other 4 makes the evaluation worse.

---

## 1. The Core Problem: Operationalising "Looks Like"

Given a query image of a fashion product, what other products should be considered
visually similar? The benchmark must define a **ground-truth relevance rule** —
a deterministic function that, for any two products, returns "relevant" (visually
similar) or "not relevant."

The Fashion Product Images dataset provides 6 categorical attributes per product:

| # | Column | Type | Example | Cardinality |
|---|--------|------|---------|-------------|
| 1 | `masterCategory` | Text | Apparel | 7 |
| 2 | `subCategory` | Text | Topwear | 45 |
| 3 | `articleType` | Text | Tshirts | 143 |
| 4 | `baseColour` | Text | Navy Blue | 46 |
| 5 | `season` | Text | Summer | 5 |
| 6 | `usage` | Text | Casual | 9 |
| (+1) | `gender` | Text | Men | 5 |

**Not all attributes are visual.** Some describe what the product **is** (type,
colour). Others describe **who** it's for (gender), **when** it's worn (season),
or **how** it's marketed (usage). Mixing non-visual attributes into the relevance
rule produces **false negatives** — visually identical products treated as
"not similar" because their metadata differs.

---

## 2. Attribute-by-Attribute Visual Analysis

### 2.1 `subCategory` — ✅ Visual (Product Type)

Defines the broad type of clothing item: Topwear, Bottomwear, Dresses, Shoes,
Accessories, etc.

- **45 unique values, median 129 items per group.**
- This is the **strongest visual signal**: a T-shirt is visually distinct from
  a shoe regardless of colour.
- Used alone: too broad — any two Topwear items are "similar" regardless of
  colour (black T-shirt ≈ blue T-shirt, which is wrong for visual similarity).

### 2.2 `articleType` — ⚠️ Partially Visual (Sub-type)

Defines the specific product type: Tshirts, Shirts, Polos, Jeans, Casual Shoes,
etc.

- **143 unique values, median 48 items per group.**
- More granular than `subCategory` — separates Tshirts from Shirts from Polos.
- **Downside**: visually similar items get separated. A black T-shirt and a
  black polo look very similar (both are black upper-body clothing) but have
  different `articleType` labels. Adding `articleType` creates false negatives.
- **Verdict**: `subCategory` is the better level of granularity for visual type.
  `articleType` is useful for product taxonomy but not for visual similarity.

### 2.3 `baseColour` — ✅ Visual (Colour, After Normalisation)

The most prominent visual feature after shape/type.

- **46 raw label values**, including near-duplicates: "Blue", "Navy Blue",
  "Turquoise Blue", "Dark Blue", "Light Blue", "Sky Blue" — all visually
  similar blue tones.
- **Raw-colour matching is too strict** (§3): 21 % of products have no
  colour-mate in the gallery. A "Navy Blue" T-shirt has zero "Blue" matches.
- **Normalised-colour matching** (§4) is the correct approach: merge 46 raw
  labels into 12 perceptual colour groups.

### 2.4 `season` — ❌ Marketing Label (Not Visual)

Values: Summer, Winter, Fall, Spring (+ Unknown).

- **A black T-shirt tagged "Summer" looks identical to a black T-shirt tagged
  "Winter".** There is no visual difference.
- Adding `season` to the relevance key splits groups into season-specific
  subgroups. Example:

  | Group | Items | Problem |
  |-------|-------|---------|
  | Black Topwear — Summer | 1,149 | ✓ |
  | Black Topwear — Fall | 832 | ✓ |
  | Black Topwear — Winter | **27** | ✗ Too few matches |
  | Black Topwear — Spring | **14** | ✗ Nearly all queries have zero relevant items |

- **Verdict**: Season is a marketing/supply-chain label. It does not change
  the visual appearance of the product. Including it **degrades** the benchmark.

### 2.5 `gender` — ❌ Demographic Label (Minimally Visual)

Values: Men, Women, Boys, Girls, Unisex (+ Boys, Girls).

- **A black men's T-shirt looks similar to a black women's T-shirt.** The cut
  may differ slightly, but the visual features (black fabric, torso shape)
  dominate.
- Adding `gender` fragments groups:

  | Group | Items |
  |-------|-------|
  | Black Topwear — Men | 1,310 |
  | Black Topwear — Women | 628 |
  | Black Topwear — Boys | **60** |
  | Black Topwear — Girls | **12** |
  | Black Topwear — Unisex | **12** |

  Boys', Girls', and Unisex groups have too few items for meaningful evaluation.

- **Verdict**: Gender adds demographic information that has minimal visual
  impact. Including it creates false negatives and breaks group sizes.

### 2.6 `usage` — ❌ Marketing Label (Not Visual)

Values: Casual, Sports, Formal, Ethnic, Smart Casual, Party, Travel, Home (+ Unknown).

- **A "Casual" black T-shirt and a "Sports" black T-shirt are visually
  identical.** The label describes the marketing occasion, not the visual
  appearance.
- Adding `usage` fragments groups similarly to `season` and `gender`: Casual
  items dominate (77 % of the dataset), while Formal, Party, Travel, and Home
  have single-digit group sizes.

### 2.7 `masterCategory` — ⚠️ Redundant

Values: Apparel, Footwear, Accessories, etc. (7 total).

- `subCategory` is already nested within `masterCategory`: "Topwear" only exists
  under "Apparel". Adding `masterCategory` to the key provides no additional
  discrimination.
- **The current implementation includes it for clarity, but it is functionally
  equivalent to using `subCategory` alone.**

### 2.8 Pattern Attribute from Per-Product JSON

The full Fashion Product Images dataset includes 44,446 per-product JSON files
(`styles/{product_id}.json`) containing a nested `articleAttributes` object.
The `Pattern` key holds one of:

| Pattern | Coverage | Visual Meaning |
|---------|----------|---------------|
| Solid | 36% | Single colour, no visible pattern |
| Printed | 26% | Graphic/text print |
| Checked | 16% | Checked/plaid pattern |
| Striped | 15% | Horizontal/vertical stripes |
| Self Design | 5% | Subtle tone-on-tone pattern |
| Unknown | 52% | No pattern data available |

Pattern is the highest-coverage, highest-visual-impact attribute beyond colour.
A checked shirt is visually very different from a solid shirt of the same colour.

The `benchmark enrich` command merges these JSON attributes with the CSV metadata,
producing dual-label split files with both `label` (primary: subCategory/colour)
and `label_pattern` (secondary: subCategory/colour/pattern).  See
[11 — Enriched Dataset](11-enriched-dataset.md) for usage.

---

## 3. Why Raw Colour Matching Fails

The dataset uses 46 distinct `baseColour` labels from free-text entry:

```
Black, Charcoal, White, Off White, Cream, Blue, Navy Blue, Dark Blue,
Light Blue, Sky Blue, Turquoise Blue, Turquoise, Teal, Aqua, Sea Green,
Red, Maroon, Burgundy, Rust, Coral, Magenta, Rose, Mauve, Peach, Pink,
Lavender, Green, Olive, Lime Green, Purple, Grey, Silver, Orange, Multi,
Brown, Coffee Brown, Mushroom Brown, Tan, Beige, Khaki, Nude, Taupe,
Copper, Bronze, Gold, Yellow, Mustard, Lemon
```

Perceptual colour groups are far fewer: humans perceive roughly 11–12 basic
colour categories (Berlin & Kay, 1969). The 46 labels collapse into these
groups:

| Normalised group | Raw labels folded in | Items |
|-----------------|---------------------|-------|
| Black | Black, Charcoal | 9,728 |
| White | White, Off White, Cream | 6,110 |
| Blue | Blue, Navy Blue, Dark Blue, Light Blue, Sky Blue, Turquoise Blue, Turquoise, Teal, Aqua, Sea Green | 6,896 |
| Red | Red, Maroon, Burgundy, Rust, Peach, Coral, Magenta, Rose, Mauve | 3,342 |
| Green | Green, Olive, Lime Green | 2,553 |
| Grey | Grey, Silver | 4,059 |
| Pink | Pink, Lavender | 1,888 |
| Purple | Purple | 1,960 |
| Orange | Orange | 530 |
| Brown/Yellow | Brown, Coffee Brown, Mushroom Brown, Tan, Beige, Khaki, Copper, Bronze, Gold, Yellow, Mustard, Lemon, Nude, Taupe | 4,724 |
| Multi | Multi | 394 |

**Impact**: Normalisation reduces the number of distinct colour values from 46
to 11, merging "Navy Blue" into "Blue", "Maroon" into "Red", etc. This cuts
the solo-item rate from 21 % to 12 % and doubles the median group size.

---

## 4. Full Relevance Scheme Matrix

All 18 attribute combinations evaluated on the 44,424-product dataset:

| # | Relevance key | Groups | Median size | Solo rate | Verdict |
|---|--------------|--------|-------------|-----------|---------|
| 1 | masterCategory | 7 | 2,403 | 1 % | Too broad |
| 2 | subCategory | 45 | 129 | 4 % | Ignores colour |
| 3 | articleType | 143 | 48 | 5 % | Ignores colour |
| 4 | norm_colour | 17 | 1,798 | 0 % | Black shoe≈Black T-shirt |
| 5 | usage | 9 | 317 | 11 % | Not visual |
| 6 | gender | 5 | 2,161 | 0 % | Not visual |
| 7 | master+sub | 47 | 118 | 6 % | Ignores colour (original) |
| **8** | **subCat+norm_colour** | **411** | **16** | **12 %** | **✅ Optimal** |
| 9 | subCat+articleType | 169 | 28 | 10 % | Ignores colour |
| 10 | articleType+norm_colour | 1,044 | 7 | 19 % | Too strict |
| 11 | subCat+season | — | — | — | Season not visual |
| 12 | subCat+gender | — | — | — | Gender not visual |
| 13 | subCat+usage | 124 | 20 | 15 % | Not visual |
| 14 | subCat+norm_colour+season | 894 | 8 | 20 % | Degraded |
| 15 | subCat+norm_colour+gender | 827 | 8 | 18 % | Degraded |
| 16 | subCat+norm_colour+usage | 747 | 7 | 21 % | Degraded |
| 17 | subCat+norm_colour+season+gender | 1,620 | 4 | 25 % | Broken |
| 18 | subCat+norm_colour+all3extra | 2,285 | 3 | 30 % | Broken |

**Conclusion**: Scheme #8 (`subCategory + normalized colour`) is the
statistically and perceptually optimal relevance rule. It:

- Merges visually similar colours (Navy Blue → Blue)
- Keeps visually distinct items separate (Topwear ≠ Shoes, Black ≠ Red)
- Provides median 16 relevant items per query — sufficient for P@5 and P@10
- Has only 12 % solo query rate — 88 % of products have valid colour-mates

Every scheme adding `season`, `gender`, or `usage` produces worse results:
fewer matches per query, more solo queries, and false negatives from splitting
visually identical items.

---

## 5. Concrete Examples: What the Benchmark Now Says

### Correctly Similar (Relevant)

| Query | Retrieved | Key | Relevant? | Why |
|-------|-----------|-----|-----------|-----|
| Black T-shirt | Black polo | Topwear/Black | ✅ Yes | Same type (topwear) + same colour (black) |
| Navy Blue jeans | Blue jeans | Bottomwear/Blue | ✅ Yes | Navy Blue normalises to Blue |
| Red dress | Maroon dress | Dresses/Red | ✅ Yes | Maroon normalises to Red |
| White sneaker | White sneaker | Shoes/White | ✅ Yes | Exact match |

### Correctly NOT Similar

| Query | Retrieved | Key | Relevant? | Why |
|-------|-----------|-----|-----------|-----|
| Black T-shirt | Blue T-shirt | Topwear/Black vs Topwear/Blue | ❌ No | Different colour |
| Black T-shirt | Black shoe | Topwear/Black vs Shoes/Black | ❌ No | Different type |
| Red dress | Blue dress | Dresses/Red vs Dresses/Blue | ❌ No | Different colour |
| Summer Black T | Winter Black T | Both Topwear/Black | ✅ Yes | Season is irrelevant visually |

### Borderline (Acknowledged Limitations)

| Query | Retrieved | Key | Relevant? | Why |
|-------|-----------|-----|-----------|-----|
| Black T-shirt | Black dress | Topwear vs Dresses | ❌ No | Different subCategory — both are black torso clothing but taxonomy separates them |
| "Navy Blue" dress | "Turquoise" dress | Dresses/Blue vs Dresses/Blue | ✅ Yes | Both fold into "Blue" — visually similar |
| "Teal" top | "Green" top | Topwear/Blue vs Topwear/Green | ❌ No | Teal → Blue, Green → Green — borderline colour case |

---

## 6. Implementation

The relevance rule is implemented in two places in
`src/benchmark/datasets/ground_truth.py`:

### `_normalize_colour(raw)` — Colour Normalisation

```python
def _normalize_colour(raw: str | float | None) -> str:
    """Map 46 raw colour labels → 11 visual colour groups."""
    if pd.isna(raw) or not isinstance(raw, str) or not raw.strip():
        return "Unknown"
    c = raw.strip()

    if any(t in c.lower() for t in ("black", "charcoal")):   return "Black"
    if any(t in c.lower() for t in ("white", "off white", "cream")): return "White"
    if any(t in c.lower() for t in ("blue", "navy", "turquoise", "teal", "aqua", "sky")):
        return "Blue"
    # … (11 groups total)
    return c  # unknown labels pass through unchanged
```

### `build_relevance_sets(df)` — Relevance Key Builder

```python
df["_norm_colour"] = df["baseColour"].apply(_normalize_colour)
df["_relevance_key"] = df.apply(
    lambda row: f"{row['subCategory']}/{row['_norm_colour']}"
    if pd.notna(row.get("subCategory"))
    else "Unknown"
)
```

### `GroundTruth.generate_splits()` — Split File Labels

The split JSON files use the same normalized colour in their `label` field:

```json
{"image_path": "images/1163.jpg", "label": "Topwear/Blue", "product_id": "1163"}
```

The evaluator (`Evaluator.evaluate_split()`) matches query labels against
gallery labels — so the 3-part key flows through the entire pipeline.

---

## Glossary

| Term | Definition |
|------|-----------|
| **Ground truth** | The set of labels that define which gallery items are "relevant" for each query. Used to compute Precision@K, Recall@K, and mAP. |
| **Relevance key** | A concatenation of attribute values (e.g., `Topwear/Blue`) that defines a relevance group. All items sharing the same key are relevant to each other. |
| **Colour normalisation** | The process of mapping 46 raw `baseColour` labels (e.g., "Navy Blue", "Turquoise Blue") into 11 perceptual colour groups (e.g., "Blue"). Addresses the "Navy Blue ≠ Blue" false-negative problem. |
| **Solo item** | A product whose relevance group contains only itself — it has no colour-mate in the dataset. Solo items cannot be evaluated for Recall@K (there's nothing to recall). |
| **False positive** | The benchmark says two items are relevant, but a human would say they're not. Example: category-only scheme calling a black T-shirt "similar" to a blue T-shirt. |
| **False negative** | The benchmark says two items are NOT relevant, but a human would say they are. Example: raw-colour scheme calling "Navy Blue" and "Blue" different colours. |
| **Stratified split** | Each fold preserves the proportion of each `masterCategory` in the full dataset. Ensures categories with few items are represented in every fold. |
| **Perceptual colour categories** | The 11 basic colour terms that humans across cultures use to partition colour space (Berlin & Kay, 1969). |

---

## References

1. **Berlin, B. & Kay, P.** (1969). *Basic Color Terms: Their Universality
   and Evolution.* University of California Press.
   — Foundation for why 11 colour groups capture human colour perception.

2. **Fashion Product Images Dataset.** Aggarwal, P. Kaggle.
   <https://www.kaggle.com/datasets/paramaggarwal/fashion-product-images-dataset>
   — Source dataset with 44,424 products, 46 raw colour labels.

3. **Liu, Z. et al.** (2016). "DeepFashion: Powering Robust Clothes Recognition
   and Retrieval with Rich Annotations." CVPR 2016.
   — Established category-based ground truth as a standard in fashion CBIR.

4. **Zheng, L. et al.** (2017). "SIFT Meets CNN: A Decade Survey of Instance
   Retrieval." IEEE TPAMI.
   — Established mAP as the standard CBIR evaluation metric.

5. **ReSys.Shop Benchmark Documentation:**
   - `benchmarks/docs/05-datasets.md` — dataset structure and columns
   - `benchmarks/docs/06-thesis-protocol.md` — thesis evaluation protocol
   - `benchmarks/docs/08-visual-similarity-pipeline.md` — end-to-end pipeline
   - `benchmarks/docs/09-documentation-review.md` — review of documentation quality

6. **Source code:**
   - `src/benchmark/datasets/ground_truth.py` — `_normalize_colour()`,
     `build_relevance_sets()`, `GroundTruth.generate_splits()`
   - `src/benchmark/evaluation/evaluator.py` — `evaluate_split()`,
     label-based relevance computation
