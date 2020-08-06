using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Newtonsoft.Json.Linq;

namespace CoordinateMapper {
    public static class Texture2D_Extensions {

        public static Texture2D DrawHeatmap(int[,] heatmapValues, Gradient colors) {
            int w = heatmapValues.GetLength(0);
            int h = heatmapValues.GetLength(1);

            Texture2D heatmap = new Texture2D(w, h, TextureFormat.RGBA32, false);

            var texColors = new Color32[w * h];
            for (int i = 0; i < texColors.Length; i++) { texColors[i] = Color.clear; }

            for (int y = 0; y < h; y++) {
                for (int x = 0; x < w; x++) {
                    if (heatmapValues[x, y] > 0) {
                        texColors[y * w + x] = colors.Evaluate(heatmapValues[x, y] / 100f);
                    }
                }
            }

            return heatmap.ApplyColors(texColors);
        }

        public static Texture2D ApplyColors(this Texture2D tex, Color32[] colors) {
            var byteColors = new byte[colors.Length * 4];
            for (int i = 0; i < colors.Length; i++) {
                Color32 c = colors[i];
                byteColors[i * 4] = c.r;
                byteColors[i * 4 + 1] = c.g;
                byteColors[i * 4 + 2] = c.b;
                byteColors[i * 4 + 3] = c.a;
            }

            tex.LoadRawTextureData(byteColors);
            tex.Apply();
            return tex;
        }

        public static byte[] CreateColorMap(int[,] heatmapValues, Gradient colors) {
            int w = heatmapValues.GetLength(0);
            int h = heatmapValues.GetLength(1);

            //Texture2D heatmap = new Texture2D(w, h, TextureFormat.RGBA32, false);

            var texColors = new Color32[w * h];
            for (int i = 0; i < texColors.Length; i++) { texColors[i] = Color.clear; }

            for (int y = 0; y < h; y++) {
                for (int x = 0; x < w; x++) {
                    if (heatmapValues[x, y] > 0) {
                        texColors[y * w + x] = colors.Evaluate(heatmapValues[x, y] / 100f);
                    }
                }
            }

            return createColors(texColors);
        }

        public static byte[] createColors(Color32[] colors) {
            var byteColors = new byte[colors.Length * 4];
            for (int i = 0; i < colors.Length; i++) {
                Color32 c = colors[i];
                byteColors[i * 4] = c.r;
                byteColors[i * 4 + 1] = c.g;
                byteColors[i * 4 + 2] = c.b;
                byteColors[i * 4 + 3] = c.a;
            }
            return byteColors;
        }
    }

    public static class JObject_Extensions {
        public static IEnumerable<JToken> AllTokens(this JObject obj) {
            var toSearch = new Stack<JToken>(obj.Children());
            while (toSearch.Count > 0) {
                var inspected = toSearch.Pop();
                yield return inspected;
                foreach (var child in inspected) {
                    toSearch.Push(child);
                }
            }
        }
    }
}
