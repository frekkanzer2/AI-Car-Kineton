using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine.SceneManagement;

public class CrossWalkCarAgent : CarAgent {
    
    //Pedastrians list
    private ArrayList listOfPedastrians;

    //riskAnalisis
    private float xDist, zDist;
    private bool xCheck, zCheck;

    // brake boolean checks
    private bool manualBrake = false, pedCheck;
 

    //Initialization
    public override void Initialize() {
        base.Initialize();
       
        spawner.Add(new Spawn(new Vector3(-2.32f, -0.07f, -26.8f), Quaternion.identity));
        spawner.Add(new Spawn(new Vector3(-2.32f, -0.07f, -12f), Quaternion.identity));
      
       //Getting RiskPoint

        GameObject parent = transform.parent.gameObject;
        //riskPoint = parent.transform.Find("RiskPoint").gameObject;
          
        
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
        //base.Heuristic(actionsOut);

        //avoid rotation for beginning training session

        actionsOut[0] = Input.GetAxisRaw("Vertical");
        

        if (actionsOut[0] < 0) AddReward(-1000f);

        if (Input.GetKeyDown(KeyCode.Q))
            actionsOut[1] = 1f;
        else actionsOut[1] = 0f;
    }

    public override void OnActionReceived(float[] vectorAction) {
        //base.OnActionReceived(vectorAction);
        
        verticalMovement(vectorAction[0], vectorAction[1]);

      
        if (vectorAction[1] >= 0.5)
        {
            //Debug.Log(vectorAction[1]);
            manualBrake = true;
            brake(1f);
        }
        else manualBrake = false;


        pedCheck = checkPedastrians();


        //mooving controls
        if (vectorAction[0] < 0) AddReward(-100f);
        if (pedCheck && (vectorAction[0] <= 0.001 && vectorAction[0] >= 0))
        {
            AddReward(10f);
            Debug.Log("correct stop");
        } else if (!pedCheck && vectorAction[1] > 0)
        {
            Debug.Log("correct move");
            AddReward(10f);
        }

        if (pedCheck && (vectorAction[0] != 0))
        {
            Debug.Log("uncorrect move");
            AddReward(-100f);
        }

        //braking controls
        if (pedCheck && manualBrake)
        {
            Debug.Log("correct brake");
            AddReward(10f);
        }  if (!pedCheck && manualBrake)
        {
            Debug.Log("uncorrect brake");
            AddReward(-100f);
        }
        
        
        //Debug.Log(GetCumulativeReward());

    }


   
    private void OnCollisionEnter(Collision collision) {
        
        if (collision.gameObject.CompareTag("Environment Object")) {

            AddReward(-0.5f);
            EndEpisode();
        }
        else if (collision.gameObject.CompareTag("Human")) {

            AddReward(-100000f);
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

                    AddReward(-5f);
                    EndEpisode();
                    break;
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other) {
        //Collision code for Crosswalk Scene
        if (other.gameObject.CompareTag("EndGame")) {
            AddReward(0.5f);
            EndEpisode();
        }

        if (other.gameObject.CompareTag("Walklimit1") || other.gameObject.CompareTag("Walklimit2"))
        {
            AddReward(-3f);
            EndEpisode();
        }
        
    }

    private void OnTriggerExit(Collider other) {
       
    }

    private void OnTriggerStay(Collider other) {
        
    }


    private bool checkPedastrians() {

        bool pedOnTrajectory = false;
        foreach (GameObject ped in listOfPedastrians)
         {
             xDist = ped.transform.position.x - transform.position.x;
             zDist = ped.transform.position.z - transform.position.z;
             xCheck = (-1.5 < xDist) && (xDist < 2);
             zCheck = (0 < zDist) && zDist < 10;

            if ((xCheck && zCheck) && !pedOnTrajectory) pedOnTrajectory = true;
             
         }

        return pedOnTrajectory;
    }
}
