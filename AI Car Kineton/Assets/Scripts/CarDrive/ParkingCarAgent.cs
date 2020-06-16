using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine.SceneManagement;

public class ParkingCarAgent : CarAgent {

    private GameObject collidedCheckpoint;
    private bool hasChecked = false;
  
    //Initialization
    public override void Initialize() {
        base.Initialize();
        spawner.Add(new Spawn(new Vector3(18.18465f, 0.4371328f, 13.53141f), Quaternion.Euler(0, 270, 1)));
        spawner.Add(new Spawn(new Vector3(-17.45f, 0.491f, 5.19f), Quaternion.identity));
        spawner.Add(new Spawn(new Vector3(-8.73f, 0.491f, 4.71f), Quaternion.Euler(0, 180, 0)));
    }
    
    /*
     
        COLLISIONS CHECKS
         
         */

    private void OnCollisionEnter(Collision collision) {
        Debug.Log("Collided with " + collision.gameObject.tag);
        if (collision.gameObject.CompareTag("Environment Object")) {
            AddReward(-0.5f);
            EndEpisode();
        }
        else if (collision.gameObject.CompareTag("Environment Car")) {
            AddReward(-2f);
            EndEpisode();
        }
        else if (collision.gameObject.CompareTag("Untagged")) {
            GameObject myParent;
            myParent = collision.gameObject.transform.parent.gameObject;
            while (true) {
                if (myParent == null) break;
                if (myParent.CompareTag("Untagged")) {
                    myParent = myParent.transform.parent.gameObject;
                    continue;
                }
                //CASES HERE
                if (myParent.CompareTag("Environment Object")){
                    AddReward(-0.5f);
                    Debug.Log("Inside collided with " + myParent.tag);
                    EndEpisode();
                    break;
                }
                else if (myParent.CompareTag("Environment Car")) {
                    AddReward(-2f);
                    Debug.Log("Inside collided with " + myParent.tag);
                    EndEpisode();
                    break;
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other) {
        //Collision code for Parking Scene
        if (other.gameObject.CompareTag("Checkpoint")) {
            Debug.Log("OnTriggerEnter collided with " + other.gameObject.tag);
            AddReward(1f);
            collidedCheckpoint = other.gameObject;
            collidedCheckpoint.SetActive(false);
        } else if (other.gameObject.CompareTag("OutOfMap")) {
            Debug.Log("OnTriggerEnter collided with " + other.gameObject.tag);
            AddReward(-3f);
            EndEpisode();
        } else if (other.gameObject.CompareTag("Walklimit1")) {
            Debug.Log("OnTriggerEnter collided with " + other.gameObject.tag);
            AddReward(-0.5f);
            EndEpisode();
        } else if (other.gameObject.CompareTag("EndGame") && !hasChecked) {
            Debug.Log("OnTriggerEnter collided with " + other.gameObject.tag);
            AddReward(3f);
            hasChecked = true;
        }

    }

    private void OnTriggerStay(Collider other) {
        //Collision code for Parking Scene
        if (other.gameObject.CompareTag("EndGame") && other.bounds.Contains(GetComponent<BoxCollider>().bounds.min)
            && other.bounds.Contains(GetComponent<BoxCollider>().bounds.max)) {
            // Inside the box collider
            AddReward(10f);
            Debug.Log("PERFECT PARKING DONE!");
            EndEpisode();
        }
    }

    public override void OnEpisodeBegin() {
        base.OnEpisodeBegin();
        if (collidedCheckpoint != null)
            collidedCheckpoint.SetActive(true);
    }

}
