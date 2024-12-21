using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using PikePush.UI;
using PikePush.Controls;
using PikePush.Utls;


namespace PikePush {

    public class MainGame : MonoBehaviour
    {
        [SerializeField]
        private ControlsManager controlsManager;

        public static MainGame instance;

        // Reference to the message box UI
        public MessageBox messageBox;

        public MeterGame meterGame;

        // Text element to display the score
        public TMPro.TMP_Text ScoreText;

        // Reference to the main camera
        public Camera mainCamera;

        #region ui vars
        [SerializeField]
        public Font GUISkinFont;
        Color successColor = Color.green;

        Color failureColor = Color.red;

        GUIStyle gameStyle = new GUIStyle();

        GUIStyle labelStyle = new GUIStyle();

        GUIStyle buttonStyle = new GUIStyle();

        GUIStyle scoreStyle = new GUIStyle();

        public int baseFontSize = 24;

        public Vector2 referenceResolution = new Vector2(1920, 1080);
        #endregion

        #region tile vars
        public Transform startPoint;

        public PlatformTile tilePrefab;

        public int tilesToPreSpawn = 15;

        public int tilesWithoutObstacles = 3;

        List<PlatformTile> spawnedTiles = new List<PlatformTile>();
        #endregion

        #region player customization
        [SerializeField]
        public Material HatMaterial;


        [SerializeField]
        public Shader TorsoShader;
        #endregion

        #region game state
        public bool gameOver = false;

        public bool gameStarted = false;

        public bool fightActive = false;

        ControlsManager.Controls activeControls;

        private GameObject enemyObject = null;

        float score = 0;
        #endregion

        // Called before the first frame update
        void Awake()
        {
            // Initialize the message box (uncomment and set if needed)
            // messageBox = GameObject.Find("MessageBox").GetComponent<MessageBox>();
        }

        public void startFight(GameObject enemyObject)
        {
            this.fightActive = true;
            this.enemyObject = enemyObject;
        }

        // Start is called at the beginning of the game
        void Start()
        {
            // Set singleton instance
            instance = this;

            // Spawn initial tiles
            Vector3 spawnPosition = startPoint.position;
            int tilesWithNoObstaclesTmp = tilesWithoutObstacles;
            for (int i = 0; i < tilesToPreSpawn; i++)
            {
                // Adjust spawn position
                spawnPosition -= tilePrefab.startPoint.localPosition;

                // Instantiate a new tile
                PlatformTile spawnedTile = Instantiate(tilePrefab, spawnPosition, Quaternion.identity) as PlatformTile;

                // Deactivate or activate obstacles based on the initial count
                if (tilesWithNoObstaclesTmp > 0)
                {
                    spawnedTile.DeactivateAllObstacles();
                    tilesWithNoObstaclesTmp--;
                }
                else
                {
                    spawnedTile.ActivateRandomObstacle();
                }

                // Update spawn position
                spawnPosition = spawnedTile.endPoint.position;

                // Parent tile to this game object
                spawnedTile.transform.SetParent(transform);

                // Add to the list of spawned tiles
                spawnedTiles.Add(spawnedTile);
            }

            // Character customization using saved preferences
            this.HatMaterial.color = this.ParseColourFromString(PlayerPrefs.GetString("ColourOne", "93,165,255"));
            GameObject.Find("PT_Male_Peasant_01_upper")
                .GetComponent<Renderer>().sharedMaterial
                .SetColor("_CLOTH4COLOR", this.ParseColourFromString(PlayerPrefs.GetString("ColourTwo", "145,156,168")));
        }

        // Recycles tiles when they move out of view
        void RecycleTiles()
        {
            // Check if the first tile is out of view
            if (mainCamera.WorldToViewportPoint(spawnedTiles[0].endPoint.position).z < 0)
            {
                // Remove the oldest tile
                PlatformTile tileTmp = spawnedTiles[0];
                spawnedTiles.RemoveAt(0);

                // Reposition the tile at the end of the sequence
                tileTmp.transform.position = spawnedTiles[spawnedTiles.Count - 1].endPoint.position - tileTmp.startPoint.localPosition;

                // Activate a random obstacle
                tileTmp.ActivateRandomObstacle();

                // Add the recycled tile to the end
                spawnedTiles.Add(tileTmp);
            }
        }

        // Update is called once per frame
        public async void Update()
        {
            // Determine if the game is active
            bool gameActive = (gameStarted && !gameOver);

            // get inputs
            this.activeControls = this.controlsManager.InputCheck();

            if (gameActive)
            {
                // Handle logic during an active fight
                if (fightActive)
                {
                    bool fightWon = await meterGame.Show();
                    if (fightWon)
                    {
                        this.fightActive = false;
                        // todo ragdoll
                        this.enemyObject.SetActive(false);
                    }
                    else
                    {
                        this.fightActive = false;
                        this.gameOver = true;
                    }
                }
                else
                {
                    // Log the player's movement speed
                    LogHelper.debug($"[MainGame][Update]IRPlayer.movementSpeed: {IRPlayer.movementSpeed}");

                    // Calculate dynamic movement speed
                    float movementSpeed = IRPlayer.movementSpeed + (score / 500);

                    // Move tiles based on the speed
                    transform.Translate(-spawnedTiles[0].transform.forward * Time.deltaTime * movementSpeed, Space.World);

                    // Update the score
                    score += Time.deltaTime * IRPlayer.movementSpeed;
                    ScoreText.text = $"{(int)score}";

                    // Recycle tiles as needed
                    this.RecycleTiles();
                }
            }
            else
            {
                // Start the game if the player presses Space
                if (this.activeControls.HasFlag(ControlsManager.Controls.Space))
                {
                    messageBox.Close();
                    this.StartGame();
                }
            }

            // Handle quitting the game
            if (this.activeControls.HasFlag(ControlsManager.Controls.Escape))
            {
                LogHelper.debug($"[MainGame][Update]Quitting game");
                LogHelper.debug(this.activeControls);
                this.QuitGame();
            }
        }

        // Starts or restarts the game
        void StartGame()
        {
            // Restart the scene if the game is over
            if (gameOver)
            {
                Scene scene = SceneManager.GetActiveScene();
                SceneManager.LoadScene(scene.buildIndex);
            }
            else
            {
                // Set the game as started
                gameStarted = true;
            }
        }

        // Quits the current game and loads the main menu
        void QuitGame()
        {
            SceneManager.LoadScene("MainMenu");
        }

        // Handles GUI rendering
        void OnGUI()
        {
            // Display game over screen or start prompt
            if (gameOver)
            {
                this.GameOver();
            }
            else if (!gameStarted)
            {
                if (messageBox)
                {
                    if (!messageBox.isActiveAndEnabled)
                    {
                        messageBox.Show(
                            "Begin",
                            "Press 'Space' to start!"
                        );

                    }
                    else
                    {
                        // Start the game if the player presses Space
                        if (this.activeControls.HasFlag(ControlsManager.Controls.Space))
                        {
                            messageBox.Close();
                            this.StartGame();
                        }
                    }
                }
            }
        }

        // Displays the Game Over screen
        private void GameOver()
        {
            if (messageBox)
            {
                if (!messageBox.isActiveAndEnabled)
                {
                    messageBox.Show(
                    "Game Over!",
                    "Press 'Space' to restart!"
                );

                }
                else
                {
                    // Start the game if the player presses Space
                    if (this.activeControls.HasFlag(ControlsManager.Controls.Space))
                    {
                        LogHelper.debug("[GameOver][MessageBox][StartGame]");
                        this.StartGame();
                    }
                }
            }

        }

        // Parses a color from a string format "R,G,B"
        private Color ParseColourFromString(string colourString)
        {
            LogHelper.debug("ParseColourFromString: " + colourString);
            byte[] colourValues = Array.ConvertAll(colourString.Split(','), byte.Parse);
            return new Color32(colourValues[0], colourValues[1], colourValues[2], 255);
        }
    }
}
