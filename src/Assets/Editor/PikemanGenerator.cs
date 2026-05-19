using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace PikePush.EditorTools
{
    public static class PikemanGenerator
    {
        const string GameScenePath = "Assets/PikePush/Scenes/Game.unity";
        const string OutputPath = "Assets/PikePush/prefabs/Pikeman.prefab";
        const string FormationName = "Formation";
        const string BeretMarker = "Beret";

        [MenuItem("PikePush/Regenerate Pikeman Prefab from Runner")]
        public static void Regenerate()
        {
            var openedScene = EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Additive);
            try
            {
                var formation = GameObject.Find(FormationName);
                if (formation == null)
                {
                    Debug.LogError($"[PikemanGenerator] Could not find '{FormationName}' in {GameScenePath}");
                    return;
                }

                Transform donor = FindBeretedChild(formation.transform);
                if (donor == null)
                {
                    if (formation.transform.childCount == 0)
                    {
                        Debug.LogError($"[PikemanGenerator] '{FormationName}' has no children to use as a donor");
                        return;
                    }
                    donor = formation.transform.GetChild(0);
                    Debug.LogWarning($"[PikemanGenerator] No bereted soldier found — falling back to first child '{donor.name}'");
                }

                Debug.Log($"[PikemanGenerator] Using donor: {donor.name}");

                var clone = Object.Instantiate(donor.gameObject);
                clone.name = "Pikeman";
                clone.SetActive(true);
                clone.transform.position = Vector3.zero;
                clone.transform.rotation = Quaternion.identity;
                clone.transform.localScale = Vector3.one;

                StripRunnerOnlyComponents(clone);

                // SaveAsPrefabAsset overwrites in place; .meta is preserved so the GUID
                // (and therefore Drill.unity's reference) stays valid.
                var savedPrefab = PrefabUtility.SaveAsPrefabAsset(clone, OutputPath);
                Object.DestroyImmediate(clone);

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                Debug.Log($"[PikemanGenerator] Saved Pikeman prefab to {OutputPath} (GUID preserved if it already existed)");
                EditorGUIUtility.PingObject(savedPrefab);
            }
            finally
            {
                if (openedScene.IsValid()) EditorSceneManager.CloseScene(openedScene, true);
            }
        }

        static Transform FindBeretedChild(Transform parent)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i);
                if (child.GetComponentsInChildren<Transform>(true)
                    .Any(t => t.name.Contains(BeretMarker)))
                {
                    return child;
                }
            }
            return null;
        }

        static void StripRunnerOnlyComponents(GameObject root)
        {
            // The runner Player has scripts/colliders we don't want on a generic pikeman.
            // Only the Player root carries these, not the modular character subtree we cloned —
            // but defensive removal in case the donor was the Player itself.
            foreach (var rb in root.GetComponentsInChildren<Rigidbody>(true))
                Object.DestroyImmediate(rb);

            foreach (var col in root.GetComponents<Collider>())
                Object.DestroyImmediate(col);

            // Strip any IRPlayer / MainGame MonoBehaviours that might have followed.
            foreach (var mb in root.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (mb == null) continue;
                var typeName = mb.GetType().FullName;
                if (typeName != null && (typeName.Contains("IRPlayer") || typeName.Contains("MainGame")))
                    Object.DestroyImmediate(mb);
            }
        }
    }
}
