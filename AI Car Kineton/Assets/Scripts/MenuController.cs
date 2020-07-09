using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour {

    class DecoButton {
        private string text;
        private Text guiText;

        public DecoButton(string text, Text guiText) {
            this.text = text;
            this.guiText = guiText;
        }

        public string getText() {
            return text;
        }

        public Text getGUI() {
            return guiText;
        }

        public string getUnderlinedText() {
            return "<u>" + text + "</u>";
        }

        public void updateGUI(bool underlined = false) {
            if (!underlined) this.guiText.text = text;
            else this.guiText.text = getUnderlinedText();
        }
    }

    [SerializeField]
    private Text startText, creditsText, exitText;

    private List<DecoButton> buttons = new List<DecoButton>();

    private void settingButtons() {
        //Populating buttons
        buttons.Add(new DecoButton("START", startText));
        buttons.Add(new DecoButton("CREDITS", creditsText));
        buttons.Add(new DecoButton("EXIT", exitText));
        //Setting buttons
        foreach (DecoButton b in buttons) b.updateGUI();
        //Setting underline on the first button
        buttons[0].updateGUI(true);
    }

    // Start is called before the first frame update
    void Start() {
        settingButtons();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

}
