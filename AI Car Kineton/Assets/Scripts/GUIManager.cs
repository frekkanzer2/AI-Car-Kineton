using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GUIManager : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
           
            if (SceneManager.GetActiveScene().name == "ParkingScene") SceneManager.LoadScene("Menu");
            if (SceneManager.GetActiveScene().name == "CrosswalkScene") SceneManager.LoadScene("Menu");
        }
    }
}
