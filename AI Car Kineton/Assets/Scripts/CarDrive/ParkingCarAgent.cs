using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine.SceneManagement;

public class ParkingCarAgent : CarAgent {

  
    //Initialization
    public override void Initialize() {
        base.Initialize();
        spawner.Add(new Spawn(new Vector3(18.18465f, 0.4371328f, 13.53141f), Quaternion.Euler(0, 270, 1)));
        spawner.Add(new Spawn(new Vector3(-17.45f, 0.491f, 5.19f), Quaternion.identity));
        spawner.Add(new Spawn(new Vector3(-8.73f, 0.491f, 4.71f), Quaternion.Euler(0, 180, 0)));
    }

    public override void OnEpisodeBegin() {
        base.OnEpisodeBegin();
    }

    public override void CollectObservations(VectorSensor sensor) {
        base.CollectObservations(sensor);
    }

    public override void Heuristic(float[] actionsOut) {
        base.Heuristic(actionsOut);
    }

    public override void OnActionReceived(float[] vectorAction) {
        base.OnActionReceived(vectorAction);
    }

    /*
     
        COLLISIONS CHECKS
         
         */

    private void OnCollisionEnter(Collision collision) {
        
        if (collision.gameObject.CompareTag("Environment Object")) {
            AddReward(-0.5f);
            EndEpisode();
        }
        else if (collision.gameObject.CompareTag("Environment Car")) {

            AddReward(-1f);
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
                    EndEpisode();
                    break;
                }
                else if (myParent.CompareTag("Environment Car")) {
                    AddReward(-1f);
                    EndEpisode();
                    break;
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other) {
        //Collision code for Parking Scene
        if (other.gameObject.CompareTag("Checkpoint")) {
            AddReward(1f);
        } else if (other.gameObject.CompareTag("OutOfMap")) {
            AddReward(-3f);
            EndEpisode();
        }
    }

    private void OnTriggerStay(Collider other) {
        //Collision code for Parking Scene
        if (other.bounds.Contains(GetComponent<BoxCollider>().bounds.min)
            && other.bounds.Contains(GetComponent<BoxCollider>().bounds.max)) {
            // Inside the box collider
            AddReward(3f);
            EndEpisode();
        }
    }

}
