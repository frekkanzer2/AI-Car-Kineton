using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine.SceneManagement;

public class CrossWalkCarAgent2 : CarAgent
{

    //Pedastrians list
    private ArrayList listOfPedastrians;

    //riskAnalisis
    private float maxZdist;
    private float xDist, zDist;
    private bool xCheck, zCheck;
    private Vector3 toTarget;

    //rotation analisis
    private float rotationAngle;
    // brake boolean checks
    private bool manualBrake = false, pedCheck;

   

    //Initialization
    public override void Initialize()
    {
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

    public override void OnEpisodeBegin()
    {
        base.OnEpisodeBegin();

    }

    public override void CollectObservations(VectorSensor sensor)
    {
        base.CollectObservations(sensor);
        sensor.AddObservation(transform.position);

    }

    public override void Heuristic(float[] actionsOut)
    {
        //base.Heuristic(actionsOut);

        //avoid rotation for beginning training session

        actionsOut[0] = Input.GetAxisRaw("Vertical");


        if (actionsOut[0] < 0) AddReward(-10f);

        if (Input.GetKeyDown(KeyCode.Q))
            actionsOut[1] = 1f;
        else actionsOut[1] = 0f;

        actionsOut[2] = Input.GetAxis("Horizontal");

    }

    public override void OnActionReceived(float[] vectorAction)
    {
        //base.OnActionReceived(vectorAction);

        //moovmemt
        verticalMovement(vectorAction[0], vectorAction[1]);
        //Rotation
        rotation(vectorAction[2]);
        followWheelRotation();
        //brake
        if (vectorAction[1] >= 0.5)
        {
            //Debug.Log(vectorAction[1]);
            manualBrake = true;
            brake(1f);
        }
        else manualBrake = false;

        pedCheck = checkPedastrians();
        // if (pedCheck) Debug.Log(gameObject.name + "Pedastrian on street");


        //mooving controls

        //retro
        /*if (vectorAction[0] < 0 && !pedCheck)
        {
            Debug.Log("Retro");
            AddReward(-0.001f);
        }*/
       

        if (vectorAction[0] > 0 && !pedCheck) AddReward(100f);

        if (pedCheck)
        {
            if (getVelocitySpeed() < 0.4 && getVelocitySpeed() > -0.4)
            {
                Debug.Log("Correct Speed " + (maxZdist * 20f));
                AddReward(maxZdist * 20f);
            }
            else
            {
                AddReward(-10f);
            }
        }
        else if (getVelocitySpeed() < 3) AddReward(-100f);

        if (pedCheck && manualBrake)
        {

            //Debug.Log("Correct Brake");
            //Debug.Log("Uncorrect brake");
            AddReward(10f);
        }

        //Debug.Log(GetCumulativeReward());

        //rotation check
        rotationAngle = debug_Destination();
        if ((vectorAction[2] > 0.1 || vectorAction[2] < 0.1) && pedCheck) AddReward(-1f);
        if (transform.position.y < -0.5) EndEpisode();

       // Debug.Log(GetCumulativeReward());
    }



    private void OnCollisionEnter(Collision collision)
    {

        if (collision.gameObject.CompareTag("Environment Object"))
        {

            AddReward(-0.5f);
            EndEpisode();
        }
        else if (collision.gameObject.CompareTag("Human"))
        {

            AddReward(-1500f);
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
                    Debug.Log(myParent.name);
                    myParent = myParent.transform.parent.gameObject;

                    continue;
                }
                //CASES HERE
                if (myParent.CompareTag("Environment Object"))
                {
                    AddReward(-0.5f);
                    EndEpisode();
                    break;
                }

                else if (myParent.CompareTag("Human"))
                {

                    AddReward(-1500f);
                    EndEpisode();
                    break;
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //Collision code for Crosswalk Scene
        if (other.gameObject.CompareTag("EndGame"))
        {
            AddReward(50f);
            EndEpisode();
        }

        if (other.gameObject.CompareTag("Walklimit1") || other.gameObject.CompareTag("Walklimit2"))
        {
            AddReward(-1500f);
            EndEpisode();
        }

    }


    private bool checkPedastrians()
    {

        maxZdist = 0;
        bool pedOnTrajectory = false;
        foreach (GameObject ped in listOfPedastrians)
        {

            toTarget = (ped.transform.position - transform.position).normalized;

            if (Vector3.Dot(toTarget, transform.forward) > 0)
            {

                xDist = (ped.transform.position.x - transform.position.x) + 2;
                zDist = Mathf.Abs(ped.transform.position.z - transform.position.z);


                xCheck = (0 < xDist) && (xDist < 5.5);
                zCheck = (0 < zDist) && (zDist < 10);


                // if (xCheck && zCheck) Debug.Log("pedastrian " + ped.name + " is in a risk point for " + gameObject.name + " .");

                if ((xCheck && zCheck) && !pedOnTrajectory)
                {
                    if (zDist > maxZdist) maxZdist = zDist;
                    pedOnTrajectory = true;
                }
            }

        }

        return pedOnTrajectory;
    }

    //local debug destination
    private float debug_Destination()
    {
        if (drawRay) Debug.DrawLine(transform.position, connectedEndGame.transform.position, Color.yellow);
        Vector3 point = transform.InverseTransformPoint(connectedEndGame.transform.position);
        float angle = Mathf.Atan2(point.x, point.z) * Mathf.Rad2Deg;
        return angle;
    }

    //Overrided methods, useful for not specializated agents
    private void Update()
    {

        //AddReward(-0.1f);
       /* if (getVelocitySpeed() > 4 || getVelocitySpeed() < -4)
         {
             Debug.Log("High speed!");
             AddReward(-0.00005f);
         }*/

        directionAssignmentSystem(0.01f, true);
    }


    protected void directionAssignmentSystem(float reward, bool debug = false)
    {
        //Getting angle
        float carAngle = getAngleMeasure(connectedEndGame);
        float assignment = 0;
        //Checking by direction
        if (actualDirection == Direction.Acceleration)
        {
            /*
             * OK CASES
             */
            //straight check
            if ((carAngle < 3 && carAngle >= 0 && wc_fl.steerAngle <= 15f && wc_fl.steerAngle >= 0) ||
                (carAngle > -3 && carAngle < 0 && wc_fl.steerAngle >= -15f && wc_fl.steerAngle < 0))
                assignment = reward * 3;
            //Car should steer of max 35*
            //right steer check
            if ((carAngle > 3 && carAngle <= 35 && wc_fl.steerAngle > 3 && wc_fl.steerAngle <= 35) ||
                //left steer check
                (carAngle < -3 && carAngle >= -35 && wc_fl.steerAngle < -3 && wc_fl.steerAngle >= -35))
                assignment = reward * 3;
            //Checking for angle > 35 or < -35
            //right steer check
            if ((carAngle > 35 && wc_fl.steerAngle > 20) ||
                //left steer check
                (carAngle < -35 && wc_fl.steerAngle < -20))
                assignment = reward * 3;
            assignment = reward * 3;
            /*
             * WRONG CASES
             */
            //straight check
            if ((carAngle < 3 && carAngle >= 0 && wc_fl.steerAngle >= -15f && wc_fl.steerAngle <= 0) ||
                (carAngle > -3 && carAngle < 0 && wc_fl.steerAngle <= 15f && wc_fl.steerAngle > 0))
                assignment = reward * (-1);
            //Car should steer of max 35*
            //right steer check
            if ((carAngle > 3 && carAngle <= 35 && wc_fl.steerAngle < -3 && wc_fl.steerAngle >= -35) ||
                //left steer check
                (carAngle < -3 && carAngle >= -35 && wc_fl.steerAngle > 3 && wc_fl.steerAngle <= 35))
                assignment = reward * (-1);
            //Checking for angle > 35 or < -35
            //right steer check
            if ((carAngle > 35 && wc_fl.steerAngle < -20) ||
                //left steer check
                (carAngle < -35 && wc_fl.steerAngle > 20))
                assignment = reward * (-1);
        }
        else if (actualDirection == Direction.Backward)
        {
            /*
             * WRONG CASES
             */
            //straight check
            if ((carAngle < 3 && carAngle >= 0 && wc_fl.steerAngle <= 15f && wc_fl.steerAngle >= 0) ||
                (carAngle > -3 && carAngle < 0 && wc_fl.steerAngle >= -15f && wc_fl.steerAngle < 0))
                assignment = reward * (-1);
            //Car should steer of max 35*
            //right steer check
            if ((carAngle > 3 && carAngle <= 35 && wc_fl.steerAngle > 3 && wc_fl.steerAngle <= 35) ||
                //left steer check
                (carAngle < -3 && carAngle >= -35 && wc_fl.steerAngle < -3 && wc_fl.steerAngle >= -35))
                assignment = reward * (-1);
            //Checking for angle > 35 or < -35
            //right steer check
            if ((carAngle > 35 && wc_fl.steerAngle > 20) ||
                //left steer check
                (carAngle < -35 && wc_fl.steerAngle < -20))
                assignment = reward * (-1);
            /*
             * OK CASES
             */
            //straight check
            if ((carAngle < 3 && carAngle >= 0 && wc_fl.steerAngle >= -15f && wc_fl.steerAngle <= 0) ||
                (carAngle > -3 && carAngle < 0 && wc_fl.steerAngle <= 15f && wc_fl.steerAngle > 0))
                assignment = reward * 3;
            //Car should steer of max 35*
            //right steer check
            if ((carAngle > 3 && carAngle <= 35 && wc_fl.steerAngle < -3 && wc_fl.steerAngle >= -35) ||
                //left steer check
                (carAngle < -3 && carAngle >= -35 && wc_fl.steerAngle > 3 && wc_fl.steerAngle <= 35))
                assignment = reward * 3;
            //Checking for angle > 35 or < -35
            //right steer check
            if ((carAngle > 35 && wc_fl.steerAngle < -20) ||
                //left steer check
                (carAngle < -35 && wc_fl.steerAngle > 20))
                assignment = reward * 3;
        }
        AddReward(assignment);

        if (pedCheck && assignment > 0) AddReward(assignment * 4);
        if (debug)
        {
            if (assignment > 0) Debug.Log("Right direction");
            if (assignment < 0) Debug.Log("Wrong direction");
        }
    }

}