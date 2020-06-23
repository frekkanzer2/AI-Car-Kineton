using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine.SceneManagement;

public class ParkingCarAgent : CarAgent {

    private Vector2 latestRecordDistance;

    //Initialization
    public override void Initialize() {
        base.Initialize();
        spawner.Add(new Spawn(new Vector3(-7.15f, 0.438f, 3.97f), Quaternion.Euler(0, 180, 0)));
        spawner.Add(new Spawn(new Vector3(-3.29f, 0.438f, 2f), Quaternion.Euler(0, 180, 0)));
        spawner.Add(new Spawn(new Vector3(5.26f, 0.438f, 0.33f), Quaternion.Euler(0, 180, 0)));
        spawner.Add(new Spawn(new Vector3(5.38f, 0.438f, 0.66f), Quaternion.Euler(0, 250, 0)));
        spawner.Add(new Spawn(new Vector3(-3.68f, 0.438f, -3.68f), Quaternion.Euler(0, 90, 0)));
        spawner.Add(new Spawn(new Vector3(-5.74f, 0.438f, 3.17f), Quaternion.Euler(0, 120, 0)));
        latestRecordDistance = getDistanceFromObject(connectedEndGame);
    }

    //Update
    private void Update() {
        debug_drawDestination();
        if (getVelocitySpeed() > 4) {
            Debug.Log("High speed!");
            AddReward(-0.02f);
        }
        episodeExecution();
        directionAssignmentSystem(0.01f, true);
        recordNewDistances(0.02f, true);
    }

    /*
     
        METHODS
         
         */

    private void recordNewDistances(float reward, bool debug = false) {
        Vector2 pickedDistance = getDistanceFromObject(connectedEndGame);
        if (pickedDistance.x < latestRecordDistance.x && pickedDistance.y < latestRecordDistance.y) {
            if (debug) Debug.Log("Car reduced her distance.");
            latestRecordDistance = pickedDistance;
            AddReward(reward);
        }
    }

    private bool checkWheelInsideBox(WheelCollider toCheck, BoxCollider container) {
        if (container.bounds.Contains(toCheck.bounds.min) &&
            container.bounds.Contains(toCheck.bounds.max))
            return true;
        else return false;
    }

    public override void OnEpisodeBegin() {
        base.OnEpisodeBegin();
        latestRecordDistance = getDistanceFromObject(connectedEndGame);
    }

    /*
     
        COLLISIONS CHECKS
         
         */

    private void OnCollisionEnter(Collision collision) {
        Debug.Log("Collided with " + collision.gameObject.tag);
        if (collision.gameObject.CompareTag("Environment Object")) {
            AddReward(-100f);
            EndEpisode();
        }
        else if (collision.gameObject.CompareTag("Environment Car")) {
            AddReward(-100f);
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
                    AddReward(-100f);
                    Debug.Log("Inside collided with " + myParent.tag);
                    EndEpisode();
                    break;
                }
                else if (myParent.CompareTag("Environment Car")) {
                    AddReward(-100f);
                    Debug.Log("Inside collided with " + myParent.tag);
                    EndEpisode();
                    break;
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other) {
        //Collision code for Parking Scene
        if (other.gameObject.CompareTag("OutOfMap")) {
            Debug.Log("OnTriggerEnter collided with " + other.gameObject.tag);
            AddReward(-100f);
            EndEpisode();
        } else if (other.gameObject.CompareTag("EndGame")) {
            Debug.Log("OnTriggerEnter collided with " + other.gameObject.tag);
            AddReward(250f);
        }

    }

    private void OnTriggerStay(Collider other) {
        //Collision code for Parking Scene
        if (other.gameObject.CompareTag("EndGame")) {
            float assign = 0;
            int counter = 0;
            if (checkWheelInsideBox(wc_fl, (BoxCollider)other)) {
                assign += 0.01f;
                counter++;
            }
            if (checkWheelInsideBox(wc_fr, (BoxCollider)other)) {
                assign += 0.01f;
                counter++;
            }
            if (checkWheelInsideBox(wc_bl, (BoxCollider)other)) {
                assign += 0.01f;
                counter++;
            }
            if (checkWheelInsideBox(wc_br, (BoxCollider)other)) {
                assign += 0.01f;
                counter++;
            }
            if (counter == 4) {
                Debug.Log("SUCCESS - All wheels inside");
                assign *= 2;
                if ((getVelocitySpeed() > 0.2f && getVelocitySpeed() < 1) || (getVelocitySpeed() < -0.2f && getVelocitySpeed() > -1)) {
                    //Moving on parking
                    Debug.Log("SUCCESS - Low speed on parking");
                    float extraGain = 1;
                    float actualVSpeed = getVelocitySpeed();
                    if (actualVSpeed < 0) actualVSpeed *= (-1);
                    extraGain -= actualVSpeed;
                    assign += (extraGain / 2);
                } else if (getVelocitySpeed() < 0.2f && getVelocitySpeed() > -0.2f) {
                    //Stopped on parking
                    Debug.Log("SUCCESS - PERFECT PARKING!");
                    assign = 500;
                    AddReward(assign);
                    EndEpisode();
                }
            }
            AddReward(assign);
        }
    }

}
