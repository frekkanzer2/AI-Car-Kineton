using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine.SceneManagement;

public class UserCarAgent : CarAgent
{

    private Vector2 latestRecordDistance;
    private bool hasCollidedWithEndGame = false;
    public List<GameObject> parckedAuto;
    public List<GameObject> pedastrians;
    private bool commandIdle = false, coroutineIdle = true;

    public TMP_Text goodParkMessage;
    //Initialization
    public override void Initialize()
    {
        base.Initialize();

        goodParkMessage.gameObject.SetActive(false);

        spawner.Add(new Spawn(new Vector3(-2.07f, 0f, -34f), Quaternion.Euler(0, 0, 0)));
        spawner.Add(new Spawn(new Vector3(-2.07f, 0f, -6f), Quaternion.Euler(0, 0, 0)));
        spawner.Add(new Spawn(new Vector3(21.5f, 0f, -15f), Quaternion.Euler(0, 0, 0)));

        spawner.Add(new Spawn(new Vector3(15.98f, 0f, 25.93f), Quaternion.Euler(0, 90f, 0)));
        spawner.Add(new Spawn(new Vector3(15.98f, 0f, 25.93f), Quaternion.Euler(0, 90f, 0)));
        spawner.Add(new Spawn(new Vector3(30f, 0f, 26.3f), Quaternion.Euler(0, 90f, 0)));
        latestRecordDistance = getDistanceFromObject(connectedEndGame);
    }


    public override void OnEpisodeBegin()
    {
        base.OnEpisodeBegin();

        commandIdle = false;
        GameObject auto = parckedAuto[Random.Range(0, parckedAuto.Count - 1)];
        Vector3 tmpPos = connectedEndGame.transform.position;
        connectedEndGame.transform.position = auto.transform.position;
        auto.transform.position = tmpPos;


        foreach (GameObject pedastrian in pedastrians)
        {
            if (Random.Range(0, 2) == 1) pedastrian.SetActive(true);
            else pedastrian.SetActive(false);
        }

    }

    public override void OnActionReceived(float[] vectorAction)
    {
        if (!commandIdle) base.OnActionReceived(vectorAction);
    }

    //Update
    private void Update()
    {
        debug_drawDestination();
        if (getVelocitySpeed() > 4 || getVelocitySpeed() < -4)
        {
            Debug.Log("High speed!");
            AddReward(-0.5f);
        }
        episodeExecution();
        directionAssignmentSystem(0.01f, true);
        recordNewDistances(0.02f, true);
        AddReward(-0.2f);


        if (checkBoxInsideBox(gameObject.GetComponent<BoxCollider>(), (BoxCollider)connectedEndGame.GetComponent<BoxCollider>()))
        {
            goodParkMessage.gameObject.SetActive(true);
            commandIdle = true;
            brake(1.0f);

            coroutineIdle = true;
            time = 0f;


            Invoke("deactiveMessage", 1.5f);
            EndEpisode();
        }
    }

    /*
     
        METHODS
         
         */

    private void recordNewDistances(float reward, bool debug = false)
    {
        Vector2 pickedDistance = getDistanceFromObject(connectedEndGame);
        if (pickedDistance.x < latestRecordDistance.x && pickedDistance.y < latestRecordDistance.y)
        {
            if (debug) Debug.Log("Car reduced her distance.");
            latestRecordDistance = pickedDistance;
            AddReward(reward);
        }
    }

    private bool checkWheelInsideBox(WheelCollider toCheck, BoxCollider container)
    {
        if (container.bounds.Contains(toCheck.bounds.min) &&
            container.bounds.Contains(toCheck.bounds.max))
            return true;
        else return false;
    }

    private bool checkBoxInsideBox(BoxCollider toCheck, BoxCollider container)
    {
        if (container.bounds.Contains(toCheck.bounds.min) &&
            container.bounds.Contains(toCheck.bounds.max))
            return true;
        else return false;
    }


    /*
     
        COLLISIONS CHECKS
         
         */

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collided with " + collision.gameObject.tag);
        if (collision.gameObject.CompareTag("Environment Object"))
        {
            AddReward(-100f);
            EndEpisode();
        }
        else if (collision.gameObject.CompareTag("Environment Car"))
        {
            AddReward(-100f);
            EndEpisode();
        }
        else if (collision.gameObject.CompareTag("Human"))
        {
            EndEpisode();
        }
        else if (collision.gameObject.CompareTag("Untagged"))
        {
            GameObject myParent;
            myParent = collision.gameObject.transform.parent.gameObject;
            while (true)
            {
                if (myParent == null) break;
                if (myParent.CompareTag("Untagged"))
                {
                    myParent = myParent.transform.parent.gameObject;
                    continue;
                }
                //CASES HERE
                if (myParent.CompareTag("Environment Object"))
                {
                    AddReward(-100f);
                    Debug.Log("Inside collided with " + myParent.tag);
                    EndEpisode();
                    break;
                }
                else if (myParent.CompareTag("Environment Car"))
                {
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

    private void OnTriggerEnter(Collider other)
    {
        //Collision code for Parking Scene
        if (other.gameObject.CompareTag("OutOfMap"))
        {
            Debug.Log("OnTriggerEnter collided with " + other.gameObject.tag);
            AddReward(-100f);
          //  EndEpisode();
        }
        else if (other.gameObject.CompareTag("EndGame") && !hasCollidedWithEndGame)
        {
            Debug.Log("OnTriggerEnter collided with " + other.gameObject.tag);
            hasCollidedWithEndGame = true;
            AddReward(250f);
        }

    }

    private void OnTriggerStay(Collider other)
    {
        //Collision code for Parking Scene
        if (other.gameObject.CompareTag("EndGame"))
        {
            float assign = 0;
            int counter = 0;
            if (checkWheelInsideBox(wc_fl, (BoxCollider)other))
            {
                assign += 0.01f;
                counter++;
            }
            if (checkWheelInsideBox(wc_fr, (BoxCollider)other))
            {
                assign += 0.01f;
                counter++;
            }
            if (checkWheelInsideBox(wc_bl, (BoxCollider)other))
            {
                assign += 0.01f;
                counter++;
            }
            if (checkWheelInsideBox(wc_br, (BoxCollider)other))
            {
                assign += 0.01f;
                counter++;
            }
            if (counter == 4)
            {
                Debug.Log("SUCCESS - All wheels inside");
                assign *= 2;
                if ((getVelocitySpeed() > 0.2f && getVelocitySpeed() < 1) || (getVelocitySpeed() < -0.2f && getVelocitySpeed() > -1))
                {
                    //Moving on parking
                    Debug.Log("SUCCESS - Low speed on parking");
                    float extraGain = 1;
                    float actualVSpeed = getVelocitySpeed();
                    if (actualVSpeed < 0) actualVSpeed *= (-1);
                    extraGain -= actualVSpeed;
                    assign += (extraGain / 2);
                }
                else if (getVelocitySpeed() < 0.2f && getVelocitySpeed() > -0.2f)
                {
                    //Stopped on parking
                    Debug.Log("SUCCESS - PERFECT PARKING!");

                    assign = 500;
                    AddReward(assign);


                }

                
            }

            AddReward(assign);
        }
    }

    protected void resetCar()
    {
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

    private float time = 0f;
    private void deactiveMessage()
    {
        goodParkMessage.gameObject.SetActive(false);
    }
}
