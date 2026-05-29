using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace PikePush.EditorTools
{
    // Extracts the MeterGame GameObject from the runner scene into a
    // Resources prefab so drill / campaign can instantiate the exact same
    // visual setup instead of rebuilding UI by hand.
    //
    // Run once via PikePush/Regenerate MeterGame Prefab from Runner. Re-run
    // any time the runner's MeterGame visual changes — drill picks up the
    // new prefab on next play.
    public static class MeterGamePrefabGenerator
    {
        const string GameScenePath = "Assets/PikePush/Scenes/Game.unity";
        const string OutputDir     = "Assets/PikePush/Resources";
        const string OutputPath    = "Assets/PikePush/Resources/MeterGame.prefab";
        const string MeterGameName = "MeterGame";

        [MenuItem("PikePush/Regenerate MeterGame Prefab from Runner")]
        public static void Regenerate()
        {
            var openedScene = EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Additive);
            try
            {
                // Look for an active OR inactive MeterGame under any canvas in the runner scene.
                var donor = FindMeterGame();
                if (donor == null)
                {
                    Debug.LogError($"[MeterGamePrefabGenerator] No GameObject named '{MeterGameName}' found in {GameScenePath}");
                    return;
                }

                Debug.Log($"[MeterGamePrefabGenerator] Using donor: {donor.name}");

                // Clone, force-active so prefab serialises with components enabled.
                var clone = Object.Instantiate(donor);
                clone.name = MeterGameName;
                clone.SetActive(true);

                EnsureFolder(OutputDir);

                var saved = PrefabUtility.SaveAsPrefabAsset(clone, OutputPath);
                Object.DestroyImmediate(clone);

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                if (saved != null)
                {
                    Debug.Log($"[MeterGamePrefabGenerator] Saved {OutputPath}");
                    EditorGUIUtility.PingObject(saved);
                }
                else
                {
                    Debug.LogError($"[MeterGamePrefabGenerator] Save returned null — check Console for serialisation errors");
                }
            }
            finally
            {
                if (openedScene.IsValid()) EditorSceneManager.CloseScene(openedScene, true);
            }
        }

        static GameObject FindMeterGame()
        {
            // GameObject.Find skips inactive — and the runner's MeterGame is
            // disabled until a fight triggers — so walk every Transform in
            // the scene (including inactive).
            foreach (var t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (t.name == MeterGameName) return t.gameObject;
            }
            return null;
        }

        static void EnsureFolder(string assetPath)
        {
            if (AssetDatabase.IsValidFolder(assetPath)) return;

            string parent = System.IO.Path.GetDirectoryName(assetPath).Replace('\\', '/');
            string leaf = System.IO.Path.GetFileName(assetPath);
            if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }
    }
}
