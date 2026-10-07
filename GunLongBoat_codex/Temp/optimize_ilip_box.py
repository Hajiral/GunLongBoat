import math
from pathlib import Path

import numpy as np
from PIL import Image, ImageDraw
from scipy import ndimage, optimize


root = Path(__file__).resolve().parents[1]
source = Image.open(root / "Assets/Images/naver_map.png").convert("RGB")
a = np.asarray(source)
x0, y0, x1, y1 = 220, 500, 500, 850
roi = np.all(a[y0:y1, x0:x1] == (245, 248, 251), axis=2)
labels, count = ndimage.label(roi)
sizes = np.bincount(labels.ravel())[1:]
target = labels == (np.argmax(sizes) + 1)


def corners(values):
    cx, cy, length, width, angle = values
    direction = np.array([math.cos(math.radians(angle)),
                          math.sin(math.radians(angle))])
    side = np.array([-direction[1], direction[0]])
    center = np.array([cx, cy])
    return [center + direction * length / 2 * u + side * width / 2 * v
            for u, v in [(-1, -1), (1, -1), (1, 1), (-1, 1)]]


def score(values):
    if not (300 < values[0] < 400 and 620 < values[1] < 720
            and 240 < values[2] < 315 and 40 < values[3] < 95
            and -60 < values[4] < -45):
        return 1
    mask = Image.new("1", (x1 - x0, y1 - y0))
    ImageDraw.Draw(mask).polygon([(float(p[0] - x0), float(p[1] - y0))
                                  for p in corners(values)], fill=1)
    predicted = np.asarray(mask, dtype=bool)
    intersection = np.count_nonzero(target & predicted)
    union = np.count_nonzero(target | predicted)
    return 1 - intersection / union


initial = np.array([352.58, 673.74, 288, 70.14, -52.5])
result = optimize.minimize(score, initial, method="Nelder-Mead",
                           options={"maxiter": 450, "xatol": 0.05, "fatol": 1e-5})
print("initial", initial, "IoU", 1 - score(initial))
print("optimized", result.x, "IoU", 1 - result.fun)
preview = source.copy()
pen = ImageDraw.Draw(preview)
for values, color in ((initial, (255, 0, 0)), (result.x, (0, 255, 0))):
    polygon = [tuple(map(float, p)) for p in corners(values)]
    pen.line(polygon + [polygon[0]], fill=color, width=2)
preview.save(root / "Temp/ilip_box_optimization.png")
