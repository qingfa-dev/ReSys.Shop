# Thesis Review Remediation Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Resolve all 22 findings from `thesis/reviews/thesis-review-MASTER-FIXLIST.md` so the CTU undergraduate thesis compiles cleanly and every number, citation, and count is internally consistent and matches its authoritative source.

**Architecture:** This is a Typst documentation-consistency remediation, not a code change. The "test" for each task is a combination of (a) `typst compile main.typ` exiting 0, (b) a ripgrep consistency sweep finding no forbidden stale strings, and (c) a Python arithmetic-recheck script (built in Task 1) that recomputes derived percentages from the benchmark JSON and asserts equality with the thesis-stated values. Edits target `.typ` source files and `backmatter/bibliography.bib`. The benchmark JSON at `benchmarks/outputs/thesis/results/` is the authoritative source for all numbers; the real published papers are authoritative for citation metadata.

**Tech Stack:** Typst 0.15.1 (thesis build), ripgrep (consistency sweeps), Python 3.12 + stdlib `json`/`pathlib` (arithmetic recheck script), BibTeX (bibliography).

## Global Constraints

Copied verbatim from `thesis/spec/spec-process-thesis-review-remediation.md`:

- **Typst 0.15.1** is the sole build tool; `typst compile main.typ` run from `thesis/` is the whole verification step (`thesis/AGENTS.md`).
- **Verify-before-fix (CON-001/CON-002/CON-003):** no review or rewrite recommendation is applied until checked against the benchmark JSON, EF Core migrations, Carter routes, or real published paper. The `thesis-rewrite-*.md` files were authored from the PDF without codebase access; their "six" model count is wrong (registry has 11), and any assumption that Table 67 is authoritative is wrong (Appendix A is authoritative).
- **Appendix A is authoritative for benchmark numbers (REQ-101 RESOLVED):** `thesis_results_category_only.json` FashionCLIP mAP = 0.9309 matches Appendix A.1, not Table 67's 0.8788. Table 67/68 are stale and must be regenerated.
- **Root-cause-first (REQ-006):** fix each root error once, then sweep its propagation set in one pass. Benchmark reconciliation (Task 2) is done before any downstream number propagation.
- **No heading numbering in chapter files** — controlled per-part in `main.typ` (`thesis/AGENTS.md`).
- **Do not delete** `chapters/part2/ch2-design/04-implementation/` (singular) until grepped across `chapters/` and `figures/` (`thesis/AGENTS.md` gotcha).
- **Compliance:** Times New Roman 13pt, margins L4/R2.5/T2.5/B2.5cm, line spacing 1.2, paragraph indent 1cm, abstract 200–350 words, 3–5 keywords, ≥15 references (`thesis/compliance.json`).
- **No git stash / worktree / revert / restore without human permission** (repo-root `AGENTS.md`).
- **Never commit unless the human explicitly asks.**

## Authoritative benchmark values (read from JSON, do not retype from this plan)

These are the ground-truth values the edits must match. The implementer must re-read them from the JSON in Task 1's script rather than trusting this table, but they are reproduced here so the plan is self-contained.

**Accuracy — category-only (`benchmarks/outputs/thesis/results/thesis_results_category_only.json`), matches Appendix A.1:**

| Model | mAP | SD | P@5 | P@10 | P@20 | R@5 | R@10 | R@20 |
|-------|-----|-----|-----|------|------|------|------|------|
| Fashion-CLIP | 0.9309 | 0.0068 | 0.9582 | 0.9493 | 0.9374 | 0.0280 | 0.0483 | 0.0810 |
| CLIP-generic | 0.9115 | 0.0077 | 0.9440 | 0.9364 | 0.9239 | 0.0264 | 0.0459 | 0.0768 |
| EfficientNet-B0 | 0.8895 | 0.0056 | 0.9340 | 0.9229 | 0.9077 | 0.0249 | 0.0426 | 0.0720 |
| ResNet-50 | 0.8857 | 0.0114 | 0.9327 | 0.9203 | 0.9035 | 0.0274 | 0.0470 | 0.0799 |

**Efficiency (`benchmarks/outputs/thesis/results/thesis_results.json`), matches Appendix A.4:**

| Model | Latency (ms) | Throughput (img/s) | Load (ms) | Storage (MB) |
|-------|--------------|--------------------|-----------|--------------|
| Fashion-CLIP | 96.76 ± 6.76 | 18.47 ± 1.25 | 5255.38 | 3.26 |
| ResNet-50 | 61.93 ± 5.81 | 13.47 ± 0.67 | 374.09 | 13.02 |
| EfficientNet-B0 | (read from JSON) | (read from JSON) | 110.24 | (read from JSON) |
| CLIP-generic | (read from JSON) | (read from JSON) | 6848.54 | (read from JSON) |

> The plan deliberately leaves some efficiency cells as "read from JSON" so the implementer runs the Task 1 script rather than copying a table that might be misread. The script prints the full set.

**Indicative recomputed derived percentages (recompute precisely in Task 3, do not copy these rounded values into the thesis blindly):**

- Fashion-CLIP vs CLIP-generic mAP: (0.9309 − 0.9115) / 0.9115 = **2.13%** (was 5.4%)
- Fashion-CLIP vs EfficientNet-B0 mAP: (0.9309 − 0.8895) / 0.8895 = **4.65%** (was 7.7%)
- Fashion-CLIP vs ResNet-50 mAP: (0.9309 − 0.8857) / 0.8857 = **5.10%** (was 8.2%)
- EfficientNet-B0 as % of Fashion-CLIP mAP: 0.8895 / 0.9309 = **95.56%** (was 92.8%)
- EfficientNet-B0 latency as % of Fashion-CLIP latency: (read both from JSON, recompute) — was stated as 26.0%
- EfficientNet-B0 × speed vs Fashion-CLIP: (recompute) — was stated as 3.8×

---

## Task 1: Build the verification oracle (Python recheck script + remediation log)

This task creates the "test harness" that every later task runs. It has no thesis edits.

**Files:**
- Create: `thesis/spec/verify_remediation.py`
- Create: `thesis/spec/remediation-log.md`
- Read: `benchmarks/outputs/thesis/results/thesis_results_category_only.json`
- Read: `benchmarks/outputs/thesis/results/thesis_results.json`

**Interfaces:**
- Consumes: the two benchmark JSON files (paths above).
- Produces: `verify_remediation.py` (run as `python3 thesis/spec/verify_remediation.py`); prints authoritative values + recomputed percentages and exits non-zero if a JSON file is missing. Later tasks run it to confirm their numbers.

- [ ] **Step 1: Write the script**

Create `thesis/spec/verify_remediation.py`:

```python
"""Verification oracle for thesis review remediation.

Reads the authoritative benchmark JSON and prints the values the thesis
must match. Later tasks run this to confirm edits. Exits non-zero if a
JSON file is missing or unreadable.
"""
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
BENCH = ROOT / "benchmarks/outputs/thesis/results"
CAT_ONLY = BENCH / "thesis_results_category_only.json"
EFFICIENCY = BENCH / "thesis_results.json"


def load(path: Path) -> list:
    if not path.exists():
        raise SystemExit(f"MISSING authoritative source: {path}")
    return json.loads(path.read_text())


def fmt(v: dict) -> str:
    return f"{round(v['mean'], 4)} ± {round(v['std'], 4)}"


def main() -> None:
    cat = load(CAT_ONLY)
    eff = load(EFFICIENCY)

    print("=== CATEGORY-ONLY (Appendix A.1 authoritative) ===")
    models = {m["model_name"]: m for m in cat}
    for name, m in models.items():
        a = m["aggregate"]
        print(f"{name}: mAP {fmt(a['map'])}")
        for k in ("precision@5", "precision@10", "precision@20",
                  "recall@5", "recall@10", "recall@20"):
            print(f"    {k}: {fmt(a[k])}")

    print("\n=== EFFICIENCY (Appendix A.4 authoritative) ===")
    emodels = {m["model_name"]: m for m in eff}
    for name, m in emodels.items():
        a = m["aggregate"]
        print(f"{name}:")
        for k in ("latency_mean_ms", "throughput_per_sec",
                  "load_time_ms", "index_storage_mb"):
            if k in a:
                print(f"    {k}: {fmt(a[k])}")

    print("\n=== RECOMPUTED DERIVED PERCENTAGES ===")
    fclip = models["FashionCLIP"]["aggregate"]["map"]["mean"]
    clip_g = models["CLIP-generic"]["aggregate"]["map"]["mean"]
    effnet = models["EfficientNet-B0"]["aggregate"]["map"]["mean"]
    resnet = models["ResNet-50"]["aggregate"]["map"]["mean"]

    def pct(numerator_gain: float, base: float) -> float:
        return round((numerator_gain - base) / base * 100, 2)

    print(f"Fashion-CLIP vs CLIP-generic mAP: {pct(fclip, clip_g)}% (was 5.4%)")
    print(f"Fashion-CLIP vs EfficientNet-B0 mAP: {pct(fclip, effnet)}% (was 7.7%)")
    print(f"Fashion-CLIP vs ResNet-50 mAP: {pct(fclip, resnet)}% (was 8.2%)")
    print(f"EfficientNet-B0 as % of Fashion-CLIP mAP: {round(effnet/fclip*100, 2)}% (was 92.8%)")
    print(f"CLIP-generic vs EfficientNet-B0 mAP: {pct(clip_g, effnet)}% (was 2.2%)")
    print(f"CLIP-generic vs ResNet-50 mAP: {pct(clip_g, resnet)}% (was 2.7%)")

    ef = emodels["FashionCLIP"]["aggregate"]
    ee = emodels["EfficientNet-B0"]["aggregate"]
    print(f"EfficientNet-B0 latency as % of Fashion-CLIP: "
          f"{round(ee['latency_mean_ms']['mean']/ef['latency_mean_ms']['mean']*100, 2)}% (was 26.0%)")
    print(f"Fashion-CLIP latency / EfficientNet-B0 latency: "
          f"{round(ef['latency_mean_ms']['mean']/ee['latency_mean_ms']['mean'], 2)}x (was 3.8x)")

    print("\n=== CONFIDENCE-INTERVAL BOUNDS (mean ± 2SD) ===")
    for name, m in models.items():
        mv = m["aggregate"]["map"]
        lo = round(mv["mean"] - 2 * mv["std"], 4)
        hi = round(mv["mean"] + 2 * mv["std"], 4)
        print(f"{name}: mAP lower {lo}, upper {hi}")


if __name__ == "__main__":
    main()
```

- [ ] **Step 2: Run the script and confirm it prints the authoritative values**

Run: `python3 thesis/spec/verify_remediation.py`
Expected: prints the category-only table (FashionCLIP mAP 0.9309 ± 0.0068), the efficiency table, the recomputed percentages (Fashion-CLIP vs CLIP-generic ≈ 2.13%), and exits 0.

- [ ] **Step 3: Write the remediation log skeleton**

Create `thesis/spec/remediation-log.md`:

```markdown
# Remediation Log

One row per finding from thesis-review-MASTER-FIXLIST.md.

| finding_id | title | status | authoritative_source | files_changed | compile_ok | notes |
|------------|-------|--------|----------------------|---------------|------------|-------|
| T1-1 | Benchmark Table 67/68 vs Appendix A | unverified | thesis_results_category_only.json; thesis_results.json | | | Appendix A authoritative per JSON |
| T1-2 | "Eleven models" claim | unverified | benchmarks/src/benchmark/models/__init__.py:44-56 (11) | | | Review/rewrite said "six"; REJECTED per CON-001 |
| T1-3 | Fashion-CLIP improvement % (15-20/5.4/6.1) | unverified | thesis_results_category_only.json | | | |
| T1-4 | Fabricated citations [6],[27] | unverified | real papers (DOIs) | | | |
| T1-5 | "Nine bounded contexts" | unverified | Table 47 (8 rows) | | | |
| T1-6 | 88 FRs / nine modules | unverified | Tables 10-17 sum = 87 | | | |
| T1-7 | "Near-zero P@20" | unverified | Appendix A.2/A.3 (~0.30) | | | |
| T1-8 | CBIR endpoint 3 URLs | unverified | service/Api/src/Module/Features/ | | | |
| T1-9 | Variable Vector Dimensions vs vector(512) | unverified | service/Api/src/Migrations/ | | | |
| T1-10 | Phantom "Section 2.1.5" | unverified | thesis TOC | | | |
| T1-11 | Four vs five UI states | unverified | Table 58 (4 rows) | | | |
| T1-12 | Thesis Outline chapter numbering | unverified | real TOC | | | |
| T2-13 | PostgreSQL 16 vs 17 | unverified | container tag pg17-trixie | | | |
| T2-14 | pgvector 0.3.2 vs 0.7.0 | unverified | Directory.Packages.props | | | |
| T2-15 | Permission-string format | unverified | service code | | | |
| T2-16 | "Eight use cases" storefront | unverified | §2.4.5.2.1-8 (9) | | | |
| T2-17 | Accuracy metrics count 3/5/7 | unverified | Table 65 (3 families) | | | |
| T2-18 | Pinterest 30% stat | unverified | pinterest2023visual ref | | | |
| T2-19 | Table 70 traceability mis-citations | unverified | section audit | | | |
| T2-20 | EfficientNet-B0 3.4% vs 7.7% | unverified | thesis_results_category_only.json | | | |
| T2-21 | DeepFashion missing co-author | unverified | real paper (Shi Qiu) | | | |
| T2-22 | Sequential selection claim | unverified | benchmarks dataset prep | | | |
| T3-1..6 | Polish items | unverified | | | | |
```

- [ ] **Step 4: Verify the baseline thesis builds before any edits**

Run: `typst compile main.typ` (from `thesis/`)
Expected: exits 0 and produces `main.pdf`. If it fails, STOP and report — the baseline must be green before remediation.

- [ ] **Step 5: Commit**

```bash
git add thesis/spec/verify_remediation.py thesis/spec/remediation-log.md
git commit -m "chore(thesis): add remediation verification oracle and log"
```

---

## Task 2: Regenerate Chapter 3 Table 67 (accuracy) from the authoritative JSON

This is the root-cause fix for TIER 1 #1. All downstream number tasks depend on it.

**Files:**
- Modify: `thesis/chapters/part2/ch3-evaluation/05-retrieval-performance.typ:20-38` (Table 67 + surrounding analysis paragraph)

**Interfaces:**
- Consumes: `verify_remediation.py` output (authoritative category-only values).
- Produces: a Table 67 whose Fashion-CLIP mAP = 0.9309 (not 0.8788) and a first analysis paragraph whose derived percentages match the recomputed values.

- [ ] **Step 1: Read the current Table 67 and confirm the stale values**

Run: `rg -n "0\.8788|0\.8341|0\.8158|0\.8120" thesis/chapters/part2/ch3-evaluation/05-retrieval-performance.typ`
Expected: matches at lines 22, 23, 30, 32, 36, 38 (the stale 0.8788 etc.).

- [ ] **Step 2: Run the oracle to get the exact replacement values**

Run: `python3 thesis/spec/verify_remediation.py`
Capture: the category-only block (FashionCLIP mAP 0.9309 ± 0.0068, P@5 0.9582, P@10 0.9493, P@20 0.9374, R@5 0.0280, R@10 0.0483, R@20 0.0810; and the other three models) and the recomputed percentages block.

- [ ] **Step 3: Rewrite Table 67 rows in `05-retrieval-performance.typ`**

Replace the four data rows (lines ~22-25) with the authoritative values. Use 4-decimal mAP with SD, 4-decimal P@K/R@K. Keep Fashion-CLIP row bolded. Example row (verify each value against the oracle output before typing):

```typst
    [Fashion-CLIP], [*0.9309 ± 0.0068*], [*0.9582*], [*0.9493*], [*0.9374*], [*0.0280*], [*0.0483*], [*0.0810*],
    [CLIP-generic], [0.9115 ± 0.0077], [0.9440], [0.9364], [0.9239], [0.0264], [0.0459], [0.0768],
    [EfficientNet-B0], [0.8895 ± 0.0056], [0.9340], [0.9229], [0.9077], [0.0249], [0.0426], [0.0720],
    [ResNet-50], [0.8857 ± 0.0114], [0.9327], [0.9203], [0.9035], [0.0274], [0.0470], [0.0799],
```

- [ ] **Step 4: Rewrite the analysis paragraph (lines ~30-38) with recomputed percentages**

Replace every stale percentage and P@K value with the recomputed ones from the oracle. Specifically:
- "mAP of 0.8788 is 5.4% above CLIP-generic (0.8341), 7.7% above EfficientNet-B0 (0.8158), and 8.2% above ResNet-50 (0.8120)" → use 0.9309, and the recomputed ~2.13% / ~4.65% / ~5.10% (use the oracle's exact figures).
- "P@5 (0.9304 vs 0.9025), P@10 (0.9155 vs 0.8862), P@20 (0.8982 vs 0.8640)" → use 0.9582 vs 0.9440, 0.9493 vs 0.9364, 0.9374 vs 0.9239.
- "CLIP-generic ... 2.2% over EfficientNet-B0 and 2.7% over ResNet-50" → use the oracle's recomputed CLIP-generic vs EfficientNet-B0 and vs ResNet-50 figures.
- "Fashion-CLIP's mAP lower bound (0.8744) exceeds the upper bound of EfficientNet-B0 (0.8172) and ResNet-50 (0.8224)" → use the oracle's confidence-interval bounds (mean ± 2SD) for all three models. **Verify the separation still holds** with the new bounds; if it does not, rewrite the sentence to describe the actual overlap (this is the Edge-5 check from the spec).

- [ ] **Step 5: Compile and verify**

Run: `typst compile main.typ` (from `thesis/`)
Expected: exits 0, `main.pdf` produced.

Run: `rg -n "0\.8788|0\.8341|0\.8158|0\.8120" thesis/chapters/part2/ch3-evaluation/05-retrieval-performance.typ`
Expected: no matches (all stale values removed).

- [ ] **Step 6: Commit**

```bash
git add thesis/chapters/part2/ch3-evaluation/05-retrieval-performance.typ
git commit -m "fix(thesis): regenerate Table 67 from authoritative category-only benchmark"
```

---

## Task 3: Regenerate Table 68 (efficiency) and the §3.7.4 "6.1%" fix

**Files:**
- Modify: `thesis/chapters/part2/ch3-evaluation/07-model-comparison.typ:14-32` (Table 68 synthesis + deployment-recommendation paragraph)
- Modify: `thesis/chapters/part2/ch3-evaluation/06-efficiency-metrics.typ:38` (RQ2 answer paragraph)
- Modify: `thesis/chapters/part2/ch3-evaluation/07-model-comparison.typ:38` (the "6.1%" bullet)

**Interfaces:**
- Consumes: `verify_remediation.py` efficiency block + recomputed latency ratios.
- Produces: Table 68 with authoritative latency/throughput/load/storage; RQ2 answer with recomputed trade-off figures; the §3.7.4 summary bullet using the recomputed Fashion-CLIP vs CLIP-generic percentage (not 6.1%, not 5.4%).

- [ ] **Step 1: Run the oracle for efficiency values and latency ratios**

Run: `python3 thesis/spec/verify_remediation.py`
Capture: the efficiency block (latency/throughput/load/storage for all four models) and the "EfficientNet-B0 latency as % of Fashion-CLIP" + "Fashion-CLIP latency / EfficientNet-B0 latency" lines.

- [ ] **Step 2: Rewrite Table 68 rows in `07-model-comparison.typ`**

The table has columns: Model, mAP, P@10, R@10, Latency (ms), Throughput, Load (ms), Storage (MB). Use category-only JSON for mAP/P@10/R@10 and efficiency JSON for latency/throughput/load/storage. Keep Storage as-is if it already matches (3.3 / 3.3 / 8.1 / 13.0 — verify against oracle's index_storage_mb). Example (verify each cell against the oracle):

```typst
    [Fashion-CLIP], [*0.9309*], [*0.9493*], [*0.0483*], [96.8], [18.5], [5,255.4], [3.3],
    [CLIP-generic], [0.9115], [0.9364], [0.0459], [<from oracle>], [<from oracle>], [6,848.5], [3.3],
    [EfficientNet-B0], [0.8895], [0.9229], [0.0426], [<from oracle>], [<from oracle>], [110.2], [8.1],
    [ResNet-50], [0.8857], [0.9203], [0.0470], [61.9], [13.5], [374.1], [13.0],
```

> Note: the original Table 68 ordered models Fashion-CLIP, CLIP-generic, EfficientNet-B0, ResNet-50 (per line 16-19 of the current file). Preserve that order. Fill the `<from oracle>` cells from the script output before typing — never leave a placeholder in the thesis.

- [ ] **Step 3: Rewrite the deployment-recommendation paragraph (lines ~24-28) with recomputed figures**

Replace "92.8% of Fashion-CLIP's mAP at 26.0% of the latency" with the oracle's recomputed EfficientNet-B0-as-%-of-Fashion-CLIP-mAP (≈95.56%) and EfficientNet-B0-latency-as-%-of-Fashion-CLIP (recompute). Replace "3.8× latency increase" with the oracle's recomputed ratio. Re-check the recommendation's ranking still holds (ResNet-50 0.8857 vs EfficientNet-B0 0.8895 are now ~0.43% apart — the efficiency advantage still favors EfficientNet-B0, so the recommendation likely holds but the stated justification must change).

- [ ] **Step 4: Rewrite the RQ2 answer in `06-efficiency-metrics.typ:38`**

Replace "92.8% of Fashion-CLIP's mAP (0.8158 vs 0.8788) at 26.0% of the latency" with the recomputed values (95.56%, 0.8895 vs 0.9309, recomputed latency %). Replace "5.4% higher" with the recomputed ~2.13%. Replace "7.7% mAP improvement against a 3.8× latency increase" with recomputed figures.

- [ ] **Step 5: Fix the "6.1%" bullet in `07-model-comparison.typ:38`**

Change "Fashion-CLIP's 6.1% relative mAP improvement over generic CLIP" to the recomputed Fashion-CLIP vs CLIP-generic percentage (≈2.13%, from the oracle). This resolves TIER 1 #3's third value.

- [ ] **Step 6: Compile and verify**

Run: `typst compile main.typ` (from `thesis/`)
Expected: exits 0.

Run: `rg -n "6\.1%|92\.8%|26\.0%|3\.8" thesis/chapters/part2/ch3-evaluation/`
Expected: no matches in the recomputed locations (these stale figures are gone from §3.5-3.7).

- [ ] **Step 7: Commit**

```bash
git add thesis/chapters/part2/ch3-evaluation/06-efficiency-metrics.typ thesis/chapters/part2/ch3-evaluation/07-model-comparison.typ
git commit -m "fix(thesis): regenerate Table 68 and RQ2/§3.7.4 from authoritative efficiency JSON"
```

---

## Task 4: Update the abstract (EN + VI) with reconciled numbers

**Files:**
- Modify: `thesis/frontmatter/abstract.typ:13` (English abstract)
- Modify: `thesis/frontmatter/abstract.typ:32` (Vietnamese abstract)

**Interfaces:**
- Consumes: the reconciled Table 67/68 values from Tasks 2-3.
- Produces: an abstract whose mAP, SD, percentages, latency, and ×-factors match the authoritative JSON.

- [ ] **Step 1: Read the current abstract stale values**

Run: `rg -n "0\.8788|0\.8341|0\.8158|5\.4%|7\.7%|8\.2%|92\.0 ms|23\.9 ms|3\.8x|3,8 lần" thesis/frontmatter/abstract.typ`
Expected: matches on lines 13 and 32.

- [ ] **Step 2: Run the oracle and capture the replacement values**

Run: `python3 thesis/spec/verify_remediation.py`
Use: FashionCLIP mAP 0.9309 ± 0.0068; CLIP-generic 0.9115 ± 0.0077; EfficientNet-B0 0.8895 ± 0.0056; recomputed percentages; FashionCLIP latency 96.8 ms; EfficientNet-B0 latency (from oracle); recomputed ×-factor.

- [ ] **Step 3: Rewrite the English abstract sentence (line 13)**

Replace: "achieved the highest mean Average Precision at 0.8788 (SD 0.0022), outperforming the generic CLIP (mAP 0.8341, SD 0.0043) by 5.4%, EfficientNet-B0 (mAP 0.8158, SD 0.0007) by 7.7%, and ResNet-50 (mAP 0.8120, SD 0.0052) by 8.2%. At the opposite end of the speed spectrum, EfficientNet-B0 delivered inference at 23.9 ms per image (SD 2.5) while maintaining competitive retrieval quality, representing a 3.8x speed advantage over Fashion-CLIP (92.0 ms, SD 5.8) for a 7.7% accuracy trade-off."

With: the authoritative values from the oracle — Fashion-CLIP mAP 0.9309 (SD 0.0068), CLIP-generic 0.9115 (SD 0.0077) by ~2.13%, EfficientNet-B0 0.8895 (SD 0.0056) by ~4.65%, ResNet-50 0.8857 (SD 0.0114) by ~5.10%; EfficientNet-B0 latency (from oracle) ms vs Fashion-CLIP 96.8 ms, ×-factor (recomputed), accuracy trade-off ~4.65%.

- [ ] **Step 4: Rewrite the Vietnamese abstract sentence (line 32) with the same values**

Mirror the English changes, using Vietnamese decimal comma convention (0,9309; 0,0077) and the same recomputed percentages and latency figures.

- [ ] **Step 5: Verify abstract compliance (200-350 words, 3-5 keywords) and compile**

Run: `typst compile main.typ` (from `thesis/`)
Expected: exits 0.

Manually verify the English abstract word count is still 200-350 and keywords are 3-5 (compliance.json). The number changes should not materially change the word count.

Run: `rg -n "0\.8788|0,8788|5\.4%|7\.7%|8\.2%|92\.0 ms|23\.9 ms" thesis/frontmatter/abstract.typ`
Expected: no matches.

- [ ] **Step 6: Commit**

```bash
git add thesis/frontmatter/abstract.typ
git commit -m "fix(thesis): update EN/VI abstract with reconciled benchmark numbers"
```

---

## Task 5: Update Part 3 conclusion with reconciled numbers

**Files:**
- Modify: `thesis/chapters/part3/ch4-conclusion.typ:11` (Summary of Work, RQ answers)
- Modify: `thesis/chapters/part3/ch4-conclusion.typ:15` (trade-off paragraph)
- Modify: `thesis/chapters/part3/ch4-conclusion.typ:74,77,80` (the three bullet items with stale 0.8788 / 5.4-8.2% / 92.8%/26.0%)

**Interfaces:**
- Consumes: reconciled values from Tasks 2-3.
- Produces: a conclusion whose RQ1/RQ2/RQ3 restatements and summary bullets match the authoritative JSON.

- [ ] **Step 1: Read current stale values**

Run: `rg -n "0\.8788|0\.8341|0\.8158|0\.8120|5\.4%|7\.7%|8\.2%|92\.8%|26\.0%|0\.9025|0\.8640" thesis/chapters/part3/ch4-conclusion.typ`
Expected: matches on lines 11, 15, 74, 77, 80.

- [ ] **Step 2: Run the oracle**

Run: `python3 thesis/spec/verify_remediation.py`

- [ ] **Step 3: Rewrite line 11 (RQ1 answer restatement)**

Replace "mAP 0.8788 vs CLIP-generic 0.8341 (+5.4%), EfficientNet-B0 0.8158 (+7.7%), ResNet-50 0.8120 (+8.2%)" with the authoritative mAP values (0.9309 / 0.9115 / 0.8895 / 0.8857) and recomputed percentages. Replace "P@5: 0.9304 vs 0.9025" and "P@20: 0.8982 vs 0.8640" with 0.9582 vs 0.9440 and 0.9374 vs 0.9239. Replace "±0.0022" with ±0.0068.

- [ ] **Step 4: Rewrite line 15 (trade-off paragraph)**

Replace "Fashion-CLIP (mAP 0.8788, 92.0 ms) ... EfficientNet-B0 (23.9 ms) achieves 92.8% of that accuracy at 26.0% of the latency ... +5.4% mAP at identical latency ... 3.8× latency increase" with the authoritative values and recomputed percentages/ratios from the oracle.

- [ ] **Step 5: Rewrite the three bullets at lines 74, 77, 80**

Each contains stale 0.8788 / 5.4-8.2% / 92.8%/26.0% figures. Replace each with the authoritative equivalents from the oracle.

- [ ] **Step 6: Compile and sweep**

Run: `typst compile main.typ` (from `thesis/`)
Expected: exits 0.

Run: `rg -n "0\.8788|0\.8341|0\.8158|0\.8120|92\.8%|26\.0%|5\.4--8\.2" thesis/chapters/part3/ch4-conclusion.typ`
Expected: no matches.

- [ ] **Step 7: Commit**

```bash
git add thesis/chapters/part3/ch4-conclusion.typ
git commit -m "fix(thesis): update Part 3 conclusion with reconciled benchmark numbers"
```

---

## Task 6: Expand Table 55 to list all eleven registered models (CON-001)

This task REJECTS the review/rewrite "six" recommendation. The registry has 11; Table 55 is stale.

**Files:**
- Modify: `thesis/chapters/part2/ch2-design/04-implementations/04-ml-sidecar/ml-sidecar.typ` (Table 55 — verify exact location with grep in Step 1)
- Read: `benchmarks/src/benchmark/models/__init__.py:44-56` (the authoritative 11-entry registry)

**Interfaces:**
- Consumes: the model registry source.
- Produces: a Table 55 listing all 11 models, so the "eleven supported by the framework" prose elsewhere is backed by evidence.

- [ ] **Step 1: Locate Table 55 and confirm it currently lists six models**

Run: `rg -n "fashion_clip|clip_vit_b16|efficientnet_b0|resnet50|dinov2|openclip" thesis/chapters/part2/ch2-design/`
Expected: a small set of matches in `04-ml-sidecar/ml-sidecar.typ` (and possibly `04-implementation/04-model-config.typ`).

- [ ] **Step 2: Confirm the authoritative 11-model registry**

Run: `rg -n "efficientnet-b0|convnext-tiny|dinov2-vits14|fashion-clip|clip-b32|clip-generic|clip-l14|clip-vit-b16|siglip|resnet-50|eva-clip" benchmarks/src/benchmark/models/__init__.py`
Expected: 11 matches in the `_register()` return dict (lines 44-56).

- [ ] **Step 3: Rewrite Table 55 to list all 11 models**

Group by architecture family. Use the registry keys as the slug column and the model's display name. Example structure (verify slugs against the registry):

```typst
// Table 55 — Model registry: eleven adapters across four architecture families
// CNN family
//   efficientnet-b0   EfficientNet-B0      1280-dim
//   resnet-50         ResNet-50            2048-dim
//   convnext-tiny     ConvNeXt-Tiny        768-dim
// ViT family (self-supervised)
//   dinov2-vits14     DINOv2 ViT-S/14      384-dim
// CLIP family (contrastive)
//   fashion-clip      Fashion-CLIP         512-dim
//   clip-vit-b16      CLIP ViT-B/16        512-dim
//   clip-b32          CLIP ViT-B/32        512-dim
//   clip-l14          CLIP ViT-L/14        768-dim
//   clip-generic      CLIP (generic)       512-dim
//   siglip            SigLIP               768-dim
//   eva-clip          EVA-CLIP             512-dim
```

Replace the existing six-row table with an eleven-row table in the same Typst table format the file already uses (preserve the column layout; add a "Family" column if it improves clarity, otherwise group rows with subheaders).

- [ ] **Step 4: Confirm the prose "eleven" occurrences are now consistent**

Run: `rg -n "eleven models|11 models|eleven supported|eleven embedding" thesis/chapters/`
Expected: the six known locations (Part 1 §V, Ch1 §1.3.4.1, Ch1 §1.6.3, Ch2 §2.1.3, Part 3 I, Part 3 II) still say "eleven" — do NOT change them. They are now backed by the expanded Table 55.

- [ ] **Step 5: Compile and verify**

Run: `typst compile main.typ` (from `thesis/`)
Expected: exits 0.

- [ ] **Step 6: Commit**

```bash
git add thesis/chapters/part2/ch2-design/04-implementations/04-ml-sidecar/ml-sidecar.typ
git commit -m "fix(thesis): expand Table 55 to all 11 registered models (CON-001: reject 'six')"
```

---

## Task 7: Fix the three "15 to 20%" occurrences in Chapter 1

**Files:**
- Modify: `thesis/chapters/part2/ch1-background/f4/04-model-selection.typ:42` (the "15 to 20 percent ... confirmed in Chapter 3" sentence)
- Modify: two more occurrences in §1.3.3.5 and §1.6.1 (locate with grep in Step 1)

**Interfaces:**
- Consumes: the recomputed Fashion-CLIP vs CLIP-generic percentage from the oracle (≈2.13%).
- Produces: Chapter 1 prose that states the authoritative figure and no longer claims "confirmed in Chapter 3" for a 15-20% number.

- [ ] **Step 1: Locate all three "15 to 20" occurrences**

Run: `rg -n "15 to 20|15-20 percent|15--20" thesis/chapters/part2/ch1-background/`
Expected: matches in `04-model-selection.typ` (§1.3.4.4 line 42) and two other files (§1.3.3.5 and §1.6.1).

- [ ] **Step 2: Run the oracle for the replacement value**

Run: `python3 thesis/spec/verify_remediation.py`
Use the "Fashion-CLIP vs CLIP-generic mAP" line (≈2.13%).

- [ ] **Step 3: Rewrite the §1.3.4.4 sentence (line 42)**

Replace: "Fashion-CLIP achieved the highest mAP among the evaluated models, with a 15 to 20 percent improvement over general CLIP on fashion-specific queries, confirmed through the systematic benchmark in Chapter 3 @chia2022fashionclip."

With: "Fashion-CLIP achieved the highest mAP among the evaluated models, outperforming general CLIP by ~2.13% under the category-only evaluation, as confirmed in Chapter 3 (§3.5) @chia2022fashionclip." (Use the oracle's exact figure, not the tilde-approximation.)

- [ ] **Step 4: Rewrite the §1.3.3.5 and §1.6.1 occurrences**

Apply the same replacement in both locations. Remove any "confirmed in Chapter 3" language that was attached to the 15-20% figure and reattach it to the corrected figure.

- [ ] **Step 5: Compile and sweep**

Run: `typst compile main.typ` (from `thesis/`)
Expected: exits 0.

Run: `rg -n "15 to 20|15-20 percent|15--20" thesis/chapters/`
Expected: no matches.

- [ ] **Step 6: Commit**

```bash
git add thesis/chapters/part2/ch1-background/
git commit -m "fix(thesis): standardize Fashion-CLIP improvement to authoritative ~2.13% in Ch1"
```

---

## Task 8: Fix the EfficientNet-B0 trade-off in §1.3.4.5

**Files:**
- Modify: `thesis/chapters/part2/ch1-background/f4/04-model-selection.typ:52` (the "3.4 percent lower mAP@10" sentence)

**Interfaces:**
- Consumes: oracle's "Fashion-CLIP vs EfficientNet-B0 mAP" (≈4.65%).
- Produces: Chapter 1 EfficientNet-B0 trade-off consistent with Chapter 3.

- [ ] **Step 1: Confirm the stale value**

Run: `rg -n "3\.4 percent lower mAP" thesis/chapters/part2/ch1-background/`
Expected: match at `04-model-selection.typ:52`.

- [ ] **Step 2: Run the oracle**

Run: `python3 thesis/spec/verify_remediation.py`
Use "Fashion-CLIP vs EfficientNet-B0 mAP" (≈4.65%).

- [ ] **Step 3: Rewrite line 52**

Replace "trading off 3.4 percent lower mAP@10 with no text-to-image capability" with "trading off ~4.65 percent lower mAP with no text-to-image capability" (use the oracle's exact figure).

- [ ] **Step 4: Compile and sweep**

Run: `typst compile main.typ` && `rg -n "3\.4 percent lower mAP" thesis/chapters/`
Expected: compile exits 0; grep no matches.

- [ ] **Step 5: Commit**

```bash
git add thesis/chapters/part2/ch1-background/f4/04-model-selection.typ
git commit -m "fix(thesis): correct EfficientNet-B0 mAP trade-off to authoritative ~4.65%"
```

---

## Task 9: Replace the two fabricated bibliography entries and fix DeepFashion

**Files:**
- Modify: `thesis/backmatter/bibliography.bib:49-54` (`chia2022fashionclip`)
- Modify: `thesis/backmatter/bibliography.bib:84-90` (`liu2016deepfashion` — add Shi Qiu)
- Modify: `thesis/backmatter/bibliography.bib:92-97` (`wu2019fashioniq`)

**Interfaces:**
- Consumes: corrected metadata from the master fix list TIER 1 #4 and TIER 2 #21 (verified against real papers).
- Produces: three bibliography entries with correct authors, titles, venues, years, pages.

- [ ] **Step 1: Read the three current entries**

Run: `rg -n "chia2022fashionclip|liu2016deepfashion|wu2019fashioniq" thesis/backmatter/bibliography.bib`
Expected: matches at lines 49, 84, 92.

- [ ] **Step 2: Replace `chia2022fashionclip` (lines 49-54)**

Replace the entire entry with:

```bibtex
@article{chia2022fashionclip,
  author    = {Chia, P. J. and Attanasio, G. and Bianchi, F. and Terragni, S. and Magalh{\~a}es, A. R. and Goncalves, D. and Greco, C. and Tagliabue, J.},
  title     = {Contrastive Language and Vision Learning of General Fashion Concepts},
  journal   = {Scientific Reports},
  year      = {2022},
  volume    = {12},
  pages     = {18958},
  doi       = {10.1038/s41598-022-23052-9},
}
```

- [ ] **Step 3: Replace `wu2019fashioniq` (lines 92-97)**

Replace the entire entry with:

```bibtex
@inproceedings{wu2019fashioniq,
  author    = {Wu, Hui and Gao, Yupeng and Guo, Xiaoxiao and Al-Halah, Ziad and Rennie, Steven and Grauman, Kristen and Feris, Rogerio},
  title     = {{Fashion IQ}: A New Dataset Towards Retrieving Images by Natural Language Feedback},
  booktitle = {Proceedings of the IEEE/CVF Conference on Computer Vision and Pattern Recognition (CVPR)},
  year      = {2021},
  pages     = {11307--11317},
}
```

- [ ] **Step 4: Fix `liu2016deepfashion` — add Shi Qiu (line 85)**

Change the author line from:
```bibtex
  author    = {Liu, Ziwei and Luo, Ping and Wang, Xiaogang and Tang, Xiaoou},
```
to:
```bibtex
  author    = {Liu, Ziwei and Luo, Ping and Qiu, Shi and Wang, Xiaogang and Tang, Xiaoou},
```

- [ ] **Step 5: Compile (this also verifies the citations resolve) and sweep**

Run: `typst compile main.typ` (from `thesis/`)
Expected: exits 0 — an unresolved `@key` would fail the build, confirming the three keys still resolve.

Run: `rg -n "Gieysztor|Al-Zahir|SIGIR" thesis/backmatter/bibliography.bib`
Expected: no matches.

Run: `rg -n "Qiu, Shi" thesis/backmatter/bibliography.bib`
Expected: one match (in `liu2016deepfashion`).

- [ ] **Step 6: Commit**

```bash
git add thesis/backmatter/bibliography.bib
git commit -m "fix(thesis): correct fabricated citations [6],[27] and add DeepFashion co-author"
```

---

## Task 10: Fix "nine bounded contexts" → "eight" (TIER 1 #5)

**Files:**
- Modify: `thesis/chapters/part2/ch2-design/03-architecture/01-system-overview.typ` (locate the "nine bounded contexts" sentence with grep)

**Interfaces:**
- Consumes: Table 47 (8 rows) in the same file.
- Produces: §2.3.1 internally consistent (eight, eight, eight).

- [ ] **Step 1: Locate the stray "nine"**

Run: `rg -n "nine bounded contexts" thesis/chapters/part2/ch2-design/03-architecture/`
Expected: one match in `01-system-overview.typ`.

- [ ] **Step 2: Edit "nine" → "eight"**

Change "partitioned into nine bounded contexts" to "partitioned into eight bounded contexts".

- [ ] **Step 3: Compile and sweep**

Run: `typst compile main.typ` && `rg -n "nine bounded contexts" thesis/chapters/`
Expected: compile exits 0; grep no matches.

- [ ] **Step 4: Commit**

```bash
git add thesis/chapters/part2/ch2-design/03-architecture/01-system-overview.typ
git commit -m "fix(thesis): correct 'nine bounded contexts' to 'eight' in §2.3.1"
```

---

## Task 11: Fix "88 functional requirements across nine business modules" (TIER 1 #6)

**Files:**
- Modify: `thesis/chapters/part2/ch2-design/01-requirements/01-functional-requirements.typ` (locate the opening "88 ... nine business modules" sentence)
- Modify: `thesis/chapters/part2/ch2-design/01-requirements/requirements.typ` (if the opening paragraph lives in the aggregator — grep will confirm)

**Interfaces:**
- Consumes: Tables 10-17 (sum = 87 across 8 modules).
- Produces: §2.1 opening consistent with the actual requirement tables.

- [ ] **Step 1: Locate the stale sentence**

Run: `rg -n "88 functional|nine business modules" thesis/chapters/part2/ch2-design/01-requirements/`
Expected: one match in the opening paragraph.

- [ ] **Step 2: Edit the sentence**

Replace "88 functional requirements across nine business modules: Catalog, Identity, Inventory, Ordering, Payment, Shipping, Profile, Location, and Dashboard" with "87 functional requirements across eight business modules: Catalog, Identity, Inventory, Ordering, Payment, Shipping, Profile, and Location". (Drop Dashboard from this list — it has no FR table; it remains a real feature per Table 50.)

- [ ] **Step 3: Compile and sweep**

Run: `typst compile main.typ` && `rg -n "88 functional|nine business modules" thesis/chapters/`
Expected: compile exits 0; grep no matches.

- [ ] **Step 4: Commit**

```bash
git add thesis/chapters/part2/ch2-design/01-requirements/
git commit -m "fix(thesis): correct FR count to 87 across 8 modules in §2.1 opening"
```

---

## Task 12: Fix "near-zero P@20" in Part 3 Limitations (TIER 1 #7)

**Files:**
- Modify: `thesis/chapters/part3/ch4-conclusion.typ` (locate the "near-zero P@20" sentence in §III Limitations)
- Read: `thesis/backmatter/appendices/a-benchmark-results.typ` (Appendix A.2/A.3 P@20 values)

**Interfaces:**
- Consumes: Appendix A.2 and A.3 P@20 (Fashion-CLIP 0.3510 and 0.2997; all four models ~0.28-0.35).
- Produces: an accurate Limitations sentence.

- [ ] **Step 1: Locate the stale sentence**

Run: `rg -n "near-zero P@20|near-zero P\\@20" thesis/chapters/part3/`
Expected: one match in `ch4-conclusion.typ`.

- [ ] **Step 2: Confirm the Appendix A.2/A.3 P@20 values**

Run: `rg -n "0\.3510|0\.2997|P@20" thesis/backmatter/appendices/a-benchmark-results.typ`
Expected: matches confirming the ~0.30 range (not near-zero).

- [ ] **Step 3: Rewrite the sentence**

Replace "The enriched-label evaluation produces near-zero P@20 values due to the finer-grained relevance criterion." with "The enriched-label evaluation reduces P@20 substantially (from ~0.90 under category-only labels to ~0.30 under category+colour+pattern labels) due to the finer-grained relevance criterion."

- [ ] **Step 4: Compile and sweep**

Run: `typst compile main.typ` && `rg -n "near-zero" thesis/chapters/`
Expected: compile exits 0; grep no matches.

- [ ] **Step 5: Commit**

```bash
git add thesis/chapters/part3/ch4-conclusion.typ
git commit -m "fix(thesis): replace 'near-zero P@20' claim with accurate ~0.30 figure"
```

---

## Task 13: Reconcile the CBIR search endpoint URL (TIER 1 #8) — verify against code

This task requires checking the actual Carter route definitions before editing prose.

**Files:**
- Modify: `thesis/chapters/part2/ch2-design/04-implementations/04-ml-sidecar/ml-sidecar.typ` (§2.4.4.3 URL)
- Modify: `thesis/chapters/part2/ch2-design/04-implementations/05-frontend-ux/f1-visual-search.typ` (§2.4.5.1 URL)
- Modify: `thesis/chapters/part2/ch2-design/03-architecture/05-api-design.typ` (§2.3.5.2 stated convention)
- Read: `service/Api/src/Module/Features/Storefront/` (Catalog search-by-image route)

**Interfaces:**
- Consumes: the actual Carter route definition for image search.
- Produces: three prose locations and the stated convention all agreeing with the implemented route.

- [ ] **Step 1: Find the actual search-by-image route in the codebase**

Run: `rg -n "search-by-image|SearchByImage|search_by_image" service/Api/src/Module/Features/`
Expected: the route definition, revealing the real path pattern (e.g., `/api/catalog/storefront/search-by-image` or `/api/storefront/catalog/search-by-image`).

- [ ] **Step 2: Confirm the actual convention by checking several other storefront routes**

Run: `rg -n "Map.*\"/api/" service/Api/src/Module/Features/Storefront/ | head -20`
Expected: a consistent pattern revealing whether `{module}` or `{surface}` comes first.

- [ ] **Step 3: Update the stated convention in §2.3.5.2 (`05-api-design.typ`)**

Make the convention match the actual code. If the code puts surface first (`/api/storefront/{module}/{resource}`), change the convention to say so. If module first, keep `/api/{module}/{surface}/{resource}`.

- [ ] **Step 4: Update the §2.4.4.3 URL in `ml-sidecar.typ`**

Make it match the actual route exactly (correct segment count and order; remove the impossible "admin/catalog/storefront" four-segment form).

- [ ] **Step 5: Update the §2.4.5.1 URL in `f1-visual-search.typ`**

Make it match the actual route (add the missing module segment if needed).

- [ ] **Step 6: Compile and verify all three locations agree**

Run: `typst compile main.typ` (from `thesis/`)
Expected: exits 0.

Run: `rg -n "search-by-image" thesis/chapters/`
Expected: all occurrences now show the same path, matching the code.

- [ ] **Step 7: Commit**

```bash
git add thesis/chapters/part2/ch2-design/03-architecture/05-api-design.typ thesis/chapters/part2/ch2-design/04-implementations/04-ml-sidecar/ml-sidecar.typ thesis/chapters/part2/ch2-design/04-implementations/05-frontend-ux/f1-visual-search.typ
git commit -m "fix(thesis): reconcile CBIR endpoint URL across §2.3.5.2/§2.4.4.3/§2.4.5.1 with code"
```

---

## Task 14: Resolve "Variable Vector Dimensions" vs `vector(512)` (TIER 1 #9) — verify against migrations

**Files:**
- Modify: `thesis/chapters/part2/ch2-design/03-architecture/04-database-design.typ` (§2.3.4.4 "Variable Vector Dimensions" bullet) AND/OR §2.3.4.3, §2.4.3.2, Appendix D Table 82, Appendix D.9
- Read: `service/Api/src/Migrations/` and the `IEntityTypeConfiguration<ImageEmbedding>` in `service/Api/src/Module/`

**Interfaces:**
- Consumes: the actual EF Core migration and entity configuration for the embedding column.
- Produces: either the "Variable Vector Dimensions" bullet removed/qualified, or the schema docs corrected to show the real per-model dimension mechanism.

- [ ] **Step 1: Find the actual embedding column type in the migrations**

Run: `rg -n "vector\(512\)|ImageEmbedding|Embedding" service/Api/src/Migrations/ service/Api/src/Module/ | head -20`
Expected: the migration line declaring `vector(512)` (or a different dimension) and the entity config.

- [ ] **Step 2: Decide the fix based on what the code shows**

- If the code has only a fixed `vector(512)` column and no per-model dimension mechanism: remove or qualify the §2.3.4.4 "Variable Vector Dimensions" bullet (reframe as a future capability).
- If the code has a real per-model mechanism (separate tables/columns per model): correct §2.3.4.3, §2.4.3.2, Appendix D Table 82, and Appendix D.9 to document it.

- [ ] **Step 3: Apply the chosen edit**

Edit `04-database-design.typ` §2.3.4.4 (and the other three locations if the second branch applies). Do not leave the contradiction in place.

- [ ] **Step 4: Compile and sweep**

Run: `typst compile main.typ` && `rg -n "Variable Vector Dimensions" thesis/chapters/ thesis/backmatter/`
Expected: compile exits 0; either no matches (bullet removed) or one match with qualifying language (bullet reframed).

- [ ] **Step 5: Commit**

```bash
git add thesis/chapters/part2/ch2-design/03-architecture/04-database-design.typ
git commit -m "fix(thesis): resolve Variable Vector Dimensions vs fixed vector(512) contradiction"
```

---

## Task 15: Fix the two phantom "Section 2.1.5" references (TIER 1 #10)

**Files:**
- Modify: `thesis/chapters/part2/ch2-design/03-architecture/04-database-design.typ` (§2.3.4.3 reference)
- Modify: the §2.4.3.2 reference (locate with grep)

**Interfaces:**
- Consumes: the real section containing the HNSW/IVFFlat comparison (likely §1.4.3-1.4.4 or §3.4.3).
- Produces: both cross-references pointing to a section that exists.

- [ ] **Step 1: Locate both phantom references**

Run: `rg -n "Section 2\.1\.5|Section 2\\.1\\.5" thesis/chapters/`
Expected: two matches.

- [ ] **Step 2: Find the real HNSW/IVFFlat comparison section**

Run: `rg -n "HNSW|IVFFlat" thesis/chapters/part2/ch1-background/ thesis/chapters/part2/ch3-evaluation/`
Expected: the section that actually contains the index-detail / ANN-algorithm comparison.

- [ ] **Step 3: Update both references**

Change "Section 2.1.5" to the correct section number (e.g., "Section 1.4.3" or "Section 3.4.3") in both locations.

- [ ] **Step 4: Compile and sweep**

Run: `typst compile main.typ` && `rg -n "Section 2\.1\.5" thesis/chapters/`
Expected: compile exits 0; grep no matches.

- [ ] **Step 5: Commit**

```bash
git add thesis/chapters/part2/ch2-design/
git commit -m "fix(thesis): correct phantom 'Section 2.1.5' references to real sections"
```

---

## Task 16: Resolve "four-state" vs "five states" UI contradiction (TIER 1 #11)

**Files:**
- Modify: `thesis/chapters/part2/ch2-design/04-implementations/05-frontend-ux/f1-visual-search.typ` (§2.4.5.2.1)

**Interfaces:**
- Consumes: Table 58 (4 rows: Empty, Upload, Loading, Results) in the same file.
- Produces: the closing sentence count matching the table.

- [ ] **Step 1: Locate the contradiction**

Run: `rg -n "four-state UI model|five visual search states|The five visual" thesis/chapters/part2/ch2-design/`
Expected: matches in `f1-visual-search.typ`.

- [ ] **Step 2: Decide the fix**

Preferred (lower risk): change "The five visual search states are illustrated below" to "The four visual search states are illustrated below" to match Table 58's four rows.

Alternative (only if an Error state is genuinely part of UC-STR-SRC's flow and worth documenting): add a fifth Error row to Table 58 and keep "five". The four-row option is the default.

- [ ] **Step 3: Apply the four-row fix**

Change "five" to "four" in the closing sentence.

- [ ] **Step 4: Compile and sweep**

Run: `typst compile main.typ` && `rg -n "five visual search states|The five visual" thesis/chapters/`
Expected: compile exits 0; grep no matches.

- [ ] **Step 5: Commit**

```bash
git add thesis/chapters/part2/ch2-design/04-implementations/05-frontend-ux/f1-visual-search.typ
git commit -m "fix(thesis): align visual-search UI state count (four) with Table 58"
```

---

## Task 17: Rewrite Part 1 §VI Thesis Outline (TIER 1 #12)

**Files:**
- Modify: `thesis/chapters/part1/ch1-introduction.typ` (§VI Thesis Outline — locate with grep)

**Interfaces:**
- Consumes: the real Table of Contents structure (Part 1, Part 2 Ch 1-3, Part 3).
- Produces: an outline that matches the real TOC (no "Chapter 4", no duplicate "Chapter 1").

- [ ] **Step 1: Locate the outline section**

Run: `rg -n "Thesis Outline|organized into five chapters|Chapter 4 synthesizes" thesis/chapters/part1/`
Expected: match in `ch1-introduction.typ`.

- [ ] **Step 2: Replace the outline paragraph**

Replace the existing outline with:

```typst
This thesis is organized into three parts.

Part 1: Introduction (this part) establishes the research context, problem statement, objectives, research questions, scope, methodology, and this outline.

Part 2: Thesis Content contains three chapters:
- Chapter 1: Background and Related Work. Surveys vector embeddings, neural architectures, vector databases, prior work in fashion image retrieval, and the technology stack.
- Chapter 2: Design and Implementation. Functional and non-functional requirements, system architecture (DDD, C4, database, API, security), and concrete implementation (.NET backend, Python ML sidecar, Vue storefront).
- Chapter 3: Testing and Evaluation. Systematic benchmark comparing retrieval accuracy and inference efficiency across embedding models using cross-validation on 5,000 fashion images.

Part 3: Conclusion and Future Work synthesizes findings, evaluates contributions and limitations, and proposes future work.
```

- [ ] **Step 3: Compile and sweep**

Run: `typst compile main.typ` && `rg -n "Chapter 4 synthesizes|organized into five chapters" thesis/chapters/`
Expected: compile exits 0; grep no matches.

- [ ] **Step 4: Commit**

```bash
git add thesis/chapters/part1/ch1-introduction.typ
git commit -m "fix(thesis): rewrite §VI outline to match real TOC structure"
```

---

## Task 18: Reconcile PostgreSQL and pgvector versions (TIER 2 #13, #14)

**Files:**
- Modify: `thesis/chapters/part2/ch3-evaluation/04-benchmark-protocol.typ` or the file containing Table 66 (locate with grep — says PostgreSQL 16)
- Modify: `thesis/chapters/part2/ch2-design/04-implementations/01-technology-stack/technology-stack.typ` (Table 51 — says pgvector 0.3.2)
- Read: `Directory.Packages.props` and the container tag reference

**Interfaces:**
- Consumes: the container tag `pgvector/pgvector:pg17-trixie` (PostgreSQL 17) and the pgvector version in `Directory.Packages.props` or Chapter 3's 0.7.0.
- Produces: Table 66 and Table 51 agreeing with the rest of the thesis (PostgreSQL 17, pgvector 0.7.0).

- [ ] **Step 1: Locate Table 66 (PostgreSQL 16) and Table 51 (pgvector 0.3.2)**

Run: `rg -n "PostgreSQL 16|pgvector 0\.3\.2" thesis/chapters/`
Expected: one match each.

- [ ] **Step 2: Confirm the authoritative versions**

Run: `rg -n "pgvector" Directory.Packages.props` and `rg -n "pg17|pgvector:pg" infra/ service/ benchmarks/`
Expected: the real pgvector version and the pg17 container tag (confirming PostgreSQL 17).

- [ ] **Step 3: Fix Table 66 — change "PostgreSQL 16" to "PostgreSQL 17"**

(Unless Step 2 reveals the benchmark genuinely used 16, in which case add a one-line note explaining why instead.)

- [ ] **Step 4: Fix Table 51 — change "pgvector 0.3.2" to the authoritative version**

Use the version from `Directory.Packages.props` (or 0.7.0 if that matches Chapter 3 and the package props).

- [ ] **Step 5: Compile and sweep**

Run: `typst compile main.typ` && `rg -n "PostgreSQL 16|pgvector 0\.3\.2" thesis/chapters/`
Expected: compile exits 0; grep no matches.

- [ ] **Step 6: Commit**

```bash
git add thesis/chapters/part2/ch3-evaluation/ thesis/chapters/part2/ch2-design/04-implementations/01-technology-stack/
git commit -m "fix(thesis): reconcile PostgreSQL 17 and pgvector version in Tables 66, 51"
```

---

## Task 19: Standardize the permission-string format (TIER 2 #15) — verify against code

**Files:**
- Modify: `thesis/chapters/part2/ch2-design/03-architecture/06-security-design.typ` (§2.3.6.2)
- Read: the permission definitions in `service/Api/src/Module/` (locate with grep)

**Interfaces:**
- Consumes: the actual permission string format in the code.
- Produces: §2.3.6.2 prose and examples matching the code and matching other mentions.

- [ ] **Step 1: Find the actual permission string format in the code**

Run: `rg -n "catalog\.products\.|domain\.category\.action|Permission.*=.*\"" service/Api/src/Module/ | head -15`
Expected: the real format (likely dot-separated `domain.resource.action`).

- [ ] **Step 2: Rewrite §2.3.6.2**

Change the stated template from `Domain.Category.Resource.Action` (4-part) to the actual 3-part format the code uses, and make the prose examples match. Also fix any colon-separated `domain:category:action` mentions elsewhere.

- [ ] **Step 3: Compile and sweep**

Run: `typst compile main.typ` && `rg -n "Domain\.Category\.Resource\.Action|domain:category:action" thesis/chapters/`
Expected: compile exits 0; grep no matches.

- [ ] **Step 4: Commit**

```bash
git add thesis/chapters/part2/ch2-design/03-architecture/06-security-design.typ
git commit -m "fix(thesis): standardize permission-string format to match code"
```

---

## Task 20: Fix "eight use cases" → "nine" (storefront, TIER 2 #16)

**Files:**
- Modify: `thesis/chapters/part2/ch2-design/04-implementations/05-frontend-ux/frontend-ux.typ` or the §2.4.5.2 opening (locate with grep)

**Interfaces:**
- Consumes: the nine storefront use case IDs (UC-STR-SRC, BRW, CRT, CHK, OHI, AUT, SES, PAY, PRF) in §2.4.5.2.1-2.4.5.2.8.
- Produces: the opening count matching the nine documented subsections.

- [ ] **Step 1: Locate the stale "eight"**

Run: `rg -n "eight use cases" thesis/chapters/part2/ch2-design/04-implementations/`
Expected: one match in the §2.4.5.2 opening.

- [ ] **Step 2: Edit "eight" → "nine"**

- [ ] **Step 3: Compile and sweep**

Run: `typst compile main.typ` && `rg -n "eight use cases" thesis/chapters/`
Expected: compile exits 0; grep no matches.

- [ ] **Step 4: Commit**

```bash
git add thesis/chapters/part2/ch2-design/04-implementations/05-frontend-ux/
git commit -m "fix(thesis): correct storefront use case count to nine in §2.4.5.2"
```

---

## Task 21: Standardize the "accuracy metrics" count (TIER 2 #17)

**Files:**
- Modify: `thesis/chapters/part2/ch3-evaluation/04-benchmark-protocol.typ` (§3.4.2 "five accuracy ... metrics")
- Modify: `thesis/chapters/part3/ch4-conclusion.typ` (the "seven accuracy" mentions, if they need alignment)
- Modify: the Table 70 row mentioning "seven accuracy"

**Interfaces:**
- Consumes: Table 65 (3 metric families: mAP, P@K, R@K).
- Produces: one consistent counting convention everywhere.

- [ ] **Step 1: Locate all three counts**

Run: `rg -n "five accuracy|seven accuracy|three accuracy" thesis/chapters/`
Expected: matches in §3.4.2 (five), Part 3 (seven ×2).

- [ ] **Step 2: Apply the convention**

Use: "three accuracy metric families (mAP, P@K, R@K), evaluated at three depths (K=5, 10, 20), for seven reported columns."

- Change §3.4.2's "five accuracy ... metrics" to "three accuracy metric families (mAP, P@K, R@K), evaluated at three depths (K=5, 10, 20), for seven reported columns".
- Align the Part 3 "seven accuracy" mentions to "seven reported accuracy columns (three metric families at three depths)" so the "seven" is explained.

- [ ] **Step 3: Compile and sweep**

Run: `typst compile main.typ` && `rg -n "five accuracy" thesis/chapters/`
Expected: compile exits 0; grep no matches.

- [ ] **Step 4: Commit**

```bash
git add thesis/chapters/part2/ch3-evaluation/04-benchmark-protocol.typ thesis/chapters/part3/ch4-conclusion.typ
git commit -m "fix(thesis): standardize accuracy-metrics count (3 families, 7 columns)"
```

---

## Task 22: Resolve the Pinterest "30% search abandonment" stat (TIER 2 #18)

**Files:**
- Modify: `thesis/chapters/part1/ch1-introduction.typ` (§I Context and Motivation)
- Modify: `thesis/chapters/part2/ch1-background/` (§1.1, if it repeats the 30% figure)
- Read: `thesis/backmatter/bibliography.bib` (`pinterest2023visual`)

**Interfaces:**
- Consumes: the `pinterest2023visual` reference (which supports search volume, not abandonment).
- Produces: either the 30% figure sourced separately, or the claim softened to what the Pinterest reference supports.

- [ ] **Step 1: Locate the 30% claim**

Run: `rg -n "30 percent|30%.*abandon|session abandonment" thesis/chapters/`
Expected: matches in Part 1 §I and Chapter 1 §1.1.

- [ ] **Step 2: Decide the fix**

Default (no new source available): soften the claim. Replace "Industry estimates place session abandonment after unsuccessful search at approximately 30 percent [2]." with "Shoppers who fail to find what they are looking for frequently abandon the session rather than reformulate the query [2]." (This drops the unsupported 30% figure but keeps the qualitative point the Pinterest reference can support — the shift toward visual search.)

Alternative (if a real Baymard-style source is found): add the new bibliography entry and cite it for the 30% figure instead of [2].

- [ ] **Step 3: Apply the softening edit in both locations**

- [ ] **Step 4: Compile and sweep**

Run: `typst compile main.typ` && `rg -n "30 percent.*abandon|approximately 30 percent" thesis/chapters/`
Expected: compile exits 0; grep no matches (unless the alternative branch with a new source was taken).

- [ ] **Step 5: Commit**

```bash
git add thesis/chapters/part1/ thesis/chapters/part2/ch1-background/
git commit -m "fix(thesis): soften unsupported 30% search-abandonment claim"
```

---

## Task 23: Audit Table 70 (Requirements Traceability) citations (TIER 2 #19)

**Files:**
- Modify: `thesis/chapters/part3/ch4-conclusion.typ` (Table 70)

**Interfaces:**
- Consumes: the real section locations for each traced requirement.
- Produces: all 11 rows' "Addressed In" citations pointing to the sections that actually contain the content.

- [ ] **Step 1: Locate Table 70**

Run: `rg -n "Requirements Traceability|Addressed In" thesis/chapters/part3/`
Expected: Table 70 in `ch4-conclusion.typ`.

- [ ] **Step 2: Fix the two confirmed mis-citations**

- "Validate pgvector feasibility" and "Set up vector search": change "Section 2.2.4" to "Section 2.3.4" (or "Section 2.4.4" — verify which section actually documents pgvector setup by grepping).
- RQ3 "Addressed In: ... Section 3.5": change to "Section 3.7.4".

- [ ] **Step 3: Audit the remaining nine rows**

For each row, follow its "Addressed In" citation and confirm the section contains the referenced content. Fix any that are wrong. Pay attention to RQ1 (§3.5) and RQ2 (§3.6) citations — verify each.

Run: `rg -n "HNSW|pgvector|vector search|CBIR" thesis/chapters/part2/ch2-design/03-architecture/04-database-design.typ thesis/chapters/part2/ch2-design/04-implementations/04-ml-sidecar/`
to confirm which sections actually contain the pgvector/vector-search content.

- [ ] **Step 4: Compile and verify**

Run: `typst compile main.typ` (from `thesis/`)
Expected: exits 0.

- [ ] **Step 5: Commit**

```bash
git add thesis/chapters/part3/ch4-conclusion.typ
git commit -m "fix(thesis): correct Table 70 traceability section citations"
```

---

## Task 24: Resolve "sequential selection preserves distribution" (TIER 2 #22) — verify against dataset prep

**Files:**
- Modify: `thesis/backmatter/appendices/` (Appendix B.1 — locate with grep)
- Read: the dataset preparation code in `benchmarks/` (the split generation script)

**Interfaces:**
- Consumes: whether the source Kaggle dataset is pre-shuffled (check the split script).
- Produces: Appendix B.1 either stating the source is pre-shuffled, or describing the actual sampling method.

- [ ] **Step 1: Locate the Appendix B.1 sentence**

Run: `rg -n "sequentially|preserve the natural category distribution|natural category distribution" thesis/backmatter/`
Expected: one match in Appendix B.1.

- [ ] **Step 2: Check the benchmark split script for shuffling**

Run: `rg -n "shuffle|random_state|stratified|train_test_split|seed" benchmarks/src/benchmark/ benchmarks/scripts/ | head -20`
Expected: reveals whether the split script shuffles (making sequential selection valid) or uses stratified sampling.

- [ ] **Step 3: Apply the matching fix**

- If the script shuffles with a seed: keep "sequentially" and add "the source dataset is pre-shuffled by the split script with a fixed seed (see §3.4.1), so sequential selection preserves the category distribution."
- If the script uses stratified sampling: replace "sequentially ... to preserve the natural category distribution" with "via stratified random sampling to preserve the natural category distribution."

- [ ] **Step 4: Compile and verify**

Run: `typst compile main.typ` (from `thesis/`)
Expected: exits 0.

- [ ] **Step 5: Commit**

```bash
git add thesis/backmatter/appendices/
git commit -m "fix(thesis): justify sequential/stratified sample selection in Appendix B.1"
```

---

## Task 25: TIER 3 polish items

Six low-risk items. Do them in one task since each is a one-line edit.

**Files:**
- Modify: `thesis/chapters/part2/ch2-design/04-implementations/05-frontend-ux/frontend-ux.typ` (§2.4.5 "Section 2.2.2" → "Section 2.2")
- Modify: `thesis/chapters/part2/ch2-design/02-use-cases/01-system-actors.typ` (§2.2.1 — add Support-actor clarifying sentence)
- Modify: `thesis/chapters/part1/ch1-introduction.typ` (optional trim of Objectives/Outline redundancy — only if length is a concern)
- Modify: `thesis/chapters/part2/ch3-evaluation/03-testing-result.typ` (§3.3 — optionally mention one real bug-and-fix)
- Modify: `thesis/backmatter/appendices/a-benchmark-results.typ` (Appendix A.2 Table 72 caption "Chapter 6" → "Chapter 3")
- Modify: `thesis/backmatter/appendices/` (Appendix B.3 "Chapter 6" → "Chapter 3")

**Interfaces:**
- Consumes: nothing new.
- Produces: six polish fixes.

- [ ] **Step 1: Fix the §2.4.5 cross-reference**

Run: `rg -n "Section 2\.2\.2" thesis/chapters/part2/ch2-design/04-implementations/05-frontend-ux/`
Change "Section 2.2.2" to "Section 2.2".

- [ ] **Step 2: Add the Support-actor clarification in §2.2.1**

In `01-system-actors.typ`, add after the three-actor statement: "Individual use cases may additionally reference supporting external systems (ML Service, Payment Gateway, Email Service, OAuth Provider) under a Support field, a standard UML convention distinct from the three primary actors."

- [ ] **Step 3 (optional): trim Objectives/Outline redundancy**

Only if the abstract or Part 1 is over-length. Skip if not.

- [ ] **Step 4 (optional): mention one real bug-and-fix in §3.3**

Only if a real bug was encountered during development and is remembered. Skip if not — do not invent one.

- [ ] **Step 5: Fix both "Chapter 6" references**

Run: `rg -n "Chapter 6" thesis/backmatter/`
Expected: matches in Appendix A.2 Table 72 caption and Appendix B.3.
Change each "Chapter 6" to "Chapter 3".

- [ ] **Step 6: Compile and sweep**

Run: `typst compile main.typ` (from `thesis/`)
Expected: exits 0.

Run: `rg -n "Chapter 6|Section 2\.2\.2" thesis/chapters/ thesis/backmatter/`
Expected: no matches.

- [ ] **Step 7: Commit**

```bash
git add thesis/chapters/ thesis/backmatter/
git commit -m "polish(thesis): TIER 3 fixes (cross-refs, support actors, phantom Chapter 6)"
```

---

## Task 26: Final consistency sweep, compile, and remediation log sign-off

This is the verification-gate task. No new content.

**Files:**
- Modify: `thesis/spec/remediation-log.md` (update every row's status)

**Interfaces:**
- Consumes: all prior tasks.
- Produces: a green build, a clean sweep, and a completed log.

- [ ] **Step 1: Run the full forbidden-string sweep**

Run: `rg -i "nine bounded contexts|88 functional|nine business modules|eight use cases|Section 2\.1\.5|Chapter 6|15 to 20|15-20 percent|6\.1%|near-zero P@20|Al-Zahir|Gieysztor|PostgreSQL 16|pgvector 0\.3\.2|0\.8788|0\.8341|0\.8158|0\.8120" thesis/chapters/ thesis/frontmatter/ thesis/backmatter/`
Expected: no matches. If any match, return to the relevant task and fix.

- [ ] **Step 2: Run the oracle and confirm recomputed percentages are reflected**

Run: `python3 thesis/spec/verify_remediation.py`
Then: `rg -n "2\.13%|4\.65%|5\.10%|95\.56%" thesis/chapters/ thesis/frontmatter/`
Expected: the recomputed figures appear in the thesis (exact oracle values, not necessarily these rounded ones).

- [ ] **Step 3: Compile the final PDF**

Run: `typst compile main.typ` (from `thesis/`)
Expected: exits 0, `main.pdf` produced, no missing-image or unresolved-citation errors.

- [ ] **Step 4: Verify compliance constraints**

Confirm: abstract is 200-350 words (count manually or with a script), 3-5 keywords present, bibliography has ≥15 entries (count: `rg -c "^@" thesis/backmatter/bibliography.bib`).

- [ ] **Step 5: Update the remediation log**

For every finding row in `thesis/spec/remediation-log.md`, set `status` to `fixed` (or `skipped` for TIER 3 items deliberately not done, with a reason in `notes`). Fill `authoritative_source`, `files_changed`, `compile_ok=true`. For T1-2, record the CON-001/CON-003 deviation in `notes`.

- [ ] **Step 6: Confirm the "eleven" model count is intact and Table 55 lists 11**

Run: `rg -n "eleven models|11 models|eleven supported" thesis/chapters/`
Expected: the six occurrences still say "eleven" (not changed to "six").

Run: confirm Table 55 has 11 data rows (visually in the source).

- [ ] **Step 7: Commit**

```bash
git add thesis/spec/remediation-log.md
git commit -m "chore(thesis): sign off remediation log after full sweep and compile"
```

---

## Self-Review

**1. Spec coverage check** (against `thesis/spec/spec-process-thesis-review-remediation.md`):

- REQ-001 (all 🔴🟠 resolved) → Task 26 Step 5.
- REQ-002 (record authoritative source) → every task's Step "Run the oracle" / "verify against code"; logged in Task 26.
- REQ-003 (root-cause fix + propagation) → Task 2 (root) + Tasks 3-5 (propagation).
- REQ-004 (compile after each fix) → every task's compile step.
- REQ-005 (remediation log) → Task 1 Step 3 + Task 26 Step 5.
- REQ-006 (benchmark reconciliation sequenced first) → Task 2 is first content task.
- CON-001 (reject "six") → Task 6.
- CON-002 (JSON tie-breaker) → Task 2 uses the oracle.
- CON-003 (rewrite files suspect) → noted in Global Constraints; Task 6 explicitly does not paste "six".
- CON-004 (BibTeX keys) → Task 9.
- REQ-101 → Task 2 (accuracy) + Task 3 (efficiency).
- REQ-102 → Task 6.
- REQ-103 → Task 7 + Task 3 Step 5 (the 6.1% fix).
- REQ-104 → Task 9.
- REQ-105 → Task 10.
- REQ-106 → Task 11.
- REQ-107 → Task 12.
- REQ-108 → Task 13.
- REQ-109 → Task 14.
- REQ-110 → Task 15.
- REQ-111 → Task 16.
- REQ-112 → Task 17.
- REQ-201 → Task 18 (PostgreSQL).
- REQ-202 → Task 18 (pgvector).
- REQ-203 → Task 19.
- REQ-204 → Task 20.
- REQ-205 → Task 21.
- REQ-206 → Task 22.
- REQ-207 → Task 23.
- REQ-208 → Task 8.
- REQ-209 → Task 9 (DeepFashion co-author).
- REQ-210 → Task 24.
- REQ-301 → Task 25 Step 1.
- REQ-302 → Task 25 Step 2.
- REQ-303 → Task 25 Step 3.
- REQ-304 → Task 25 Step 4.
- REQ-305 → Task 25 Step 5.
- REQ-306 → optional, noted in Task 11 alternative.

All 34 requirements + 4 constraints covered.

**2. Placeholder scan:** Searched the plan for "TBD", "TODO", "implement later", "add appropriate", "similar to Task N". The only intentional `<from oracle>` markers are in Task 3 Step 2's example table — these are explicitly instructed to be filled from the script output before typing, with a note never to leave a placeholder in the thesis. No other placeholders.

**3. Type consistency:** The oracle script (Task 1) defines the authoritative value names; Tasks 2-5 reference the same JSON paths and the same recomputed percentages. The `verify_remediation.py` filename is consistent across all tasks. BibTeX keys (`chia2022fashionclip`, `wu2019fashioniq`, `liu2016deepfashion`) are consistent between Task 9 and the sweep in Task 26. File paths are consistent across tasks.

---

## Execution Handoff

Plan complete and saved to `thesis/docs/plans/2026-08-19-thesis-review-remediation.md`. Two execution options:

**1. Subagent-Driven (recommended)** — I dispatch a fresh subagent per task, review between tasks, fast iteration.

**2. Inline Execution** — Execute tasks in this session using executing-plans, batch execution with checkpoints.

Which approach?
