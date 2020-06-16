using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine.SceneManagement;

public class CrossWalkCarAgent : CarAgent {
    
    //Pedastrians list
    private ArrayList listOfPedastrians;

    //riskPoint
    private GameObject riskPoint;
    
    
    //Initialization
    public override void Initialize() {
        base.Initialize();
       
        spawner.Add(new Spawn(new Vector3(-2.32f, -0.07f, -26.8f), Quaternion.identity));
        spawner.Add(new Spawn(new Vector3(-2.32f, -0.07f, -12f), Quaternion.identity));
      
       //Getting RiskPoint

        GameObject parent = transform.parent.gameObject;
        riskPoint = parent.transform.Find("RiskPoint").gameObject;
          
        
        //Getting Pedastrian List
        listOfPedastrians = new ArrayList();
        GameObject childContainerOfWPs = null;
        foreach (Transform child in transform.parent.gameObject.transform)
            if (child.gameObject.name.Equals("Waypoints container"))
            {
                childContainerOfWPs = child.gameObject;
                break;
            }
        if (childContainerOfWPs != null)
            foreach (Transform wpt in childContainerOfWPs.transform)
                foreach (Transform item in wpt.gameObject.transform)
                    if (item.gameObject.name.Equals("Pedastrian1") || item.gameObject.name.Equals("Pedastrian2") || item.gameObject.name.Equals("Pedastrian3"))
                    {
                        listOfPedastrians.Add(item.gameObject);
                        break;
                    }

           
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
        if (vectorAction[1] < 0) AddReward(-0.1f);
       
    }

    private void OnCollisionEnter(Collision collision) {
        
        if (collision.gameObject.CompareTag("Environment Object")) {

            AddReward(-0.5f);
            EndEpisode();
        }
        /*else if (collision.gameObject.CompareTag("Environment Car")) {

            AddReward(-1f);
            EndEpisode();
        }*/
        else if (collision.gameObject.CompareTag("Human")) {

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
                /*else if (myParent.CompareTag("Environment Car")) {

                    AddReward(-1f);
                    EndEpisode();
                    break;
                }*/
                else if (myParent.CompareTag("Human")) {

                    AddReward(-1f);
                    EndEpisode();
                    break;
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other) {
        //Collision code for Crosswalk Scene
        if (other.gameObject.CompareTag("EndGame")) {
            AddReward(2f);
            EndEpisode();
        }

        if (other.gameObject.CompareTag("Walklimit1") || other.gameObject.CompareTag("Walklimit2"))
        {
            AddReward(-3f);
            EndEpisode();
        }

        if (other.gameObject.CompareTag("RiskZone"))
        {
            foreach (GameObject ped in listOfPedastrians)
                if (ped.GetComponent<WaypointNavigator>().street) pedOnStreet = true;
            if (pedOnStreet)
            {
                    AddReward(-0.5f);
            }


        }

        
    }

    private void OnTriggerExit(Collider other) {
       
    }

    private void OnTriggerStay(Collider other) {
        
    }

}
