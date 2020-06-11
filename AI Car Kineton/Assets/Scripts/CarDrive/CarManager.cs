using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents.Policies;
using Unity.MLAgents;

public class CarManager : MonoBehaviour {

    [Range(1, 2)]
    [Tooltip("ALERT: YOU HAVE TO SET CAR BEHAVIOUR SCRIPT AND AGENT SCRIPT TO USE THIS MANAGER" + "\r\n" + "Set 1 to use the manual car behaviour | " +
                 "set 2 to use the AI car behaviour (agent)")]
    public int choose = 1;

    // Start is called before the first frame update
    void Start() {
        if (choose == 1) {
            //Removing agent on the object
            GetComponent<CarAgent>().enabled = false;
            GetComponent<BehaviorParameters>().enabled = false;
            GetComponent<DecisionRequester>().enabled = false;
        } else if (choose == 2) {
            GetComponent<CarBehaviour>().enabled = false;
        }
    }
    
}
