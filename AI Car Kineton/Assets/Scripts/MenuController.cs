using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MenuController : MonoBehaviour {

    class DecoButton {
        private string text;
        private TMP_Text guiText;

        public DecoButton(string text, TMP_Text guiText) {
            this.text = text;
            this.guiText = guiText;
        }

        public string getText() {
            return text;
        }

        public TMP_Text getGUI() {
            return guiText;
        }

        public void updateGUI(bool underlined = false) {
            this.guiText.text = text;
            if (underlined) guiText.fontStyle = FontStyles.Underline;
            else guiText.fontStyle &= ~FontStyles.Underline;
        }
    }

    [SerializeField]
    private TMP_Text startText, creditsText, exitText;
    private List<DecoButton> buttons = new List<DecoButton>();

    private int selection_first = 1;

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

    private int changeSelectionCounter(int selectionValue, int maxValue, bool increment) {
        //selectionValue goes from 1 to max
        if (increment) {
            if (selectionValue == maxValue)
                selectionValue = 1;
            else selectionValue += 1;
        } else {
            if (selectionValue == 1)
                selectionValue = maxValue;
            else selectionValue -= 1;
        }
        return selectionValue;
    }

    // Start is called before the first frame update
    void Start() {
        settingButtons();
    }

    // Update is called once per frame
    void Update() {

        if (Input.GetKeyUp(KeyCode.DownArrow)) {
            buttons[selection_first - 1].updateGUI(false);
            selection_first = changeSelectionCounter(selection_first, buttons.Count, true);
            buttons[selection_first - 1].updateGUI(true);
        } else if (Input.GetKeyUp(KeyCode.UpArrow)) {
            buttons[selection_first - 1].updateGUI(false);
            selection_first = changeSelectionCounter(selection_first, buttons.Count, false);
            buttons[selection_first - 1].updateGUI(true);
        }

    }

}
