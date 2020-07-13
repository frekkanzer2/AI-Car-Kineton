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
    private TMP_Text startText, creditsText, exitText, mode_autopilotText, mode_manualpilotText, level_parking, level_crosswalk,
        credits_devBy, credits_name1, credits_name2, credits_githubLink, credits_help;
    [SerializeField]
    private Button button_name1, button_name2, button_githubLink;

    //Lists of buttons
    private List<DecoButton> buttons = new List<DecoButton>();
    private List<DecoButton> modeButtons = new List<DecoButton>();
    private List<DecoButton> levelButtons = new List<DecoButton>();
    private List<DecoButton> creditButtons = new List<DecoButton>();

    //Animation vars
    [SerializeField]
    private Camera homeCamera;
    [SerializeField]
    private GameObject directionalLight;
    private Animator animator_camera, animator_light;

    //Lists of animations
    private List<DecoAnimation> toAnimate = new List<DecoAnimation>();
    private List<DecoAnimation> backupToAnimate = new List<DecoAnimation>();

    //Menu things
    private int selection = 1;
    private bool canSwitch = true;
    private Tab actualTab = Tab.HOME_MENU;
    private bool canClick = false;

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
        creditButtons.Add(new DecoButton("DEVELOPED BY", credits_devBy));
        creditButtons.Add(new DecoButton("FRANCESCO ABATE", credits_name1));
        creditButtons.Add(new DecoButton("CARMINE FERRARA", credits_name2));
        creditButtons.Add(new DecoButton("ABOUT THE PROJECT", credits_githubLink));
        creditButtons.Add(new DecoButton("CLICK WITH THE MOUSE TO CHOOSE", credits_help));

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
        foreach (DecoButton b in creditButtons) {
            b.updateGUI();
            b.changeAlpha(0f);
        }

        //Setting underline on the first button of each category
        buttons[0].updateGUI(true);
        modeButtons[0].updateGUI(true);
        levelButtons[0].updateGUI(true);

        //Setting credit buttons
        button_name1.onClick.AddListener(clickedFirstName);
        button_name2.onClick.AddListener(clickedSecondName);
        button_githubLink.onClick.AddListener(clickedGitHub);
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
        animator_camera = homeCamera.GetComponent<Animator>();
        animator_light = directionalLight.GetComponent<Animator>();
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

        //check if can click links in the credits section
        if (actualTab == Tab.CREDITS)
            canClick = true;
        else canClick = false;

        //check if can switch between texts
        if (toAnimate.Count == 0) canSwitch = true;
        else canSwitch = false;

        //Checking input in tabs
        if (actualTab == Tab.HOME_MENU && canSwitch)
            if (Input.GetKeyUp(KeyCode.DownArrow) || Input.GetKeyUp(KeyCode.S)) {
                buttons[selection - 1].updateGUI(false);
                selection = changeSelectionCounter(selection, buttons.Count, true);
                buttons[selection - 1].updateGUI(true);
            }
            else if (Input.GetKeyUp(KeyCode.UpArrow) || Input.GetKeyUp(KeyCode.W)) {
                buttons[selection - 1].updateGUI(false);
                selection = changeSelectionCounter(selection, buttons.Count, false);
                buttons[selection - 1].updateGUI(true);
            }
            else if (Input.GetKeyUp(KeyCode.RightArrow) || Input.GetKeyUp(KeyCode.D) || Input.GetKeyUp(KeyCode.Space)) {
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
                    }
                    selection = 1;
                    modeButtons[0].updateGUI(true);
                }
                if (selection == 2) {
                    actualTab = Tab.CREDITS;
                    canSwitch = false;
                    animator_camera.SetBool("menuSelection", true);
                    animator_light.SetBool("menuSelection", true);
                    selection = 1;
                    foreach (DecoButton b in creditButtons) {
                        b.changeAlpha(0f);
                        b.changeAlphaOfIcon(0f);
                        toAnimate.Add(new DecoAnimation(b, false, AnimationType.FADE));
                        backupToAnimate.Add(new DecoAnimation(b, false, AnimationType.FADE));
                    }
                }
                if (selection == 3) {
                    Application.Quit();
                }
            }
        if (actualTab == Tab.PLAY_CHOISE && canSwitch)
            if (Input.GetKeyUp(KeyCode.DownArrow) || Input.GetKeyUp(KeyCode.S)) {
                modeButtons[selection - 1].updateGUI(false);
                selection = changeSelectionCounter(selection, modeButtons.Count, true);
                modeButtons[selection - 1].updateGUI(true);
            }
            else if (Input.GetKeyUp(KeyCode.UpArrow) || Input.GetKeyUp(KeyCode.W)) {
                modeButtons[selection - 1].updateGUI(false);
                selection = changeSelectionCounter(selection, modeButtons.Count, false);
                modeButtons[selection - 1].updateGUI(true);
            }
            else if (Input.GetKeyUp(KeyCode.LeftArrow) || Input.GetKeyUp(KeyCode.Escape) || Input.GetKeyUp(KeyCode.A)) {
                actualTab = Tab.HOME_MENU;
                canSwitch = false;
                foreach (DecoButton b in modeButtons) {
                    b.changeAlpha(1f);
                    b.changeAlphaOfIcon(1f);
                    toAnimate.Add(new DecoAnimation(b, true, AnimationType.FADE));
                    backupToAnimate.Add(new DecoAnimation(b, true, AnimationType.FADE));
                    selection = 1;
                }
            }
            else if (Input.GetKeyUp(KeyCode.RightArrow) || Input.GetKeyUp(KeyCode.D) || Input.GetKeyUp(KeyCode.Space)) {
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
                else if (selection == 2) {
                    PlayerPrefs.SetString("SCENE_NAME", "UserTrack");
                    SceneManager.LoadScene("LoadingScene", LoadSceneMode.Single);
                }
            }
        if (actualTab == Tab.LEVEL_CHOISE && canSwitch)
            if (Input.GetKeyUp(KeyCode.DownArrow) || Input.GetKeyUp(KeyCode.S)) {
                levelButtons[selection - 1].updateGUI(false);
                selection = changeSelectionCounter(selection, levelButtons.Count, true);
                levelButtons[selection - 1].updateGUI(true);
            }
            else if (Input.GetKeyUp(KeyCode.UpArrow) || Input.GetKeyUp(KeyCode.W)) {
                levelButtons[selection - 1].updateGUI(false);
                selection = changeSelectionCounter(selection, levelButtons.Count, false);
                levelButtons[selection - 1].updateGUI(true);
            }
            else if (Input.GetKeyUp(KeyCode.RightArrow) || Input.GetKeyUp(KeyCode.D) || Input.GetKeyUp(KeyCode.Space)) {
                if (selection == 1) {
                    PlayerPrefs.SetString("SCENE_NAME", "ParkingScene");
                    SceneManager.LoadScene("LoadingScene", LoadSceneMode.Single);
                }
                else if (selection == 2) {
                    PlayerPrefs.SetString("SCENE_NAME", "CrosswalkScene");
                    SceneManager.LoadScene("LoadingScene", LoadSceneMode.Single);
                }
            }
            else if (Input.GetKeyUp(KeyCode.LeftArrow) || Input.GetKeyUp(KeyCode.Escape) || Input.GetKeyUp(KeyCode.A)) {
                actualTab = Tab.PLAY_CHOISE;
                foreach (DecoButton b in levelButtons) {
                    b.changeAlpha(1f);
                    b.changeAlphaOfIcon(1f);
                    toAnimate.Add(new DecoAnimation(b, true, AnimationType.FADE));
                    backupToAnimate.Add(new DecoAnimation(b, true, AnimationType.FADE));
                }
                selection = 1;
            }
        if (actualTab == Tab.CREDITS && canSwitch) {
            if (Input.GetKeyUp(KeyCode.LeftArrow) || Input.GetKeyUp(KeyCode.Escape) || Input.GetKeyUp(KeyCode.A)) {
                actualTab = Tab.HOME_MENU;
                canSwitch = false;
                foreach (DecoButton b in creditButtons) {
                    b.changeAlpha(1f);
                    b.changeAlphaOfIcon(1f);
                    toAnimate.Add(new DecoAnimation(b, true, AnimationType.FADE));
                    backupToAnimate.Add(new DecoAnimation(b, true, AnimationType.FADE));
                    animator_camera.SetBool("menuSelection", false);
                    animator_light.SetBool("menuSelection", false);
                }
                selection = 2;
            }
        }

    }

    private void clickedFirstName() {
        if (canClick)
            Application.OpenURL("https://www.linkedin.com/in/francescoabateimtech/");
    }

    private void clickedSecondName() {
        if (canClick)
            Application.OpenURL("https://www.linkedin.com/in/carmine-ferrara-67412a167/");
    }

    private void clickedGitHub() {
        if (canClick)
            Application.OpenURL("https://github.com/frekkanzer2/AI-Car-Kineton");
    }

}
