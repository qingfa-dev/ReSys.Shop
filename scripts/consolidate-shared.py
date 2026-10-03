#!/usr/bin/env python3
"""Deterministic Shared-folder consolidation for ReSys.Shop (plan: refactor-shared-consolidation-1).

Consolidates every per-feature `Shared` folder under `Features/{Admin|Storefront}/` of the modules
listed in `consolidate_maps.py` into one `{Module}/Features/{Area}/Shared/{Mappings,Models,Validators}/`,
merging files that share an (Entity, Kind) into a single `{Entity}.{Kind}.cs`, rewriting namespaces
per the Appendix A map, and rewriting every referencing `.cs` file under service/Api.

CLI:
    python3 scripts/consolidate-shared.py --module <name> [--module <name>...] [--area {admin,storefront}] --dry-run|--apply

Behavior (per module+area):
  1. For each target, read every listed source; a 0-byte source is deleted and skipped; split each
     non-empty source into `using` lines, the file-scoped `namespace X;` line, and body lines; rewrite
     every `using`/fully-qualified reference through the namespace map; compose the target per PAT-001;
     write the target and delete its sources (never delete when source == target).
  2. Rewrite every `.cs` file under service/Api/src/Module, service/Api/src/Api, service/Api/src/Migrations,
     and service/Api/tests by applying the namespace map of the modules passed via --module to
     `using <old>;` and to `<old>.` occurrences (descending old-namespace length), then remove duplicate
     `using` directive lines (CS0105 guard). `.cs` files under bin/, obj/, and any path containing
     `/.superpowers/` are skipped.
  3. Delete now-empty kind directories and orphaned `Docs/.gitkeep` under the old `Shared` folders;
     never delete a directory that still contains `Services/`, `Clients/`, or a root-level `.cs`.
  4. Fail loudly (exit non-zero) if two different sources for one target declare the same type name
     (identical `partial` declarations are allowed), or if a source's declared namespace has no entry
     in the namespace map, or if all sources for one target do not resolve to the same new namespace,
     or if a target already exists while its sources still exist (would overwrite).
  5. Re-running after a successful `--apply` produces no changes and exits 0.

Pure Python stdlib. Does NOT touch the Shipping module (excluded from the maps) and does NOT run any
git mutation commands.
"""

from __future__ import annotations

import argparse
import os
import re
import sys
from pathlib import Path

SCRIPT_DIR = Path(__file__).resolve().parent
if str(SCRIPT_DIR) not in sys.path:
    sys.path.insert(0, str(SCRIPT_DIR))

from consolidate_maps import MODULES, NAMESPACES, TARGETS  # noqa: E402

REPO_ROOT = SCRIPT_DIR.parent
MODULE_ROOT = REPO_ROOT / "service" / "Api" / "src" / "Module"
SCAN_ROOTS = (
    "service/Api/src/Module",
    "service/Api/src/Api",
    "service/Api/src/Migrations",
    "service/Api/tests",
)
AREA_DIR = {"admin": "Admin", "storefront": "Storefront"}

NS_LINE_RE = re.compile(r"^[ \t]*namespace\s+([\w.]+);[ \t]*$", re.MULTILINE)
USING_DIR_RE = re.compile(r"^[ \t]*using\s+(?:static\s+)?(?:[\w.]+\s*=\s*)?[\w.]+\s*;\s*$")
TYPE_DECL_RE = re.compile(
    r"^[ \t]*(?:(?:public|internal|sealed|abstract|static|partial|readonly|ref|required)\s+)*"
    r"(?:record\s+(?:struct|class)|record|class|interface|enum|struct)\s+(\w+)",
    re.MULTILINE,
)


class MigrationError(Exception):
    """Fatal, script-aborting condition (exit 1)."""


# ---------------------------------------------------------------------------
# Text helpers
# ---------------------------------------------------------------------------

def _newline_of(data: bytes) -> str:
    return "\r\n" if b"\r\n" in data else "\n"


def read_text(path: Path) -> tuple[str, str]:
    """Read a file, normalising to LF, and report the dominant newline to restore on write."""
    raw = path.read_bytes()
    newline = _newline_of(raw)
    return raw.decode("utf-8").replace("\r\n", "\n"), newline


def write_text(path: Path, text: str, newline: str = "\n") -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_bytes(text.replace("\n", newline).encode("utf-8"))


def module_map_entries(modules: list[str]) -> list[tuple[str, str]]:
    """Union of namespace-map entries for the given modules, descending by old-name length."""
    entries: dict[str, str] = {}
    for module in modules:
        entries.update(NAMESPACES[module])
    return sorted(entries.items(), key=lambda kv: len(kv[0]), reverse=True)


def rewrite_text(text: str, entries: list[tuple[str, str]]) -> tuple[str, int]:
    """Rewrite `{old}.` and `{old};`-terminated namespace references through the map.

    A replacement fires only when the old name is preceded by a non-identifier boundary and followed
    by `.`, `;`, or whitespace, so partial-name matches are impossible. Identity mappings are no-ops.
    Returns (new_text, number_of_replacements).
    """
    count = 0
    for old, new in entries:
        if old == new:
            continue
        pattern = re.compile(r"\b" + re.escape(old) + r"(?=[.;\s])")
        text, n = pattern.subn(new, text)
        count += n
    return text, count


def dedupe_using_lines(text: str) -> str:
    """Remove duplicate `using` directive lines, keeping the first occurrence of each.

    The brief requires collapsing consecutive duplicate using lines (CS0105); identical lines that
    arise from two old namespaces mapping to the same new one are removed everywhere they occur,
    which is a strict superset and fully guards CS0105 under warnings-as-errors.
    """
    seen: set[str] = set()
    out: list[str] = []
    for line in text.split("\n"):
        if USING_DIR_RE.match(line):
            if line in seen:
                continue
            seen.add(line)
        out.append(line)
    return "\n".join(out)


def split_source(text: str) -> tuple[list[str], str | None, list[str]]:
    """Split a rewritten source into (using_lines, old_namespace, body_lines).

    A line is a using directive only if it matches USING_DIR_RE (so `using (var x = ...)` statements
    stay in the body). The file-scoped `namespace X;` line is extracted, not copied into the body.
    """
    usings: list[str] = []
    body: list[str] = []
    namespace: str | None = None
    for line in text.split("\n"):
        m = NS_LINE_RE.match(line)
        if m:
            namespace = m.group(1)
            continue
        if USING_DIR_RE.match(line):
            usings.append(line)
            continue
        body.append(line)
    return usings, namespace, body


def declared_types(text: str) -> list[tuple[str, bool]]:
    """Top-level type declarations as (type_name, is_partial). Anchored to line start to avoid
    inline false positives; nested types on their own line are tolerated as documented.
    """
    result: list[tuple[str, bool]] = []
    for m in TYPE_DECL_RE.finditer(text):
        line_start = text.rfind("\n", 0, m.start()) + 1
        line_end = text.find("\n", m.start())
        if line_end == -1:
            line_end = len(text)
        line = text[line_start:line_end]
        result.append((m.group(1), bool(re.search(r"\bpartial\b", line))))
    return result


# ---------------------------------------------------------------------------
# Step 1: per-module+area move / merge
# ---------------------------------------------------------------------------

def compose_target(module: str, sources: list[tuple[str, Path, str]]) -> str:
    """Compose the PAT-001 merged target for one (module, target) from (rel_path, path, text) sources.

    Fails loudly on: missing namespace in the map, divergent new namespaces across sources, or a
    non-partial type-name collision between two different sources.
    """
    if not sources:
        raise MigrationError(f"{module}: target composed with no sources")
    entries = module_map_entries([module])
    new_ns: str | None = None
    all_usings: list[str] = []
    bodies: list[tuple[str, list[str]]] = []
    seen_types: dict[str, tuple[str, bool]] = {}

    for rel, _path, orig in sorted(sources, key=lambda s: s[0]):
        ns_match = NS_LINE_RE.search(orig)
        if not ns_match:
            raise MigrationError(
                f"{module}: {rel}: no file-scoped `namespace X;` line (or more than one file): "
                f"cannot migrate"
            )
        old_ns = ns_match.group(1)
        if old_ns not in NAMESPACES[module]:
            raise MigrationError(
                f"{module}: {rel}: declared namespace `{old_ns}` has no entry in the namespace map; "
                f"refusing to migrate"
            )
        new = NAMESPACES[module][old_ns]
        if new_ns is None:
            new_ns = new
        elif new != new_ns:
            raise MigrationError(
                f"{module}: sources for one target resolve to different new namespaces "
                f"(`{new_ns}` vs `{new}`); refusing to migrate"
            )

        for name, is_partial in declared_types(orig):
            if name in seen_types:
                prev_rel, prev_partial = seen_types[name]
                if not (is_partial and prev_partial):
                    raise MigrationError(
                        f"{module}: type-name collision for `{name}`: declared in both "
                        f"`{prev_rel}` and `{rel}` (neither is a partial declaration); "
                        f"refusing to migrate"
                    )
            else:
                seen_types[name] = (rel, is_partial)

        rewritten, _ = rewrite_text(orig, entries)
        usings, _ns, body = split_source(rewritten)
        all_usings.extend(usings)
        while body and body[0].strip() == "":
            body.pop(0)
        while body and body[-1].strip() == "":
            body.pop()
        bodies.append((rel, body))

    assert new_ns is not None
    deduped_usings = sorted(set(all_usings), key=str.casefold)
    joined_bodies = "\n\n".join("\n".join(body) for _rel, body in bodies)
    if deduped_usings:
        parts = ["\n".join(deduped_usings), "", f"namespace {new_ns};"]
    else:
        parts = [f"namespace {new_ns};"]
    if joined_bodies:
        parts += ["", joined_bodies]
    return "\n".join(parts) + "\n"


def plan_module(module: str, areas: list[str]) -> list[dict]:
    """Plan (and, for dry-run, validate) the move/merge operations for one module.

    Returns a list of ops in deterministic order. Raises MigrationError on collisions,
    missing namespaces, divergent target namespaces, or overwrite risk.
    """
    ops: list[dict] = []
    for t in TARGETS:
        if t["module"] != module:
            continue
        if t["area"] not in areas:
            continue
        base = MODULE_ROOT / module / "Features" / AREA_DIR[t["area"]]
        target_path = MODULE_ROOT / module / t["target"]
        existing: list[tuple[str, Path]] = []
        placeholders: list[Path] = []
        for s in sorted(t["sources"]):
            full = base / s
            if s == t["target"]:
                continue  # never delete when source == target
            if not full.exists():
                continue  # already migrated -> skip
            if full.stat().st_size == 0:
                placeholders.append(full)
                continue
            existing.append((s, full))

        if target_path.exists() and existing:
            raise MigrationError(
                f"{module}: target `{t['target']}` already exists while its sources still exist "
                f"({', '.join(s for s, _ in existing)}); refusing to overwrite"
            )
        if not existing and not placeholders:
            continue  # fully migrated already

        sources = [(rel, path, read_text(path)[0]) for rel, path in existing]
        content = compose_target(module, sources) if sources else None
        kind = "MOVE" if len(existing) == 1 else "MERGE"
        ops.append(
            {
                "kind": kind,
                "module": module,
                "area": t["area"],
                "area_dir": AREA_DIR[t["area"]],
                "sources": [s for s, _ in existing],
                "target": t["target"],
                "target_path": target_path,
                "content": content,
                "delete_paths": [p for _, p in existing] + placeholders,
            }
        )
    return ops

# ---------------------------------------------------------------------------
# Step 3: empty-dir and orphaned Docs/.gitkeep cleanup
# ---------------------------------------------------------------------------

def plan_cleanup(module: str, areas: list[str], deleted_files: set[Path]) -> list[tuple[str, Path]]:
    """Plan deletion of orphaned `Docs/.gitkeep` and now-empty directories under old `Shared`
    folders. Simulated against `deleted_files` so a dry-run matches what --apply will do.
    """
    ops: list[tuple[str, Path]] = []
    removed_dirs: set[Path] = set()
    for area in areas:
        base = MODULE_ROOT / module / "Features" / AREA_DIR[area]
        if not base.exists():
            continue
        for d in sorted(base.rglob("Docs"), key=lambda p: len(p.parts), reverse=True):
            if "Shared" not in d.parts:
                continue
            entries = [e for e in d.iterdir() if e not in deleted_files and e not in removed_dirs]
            if entries and all(e.name == ".gitkeep" for e in entries):
                ops.append(("DELETE_FILE", d / ".gitkeep"))
                deleted_files.add(d / ".gitkeep")
        dirs = [p for p in base.rglob("*") if p.is_dir() and "Shared" in p.parts]
        for d in sorted(dirs, key=lambda p: len(p.parts), reverse=True):
            remaining = [e for e in d.iterdir() if e not in deleted_files and e not in removed_dirs]
            if not remaining:
                ops.append(("DELETE_DIR", d))
                removed_dirs.add(d)
    return ops


# ---------------------------------------------------------------------------
# Step 2: global .cs namespace scan
# ---------------------------------------------------------------------------

def excluded_from_scan(modules: list[str]) -> set[Path]:
    """Source and target files of the migrated modules are handled by step 1; the global scan must
    not touch them (their declared old namespaces must survive until the module's own compose runs).
    """
    excluded: set[Path] = set()
    for t in TARGETS:
        if t["module"] not in modules:
            continue
        base = MODULE_ROOT / t["module"] / "Features" / AREA_DIR[t["area"]]
        excluded.add((base / t["target"]).resolve())
        for s in t["sources"]:
            excluded.add((base / s).resolve())
    return excluded


def plan_global_scan(modules: list[str]) -> list[tuple[Path, str, str, int]]:
    """Scan service/Api for `.cs` files referencing the migrated namespaces.

    Returns (path, newline, new_text, replacement_count) for every file that would change.
    Skips bin/, obj/, and any path component named `.superpowers`.
    """
    entries = module_map_entries(modules)
    excluded = excluded_from_scan(modules)
    rewrites: list[tuple[Path, str, str, int]] = []
    for root in SCAN_ROOTS:
        base = REPO_ROOT / root
        if not base.exists():
            continue
        for dp, dirs, fns in os.walk(base):
            dirs[:] = [d for d in dirs if d not in ("bin", "obj", ".superpowers")]
            for fn in fns:
                if not fn.endswith(".cs"):
                    continue
                path = Path(dp) / fn
                if path.resolve() in excluded:
                    continue
                text, newline = read_text(path)
                rewritten, n = rewrite_text(text, entries)
                if n == 0:
                    continue
                rewritten = dedupe_using_lines(rewritten)
                if rewritten != text:
                    rewrites.append((path, newline, rewritten, n))
    rewrites.sort(key=lambda r: os.path.relpath(r[0], REPO_ROOT))
    return rewrites


# ---------------------------------------------------------------------------
# CLI
# ---------------------------------------------------------------------------

def build_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(
        prog="consolidate-shared.py",
        description="Consolidate per-feature Shared folders into one per-module+area Shared folder.",
    )
    parser.add_argument(
        "--module",
        action="append",
        nargs="+",
        metavar="NAME",
        required=True,
        help="module(s) to migrate (repeatable); one of " + ", ".join(MODULES),
    )
    parser.add_argument(
        "--area",
        choices=["admin", "storefront"],
        help="area filter (default: both)",
    )
    mode = parser.add_mutually_exclusive_group(required=True)
    mode.add_argument("--dry-run", action="store_true", help="print planned operations, write nothing")
    mode.add_argument("--apply", action="store_true", help="execute the planned operations")
    return parser


def main(argv: list[str] | None = None) -> int:
    parser = build_parser()
    args = parser.parse_args(argv)
    modules = [m for group in args.module for m in group]

    unknown = [m for m in modules if m not in MODULES]
    if unknown:
        print(
            f"consolidate-shared.py: error: unknown module(s): {', '.join(unknown)}; "
            f"valid modules: {', '.join(MODULES)}",
            file=sys.stderr,
        )
        return 2

    areas = [args.area] if args.area else ["admin", "storefront"]
    dry = args.dry_run
    ops_planned = 0
    per_module_moves: dict[str, int] = {m: 0 for m in modules}
    apply_deleted: set[Path] = set()

    for module in modules:
        ops = plan_module(module, areas)
        moves = sum(1 for op in ops if op["kind"] in ("MOVE", "MERGE"))
        per_module_moves[module] = moves
        for op in ops:
            label = op["kind"]
            if dry:
                if op["content"] is None:
                    for p in op["delete_paths"]:
                        print(f"DELETE {os.path.relpath(p, REPO_ROOT)}")
                    continue
                src = " + ".join(op["sources"])
                print(
                    f"{label:<5} {module}/Features/{op['area_dir']}/{src} -> {op['target']}"
                )
            else:
                if op["content"] is not None:
                    write_text(op["target_path"], op["content"])
                for p in op["delete_paths"]:
                    if p.exists():
                        p.unlink()
                apply_deleted.update(p for p in op["delete_paths"] if p.exists())
            ops_planned += 1

        if dry:
            planned_deletes = set()
            for op in ops:
                planned_deletes.update(p for p in op["delete_paths"])
            cleanup = plan_cleanup(module, areas, planned_deletes)
        else:
            cleanup = plan_cleanup(module, areas, apply_deleted)
        for action, path in cleanup:
            rel = os.path.relpath(path, REPO_ROOT)
            if dry:
                print(f"DELETE {rel}")
            else:
                if action == "DELETE_DIR":
                    if path.exists() and not any(path.iterdir()):
                        path.rmdir()
                else:
                    if path.exists():
                        path.unlink()

    rewrites = plan_global_scan(modules)
    for path, newline, new_text, n in rewrites:
        rel = os.path.relpath(path, REPO_ROOT)
        if dry:
            print(f"REWRITE {rel} ({n} namespace rewrites)")
        else:
            write_text(path, new_text, newline)

    total_moves = sum(per_module_moves.values())
    area_label = f" ({args.area})" if args.area else ""
    for module in modules:
        print(f"{module}{area_label}: {per_module_moves[module]} move/merge operations")
    print(f"Total: {total_moves} move/merge operations, {len(rewrites)} namespace rewrites")
    if dry and ops_planned == 0 and not rewrites:
        print("No changes.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
