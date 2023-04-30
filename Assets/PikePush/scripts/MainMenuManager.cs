#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;

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
        SceneManager.LoadScene("Game");
    }

    public void GameCustomize() {
        SceneManager.LoadScene("CustomizeMenu");
    }

    public void GameSettings() {
        SceneManager.LoadScene("SettingsMenu");
    }

    public void GameQuit() {
        Application.Quit();

        #if UNITY_EDITOR
        EditorApplication.ExitPlaymode();
        #endif
    }
}
