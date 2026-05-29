using System.Collections;
using NUnit.Framework;
using PikePush.Drill;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace PikePush.Tests.Drill
{
    // Play-mode smoke tests for Drill.unity. The whole point: catch
    // "drill mode doesn't boot / has no UI / no blocks" issues without
    // having to hit Play and stare at the screen.
    //
    // Requires Drill.unity to be in the project's Build Settings scene
    // list (which it already is — it's build index 5).
    public class DrillSmokeTests
    {
        const string DrillSceneName = "Drill";

        [UnityTest]
        public IEnumerator DrillScene_BootsAndSpawnsAtLeastOneFriendlyBlock()
        {
            yield return SceneManager.LoadSceneAsync(DrillSceneName);

            // Give DrillBootstrap.Awake a frame or two to settle.
            yield return null;
            yield return null;

            var bootstrap = Object.FindAnyObjectByType<DrillBootstrap>();
            Assert.IsNotNull(bootstrap, $"DrillBootstrap not present in {DrillSceneName} scene");
            Assert.GreaterOrEqual(bootstrap.FriendlyCount, 1,
                "Drill mode launched but no friendly block was spawned.");
            Assert.AreEqual(0, bootstrap.EnemyCount,
                "Drill mode launched with non-zero enemy count by default — initial scene state regression.");
        }

        [UnityTest]
        public IEnumerator DrillScene_MeterGamePrefabIsReachable()
        {
            yield return SceneManager.LoadSceneAsync(DrillSceneName);
            yield return null;

            var prefab = Resources.Load<GameObject>("MeterGame");
            Assert.IsNotNull(prefab,
                "MeterGame prefab not loadable from Resources at runtime — drill engagements will silently lose their HUD.");
        }

        [UnityTest]
        public IEnumerator DrillScene_NoFatalBannerOnBoot()
        {
            yield return SceneManager.LoadSceneAsync(DrillSceneName);
            yield return null;
            yield return null;

            // If the bootstrap couldn't load the prefab it spawns a banner
            // GameObject named DrillFatalBanner. Its presence after a clean
            // boot means something failed — fail loudly.
            var banner = GameObject.Find("DrillFatalBanner");
            Assert.IsNull(banner,
                "DrillBootstrap raised a fatal banner on boot. The banner itself carries the explanation — " +
                "check the Game view for the red panel content, or the prior log line for the message.");
        }
    }
}
