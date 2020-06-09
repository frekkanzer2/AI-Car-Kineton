using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointNavigator : MonoBehaviour
{

    CharacterNavigationController controller;
    public WayPoint currentWaypoint;
    public bool direction = true;

    private void Awake()
    {

        controller = GetComponent<CharacterNavigationController>(); 
       
    }

    private void Start()
    {
        controller.SetDestination(currentWaypoint.getPosition());
        
        
    }
    // Update is called once per frame
    void Update()
    {
        
        if (controller.reachedDestination && direction)
        {

            if (currentWaypoint.nextWaypoint == null)
            {
                direction = false;
                
            }

            else
            {
                currentWaypoint = currentWaypoint.nextWaypoint;
                controller.SetDestination(currentWaypoint.getPosition());
            }
        } 
        else if (controller.reachedDestination && !direction)
            {

            if (currentWaypoint.previousWaypoint == null)
            {

                direction = true;
            }
            else
            {
                currentWaypoint = currentWaypoint.previousWaypoint;
                controller.SetDestination(currentWaypoint.getPosition());
            }
        }
    }
}
