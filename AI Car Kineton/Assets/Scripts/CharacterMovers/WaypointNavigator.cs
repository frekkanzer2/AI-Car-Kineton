using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointNavigator : MonoBehaviour
{

    CharacterNavigationController controller;
    public WayPoint currentWaypoint, startWaypoint;
    public bool direction = true, startDirection;
   
    /*public bool street = false;*/
   
    private void Awake()
    {
        startWaypoint = currentWaypoint;
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

    private void OnCollisionEnter(Collision collision)
    {

        if (collision.gameObject.CompareTag("Human") || collision.gameObject.CompareTag("Environment Object") || collision.gameObject.CompareTag("Player"))
        {
            respawn();

        }

    }
    public void respawn()
    {
        currentWaypoint = startWaypoint;
        controller.respawn();
    }

    /*private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Walklimit1") || other.gameObject.CompareTag("Walklimit2"))
        {
            if (street) street = false;
            else street = true;
        }
    }*/


}
