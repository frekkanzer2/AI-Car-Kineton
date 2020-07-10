using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CameraController : MonoBehaviour {

    private ArrayList cameraList;
    private ArrayList keys;
    public Camera camera1;
    public Camera camera2;
    public Camera camera3;
    public Camera camera4;
    public Camera camera5;
    public Camera camera6;
    public Camera camera7;
    public Camera camera8;
    public Camera camera9;
    public Camera camera0;

    public TMP_Text actualCam;

    // Start is called before the first frame update
    void Start() {
        cameraList = new ArrayList();
        cameraList.Add(camera0);
        cameraList.Add(camera1);
        cameraList.Add(camera2);
        cameraList.Add(camera3);
        cameraList.Add(camera4);
        cameraList.Add(camera5);
        cameraList.Add(camera6);
        cameraList.Add(camera7);
        cameraList.Add(camera8);
        cameraList.Add(camera9);
        keys = new ArrayList();
        keys.Add(KeyCode.Alpha0);
        keys.Add(KeyCode.Alpha1);
        keys.Add(KeyCode.Alpha2);
        keys.Add(KeyCode.Alpha3);
        keys.Add(KeyCode.Alpha4);
        keys.Add(KeyCode.Alpha5);
        keys.Add(KeyCode.Alpha6);
        keys.Add(KeyCode.Alpha7);
        keys.Add(KeyCode.Alpha8);
        keys.Add(KeyCode.Alpha9);

        //Initialize from camera 1
        for (int i = 0; i < 10; i++)
        {
            Camera c = (Camera)cameraList[i];
            if (c == null) continue;
            if (i == 1){
                c.enabled = true;
                actualCam.text = "Actual Camera - 1";
                Debug.Log("Pippo");
            }
            else c.enabled = false;
        }
    }

    // Update is called once per frame
    void Update() {
        foreach (KeyCode key in keys) {
            if (Input.GetKeyDown(key)) {
                Camera check;
                switch (key) {
                    case KeyCode.Alpha0:
                        check = (Camera)cameraList[0];
                        if (check != null)
                            for (int i = 0; i < 10; i++) {
                                Camera c = (Camera)cameraList[i];
                                if (c == null) continue;
                                if (i == 0)
                                {
                                    c.enabled = true;
                                    actualCam.text = "Actual Camera - 0";
                                }
                                else c.enabled = false;
                            }
                        break;
                    case KeyCode.Alpha1:
                        check = (Camera)cameraList[1];
                        if (check != null)
                            for (int i = 0; i < 10; i++) {
                                Camera c = (Camera)cameraList[i];
                                if (c == null) continue;
                                if (i == 1)
                                {
                                    c.enabled = true;
                                    actualCam.text = "Actual Camera - 1";
                                }
                                else c.enabled = false;
                            }
                        break;
                    case KeyCode.Alpha2:
                        check = (Camera)cameraList[2];
                        if (check != null)
                            for (int i = 0; i < 10; i++) {
                               Camera c = (Camera)cameraList[i];
                                if (c == null) continue;
                                if (i == 2)
                                {
                                    c.enabled = true;
                                    actualCam.text = "Actual Camera - 2";
                                }
                                else c.enabled = false;
                            }
                        break;
                    case KeyCode.Alpha3:
                        check = (Camera)cameraList[3];
                        if (check != null)
                            for (int i = 0; i < 10; i++) {
                               Camera c = (Camera)cameraList[i];
                                if (c == null) continue;
                                if (i == 3)
                                {
                                    c.enabled = true;
                                    actualCam.text = "Actual Camera - 3";
                                }
                                else c.enabled = false;
                            }
                        break;
                    case KeyCode.Alpha4:
                        check = (Camera)cameraList[4];
                        if (check != null)
                            for (int i = 0; i < 10; i++) {
                                Camera c = (Camera)cameraList[i];
                                if (c == null) continue;
                                if (i == 4)
                                {
                                    c.enabled = true;
                                    actualCam.text = "Actual Camera - 4";
                                }
                                else c.enabled = false;
                            }
                        break;
                    case KeyCode.Alpha5:
                        check = (Camera)cameraList[5];
                        if (check != null)
                            for (int i = 0; i < 10; i++) {
                                Camera c = (Camera)cameraList[i];
                                if (c == null) continue;
                                if (i == 5)
                                {
                                    c.enabled = true;
                                    actualCam.text = "Actual Camera - 5";
                                }
                                else c.enabled = false;
                            }
                        break;
                    case KeyCode.Alpha6:
                        check = (Camera)cameraList[6];
                        if (check != null)
                            for (int i = 0; i < 10; i++) {
                                Camera c = (Camera)cameraList[i];
                                if (c == null) continue;
                                if (i == 6)
                                {
                                    c.enabled = true;
                                    actualCam.text = "Actual Camera - 6";
                                }
                                else c.enabled = false;
                            }
                        break;
                    case KeyCode.Alpha7:
                        check = (Camera)cameraList[7];
                        if (check != null)
                            for (int i = 0; i < 10; i++) {
                                Camera c = (Camera)cameraList[i];
                                if (c == null) continue;
                                if (i == 7)
                                {
                                    c.enabled = true;
                                    actualCam.text = "Actual Camera - 7";
                                }
                                else c.enabled = false;
                            }
                        break;
                    case KeyCode.Alpha8:
                        check = (Camera)cameraList[8];
                        if (check != null)
                            for (int i = 0; i < 10; i++) {
                                Camera c = (Camera)cameraList[i];
                                if (c == null) continue;
                                if (i == 8)
                                {
                                    c.enabled = true;
                                    actualCam.text = "Actual Camera - 8";
                                }
                                else c.enabled = false;
                            }
                        break;
                    case KeyCode.Alpha9:
                        check = (Camera)cameraList[9];
                        if (check != null)
                            for (int i = 0; i < 10; i++) {
                               Camera c = (Camera)cameraList[i];
                                if (c == null) continue;
                                if (i == 9)
                                {
                                    c.enabled = true;
                                    actualCam.text = "Actual Camera - 9";
                                }
                                else c.enabled = false;
                            }
                        break;
                }
            }
        }
    }

    public void activateCamera(Camera camToActive) {
        for (int i = 0; i < 10; i++) {
            Camera c = (Camera)cameraList[i];
            if (c == null) continue;
            if (c.Equals(camToActive)) c.enabled = true;
            else c.enabled = false;
        }
    }

}
