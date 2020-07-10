using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
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
        private bool start;
        public AnimationType type;
        public DecoAnimation(DecoButton button, bool start, AnimationType type) {
            this.button = button;
            this.start = start;
            this.type = type;
        }
        public DecoButton getButton() {
            return button;
        }
        public bool isStart() {
            return start;
        }
        public void setStatus(bool visible) {
            this.start = visible;
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
    private TMP_Text startText, creditsText, exitText, mode_autopilotText, mode_manualpilotText, level_parking, level_crosswalk;

    //Lists of buttons
    private List<DecoButton> buttons = new List<DecoButton>();
    private List<DecoButton> modeButtons = new List<DecoButton>();
    private List<DecoButton> levelButtons = new List<DecoButton>();
    
    //Lists of animations
    private List<DecoAnimation> toAnimate = new List<DecoAnimation>();
    private List<DecoAnimation> backupToAnimate = new List<DecoAnimation>();

    //Menu things
    private int selection = 1;
    private bool canSwitch = true;
    private Tab actualTab = Tab.HOME_MENU;

    //Scrolling animation variables
    private float differencePositionAnimation;
    private float variationPositionAnimation;
    private bool backwardScroll = false;

    private void settingButtons() {
        //Populating buttons
        buttons.Add(new DecoButton("START", startText));
        buttons.Add(new DecoButton("CREDITS", creditsText));
        buttons.Add(new DecoButton("EXIT", exitText));
        modeButtons.Add(new DecoButton("AUTOPILOT", mode_autopilotText));
        modeButtons.Add(new DecoButton("MANUAL", mode_manualpilotText));
        levelButtons.Add(new DecoButton("PARKING", level_parking));
        levelButtons.Add(new DecoButton("CROSSWALK", level_crosswalk));

        //Setting buttons
        foreach (DecoButton b in buttons) b.updateGUI();
        foreach (DecoButton b in modeButtons) {
            b.updateGUI();
            b.changeAlpha(0f);
            b.changeAlphaOfIcon(0f);
        }
        foreach (DecoButton b in levelButtons) {
            b.updateGUI();
            b.changeAlpha(0f);
            b.changeAlphaOfIcon(0f);
        }

        //Setting underline on the first button of each category
        buttons[0].updateGUI(true);
        modeButtons[0].updateGUI(true);
        levelButtons[0].updateGUI(true);
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
        differencePositionAnimation = 90f;
        variationPositionAnimation = 0f;
    }

    // Update is called once per frame
    void Update() {

        //Animate buttons
        try {
            if (toAnimate.Count > 0) {
                if (toAnimate[0].type == AnimationType.FADE)
                    foreach (DecoAnimation anim in toAnimate) {
                        //starting alpha = 0 case
                        if (!anim.isStart()) {
                            anim.getButton().changeAlpha(anim.getButton().getAlpha() + 0.05f);
                            anim.getButton().changeAlphaOfIcon(anim.getButton().getAlphaOfIcon() + 0.05f);
                            if (anim.getButton().getAlpha() >= 1f) {
                                toAnimate.Remove(anim);
                                if (toAnimate.Count == 0) break;
                            }
                        }
                        else {
                            anim.getButton().changeAlpha(anim.getButton().getAlpha() - 0.05f);
                            anim.getButton().changeAlphaOfIcon(anim.getButton().getAlphaOfIcon() - 0.05f);
                            if (anim.getButton().getAlpha() <= 0f) {
                                toAnimate.Remove(anim);
                                if (actualTab == Tab.PLAY_CHOISE) backwardScroll = true;
                                if (toAnimate.Count == 0) break;
                            }
                        }
                    }
                else if (toAnimate[0].type == AnimationType.SCROLLING) {
                    if (toAnimate[0].isStart()) {
                        variationPositionAnimation += 5;
                        toAnimate[0].getButton().getGUI().transform.localPosition =
                            new Vector3(
                                    toAnimate[0].getButton().getGUI().transform.localPosition.x,
                                    toAnimate[0].getButton().getGUI().transform.localPosition.y + 3,
                                    toAnimate[0].getButton().getGUI().transform.localPosition.z
                                );
                        if (differencePositionAnimation <= variationPositionAnimation) {
                            variationPositionAnimation = 0;
                            toAnimate.Remove(toAnimate[0]);
                            /*
                                Displaying here the levels 
                            */
                            toAnimate.Add(new DecoAnimation(levelButtons[0], false, AnimationType.FADE));
                            toAnimate.Add(new DecoAnimation(levelButtons[1], false, AnimationType.FADE));
                        }
                    }
                    else {
                        variationPositionAnimation -= 5;
                        toAnimate[0].getButton().getGUI().transform.localPosition =
                            new Vector3(
                                    toAnimate[0].getButton().getGUI().transform.localPosition.x,
                                    toAnimate[0].getButton().getGUI().transform.localPosition.y - 3,
                                    toAnimate[0].getButton().getGUI().transform.localPosition.z
                                );
                        if (variationPositionAnimation <= 0) {
                            variationPositionAnimation = 0;
                            toAnimate.Remove(toAnimate[0]);
                        }
                    }
                }
            }
        } catch (InvalidOperationException e) {
            /*
                CATCH SECTION HERE
             */
            Debug.LogWarning("Catched InvalidOperationException");
            if (toAnimate.Count > 0) toAnimate.Clear();
            if (backupToAnimate.Count > 0 && backupToAnimate[0].type == AnimationType.FADE)
                foreach (DecoAnimation anim in backupToAnimate)
                    if (anim.getButton().getAlpha() > 0.9f) {
                        anim.getButton().changeAlpha(1f);
                        anim.getButton().changeAlphaOfIcon(1f);
                    }
                    else if (anim.getButton().getAlpha() < 0.1f) {
                        anim.getButton().changeAlpha(0f);
                        anim.getButton().changeAlphaOfIcon(0f);
                    }
            backupToAnimate.Clear();
            if (backwardScroll) {
                toAnimate.Add(new DecoAnimation(modeButtons[0], false, AnimationType.SCROLLING));
                variationPositionAnimation = differencePositionAnimation;
                backwardScroll = false;
            }
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
                    canSwitch = false;
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
                 * 
                 * 
                        ADD HERE OTHER SELECTIONS 
                 *
                 * 
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
                if (selection == 1) {
                    //Moving first button
                    actualTab = Tab.LEVEL_CHOISE;
                    canSwitch = false;
                    transform.Translate(new Vector3(0, 10, 0) * Time.deltaTime);
                    toAnimate.Add(new DecoAnimation(modeButtons[0], true, AnimationType.SCROLLING));
                    selection = 1;
                    levelButtons[0].updateGUI(true);
                    levelButtons[1].updateGUI(false);
                }
                else if (selection == 2) SceneManager.LoadScene("UserTrack", LoadSceneMode.Single);
            }
        if (actualTab == Tab.LEVEL_CHOISE && canSwitch)
            if (Input.GetKeyUp(KeyCode.DownArrow)) {
                levelButtons[selection - 1].updateGUI(false);
                selection = changeSelectionCounter(selection, levelButtons.Count, true);
                levelButtons[selection - 1].updateGUI(true);
            }
            else if (Input.GetKeyUp(KeyCode.UpArrow)) {
                levelButtons[selection - 1].updateGUI(false);
                selection = changeSelectionCounter(selection, levelButtons.Count, false);
                levelButtons[selection - 1].updateGUI(true);
            }
            else if (Input.GetKeyUp(KeyCode.RightArrow)) {
                if (selection == 1) SceneManager.LoadScene("ParkingScene", LoadSceneMode.Single);
                else if (selection == 2) SceneManager.LoadScene("CrosswalkScene", LoadSceneMode.Single);
            }
            else if (Input.GetKeyUp(KeyCode.LeftArrow)) {
                actualTab = Tab.PLAY_CHOISE;
                foreach (DecoButton b in levelButtons) {
                    b.changeAlpha(1f);
                    b.changeAlphaOfIcon(1f);
                    toAnimate.Add(new DecoAnimation(b, true, AnimationType.FADE));
                    backupToAnimate.Add(new DecoAnimation(b, true, AnimationType.FADE));
                    selection = 1;
                }
            }

    }

}
