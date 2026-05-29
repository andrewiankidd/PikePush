using NUnit.Framework;
using UnityEngine;

namespace PikePush.Tests.Drill
{
    // Asset-existence tests. These exist because every "no UI" debugging
    // session has started with "the prefab wasn't generated" — running this
    // suite in the Test Runner catches that without having to enter Play
    // mode and stare at an empty scene.
    public class DrillAssetTests
    {
        [Test]
        public void MeterGamePrefab_LoadsFromResources()
        {
            var prefab = Resources.Load<GameObject>("MeterGame");
            Assert.IsNotNull(prefab,
                "Expected MeterGame.prefab at Assets/PikePush/Resources/MeterGame.prefab. " +
                "Should auto-generate on Unity script reload via MeterGamePrefabGenerator. " +
                "If it's still missing, run the Unity menu: PikePush > Regenerate MeterGame Prefab from Runner.");
        }

        [Test]
        public void MeterGamePrefab_HasMeterGameComponent()
        {
            var prefab = Resources.Load<GameObject>("MeterGame");
            if (prefab == null) Assert.Inconclusive("Prefab missing — see MeterGamePrefab_LoadsFromResources");

            var mg = prefab.GetComponent<PikePush.UI.MeterGame>();
            Assert.IsNotNull(mg,
                "MeterGame prefab is loadable but has no MeterGame component on the root. " +
                "Regenerate it from the runner (PikePush > Regenerate MeterGame Prefab from Runner).");
        }

        [Test]
        public void MeterGamePrefab_HasASlider()
        {
            var prefab = Resources.Load<GameObject>("MeterGame");
            if (prefab == null) Assert.Inconclusive("Prefab missing — see MeterGamePrefab_LoadsFromResources");

            var slider = prefab.GetComponentInChildren<UnityEngine.UI.Slider>(includeInactive: true);
            Assert.IsNotNull(slider,
                "MeterGame prefab has no Slider in its hierarchy — the visual mash bar is missing. " +
                "Re-extract it from Game.unity (its MeterGame uses a Slider as the meter visual).");
        }
    }
}
