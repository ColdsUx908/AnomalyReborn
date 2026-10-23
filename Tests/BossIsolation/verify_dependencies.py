"""Check direct namespace and resource dependencies across removable boss modules."""
from pathlib import Path
import re

root = Path(__file__).resolve().parents[2]
boss_root = root / "Anomalies/Bosses"
bosses = {p.name for p in boss_root.iterdir() if p.is_dir()}
errors = []
for project in ("Anomalies", "Transoceanic"):
    for path in (root / project).rglob("*.cs"):
        if {"obj", "bin"} & set(path.parts):
            continue
        relative = path.relative_to(boss_root) if path.is_relative_to(boss_root) else None
        owner = relative.parts[0] if relative else None
        for line_number, line in enumerate(path.read_text(encoding="utf-8-sig").splitlines(), 1):
            # Localization categories intentionally remain stable when a module is removed.
            code = re.sub(r'"(?:\\.|[^"\\])*"', '""', line)
            referenced = set(re.findall(r"\b(?:Anomalies\.)?Bosses\.(\w+)", code))
            referenced |= set(re.findall(r'"[^"\n]*\bBosses/(\w+)/', line))
            for boss in (referenced & bosses) - {owner}:
                errors.append(f"{path.relative_to(root)}:{line_number}: depends on {boss}")

if errors:
    raise SystemExit("\n".join(errors))
print(f"Verified namespace and resource boundaries for {len(bosses)} boss modules.")
