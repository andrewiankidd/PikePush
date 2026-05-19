#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PikePush.Menus {

    public class MainMenuManager : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void GameStart() {
            var modeSelect = FindAnyObjectByType<ModeSelectOverlay>();
            if (modeSelect == null) {
                var go = new GameObject("ModeSelectOverlay");
                modeSelect = go.AddComponent<ModeSelectOverlay>();
            }
            modeSelect.Show();
        }

        public void GameCustomize() {
            SceneManager.LoadScene("CustomizeMenu");
        }

        public void GameSettings() {
            SceneManager.LoadScene("SettingsMenu");
        }

        public void GameCredits() {
            var overlay = FindAnyObjectByType<CreditsOverlay>();
            if (overlay == null) {
                var go = new GameObject("CreditsOverlay");
                overlay = go.AddComponent<CreditsOverlay>();
            }
            overlay.Show();
        }

        public void GameQuit() {
            Application.Quit();

            #if UNITY_EDITOR
            EditorApplication.ExitPlaymode();
            #endif
        }
    }
}
