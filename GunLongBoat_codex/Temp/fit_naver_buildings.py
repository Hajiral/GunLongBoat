from pathlib import Path

import numpy as np
from PIL import Image, ImageDraw
from scipy import ndimage, optimize


root = Path(__file__).resolve().parents[1]
im = Image.open(root / "Assets/Images/naver_map.png").convert("RGB")
a = np.asarray(im)
mask = np.all(a == (245, 248, 251), axis=2)
rois = {
    "Gal": (125, 195, 270, 420),
    "Bok": (295, 165, 555, 285),
    "Mil": (475, 230, 615, 445),
    "Mo": (115, 390, 375, 515),
    "Ilip": (240, 510, 500, 845),
}

preview = im.copy()
d = ImageDraw.Draw(preview)
for name, (x0, y0, x1, y1) in rois.items():
    roi = mask[y0:y1, x0:x1]
    labels, count = ndimage.label(roi)
    sizes = np.bincount(labels.ravel())[1:]
    top = np.argsort(sizes)[::-1][:3] + 1
    print(name, "components", [(int(t), int(sizes[t - 1])) for t in top])
    points = np.argwhere(labels == top[0])[:, ::-1].astype(float)
    points[:, 0] += x0
    points[:, 1] += y0
    center = points.mean(axis=0)
    covariance = np.cov((points - center).T)
    eigenvalues, eigenvectors = np.linalg.eigh(covariance)
    axis = eigenvectors[:, np.argmax(eigenvalues)]
    if axis[0] < 0:
        axis = -axis
    side = np.array([-axis[1], axis[0]])
    long_projection = points @ axis
    short_projection = points @ side
    lo, hi = np.percentile(long_projection, [0.5, 99.5])
    slo, shi = np.percentile(short_projection, [0.5, 99.5])
    center = axis * ((lo + hi) / 2) + side * ((slo + shi) / 2)
    corners = [center + axis * u * (hi - lo) / 2 + side * v * (shi - slo) / 2
               for u, v in [(-1, -1), (1, -1), (1, 1), (-1, 1)]]
    d.line([tuple(p) for p in corners] + [tuple(corners[0])], fill=(255, 0, 0), width=2)
    d.text(tuple(center), name, fill=(255, 0, 0))
    print(name, "center", tuple(np.round(center, 2)),
          "length,width", round(hi - lo, 2), round(shi - slo, 2),
          "axis", tuple(np.round(axis, 4)))
    target = labels == top[0]
    angle = float(np.degrees(np.arctan2(axis[1], axis[0])))
    initial = np.array([center[0], center[1], hi - lo, shi - slo, angle])

    def score(values):
        cx, cy, length, width, degrees = values
        if not (x0 < cx < x1 and y0 < cy < y1
                and initial[2] * 0.8 < length < initial[2] * 1.2
                and initial[3] * 0.65 < width < initial[3] * 1.35
                and abs(degrees - angle) < 8):
            return 1
        theta = np.radians(degrees)
        direction = np.array([np.cos(theta), np.sin(theta)])
        side = np.array([-direction[1], direction[0]])
        box = [np.array([cx, cy]) + direction * length / 2 * u + side * width / 2 * v
               for u, v in [(-1, -1), (1, -1), (1, 1), (-1, 1)]]
        image = Image.new("1", (x1 - x0, y1 - y0))
        ImageDraw.Draw(image).polygon([(float(p[0] - x0), float(p[1] - y0)) for p in box], fill=1)
        prediction = np.asarray(image, dtype=bool)
        intersection = np.count_nonzero(target & prediction)
        union = np.count_nonzero(target | prediction)
        return 1 - intersection / union

    result = optimize.minimize(score, initial, method="Nelder-Mead",
                               options={"maxiter": 450, "xatol": 0.05, "fatol": 1e-5})
    optimized = result.x
    theta = np.radians(optimized[4])
    direction = np.array([np.cos(theta), np.sin(theta)])
    side = np.array([-direction[1], direction[0]])
    center_optimized = optimized[:2]
    box_optimized = [center_optimized + direction * optimized[2] / 2 * u
                     + side * optimized[3] / 2 * v
                     for u, v in [(-1, -1), (1, -1), (1, 1), (-1, 1)]]
    d.line([tuple(p) for p in box_optimized] + [tuple(box_optimized[0])],
           fill=(0, 180, 0), width=2)
    print(name, "optimized", tuple(np.round(optimized, 2)),
          "IoU", round(1 - score(initial), 3), "->", round(1 - result.fun, 3))
preview.save(root / "Temp/naver_building_fit.png")
