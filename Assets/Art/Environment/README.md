# Arkham — Sprite Exports

Flat, top-down, transparent art for the map tiles. Generated to match the locked
mockup design (puzzle streets, stone bridges, violet eldritch accent `#9a7be0`).

Each shape is provided as:
- **`.svg`** — vector, infinitely scalable. Best source of truth; re-export to any size.
- **`.png`** — 2048 px, transparent background. Drop straight into Unity as sprites.

Open **`sprite-sheet.html`** for a visual contact sheet.

## Files

| File | What it is |
|------|------------|
| `district_ground` | Circular district ground. Goes *under* the street tiles. |
| `street_puzzle` | One street tile (locked style). **Three of these = one district.** |
| `street_puzzle_void` | Corrupted street tile (violet); make it emissive for the void breach. |
| `street_petals`, `street_wedges` | Alternate street styles, in case you revisit. |
| `district_*_assembled` | Reference renders (ground + 3 streets). Not for direct import. |
| `bridge_stone` | Bridge tile (locked style). |
| `bridge_planks`, `bridge_tapered` | Alternate bridge styles. |

## Key fact: streets are ONE piece rotated 120°

In every district the three street tiles are **congruent** — the same shape rotated.
So you only import **`street_puzzle`** once and instantiate it three times.

All district/street PNGs use a **1024² canvas centred on the district centre**, so:
- Sprite **Pivot = Center**
- Place 3 street instances at the district centre, rotate **0° / 120° / 240°**
- They interlock automatically and fill the circular ground.

## Suggested Unity import settings

- **Texture Type:** Sprite (2D and UI)
- **Sprite Mode:** Single
- **Pivot:** Center
- **Pixels Per Unit:** pick one value and keep it consistent across all tiles
  (e.g. `512` → a district ≈ 4 world units across; tune to taste).
- **Filter Mode:** Bilinear · **Compression:** None (or High Quality) for clean edges
- **Alpha Is Transparency:** ✓ · **Generate Mip Maps:** ✓ if the camera zooms out

### Layering (sorting order, back → front)
1. board / table
2. `district_ground` (×3)
3. `bridge_*` (×3)
4. `street_*` (×9)
5. corrupted `street_puzzle_void` (replaces the affected street)
6. tokens (player, clues, enemies)

### Corrupted tile / eldritch glow
Don't bake the glow into the sprite — drive it in-engine so it can pulse:
- Material with **emission** enabled, emission color = `#9a7be0` (HDR, intensity ~2–4).
- Add **Bloom** in your URP Volume profile (you already have one) to make it bleed.
- Animate emission intensity for the breach pulse.

## Palette (matches the mockup)
- Board / background: `#0d0f14`
- District ground: radial `#2b2e35` → `#171a20`, rim `#474f5e`
- Streets: `#3b4453` / `#374852` / `#403c54` (subtle per-street tint; optional)
- Mortar / tile edge: `#11151c`
- Bridge stone: `#46443c` → `#2a2823`, edge `#161310`
- Eldritch accent: `#9a7be0`

> Need different sizes, individually-tinted street pieces, or PNGs without the
> mortar stroke? Re-export from the `.svg` files or ask and I'll regenerate.
