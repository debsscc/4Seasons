#!/usr/bin/env python3
"""Fix Unity UI CanvasScaler and harmful Dialogue System prefab overrides."""

from __future__ import annotations

import re
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
SOURCES = ROOT / "Assets" / "_Sources"
DIALOGUE_GUID = "a5c85665d060b644b84a16aaf2b78fb5"

HARMFUL = {
    "8572666163549193176": {
        "m_AnchorMax.y", "m_AnchorMin.y", "m_SizeDelta.y",
        "m_AnchoredPosition.x", "m_AnchoredPosition.y",
    },
    "5966181726367265622": {
        "m_AnchorMax.y", "m_AnchorMin.y", "m_SizeDelta.x",
        "m_AnchoredPosition.x", "m_AnchoredPosition.y",
    },
    "5820181997030888676": {"m_SizeDelta.y"},
    "6774043749298185568": {"m_SizeDelta.x"},
    "8586434540972666658": {
        "m_SizeDelta.x", "m_SizeDelta.y",
        "m_LocalScale.x", "m_LocalScale.y", "m_LocalScale.z",
        "m_Pivot.x", "m_Pivot.y",
        "m_AnchoredPosition.x", "m_AnchoredPosition.y",
    },
}


def fix_scaler_content(content: str) -> str:
    content = content.replace("m_UiScaleMode: 0", "m_UiScaleMode: 1")
    content = content.replace(
        "m_ReferenceResolution: {x: 800, y: 600}",
        "m_ReferenceResolution: {x: 1920, y: 1080}",
    )
    content = re.sub(
        r"m_MatchWidthOrHeight: (?:0|1)(?:\.0)?(?!\.)",
        "m_MatchWidthOrHeight: 0.5",
        content,
    )
    return content


def should_remove_override(target_line: str, prop_line: str) -> bool:
    if DIALOGUE_GUID not in target_line:
        return False
    m = re.search(r"propertyPath: (\S+)", prop_line)
    if not m:
        return False
    prop = m.group(1)
    for file_id, props in HARMFUL.items():
        if f"{file_id}, guid: {DIALOGUE_GUID}" in target_line and prop in props:
            return True
    return False


def remove_harmful_overrides(content: str) -> tuple[str, int]:
    lines = content.splitlines(keepends=True)
    out: list[str] = []
    i = 0
    removed = 0
    while i < len(lines):
        line = lines[i]
        if line.strip().startswith("- target:") and DIALOGUE_GUID in line and i + 3 < len(lines):
            if should_remove_override(line, lines[i + 1]):
                i += 4
                removed += 1
                continue
        out.append(line)
        i += 1
    return "".join(out), removed


def process_file(path: Path) -> bool:
    original = path.read_text(encoding="utf-8")
    updated = fix_scaler_content(original)
    removed = 0
    if path.suffix == ".unity":
        updated, removed = remove_harmful_overrides(updated)
    if updated != original:
        path.write_text(updated, encoding="utf-8")
        if removed:
            print(f"  {path.relative_to(ROOT)}: removed {removed} override(s)")
        return True
    return False


def main() -> None:
    changed = []
    for path in sorted(SOURCES.rglob("*.unity")) + sorted(SOURCES.rglob("*.prefab")):
        if process_file(path):
            changed.append(str(path.relative_to(ROOT)))
    print(f"Updated {len(changed)} file(s)")


if __name__ == "__main__":
    main()
