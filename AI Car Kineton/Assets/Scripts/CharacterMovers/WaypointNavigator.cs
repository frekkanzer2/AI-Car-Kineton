using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointNavigator : MonoBehaviour
{

    CharacterNavigationController controller;
    public WayPoint currentWaypoint;
    public bool direction = true, startDirection;
    
    public bool street = false;
   
    private void Awake()
    {
        startDirection = direction;
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

    private void OnTriggerEnter(Collider other)
    {
        if (startDirection)
        {
            if (other.gameObject.CompareTag("Walklimit1"))
            {
                if (direction)
                {
                    street = true;
                }
                else street = false;
            }
            else if (other.gameObject.CompareTag("Walklimit2"))
            {
                if (direction)
                {
                    street = false;
                }
                else street = true; 
            }
        } else
        {
            if (other.gameObject.CompareTag("Walklimit1"))
            {
                if (direction)
                {
                    street = false;
                }
                else street = true;
            }
            else if (other.gameObject.CompareTag("Walklimit2"))
            {
                if (direction)
                {
                    street = true;
                }
                else street = false; 
            }
        }
      


    }
}
