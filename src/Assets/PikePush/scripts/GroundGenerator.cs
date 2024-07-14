using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GroundGenerator : MonoBehaviour
{
    [SerializeField]
    public Font GUISkinFont;

    [SerializeField]
    public Material HatMaterial;

    [SerializeField]
    public Shader TorsoShader;

    //
    public Camera mainCamera;

    // ui vars
    Color successColor = Color.green;
    Color failureColor = Color.red;

    GUIStyle gameStyle = new GUIStyle();
    GUIStyle labelStyle = new GUIStyle();
    GUIStyle buttonStyle = new GUIStyle();
    GUIStyle scoreStyle = new GUIStyle();
    public int baseFontSize = 24;
    public Vector2 referenceResolution = new Vector2(1920, 1080);

    // runner game vars
    public Transform startPoint; //Point from where ground tiles will start
    public PlatformTile tilePrefab;
    public float movingSpeed = 12;
    public int tilesToPreSpawn = 15; //How many tiles should be pre-spawned
    public int tilesWithoutObstacles = 3; //How many tiles at the beginning should not have obstacles, good for warm-up
    List<PlatformTile> spawnedTiles = new List<PlatformTile>();
    // int nextTileToActivate = -1;
    // [HideInInspector]

    // session vars
    public bool gameOver = false;
    static bool gameStarted = false;
    float score = 0;

    public static GroundGenerator instance;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;

        // runner game
        Vector3 spawnPosition = startPoint.position;
        int tilesWithNoObstaclesTmp = tilesWithoutObstacles;
        for (int i = 0; i < tilesToPreSpawn; i++)
        {
            spawnPosition -= tilePrefab.startPoint.localPosition;
            PlatformTile spawnedTile = Instantiate(tilePrefab, spawnPosition, Quaternion.identity) as PlatformTile;
            if(tilesWithNoObstaclesTmp > 0)
            {
                spawnedTile.DeactivateAllObstacles();
                tilesWithNoObstaclesTmp--;
            }
            else
            {
                spawnedTile.ActivateRandomObstacle();
            }
            
            spawnPosition = spawnedTile.endPoint.position;
            spawnedTile.transform.SetParent(transform);
            spawnedTiles.Add(spawnedTile);
        }

        // character customization
        this.HatMaterial.color = this.ParseColourFromString(PlayerPrefs.GetString("ColourOne", "93,165,255"));
        GameObject.Find("PT_Male_Peasant_01_upper").GetComponent<Renderer>().sharedMaterial.SetColor("_CLOTH4COLOR", this.ParseColourFromString(PlayerPrefs.GetString("ColourTwo", "145,156,168")));
    }

    // Update is called once per frame
    void Update()
    {
        // Move the object upward in world space x unit/second.
        //Increase speed the higher score we get
        if (!gameOver && gameStarted)
        {
            transform.Translate(-spawnedTiles[0].transform.forward * Time.deltaTime * (movingSpeed + (score/500)), Space.World);
            score += Time.deltaTime * movingSpeed;
        }

        if (mainCamera.WorldToViewportPoint(spawnedTiles[0].endPoint.position).z < 0)
        {
            //Move the tile to the front if it's behind the Camera
            PlatformTile tileTmp = spawnedTiles[0];
            spawnedTiles.RemoveAt(0);
            tileTmp.transform.position = spawnedTiles[spawnedTiles.Count - 1].endPoint.position - tileTmp.startPoint.localPosition;
            tileTmp.ActivateRandomObstacle();
            spawnedTiles.Add(tileTmp);
        }

        // aw shucks
        if (gameOver || !gameStarted)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                this.startGame();
            }
        }
    }

    void startGame() {
        if (gameOver)
        {
            //Restart current scene
            Scene scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.name);
        }
        else
        {
            //Start the game
            gameStarted = true;
        }
    }

    void OnGUI()
    {
        // todo gui style refact
        if (gameStyle.name != "PikePush")
        {
            // Debug.Log("Updating Theme stuff");
            gameStyle.font = GUISkinFont;
            gameStyle.fontSize = AdjustFontSize();
            gameStyle.normal.textColor = failureColor;
            // gameStyle.wordWrap = true;
            // gameStyle.alignment = TextAnchor.MiddleCenter;
            // debugging layout: labelStyle = new GUIStyle(GUI.skin.box);
            labelStyle.normal.textColor = gameStyle.normal.textColor;
            labelStyle.wordWrap = true;
            labelStyle.alignment = TextAnchor.MiddleCenter;
            // labelStyle.fontSize = FontStyle.si
            buttonStyle = new GUIStyle(GUI.skin.button);
            // buttonStyle.normal.textColor = gameStyle.normal.textColor;
            // buttonStyle.wordWrap = gameStyle.wordWrap;
            // buttonStyle.alignment = gameStyle.alignment;
            scoreStyle = new GUIStyle(GUI.skin.label);
            scoreStyle.fontSize = gameStyle.fontSize;
            scoreStyle.normal.textColor = successColor;
            gameStyle.name = "PikePush";

        }
        if (gameOver)
        {
            this.GameOver();
        }
        else
        {
            if (!gameStarted)
            {
                string labelMessage = "Press 'Space' to start!";
                float labelY = 30;
                GUI.Label(
                    Centered(labelMessage, labelY),
                    labelMessage,
                    labelStyle
                );

                string buttonMessage = "Space!";
                int buttonY = 150;
                if (GUI.Button(
                    Centered(buttonMessage, buttonY),
                    buttonMessage,
                    buttonStyle
                ))
                {
                    this.startGame();
                }
            }
        }

        string scoreLabelMessage = $"Score: {(int)score}";
        float scoreLabelY = 5;
        GUI.Label(
            new Rect(5, scoreLabelY, 200, 25),
            scoreLabelMessage,
            scoreStyle
        );
    }

    private void GameOver()
    {
        float label0Y = 20;
        string labelMessage0 = "Game Over!";
        GUIStyle bigTitleStyle = gameStyle;
        bigTitleStyle.fontSize = 36;
        GUI.Label(
            Centered(labelMessage0, label0Y),
            labelMessage0,
            bigTitleStyle
        );
        string labelMessage1 = "Your score is:";
        GUI.Label(
            Centered(labelMessage1, 60),
            labelMessage1,
            labelStyle
        );


        // GUI.skin.label.fontSize = GUI.skin.label.fontSize * 2;
        // GUILayout.Label("HELLO WORLD", GUILayout.Width(300), GUILayout.Height(50));
        string labelMessage2 = $"{(int)score}";
        GUIStyle bigScoreStyle = gameStyle;
        bigScoreStyle.fontSize = 64;
        GUI.Label(
            Centered(labelMessage2, 90),
            labelMessage2,
            bigScoreStyle
        );
        // GUI.skin.label.fontSize = GUI.skin.label.fontSize / 2;


        string buttonMessage = "Press 'Space' to restart";
        int buttonY = 150;
        if (GUI.Button(
            Centered(buttonMessage, buttonY, 0f, 1.2f, 2f),
            buttonMessage,
            buttonStyle
        ))
        {
            this.startGame();
        }
    }

    private Rect Centered(string message, float horizontalMargin = 0f, float verticalMargin = 0f, float horizontalPadding = 2.0f, float verticalPadding = 2.0f) {
        GUIContent content = new GUIContent(message);
        Vector2 size = labelStyle.CalcSize(content);
        float labelWidth = horizontalPadding * size.x;
        float labelHeight = verticalPadding * size.y;
        return new Rect(
            (Screen.width / 2) - (labelWidth / 2),
            horizontalMargin,
            labelWidth,
            labelHeight
        );
    }

    private Color ParseColourFromString(string colourString) {
        Debug.Log("ParseColourFromString: " + colourString);
        byte[] colourValues = Array.ConvertAll(colourString.Split(','), byte.Parse);
        return new Color32(colourValues[0], colourValues[1], colourValues[2], 255);
    }

    int AdjustFontSize()
    {
        // Get the current screen resolution
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        // Calculate the scale factor based on the reference resolution
        float widthScaleFactor = screenWidth / referenceResolution.x;
        float heightScaleFactor = screenHeight / referenceResolution.y;

        // Use the smaller scale factor to maintain aspect ratio
        float scaleFactor = Mathf.Min(widthScaleFactor, heightScaleFactor);

        // Adjust the font size
        int adjustedFontSize = Mathf.RoundToInt(baseFontSize * scaleFactor);

        // Set the new font size
        if (gameStyle.fontSize != adjustedFontSize || GUI.skin.label.fontSize != adjustedFontSize)
        {
            gameStyle.fontSize = adjustedFontSize;
            GUI.skin.label.fontSize = adjustedFontSize;

            // Optionally, you can debug the values
            Debug.Log("Screen Width: " + screenWidth);
            Debug.Log("Screen Height: " + screenHeight);
            Debug.Log("Scale Factor: " + scaleFactor);
            Debug.Log("Adjusted Font Size: " + adjustedFontSize);
        }

        return adjustedFontSize;
    }
}