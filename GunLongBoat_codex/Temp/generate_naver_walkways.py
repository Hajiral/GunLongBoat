import json
from pathlib import Path

import numpy as np
from PIL import Image, ImageDraw
from scipy import ndimage


ROOT = Path(__file__).resolve().parents[1]
IMAGE = ROOT / "Assets/Images/naver_map.png"
STEP = 4  # pixels per cube grid cell; 1 pixel is 0.15 Unity units

source = Image.open(IMAGE).convert("RGB")
pixels = np.asarray(source)
height, width = pixels.shape[:2]
walkable = np.all(pixels == (229, 237, 244), axis=2) | np.all(pixels == (249, 249, 249), axis=2)

# Limit the map's road color to the school grounds; the same color is used by
# streets and sidewalks outside the campus.
campus = Image.new("1", (width, height))
campus_draw = ImageDraw.Draw(campus)
campus_draw.polygon(
    [(0, 235), (815, 20), (740, 120), (650, 270), (570, 410),
     (490, 555), (390, 720), (270, 879), (120, 550)],
    fill=1,
)
walkable &= np.asarray(campus, dtype=bool)

# The flower bed and the small central garden use the walkway color on the map.
# Keep the paths around them but leave their interiors empty.
exclusion = Image.new("1", (width, height))
draw = ImageDraw.Draw(exclusion)
draw.polygon([(231, 510), (345, 530), (349, 562), (237, 544)], fill=1)
draw.ellipse((425, 314, 452, 342), fill=1)
walkable &= ~np.asarray(exclusion, dtype=bool)

cols = (width + STEP - 1) // STEP
rows = (height + STEP - 1) // STEP
cells = np.zeros((rows, cols), dtype=bool)
for row in range(rows):
    for col in range(cols):
        part = walkable[row * STEP : (row + 1) * STEP,
                        col * STEP : (col + 1) * STEP]
        cells[row, col] = part.mean() >= 0.1

# Combine adjacent occupied cells into rectangles to reduce the object count.
rectangles = []
open_runs = {}
for row in range(rows + 1):
    runs = set()
    if row < rows:
        col = 0
        while col < cols:
            if not cells[row, col]:
                col += 1
                continue
            start = col
            while col < cols and cells[row, col]:
                col += 1
            runs.add((start, col))
    for run, start_row in list(open_runs.items()):
        if run not in runs:
            rectangles.append((run[0], start_row, run[1], row))
            del open_runs[run]
    for run in runs:
        open_runs.setdefault(run, row)

data = {
    "imageSize": [width, height],
    "pixelWorldSize": 0.15,
    "rectangles": rectangles,
    "occupiedCells": int(cells.sum()),
}
(ROOT / "Temp/naver_walkway_rectangles.json").write_text(
    json.dumps(data, separators=(",", ":")), encoding="utf-8"
)
(ROOT / "Temp/naver_walkway_rectangles.csv").write_text(
    "\n".join(",".join(map(str, rect)) for rect in rectangles), encoding="ascii"
)

preview = source.convert("RGBA")
overlay = Image.new("RGBA", preview.size)
pen = ImageDraw.Draw(overlay)
for left, top, right, bottom in rectangles:
    pen.rectangle((left * STEP, top * STEP, right * STEP - 1, bottom * STEP - 1),
                  fill=(40, 220, 80, 115))
preview = Image.alpha_composite(preview, overlay)
preview.save(ROOT / "Temp/naver_walkway_preview.png")
print(f"occupied cells: {cells.sum()}, cube rectangles: {len(rectangles)}")
labels, count = ndimage.label(cells)
sizes = np.bincount(labels.ravel())[1:]
print(f"connected components: {count}, largest: {sorted(sizes, reverse=True)[:10]}")
for label in np.argsort(sizes)[::-1][:10] + 1:
    ys, xs = np.where(labels == label)
    print(f"component {label}: {len(xs)} cells, px bounds=({xs.min()*STEP},{ys.min()*STEP})-({(xs.max()+1)*STEP},{(ys.max()+1)*STEP})")




