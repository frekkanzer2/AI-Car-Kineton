using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterNavigationController : MonoBehaviour
{

    public Vector3 destination;
    public bool reachedDestination = false;
    public float MovementSpeed = 1f;
    public float RotationSpeed = 120f;
    public float stopDistance = 2.5f;
    public bool ReachedDestination = false;
    private Animator animator;
    private Vector3 startPosition, startDestination;
    private Quaternion startRotation;

    // Start is called before the first frame update
    void Start()
    {
        startPosition = transform.position;
        startDestination = destination;
        startRotation = transform.rotation;
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
        if(Vector3.Distance(transform.position, destination) > 2.5f)
        {
            Vector3 destinationDirection = destination - transform.position;
            destinationDirection.y = 0;
            float destinationDistance = destinationDirection.magnitude;
            animator.SetBool("move", true);

            if (destinationDistance >= stopDistance)
            {
                reachedDestination = false;
                Quaternion targetRotation = Quaternion.LookRotation(destinationDirection);
                transform.rotation = Quaternion.RotateTowards(transform.rotation.normalized, targetRotation, RotationSpeed * Time.deltaTime);
               

            }
        }  else
        {

          
            reachedDestination = true;
            animator.SetBool("move", false);
        }
    }

    public void SetDestination(Vector3 destination)
    {
        this.destination = destination;
        reachedDestination = false;
    }

    private void OnCollisionEnter(Collision collision)
    {

        if(collision.gameObject.CompareTag("Human") || collision.gameObject.CompareTag("Environment Object"))
        {
            transform.position = startPosition;
            transform.rotation = startRotation;
            destination = startDestination;
        }
           
    }
}
