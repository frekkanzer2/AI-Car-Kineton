using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine.SceneManagement;

public class CarAgent : Agent {

    /*
     
        SPAWNER CLASS

         */

    public class Spawn {
        public Vector3 spawnPosition;
        public Quaternion spawnRotation;
        public Spawn(Vector3 position, Quaternion rotation) {
            spawnPosition = position;
            spawnRotation = rotation;
        }
    }

    public bool drawRay = false;
    [Tooltip("Insert here the duration of the episode in seconds.")]
    public int episodeDuration = 1;
    [Tooltip("Insert here the negative reward when the episode reaches the time limit.")]
    public float epDurationNegativeReward = 0f;
    private int episode_actualFrames = 0;

    public float staticSpeed = 5000f;
    public float brakeForce = 100000f;
    protected float speed = 0;
    protected GameObject connectedEndGame;
    protected Rigidbody carBody;
    protected WheelCollider wc_fl, wc_fr, wc_bl, wc_br; // f -> front, l -> left, r -> right
    protected GameObject wheel_front_left, wheel_front_right, wheel_back_left, wheel_back_right;
    protected GameObject steering_wheel;
    

    protected GameObject brakeLight1R, brakeLight1L, brakeLight2R, brakeLight2L;

    protected Direction actualDirection;
    protected bool isBraking = false, pedOnStreet = false;
    protected Vector3 myPosition;
    protected Quaternion myRotation;

    //vars for curve steering fix
    [Tooltip("Default value: 14. You can insert 0 and the system will automatically load 14.")]
    public float SPD_LIMIT_STEERING = 14f;
    protected float SPD_LIMIT_STEERING_MAX = 28.0f;
    protected float friction = 0; //difference to set in rotation for high speed -> x variable


   
    //Automatic cam manager
    [Tooltip("Positive: automatic camera activated | Negative: manual camera" + "\r\n" + "If positive, attach the camera controller in the following field.")]
    public bool autoCam = true;
    public GameObject camControllerObj;
    protected CameraController camController;
    protected Camera frontcam, backcam;
    
    //Agent spawn position
    protected ArrayList spawner;
    [Tooltip("Add here a fixed custom position. Leave empty for the random generator.")]
    public Vector3 customSpawnPosition;
    [Tooltip("Add here a fixed custom rotation. Leave empty for the random generator.")]
    public Quaternion customSpawnRotation;
    
    // Start code in initialization method

    //Enumarator
    protected enum Direction {
        Acceleration,
        Backward,
        Stop
    }

    /*
                AGENT FUNCTIONS
         */

    //Initialization
    public override void Initialize() {
        //Spawner ArrayList initialization
        spawner = new ArrayList();

        //Initialization Parameters
        myPosition = new Vector3(0, 0, 0);
        myRotation = new Quaternion(0, 0, 0, 0);
        carBody = GetComponent<Rigidbody>();

        //Setting episode duration
        episodeDuration *= 60;
        episode_actualFrames = episodeDuration;

        /*recursive Hierarchy assigment for Wheels, Wheelcolliders and lights*/ 
        componentsAssigment(transform);
        getLocalEndGame(transform.parent);

        wc_fl = wheel_front_left.GetComponentInParent<WheelCollider>();
        wc_fr = wheel_front_right.GetComponentInParent<WheelCollider>();
        wc_bl = wheel_back_left.GetComponentInParent<WheelCollider>();
        wc_br = wheel_back_right.GetComponentInParent<WheelCollider>();

        if (SPD_LIMIT_STEERING == 0) SPD_LIMIT_STEERING = 14.0f;
        if (autoCam) {
            camController = camControllerObj.GetComponent<CameraController>();
            frontcam = GameObject.Find("AutoCamera_Front").GetComponent<Camera>();
            backcam = GameObject.Find("AutoCamera_Behind").GetComponent<Camera>();
        }

    }

    public override void OnEpisodeBegin() {
        resetCar();
    }

    public override void CollectObservations(VectorSensor sensor) {
        sensor.AddObservation(transform.position);
        sensor.AddObservation(transform.rotation);
        //saving wheels rotations
        sensor.AddObservation(wheel_front_left.transform.rotation);
        sensor.AddObservation(wheel_front_right.transform.rotation);
        sensor.AddObservation(wheel_back_left.transform.rotation);
        sensor.AddObservation(wheel_back_right.transform.rotation);
    }

    public override void Heuristic(float[] actionsOut) {
        actionsOut[0] = Input.GetAxis("Horizontal");
        actionsOut[1] = Input.GetAxis("Vertical");
        if (Input.GetKey(KeyCode.Q))
            actionsOut[2] = 1f;
        else actionsOut[2] = 0f;
    }

    public override void OnActionReceived(float[] vectorAction) {

        //Acceleration and brake
        verticalMovement(vectorAction[1], vectorAction[2]);

        //Rotation
        rotation(vectorAction[0]);
        followWheelRotation();

        //Automatic camera
        if (autoCam) autocamManager();

    }

    /*
                CUSTOM FUNCTIONS
         */

    protected void verticalMovement(float vertical, float brakeAgent) {

        /*
            Car uses speed to move.
            The v parameter is an acceleration gain on the speed.
         */

        // Getting acceleration
        float v = vertical * staticSpeed;
        if (v > 1000) v = 1000;
        if (v < -1000) v = -1000;
        speed += v;
        // Checking acceleration limit
        if (speed > staticSpeed) speed = staticSpeed;
        if (speed < staticSpeed * (-1)) speed = staticSpeed * (-1);
        // Getting direction
        float temp = 0;
        if (vertical <= 1f && vertical > 0f) temp = 1;
        else if (vertical >= -1 && vertical < 0f) temp = -1;
        else if (vertical == 0) temp = 0;
        switch (temp) {
            case 1:
                actualDirection = Direction.Acceleration;
                break;
            case -1:
                actualDirection = Direction.Backward;
                break;
            case 0:
                actualDirection = Direction.Stop;
                break;
        }
        // Getting localVelocity
        Vector3 localVelocity = transform.InverseTransformDirection(carBody.velocity);
        if (localVelocity.z > 0.1f && actualDirection == Direction.Backward) {
            brake(0.035f);
            isBraking = true;
            speed = 0;
            switchLight(true);
        }
        else if (localVelocity.z < 0.1f * (-1) && actualDirection == Direction.Acceleration) {
            brake(0.035f * 2);
            isBraking = true;
            speed = 0;
            switchLight(true);
        }
        else {
            isBraking = false;
            switchLight(false);
        }
        // Manual brake
        /*
         
            BRAKE PARAMETER CORRECTED HERE
         
         */
        if (brakeAgent >= 0.5f) {
            brake(0.035f);
            switchLight(true);
        }
        else if (!isBraking) {
            wc_bl.brakeTorque = 0;
            wc_br.brakeTorque = 0;
            wc_fl.brakeTorque = 0;
            wc_fr.brakeTorque = 0;
            wc_bl.motorTorque = speed;
            wc_br.motorTorque = speed;
        }
        // Idle
        if (actualDirection == Direction.Stop) brake(0.005f);
        if (localVelocity == Vector3.zero) {
            speed = 0;
            v = 0;
            actualDirection = Direction.Stop;
        }
    }

    protected void rotation(float horizontal) {
        float localRotation = horizontal * 35;
        float startingRotationSpeed = getVelocitySpeed(); //auto speed in velocity
        if (startingRotationSpeed > SPD_LIMIT_STEERING) {
            // auto speed in velocity > 14.0f 
            if (startingRotationSpeed > SPD_LIMIT_STEERING_MAX)
                startingRotationSpeed = SPD_LIMIT_STEERING_MAX;
            // auto speed in velocity will be set at max of 28.0f
            // startingRotationSpeed is a variable -> y
            // 14 < y <= 28
            // SPD_LIMIT_STEERING < y <= SPD_LIMIT_STEERING_MAX
            // at 14 spd correspond 35 rotation
            // at y spd correspond x rotation
            // 14 : 35 = y : x
            friction = 35 * startingRotationSpeed / SPD_LIMIT_STEERING;
            // friction is the rotation difference, so...
            friction -= 35;
        }

        //Reliability checks
        if (friction < 0) friction = 0;
        else if (friction > 0) {
            if (friction > 35) friction = 35;
            friction /= 100;
        }

        wc_fl.steerAngle = localRotation;
        wc_fr.steerAngle = localRotation;

        if (startingRotationSpeed > SPD_LIMIT_STEERING &&
            (localRotation == 35 || localRotation == 35 * (-1))) {
            brake(friction);
            switchLight(true);
        }

    }

    protected Vector2 getDistanceFromObject(GameObject destination) {
        float dest_x, dest_z, from_x, from_z;
        dest_x = destination.transform.position.x;
        dest_z = destination.transform.position.z;
        from_x = transform.position.x;
        from_z = transform.position.z;
        return new Vector2(Mathf.Abs(dest_x - from_x), Mathf.Abs(dest_z - from_z));
    }

    protected void directionAssignmentSystem(float reward, bool debug = false) {
        //Getting angle
        float carAngle = getAngleMeasure(connectedEndGame);
        float assignment = 0;
        //Checking by direction
        if (actualDirection == Direction.Acceleration) {
            /*
             * OK CASES
             */
            //straight check
            if ((carAngle < 3 && carAngle >= 0 && wc_fl.steerAngle <= 15f && wc_fl.steerAngle >= 0) ||
                (carAngle > -3 && carAngle < 0 && wc_fl.steerAngle >= -15f && wc_fl.steerAngle < 0))
                assignment = reward;
            //Car should steer of max 35*
            //right steer check
            if ((carAngle > 3 && carAngle <= 35 && wc_fl.steerAngle > 3 && wc_fl.steerAngle <= 35) ||
                //left steer check
                (carAngle < -3 && carAngle >= -35 && wc_fl.steerAngle < -3 && wc_fl.steerAngle >= -35))
                assignment = reward;
            //Checking for angle > 35 or < -35
            //right steer check
            if ((carAngle > 35 && wc_fl.steerAngle > 20) ||
                //left steer check
                (carAngle < -35 && wc_fl.steerAngle < -20))
                assignment = reward;
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
        else if (actualDirection == Direction.Backward) {
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
                assignment = reward;
            //Car should steer of max 35*
            //right steer check
            if ((carAngle > 3 && carAngle <= 35 && wc_fl.steerAngle < -3 && wc_fl.steerAngle >= -35) ||
                //left steer check
                (carAngle < -3 && carAngle >= -35 && wc_fl.steerAngle > 3 && wc_fl.steerAngle <= 35))
                assignment = reward;
            //Checking for angle > 35 or < -35
            //right steer check
            if ((carAngle > 35 && wc_fl.steerAngle < -20) ||
                //left steer check
                (carAngle < -35 && wc_fl.steerAngle > 20))
                assignment = reward;
        }
        AddReward(assignment);
        if (debug) {
            if (assignment > 0) Debug.Log("Right direction");
            if (assignment < 0) Debug.Log("Wrong direction");
        }
    }

    protected void followWheelRotation() {
        //front right
        wc_fr.GetWorldPose(out myPosition, out myRotation);
        wheel_front_right.transform.position = myPosition;
        wheel_front_right.transform.rotation = myRotation;
        //front left
        wc_fl.GetWorldPose(out myPosition, out myRotation);
        wheel_front_left.transform.position = myPosition;
        wheel_front_left.transform.rotation = myRotation;
        //back right
        wc_br.GetWorldPose(out myPosition, out myRotation);
        wheel_back_right.transform.position = myPosition;
        wheel_back_right.transform.rotation = myRotation;
        //back left
        wc_bl.GetWorldPose(out myPosition, out myRotation);
        wheel_back_left.transform.position = myPosition;
        wheel_back_left.transform.rotation = myRotation;
    }

    protected void brake(float decrement) {
        wc_bl.brakeTorque = brakeForce;
        wc_br.brakeTorque = brakeForce;
        wc_fl.brakeTorque = brakeForce;
        wc_fr.brakeTorque = brakeForce;
        decrementSpeed(1f, decrement);
    }

    protected void autocamManager() {
        float localVelocity = getVelocitySpeed();
        if (localVelocity >= -1) camController.activateCamera(frontcam);
        else camController.activateCamera(backcam);
    }

    protected void resetCar() {
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
        episode_actualFrames = episodeDuration;
    }

    protected void executeSpawn() {
        
        Spawn mySpawn;
        if (Vector3.Distance(customSpawnPosition, Vector3.zero) == 0 &&
            (customSpawnRotation == new Quaternion(0, 0, 0, 0) || customSpawnRotation.Equals(new Quaternion(0, 0, 0, 0)))) {

            mySpawn = (Spawn)spawner[Random.Range(0, spawner.Count)];
            transform.position = mySpawn.spawnPosition + transform.parent.position;
            transform.rotation = mySpawn.spawnRotation;
        } else {
            mySpawn = new Spawn(customSpawnPosition, customSpawnRotation);
            transform.position = mySpawn.spawnPosition + transform.parent.position;
            transform.rotation = Quaternion.Euler(mySpawn.spawnRotation.x, mySpawn.spawnRotation.y, mySpawn.spawnRotation.z);
        }
    }

    //Other functions
    protected float getVelocitySpeed() {
        return transform.InverseTransformDirection(carBody.velocity).z;
    }

    protected void decrementSpeed(float toCheck, float decrement) {
        if ((carBody.velocity.x > toCheck || carBody.velocity.x < toCheck * (-1)) || (carBody.velocity.y > toCheck || carBody.velocity.y < toCheck * (-1))
            || (carBody.velocity.z > toCheck || carBody.velocity.z < toCheck * (-1))) {
            if (carBody.velocity.x > toCheck)
                carBody.velocity = new Vector3(carBody.velocity.x - decrement, carBody.velocity.y, carBody.velocity.z);
            if (carBody.velocity.x < toCheck * (-1))
                carBody.velocity = new Vector3(carBody.velocity.x + decrement, carBody.velocity.y, carBody.velocity.z);
            if (carBody.velocity.y > toCheck)
                carBody.velocity = new Vector3(carBody.velocity.x, carBody.velocity.y - decrement, carBody.velocity.z);
            if (carBody.velocity.y < toCheck * (-1))
                carBody.velocity = new Vector3(carBody.velocity.x, carBody.velocity.y + decrement, carBody.velocity.z);
            if (carBody.velocity.z > toCheck)
                carBody.velocity = new Vector3(carBody.velocity.x, carBody.velocity.y, carBody.velocity.z - decrement);
            if (carBody.velocity.z < toCheck * (-1))
                carBody.velocity = new Vector3(carBody.velocity.x, carBody.velocity.y, carBody.velocity.z + decrement);
        }
    }

    protected void switchLight(bool switchCommand) {
        brakeLight1R.SetActive(switchCommand);
        brakeLight1L.SetActive(switchCommand);
        brakeLight2R.SetActive(switchCommand);
        brakeLight2L.SetActive(switchCommand);
    }

    protected void componentsAssigment(Transform t)
    {
        foreach (Transform child in t)
        {
            componentsAssigment(child);
            GameObject tmp = child.gameObject;
            switch (tmp.name)
            {
         
                case "Wheel_front_left":
                    wheel_front_left = tmp;
                    break;
                case "Wheel_front_right":
                    wheel_front_right = tmp;
                    break;
                case "Wheel_back_left":
                    wheel_back_left = tmp;
                    break;
                case "Wheel_back_right":
                    wheel_back_right = tmp;
                    break;
                case "BrakeLight1_dx":
                    brakeLight1R = tmp;
                    break;
                case "BrakeLight1_sx":
                    brakeLight1L = tmp;
                    break;
                case "BrakeLight2_dx":
                    brakeLight2R = tmp;
                    break;
                case "BrakeLight2_sx":
                    brakeLight2L = tmp;
                    break;
                
            }
        }
    }

    protected void getLocalEndGame(Transform t) {
        foreach (Transform child in t) {
            getLocalEndGame(child);
            GameObject tmp = child.gameObject;
            switch (tmp.name) {

                case "EndGame":
                    connectedEndGame = tmp;
                    break;

            }
        }
    }

    protected int getRewardOnDirection(GameObject toObj) {
        BoxCollider toReach = toObj.GetComponent<BoxCollider>();
        Vector3 direction = (toReach.transform.position - transform.position).normalized;
        return (int) (Vector3.Dot(carBody.velocity.normalized, direction) * 10);
    }

    protected float getAngleMeasure(GameObject destination) {
        Vector3 point = transform.InverseTransformPoint(destination.transform.position);
        return Mathf.Atan2(point.x, point.z) * Mathf.Rad2Deg;
    }

    protected float getAngleMeasure(GameObject origin, GameObject destination) {
        Vector3 point = origin.transform.InverseTransformPoint(destination.transform.position);
        return Mathf.Atan2(point.x, point.z) * Mathf.Rad2Deg;
    }

    // YOU SHOULD CALL IT IN THE UPDATE METHOD
    protected void episodeExecution() {
        episode_actualFrames--;
        if (episode_actualFrames == 0) {
            AddReward(epDurationNegativeReward);
            EndEpisode();
        }
    }

    //Testing method
    protected void debug_localVelocity() {
        Debug.Log(transform.InverseTransformDirection(carBody.velocity));
    }

    protected Vector3 debug_localPosition(){
        return transform.localPosition;
    }

    protected void debug_drawDestination() {
        if (drawRay) {
            Debug.DrawLine(transform.position, connectedEndGame.transform.position, Color.yellow);
        }
    }

    //Overrided methods, useful for not specializated agents
    private void Update() {
        debug_drawDestination();
        directionAssignmentSystem(0.02f);
    }


  
}
