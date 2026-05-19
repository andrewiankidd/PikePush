using System;
using PikePush.Utls;
using UnityEngine;

namespace PikePush
{
    public static class PikemanCustomizer
    {
        const string TorsoChildName = "PT_Male_Peasant_01_upper";
        const string ClothColorProperty = "_CLOTH4COLOR";

        public const string PrefColourOneKey = "ColourOne";
        public const string PrefColourTwoKey = "ColourTwo";
        public const string DefaultColourOne = "93,165,255";
        public const string DefaultColourTwo = "145,156,168";

        public static void Customize(GameObject pikeman, Material hatMaterial = null)
        {
            var c1 = LoadColor(PrefColourOneKey, DefaultColourOne);
            var c2 = LoadColor(PrefColourTwoKey, DefaultColourTwo);
            LogHelper.debug($"[PikemanCustomizer] {pikeman.name}: ColourOne={c1} ColourTwo={c2}");

            if (hatMaterial != null) hatMaterial.color = c1;
            ApplyTorsoColor(pikeman, c2);
        }

        public static Color LoadColor(string playerPrefsKey, string defaultRgb)
        {
            var raw = PlayerPrefs.GetString(playerPrefsKey, defaultRgb);
            byte[] vals = Array.ConvertAll(raw.Split(','), byte.Parse);
            return new Color32(vals[0], vals[1], vals[2], 255);
        }

        static void ApplyTorsoColor(GameObject pikeman, Color color)
        {
            foreach (var renderer in pikeman.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer.gameObject.name == TorsoChildName)
                {
                    renderer.sharedMaterial.SetColor(ClothColorProperty, color);
                    return;
                }
            }
            LogHelper.warn($"[PikemanCustomizer] No child named '{TorsoChildName}' under '{pikeman.name}' — torso colour skipped");
        }
    }
}
