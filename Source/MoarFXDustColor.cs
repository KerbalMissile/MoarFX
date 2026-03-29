using System;
using System.Collections.Generic;
using UnityEngine;

namespace MoarFX
{
    public static class MoarFXDustColors
    {
        private static bool loaded = false;
        private static Dictionary<string, Dictionary<string, Color>> bodyBiomeColors = new Dictionary<string, Dictionary<string, Color>>();

        public static Color GetColor(CelestialBody body, string biomeName, Color defaultColor)
        {
            if (!loaded)
            {
                LoadConfigs();
            }

            if (body == null || string.IsNullOrEmpty(biomeName))
            {
                return defaultColor;
            }

            string cleanBiome = biomeName.Replace(" ", "").ToLowerInvariant();

            Dictionary<string, Color> biomes;
            if (bodyBiomeColors.TryGetValue(body.bodyName, out biomes))
            {
                Color c;
                if (biomes.TryGetValue(cleanBiome, out c))
                {
                    c.a = defaultColor.a;
                    return c;
                }
            }

            return defaultColor;
        }

        private static void LoadConfigs()
        {
            loaded = true;
            bodyBiomeColors.Clear();

            ConfigNode[] nodes = GameDatabase.Instance.GetConfigNodes("VO_WHEEL_DUST_COLOR");
            if (nodes == null) return;

            for (int i = 0; i < nodes.Length; i++)
            {
                ConfigNode node = nodes[i];

                string bodyName = node.GetValue("body");
                string biomeName = node.GetValue("biomeName");
                string colorStr = node.GetValue("color");

                if (string.IsNullOrEmpty(bodyName) || string.IsNullOrEmpty(biomeName) || string.IsNullOrEmpty(colorStr))
                    continue;

                Color color;
                if (TryParseColor(colorStr, out color))
                {
                    if (!bodyBiomeColors.ContainsKey(bodyName))
                        bodyBiomeColors[bodyName] = new Dictionary<string, Color>();

                    string cleanName = biomeName.Replace(" ", "").ToLowerInvariant();
                    bodyBiomeColors[bodyName][cleanName] = color;
                }
            }
        }

        private static bool TryParseColor(string value, out Color color)
        {
            color = Color.white;

            string[] parts = value.Split(
                new char[] { ',', ' ' },
                StringSplitOptions.RemoveEmptyEntries
            );

            if (parts.Length < 3)
            {
                return false;
            }

            float r;
            float g;
            float b;

            if (!float.TryParse(parts[0], System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out r)) return false;
            if (!float.TryParse(parts[1], System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out g)) return false;
            if (!float.TryParse(parts[2], System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out b)) return false;

            if (r > 1.0f || g > 1.0f || b > 1.0f)
            {
                r /= 255f;
                g /= 255f;
                b /= 255f;
            }

            color = new Color(r, g, b, 1.0f);
            return true;
        }
    }
}
