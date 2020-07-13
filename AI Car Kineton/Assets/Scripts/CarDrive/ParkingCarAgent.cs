using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine.SceneManagement;

public class ParkingCarAgent : CarAgent {

    private Vector2 latestRecordDistance;
    private bool hasCollidedWithEndGame = false;

    //Initialization
    public override void Initialize() {
        base.Initialize();
        getLocalEndGame(transform.parent);

        spawner.Add(new Spawn(new Vector3(-6.24f, 0.438f, 4.27f), Quaternion.Euler(0, 180, 0)));
        spawner.Add(new Spawn(new Vector3(-7.85f, 0.438f, 4.27f), Quaternion.Euler(0, 180, 0)));
        spawner.Add(new Spawn(new Vector3(-4.16f, 0.438f, 4.27f), Quaternion.Euler(0, 180, 0)));
        spawner.Add(new Spawn(new Vector3(-5.94f, 0.438f, 0.62f), Quaternion.Euler(0, 270, 0)));
        spawner.Add(new Spawn(new Vector3(-5.94f, 0.438f, 0.62f), Quaternion.Euler(0, 90, 0)));
        spawner.Add(new Spawn(new Vector3(-6.87f, 0.438f, 3.41f), Quaternion.Euler(0, 90, 0)));
        
        latestRecordDistance = getDistanceFromObject(connectedEndGame);
    }

    //Update
    private void Update() {
        debug_drawDestination();
        if (getVelocitySpeed() > 4 || getVelocitySpeed() < -4) {
            Debug.Log("High speed!");
            AddReward(-0.5f);
        }
        episodeExecution();
        directionAssignmentSystem(0.01f, true);
        recordNewDistances(0.02f, true);
        AddReward(-0.2f);
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
        else if (collision.gameObject.CompareTag("Human"))
        {
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

                else if (collision.gameObject.CompareTag("Human"))
                {
                    EndEpisode();
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
        } else if (other.gameObject.CompareTag("EndGame") && !hasCollidedWithEndGame) {
            Debug.Log("OnTriggerEnter collided with " + other.gameObject.tag);
            hasCollidedWithEndGame = true;
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
            }/* else if (counter < 4 && getVelocitySpeed() < 0.2f && getVelocitySpeed() > -0.2f) {
                Debug.Log("ERROR - Speed to low without 4 wheels in parking");
                assign = -0.2f;
            }*/
            AddReward(assign);
        }
    }

    protected void resetCar() {
        carBody.isKinematic = true;
        executeSpawn();
        speed = 0;
        // wc_fl, wc_fr, wc_bl, wc_br
        wc_fl.brakeTorque = Mathf.Infinity;
        wc_fr.brakeTorque = Mathf.Infinity;
        wc_bl.brakeTorque = Mathf.Infinity;
        wc_br.brakeTorque = Mathf.Infinity;
        carBody.velocity = Vector3.zero;
        carBody.angularVelocity = Vector3.zero;
        carBody.isKinematic = false;
        episode_actualSeconds = episodeDuration;
        hasCollidedWithEndGame = false;
    }

}
