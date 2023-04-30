#if UNITY_EDITOR
using TMPro;
using UnityEditor;
#endif
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CustomizationMenuManager : MonoBehaviour
{
    TMPro.TMP_InputField nameInput;

    Button flagImageButton;

    Button colourOneButton;
    Button colourTwoButton;
    Button colourThreeButton;

    FlexibleColorPicker fcp;

    // Start is called before the first frame update
    void Start()
    {
        this.nameInput = GameObject.Find("NameInput").GetComponent<TMPro.TMP_InputField>();
        this.nameInput.text = PlayerPrefs.GetString("Name", "Scotland");

        this.flagImageButton = GameObject.Find("FlagImageButton").GetComponent<Button>();

        string imagedata = PlayerPrefs.GetString("FlagImage", Convert.ToBase64String(this.flagImageButton.image.sprite.texture.EncodeToPNG()));
        this.flagImageButton.image.sprite.texture.LoadImage(Convert.FromBase64String(imagedata));

        this.colourOneButton = GameObject.Find("ColourOneButton").GetComponent<Button>();
        this.colourOneButton.image.color = this.ParseColourFromString(PlayerPrefs.GetString("ColourOne", "93,165,255"));

        this.colourTwoButton = GameObject.Find("ColourTwoButton").GetComponent<Button>();
        this.colourTwoButton.image.color = this.ParseColourFromString(PlayerPrefs.GetString("ColourTwo", "145,156,168"));

        this.colourThreeButton = GameObject.Find("ColourThreeButton").GetComponent<Button>();
        this.colourThreeButton.image.color = this.ParseColourFromString(PlayerPrefs.GetString("ColourThree", "206,207,97"));

        this.fcp = GameObject.Find("FlexibleColorPicker").GetComponent<FlexibleColorPicker>();
        this.closeFCP();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void BackMenu() {
        SceneManager.LoadScene("MainMenu");
    }

    public void UpdateNameInput() {
        PlayerPrefs.SetString("Name", this.nameInput.text);
    }

    public void UpdateFlagImage() {
        SceneManager.LoadScene("FlagDraw");
    }

    public void UpdateColourOne() {
        if (!this.fcp.gameObject.activeSelf) {
            this.fcp.onColorChange.RemoveAllListeners();
            this.fcp.gameObject.SetActive(true);
            this.fcp.onColorChange.AddListener((Color colour) => {
                this.colourOneButton.image.color = colour;
                PlayerPrefs.SetString("ColourOne", ParseRGBStringFromColour(colour));
            });
        }
    }
    
    public void UpdateColourTwo() {
        if (!this.fcp.gameObject.activeSelf) {
            this.fcp.onColorChange.RemoveAllListeners();
            this.fcp.gameObject.SetActive(true);
            this.fcp.onColorChange.AddListener((Color colour) => {
                this.colourTwoButton.image.color = colour;
                PlayerPrefs.SetString("ColourTwo", ParseRGBStringFromColour(colour));
            });
        }
    }

    public void UpdateColourThree() {
        if (!this.fcp.gameObject.activeSelf) {
            this.fcp.onColorChange.RemoveAllListeners();
            this.fcp.gameObject.SetActive(true);
            this.fcp.onColorChange.AddListener((Color colour) => {
                this.colourThreeButton.image.color = colour;
                PlayerPrefs.SetString("ColourThree", ParseRGBStringFromColour(colour));
            });
        }  
    }

    private byte[] ParseImageFromString(string textureString) {
        Debug.Log("ParseImageFromString: " + textureString);
        return Convert.FromBase64String(textureString);
    }

    private Color ParseColourFromString(string colourString) {
        Debug.Log("ParseColourFromString: " + colourString);
        byte[] colourValues = Array.ConvertAll(colourString.Split(','), byte.Parse);
        return new Color32(colourValues[0], colourValues[1], colourValues[2], 255);
    }

    private String ParseRGBStringFromColour(Color colour) {
        return $"{Math.Round(colour.r * 255, 0)},{Math.Round(colour.g * 255, 0)},{Math.Round(colour.b * 255, 0)}";
    }

    public void closeFCP() {
        this.fcp.gameObject.SetActive(false);
    }
}
