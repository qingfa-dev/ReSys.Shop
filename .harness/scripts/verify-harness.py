#!/usr/bin/env python3
"""Verify harness coherence against the actual repository.

Run from repo root:
    python .harness/scripts/verify-harness.py

Exit code 0 if everything is coherent; non-zero with agent-legible remediation
instructions otherwise.
"""

from __future__ import annotations

import os
import re
import sys
from pathlib import Path

REPO_ROOT = Path(__file__).resolve().parents[2]


def error(title: str, what: str, why: str, fix: str, where: str) -> None:
    print(f"\n[FAIL] {title}")
    print(f"  What's wrong: {what}")
    print(f"  Why it matters: {why}")
    print(f"  How to fix: {fix}")
    print(f"  Where to look: {where}")


def warn(title: str, what: str, fix: str, where: str) -> None:
    print(f"\n[WARN] {title}")
    print(f"  What: {what}")
    print(f"  Suggested fix: {fix}")
    print(f"  Where to look: {where}")


def extract_domains(domains_yml: Path) -> list[dict[str, str | list[str]]]:
    """Extract domain blocks from the 'domains:' section of the YAML."""
    lines = domains_yml.read_text(encoding="utf-8").splitlines()

    # Find the top-level 'domains:' key.
    start_idx = None
    for i, line in enumerate(lines):
        if line.strip() == "domains:":
            start_idx = i
            break
    if start_idx is None:
        return []

    domains: list[dict[str, str | list[str]]] = []
    current: dict[str, str | list[str]] | None = None
    in_layers = False

    for line in lines[start_idx + 1:]:
        # Stop at the next top-level key (no leading whitespace).
        if line and not line.startswith((" ", "\t")) and not line.startswith("-"):
            if current is not None:
                domains.append(current)
            break

        # New domain block starts at indent 2 with '- name:'.
        if re.match(r"^  - name:\s*(.+)$", line):
            if current is not None:
                domains.append(current)
            current = {"name": line.split(":", 1)[1].strip(), "path": "", "layers": []}
            in_layers = False
            continue

        if current is None:
            continue

        # Path at indent 4.
        path_match = re.match(r"^    path:\s*(\S.*)$", line)
        if path_match:
            current["path"] = path_match.group(1).strip()
            in_layers = False
            continue

        # Layers section at indent 4.
        if re.match(r"^    layers:\s*$", line):
            in_layers = True
            continue

        # Layer key-value pairs at indent 6 while inside layers.
        if in_layers:
            layer_match = re.match(r"^      (\w+):\s*(\S.*)$", line)
            if layer_match:
                value = layer_match.group(2).strip()
                if value not in ("null", "", "~"):
                    current["layers"].append(value)
                continue
            # Anything else at indent 4 or less exits the layers block.
            if re.match(r"^    \S", line):
                in_layers = False

    if current is not None:
        domains.append(current)
    return domains


def check_domain_paths(domains: list[dict[str, str | list[str]]]) -> int:
    failures = 0
    for domain in domains:
        domain_path = REPO_ROOT / str(domain["path"])
        if not domain_path.exists():
            failures += 1
            error(
                title=f"Domain '{domain['name']}' path missing",
                what=f"domains.yml declares path '{domain['path']}' but it does not exist on disk.",
                why="Agents use .harness/domains.yml to locate code. A missing path means the map is stale.",
                fix="Update the path in domains.yml, restore the directory, or remove the domain if it was deleted.",
                where=".harness/domains.yml",
            )
            continue
        for layer_path in domain["layers"]:
            if layer_path in ("null", "", "~"):
                continue
            full = REPO_ROOT / layer_path
            if not full.exists():
                failures += 1
                error(
                    title=f"Domain '{domain['name']}' layer path missing",
                    what=f"Layer path '{layer_path}' for domain '{domain['name']}' does not exist.",
                    why="Layer paths tell agents where each architectural layer lives for a domain.",
                    fix="Update the layer path in domains.yml or create the expected directory.",
                    where=f".harness/domains.yml -> {domain['name']}",
                )
    return failures


def check_readme_module_count(domains: list[dict[str, str | list[str]]]) -> int:
    readme = REPO_ROOT / "README.md"
    text = readme.read_text(encoding="utf-8")
    business_modules = [d for d in domains if d["path"].startswith("service/Api/src/Module/")]
    expected = len(business_modules)

    matches = re.findall(r"(\d+)\s+business modules", text)
    failures = 0
    for count_str in matches:
        if int(count_str) != expected:
            failures += 1
            error(
                title="README module count drift",
                what=f"README.md says {count_str} business modules but domains.yml has {expected}.",
                why="Module count in the human-facing README must match the machine-readable domain spec or agents get conflicting context.",
                fix=f"Update every 'N business modules' mention in README.md to {expected}.",
                where="README.md",
            )
    return failures


def check_removed_webhooks() -> int:
    failures = 0
    webhooks_dir = REPO_ROOT / "service" / "Api" / "src" / "Module" / "Webhooks"
    if webhooks_dir.exists():
        failures += 1
        error(
            title="Removed Webhooks module resurrected",
            what=f"Directory '{webhooks_dir.relative_to(REPO_ROOT)}' exists but Webhooks was removed from the harness.",
            why="A resurrected module means the knowledge layer and build will drift again.",
            fix="Delete the directory and update .harness/ + docs if the module is intentionally being restored.",
            where="service/Api/src/Module/Webhooks",
        )

    # Source-code references (excluding migration Designer/Snapshot files, which are intentionally retained).
    src = REPO_ROOT / "service" / "Api" / "src"
    for pattern in ("Module.Webhooks", "Shared.Operational.Webhooks", "AddWebhooksModule", "AddWebhooks"):
        hits = list(src.rglob("*.cs"))
        for hit in hits:
            if "Migrations" in hit.parts and (hit.name.endswith(".Designer.cs") or hit.name == "ApplicationDbContextModelSnapshot.cs"):
                continue
            if pattern in hit.read_text(encoding="utf-8"):
                failures += 1
                error(
                    title=f"Stale Webhooks reference in source",
                    what=f"File '{hit.relative_to(REPO_ROOT)}' still references '{pattern}'.",
                    why="After module removal, no runtime source should depend on the deleted module.",
                    fix="Remove the reference or migrate it to the Ordering outbound-webhook job.",
                    where=str(hit.relative_to(REPO_ROOT)),
                )
                break
    return failures


def check_agents_md_links() -> int:
    agents = REPO_ROOT / "AGENTS.md"
    text = agents.read_text(encoding="utf-8")
    failures = 0
    for match in re.finditer(r"\[([^\]]+)\]\(([^)]+)\)", text):
        label, path = match.group(1), match.group(2)
        if path.startswith(("http://", "https://", "#")):
            continue
        target = REPO_ROOT / path
        if not target.exists():
            failures += 1
            error(
                title=f"AGENTS.md broken link: '{label}'",
                what=f"AGENTS.md links to '{path}' but that path does not exist.",
                why="AGENTS.md is the primary routing table. Broken links waste agent context and hide docs.",
                fix="Fix the path, create the missing file, or remove the obsolete link.",
                where="AGENTS.md",
            )
    return failures


def check_agents_md_length() -> int:
    agents = REPO_ROOT / "AGENTS.md"
    lines = agents.read_text(encoding="utf-8").splitlines()
    if len(lines) > 100:
        warn(
            title="AGENTS.md is getting long",
            what=f"AGENTS.md has {len(lines)} lines (recommended max ~100).",
            fix="Move detailed content to docs/codebase/ and keep AGENTS.md as a routing table.",
            where="AGENTS.md",
        )
    return 0


def main() -> int:
    print("ReSys.Shop harness verification")
    print("=" * 40)

    domains_yml = REPO_ROOT / ".harness" / "domains.yml"
    if not domains_yml.exists():
        error(
            title="domains.yml missing",
            what="Required harness file .harness/domains.yml was not found.",
            why="domains.yml is the source of truth for business domains and layer rules.",
            fix="Create .harness/domains.yml from the architecture spec.",
            where=".harness/",
        )
        return 1

    domains = extract_domains(domains_yml)
    if not domains:
        error(
            title="No domains parsed",
            what="Could not extract any domains from .harness/domains.yml.",
            why="The verification script needs a parseable list of domains.",
            fix="Verify domains.yml follows the expected schema (see references/yml-schemas.md).",
            where=".harness/domains.yml",
        )
        return 1

    failures = 0
    failures += check_domain_paths(domains)
    failures += check_readme_module_count(domains)
    failures += check_removed_webhooks()
    failures += check_agents_md_links()
    failures += check_agents_md_length()

    print("\n" + "=" * 40)
    if failures:
        print(f"Result: {failures} harness issue(s) found.")
        return 1
    print("Result: Harness is coherent.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
