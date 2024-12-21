using System.Collections;
using System.Collections.Generic;
using PikePush.Controls;
using UnityEngine;
using UnityEngine.UI;

namespace PikePush.UI {

    public class MessageBox : MonoBehaviour
    {
        [SerializeField]
        private ControlsManager controlsManager;

        public TMPro.TMP_Text TitleLabel;
        public TMPro.TMP_Text MessageLabel;
        public Button SpaceButton;

        void Awake () {
            // TouchControlsDropdown
            // TitleLabel = GameObject.Find("TitleLabel").GetComponent<TMPro.TMP_Text>();
            // MessageLabel = GameObject.Find("MessageLabel").GetComponent<TMPro.TMP_Text>();
            // SpaceButton = GameObject.Find("SpaceButton").GetComponent<Button>();
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public Button Show(string messageTitle, string messageBody)
        {
            Debug.Log($"[MessageBox][Show]messageTitle: {messageTitle}, messageBody: {messageBody}");
            this.TitleLabel.text = messageTitle;
            this.MessageLabel.text = messageBody;
            this.gameObject.SetActive(true);
            return this.SpaceButton;
        }

        public void Close()
        {
            Debug.Log("[MessageBox][Close]");
            this.gameObject.SetActive(false);
        }
    }

}