using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SeongseoGO.Art.Editor
{
    /// <summary>
    /// 임시 선 아이콘(96x96, 투명 배경, 흰 선)을 Art/Icons에 PNG로 그린다.
    /// 색은 USS의 -unity-background-image-tint-color로 바꾼다.
    /// 같은 이름의 파일이 이미 있으면 건너뛰므로, 진짜 아이콘으로 덮어쓴 파일은 지워지지 않는다.
    /// </summary>
    public static class IconGenerator
    {
        const string Folder = "Assets/_Project/Art/Icons";
        const int Size = 96;
        const float Grid = 24f;         // 아이콘은 24x24 격자 좌표로 정의 (y는 아래 방향)
        const float HalfStroke = 1f;    // 선 굵기 2 (96px에서 8px)

        [MenuItem("SeongseoGO/Icons/Generate Placeholder Icons (missing only)")]
        public static void GenerateMissing() => Generate(false);

        [MenuItem("SeongseoGO/Icons/Regenerate All Placeholder Icons (overwrite)")]
        public static void RegenerateAll()
        {
            if (EditorUtility.DisplayDialog("임시 아이콘 덮어쓰기",
                    "Art/Icons의 임시 아이콘 이름과 같은 파일을 모두 다시 그립니다. 진짜 아이콘으로 바꾼 파일도 덮어씁니다.",
                    "덮어쓰기", "취소"))
                Generate(true);
        }

        public static void Generate(bool overwrite)
        {
            Directory.CreateDirectory(Folder);
            int written = 0, skipped = 0;
            foreach (var icon in Icons())
            {
                string path = $"{Folder}/{icon.Key}.png";
                if (!overwrite && File.Exists(path)) { skipped++; continue; }
                File.WriteAllBytes(path, Render(icon.Value).EncodeToPNG());
                written++;
            }
            AssetDatabase.Refresh();
            Debug.Log($"[IconGenerator] 새로 그림 {written}개, 이미 있어서 건너뜀 {skipped}개 ({Folder})");
        }

        // ---------- 아이콘 정의 (24x24 격자) ----------
        static Dictionary<string, Shape[]> Icons() => new Dictionary<string, Shape[]>
        {
            ["back"] = new[] { Poly(15, 5, 8, 12, 15, 19) },
            ["chevron-right"] = new[] { Poly(9, 5, 16, 12, 9, 19) },
            ["close"] = new[] { Line(6, 6, 18, 18), Line(18, 6, 6, 18) },
            ["search"] = new[] { Ring(10.5f, 10.5f, 6.5f), Line(15.5f, 15.5f, 20, 20) },
            ["location"] = new[]
            {
                Ring(12, 12, 7), Dot(12, 12, 2.5f),
                Line(12, 2, 12, 5), Line(12, 19, 12, 22), Line(2, 12, 5, 12), Line(19, 12, 22, 12),
            },
            ["pin"] = Pin(),
            ["walk"] = new[]
            {
                Dot(13.5f, 4, 2.2f),
                Line(12.5f, 8.5f, 10.5f, 14.5f),
                Poly(10.5f, 14.5f, 8, 21),
                Poly(10.5f, 14.5f, 14, 17, 15, 21),
                Poly(7, 12.5f, 11.5f, 9.5f, 15.5f, 12.5f),
            },
            ["home"] = new[]
            {
                Poly(3, 11, 12, 3.5f, 21, 11),
                Poly(5.5f, 9.5f, 5.5f, 20, 18.5f, 20, 18.5f, 9.5f),
                Poly(10, 20, 10, 14.5f, 14, 14.5f, 14, 20),
            },
            ["camera"] = new[]
            {
                RoundRect(3, 7, 18, 13, 2.5f), Ring(12, 13.5f, 3.5f),
                Poly(8.5f, 7, 10, 4.5f, 14, 4.5f, 15.5f, 7),
            },
            ["check"] = new[] { Poly(5, 12.5f, 10, 17.5f, 19, 7) },
            ["sound"] = new[]
            {
                Poly(4, 9.5f, 8, 9.5f, 13, 5, 13, 19, 8, 14.5f, 4, 14.5f, 4, 9.5f),
                Arc(13, 12, 4, -45, 45), Arc(13, 12, 7.5f, -50, 50),
            },
            ["more"] = new[] { Dot(5, 12, 2), Dot(12, 12, 2), Dot(19, 12, 2) },
            ["arrow-right"] = new[] { Line(4, 12, 20, 12), Poly(14, 6, 20, 12, 14, 18) },
            ["sparkle"] = new[]
            {
                Poly(11, 4, 12.6f, 10.4f, 19, 12, 12.6f, 13.6f, 11, 20, 9.4f, 13.6f, 3, 12, 9.4f, 10.4f, 11, 4),
                Line(19, 2.5f, 19, 6.5f), Line(17, 4.5f, 21, 4.5f),
            },
            ["navigation"] = new[] { Poly(3, 11, 21, 3, 13, 21, 11, 13, 3, 11) },
            ["scan"] = new[]
            {
                Poly(3, 8, 3, 3, 8, 3), Poly(16, 3, 21, 3, 21, 8),
                Poly(21, 16, 21, 21, 16, 21), Poly(8, 21, 3, 21, 3, 16),
                Line(7, 12, 17, 12),
            },
        };

        static Shape[] Pin()
        {
            // 원(중심 12,10 반지름 7)의 위쪽 호 + 아래 꼭짓점(12,22)으로 가는 접선 2개 + 안쪽 작은 원
            const float cx = 12, cy = 10, r = 7, tipY = 22;
            float alpha = Mathf.Acos(r / (tipY - cy)) * Mathf.Rad2Deg;   // 접점 각도 (아래 방향 90°에서 ±)
            float a0 = 90 + alpha, a1 = 90 - alpha + 360;
            var center = new Vector2(cx, cy);
            Vector2 p0 = center + r * Dir(a0), p1 = center + r * Dir(a1);
            return new[]
            {
                Arc(cx, cy, r, a0, a1),
                Line(p0.x, p0.y, cx, tipY), Line(p1.x, p1.y, cx, tipY),
                Ring(cx, cy, 2.5f),
            };
        }

        // ---------- 그리기 ----------
        // Shape: 격자 좌표의 점 → "모양 바깥까지의 거리"(안쪽이면 0 이하)
        delegate float Shape(Vector2 p);

        static Texture2D Render(Shape[] shapes)
        {
            var tex = new Texture2D(Size, Size, TextureFormat.RGBA32, false);
            var pixels = new Color32[Size * Size];
            float scale = Size / Grid;
            for (int y = 0; y < Size; y++)
            {
                for (int x = 0; x < Size; x++)
                {
                    // 텍스처는 아래가 0, 격자는 위가 0
                    var p = new Vector2((x + 0.5f) / scale, (Size - 1 - y + 0.5f) / scale);
                    float d = float.MaxValue;
                    foreach (var s in shapes) d = Mathf.Min(d, s(p));
                    float a = Mathf.Clamp01(0.5f - d * scale);   // 1px 안티앨리어싱
                    pixels[y * Size + x] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(a * 255));
                }
            }
            tex.SetPixels32(pixels);
            tex.Apply();
            return tex;
        }

        static Vector2 Dir(float deg) => new Vector2(Mathf.Cos(deg * Mathf.Deg2Rad), Mathf.Sin(deg * Mathf.Deg2Rad));

        static float SegmentDistance(Vector2 p, Vector2 a, Vector2 b)
        {
            var ab = b - a;
            float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / Vector2.Dot(ab, ab));
            return Vector2.Distance(p, a + t * ab);
        }

        static Shape Line(float x1, float y1, float x2, float y2)
        {
            var a = new Vector2(x1, y1);
            var b = new Vector2(x2, y2);
            return p => SegmentDistance(p, a, b) - HalfStroke;
        }

        static Shape Poly(params float[] xy)
        {
            var points = new List<Vector2>();
            for (int i = 0; i + 1 < xy.Length; i += 2) points.Add(new Vector2(xy[i], xy[i + 1]));
            return p =>
            {
                float d = float.MaxValue;
                for (int i = 0; i + 1 < points.Count; i++) d = Mathf.Min(d, SegmentDistance(p, points[i], points[i + 1]));
                return d - HalfStroke;
            };
        }

        static Shape Ring(float cx, float cy, float r)
        {
            var c = new Vector2(cx, cy);
            return p => Mathf.Abs(Vector2.Distance(p, c) - r) - HalfStroke;
        }

        static Shape Dot(float cx, float cy, float r)
        {
            var c = new Vector2(cx, cy);
            return p => Vector2.Distance(p, c) - r;
        }

        /// <summary>각도는 도 단위, 0° = 오른쪽, 90° = 아래. start에서 end로 시계 방향(화면 기준).</summary>
        static Shape Arc(float cx, float cy, float r, float startDeg, float endDeg)
        {
            var c = new Vector2(cx, cy);
            Vector2 a = c + r * Dir(startDeg), b = c + r * Dir(endDeg);
            float span = endDeg - startDeg;
            return p =>
            {
                var v = p - c;
                float ang = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;
                float rel = Mathf.Repeat(ang - startDeg, 360f);
                float d = rel <= span
                    ? Mathf.Abs(v.magnitude - r)
                    : Mathf.Min(Vector2.Distance(p, a), Vector2.Distance(p, b));
                return d - HalfStroke;
            };
        }

        static Shape RoundRect(float x, float y, float w, float h, float r)
        {
            var center = new Vector2(x + w / 2, y + h / 2);
            var half = new Vector2(w / 2 - r, h / 2 - r);
            return p =>
            {
                var q = new Vector2(Mathf.Abs(p.x - center.x), Mathf.Abs(p.y - center.y)) - half;
                float outside = new Vector2(Mathf.Max(q.x, 0), Mathf.Max(q.y, 0)).magnitude;
                float sd = outside + Mathf.Min(Mathf.Max(q.x, q.y), 0) - r;
                return Mathf.Abs(sd) - HalfStroke;
            };
        }
    }
}
