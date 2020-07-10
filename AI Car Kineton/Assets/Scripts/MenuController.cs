using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

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
        public void changeAlpha(float alpha) {
            Color temp_c = guiText.color;
            temp_c.a = alpha;
            guiText.color = temp_c;
        }
        public float getAlpha() {
            Color temp_c = guiText.color;
            return temp_c.a;
        }
        public void changeAlphaOfIcon(float alpha) {
            GameObject associatedIcon = guiText.transform.GetChild(0).gameObject;
            Image temp_img = associatedIcon.GetComponent<Image>();
            Color temp_c_img = temp_img.color;
            temp_c_img.a = alpha;
            temp_img.color = temp_c_img;
        }
        public float getAlphaOfIcon() {
            GameObject associatedIcon = guiText.transform.GetChild(0).gameObject;
            Image temp_img = associatedIcon.GetComponent<Image>();
            Color temp_c_img = temp_img.color;
            return temp_c_img.a;
        }
    }

    class DecoAnimation {
        private DecoButton button;
        private bool visible;
        public AnimationType type;
        public DecoAnimation(DecoButton button, bool visible, AnimationType type) {
            this.button = button;
            this.visible = visible;
            this.type = type;
        }
        public DecoButton getButton() {
            return button;
        }
        public bool isVisible() {
            return visible;
        }
        public void setVisible(bool visible) {
            this.visible = visible;
        }
    }

    enum Tab {
        HOME_MENU,
        PLAY_CHOISE,
        LEVEL_CHOISE,
        CREDITS
    }

    enum AnimationType {
        FADE,
        SCROLLING
    }

    [SerializeField]
    private TMP_Text startText, creditsText, exitText, mode_autopilotText, mode_manualpilotText;
    private List<DecoButton> buttons = new List<DecoButton>();
    private List<DecoButton> modeButtons = new List<DecoButton>();
    
    private List<DecoAnimation> toAnimate = new List<DecoAnimation>();
    private List<DecoAnimation> backupToAnimate = new List<DecoAnimation>();
    private int selection = 1;
    private bool canSwitch = true;
    private Tab actualTab = Tab.HOME_MENU;

    private void settingButtons() {
        //Populating buttons
        buttons.Add(new DecoButton("START", startText));
        buttons.Add(new DecoButton("CREDITS", creditsText));
        buttons.Add(new DecoButton("EXIT", exitText));
        modeButtons.Add(new DecoButton("AUTOPILOT", mode_autopilotText));
        modeButtons.Add(new DecoButton("MANUAL", mode_manualpilotText));

        //Setting buttons
        foreach (DecoButton b in buttons) b.updateGUI();
        foreach (DecoButton b in modeButtons) {
            b.updateGUI();
            b.changeAlpha(0f);
            b.changeAlphaOfIcon(0f);
        }

        //Setting underline on the first button of each category
        buttons[0].updateGUI(true);
        modeButtons[0].updateGUI(true);
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

        //Animate buttons
        try {
            if (toAnimate.Count > 0)
                if (toAnimate[0].type == AnimationType.FADE)
                    foreach (DecoAnimation anim in toAnimate) {
                        //starting alpha = 0 case
                        if (!anim.isVisible()) {
                            anim.getButton().changeAlpha(anim.getButton().getAlpha() + 0.02f);
                            anim.getButton().changeAlphaOfIcon(anim.getButton().getAlphaOfIcon() + 0.02f);
                            if (anim.getButton().getAlpha() >= 1f) {
                                toAnimate.Remove(anim);
                                if (toAnimate.Count == 0) break;
                            }
                        }
                        else {
                            anim.getButton().changeAlpha(anim.getButton().getAlpha() - 0.02f);
                            anim.getButton().changeAlphaOfIcon(anim.getButton().getAlphaOfIcon() - 0.02f);
                            if (anim.getButton().getAlpha() <= 0f) {
                                toAnimate.Remove(anim);
                                if (toAnimate.Count == 0) break;
                            }
                        }
                    }
        } catch (InvalidOperationException e) {
            Debug.LogWarning("Catched InvalidOperationException" + Environment.NewLine +
                "Flushing toAnimate list | Restoring elements");
            if (toAnimate.Count > 0) toAnimate.Clear();
            if (backupToAnimate[0].type == AnimationType.FADE)
                foreach (DecoAnimation anim in backupToAnimate)
                    if (anim.getButton().getAlpha() > 0.9f)
                        anim.getButton().changeAlpha(1f);
                    else if (anim.getButton().getAlpha() < 0.1f)
                        anim.getButton().changeAlpha(0f);
            backupToAnimate.Clear();
        }

        if (toAnimate.Count == 0) canSwitch = true;
        else canSwitch = false;

        //Checking input in tabs
        if (actualTab == Tab.HOME_MENU && canSwitch)
            if (Input.GetKeyUp(KeyCode.DownArrow)) {
                buttons[selection - 1].updateGUI(false);
                selection = changeSelectionCounter(selection, buttons.Count, true);
                buttons[selection - 1].updateGUI(true);
            }
            else if (Input.GetKeyUp(KeyCode.UpArrow)) {
                buttons[selection - 1].updateGUI(false);
                selection = changeSelectionCounter(selection, buttons.Count, false);
                buttons[selection - 1].updateGUI(true);
            }
            else if (Input.GetKeyUp(KeyCode.RightArrow)) {
                if (selection == 1) {
                    //Spawning other buttons (tab play_choise)
                    actualTab = Tab.PLAY_CHOISE;
                    foreach (DecoButton b in modeButtons) {
                        b.changeAlpha(0f);
                        b.changeAlphaOfIcon(0f);
                        b.updateGUI();
                        toAnimate.Add(new DecoAnimation(b, false, AnimationType.FADE));
                        backupToAnimate.Add(new DecoAnimation(b, false, AnimationType.FADE));
                        selection = 1;
                    }
                    modeButtons[0].updateGUI(true);
                }
                /*
                    ADD HERE OTHER SELECTIONS 
                */
                if (selection == 3) {
                    Application.Quit();
                }
            }
        if (actualTab == Tab.PLAY_CHOISE && canSwitch)
            if (Input.GetKeyUp(KeyCode.DownArrow)) {
                modeButtons[selection - 1].updateGUI(false);
                selection = changeSelectionCounter(selection, modeButtons.Count, true);
                modeButtons[selection - 1].updateGUI(true);
            }
            else if (Input.GetKeyUp(KeyCode.UpArrow)) {
                modeButtons[selection - 1].updateGUI(false);
                selection = changeSelectionCounter(selection, modeButtons.Count, false);
                modeButtons[selection - 1].updateGUI(true);
            }
            else if (Input.GetKeyUp(KeyCode.LeftArrow)) {
                actualTab = Tab.HOME_MENU;
                foreach (DecoButton b in modeButtons) {
                    b.changeAlpha(1f);
                    b.changeAlphaOfIcon(1f);
                    toAnimate.Add(new DecoAnimation(b, true, AnimationType.FADE));
                    backupToAnimate.Add(new DecoAnimation(b, true, AnimationType.FADE));
                    selection = 1;
                }
            }
            else if (Input.GetKeyUp(KeyCode.RightArrow)) {
                
            }

    }

}
