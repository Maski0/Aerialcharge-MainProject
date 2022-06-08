using System;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;


namespace Aircraft
{
    public class AircraftAgent : Agent
    {
        [Header("Movement Parameters")]
        public float thrust = 100000f;
        public float pitchSpeed = 100f;
        public float yawSpeed = 100f;
        public float rollSpeed = 100f;
        public float boostMultipler = 2f;

        [Header("Explosion EFX")]
        [Tooltip("The mesh to dissapear")]
        public GameObject meshObject;

        [Tooltip("Explosion Particle Affect object")]
        public GameObject explosionEffect;

        [Header("Training")]
        [Tooltip("Number of steps to time out after in training")]
        public int stepTimeout = 300;

        public int NextCheckPointIndex { get; set; }

        // Componets to keep track of
        private AircraftArea area;
        private Rigidbody rb;
        private TrailRenderer trail;

        //When the next step Timeout will be during training
        private float nextStepTimeout;

        //whether the aircraft is frozen (intentionaly not flying)
        private bool frozen = false;

        // Controls
        private float pitchChange = 0f;
        private float smoothPitchChange = 0f;
        private float maxPitchAngle = 45f;
        private float yawChange = 0f;
        private float smoothYawChange = 0f;
        private float rollChange = 0f;
        private float smoothRollChange = 0f;
        private float maxRollAngle = 45f;
        private bool boost;

        public override void Initialize()
        {
            area = GetComponentInParent<AircraftArea>();
            rb = GetComponent<Rigidbody>();
            trail = GetComponent<TrailRenderer>();

            MaxStep = area.trainingMode ? 5000 : 0;
        }

        public override void OnEpisodeBegin()
        {
            // Reset the velocity pos and orientation
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            trail.emitting = false;
            area.ResetAgentPosition(this, area.trainingMode);

            if (area.trainingMode) nextStepTimeout = StepCount + stepTimeout;
        }

        

        public override void OnActionReceived(ActionBuffers actions)
        {
            if (frozen) return;

            //Read vallues for pitch and yaw
            pitchChange = actions.DiscreteActions[0]; // UP 
            if (pitchChange == 2) pitchChange = -1f; //Down

            yawChange = actions.DiscreteActions[1]; // Trun Right
            if (yawChange == 2) yawChange = -1f; // Turn left

            // Read value for boost and enable/Disable trail render
            boost = actions.DiscreteActions[2] == 1;
            if (boost && !trail.emitting) trail.Clear();
            trail.emitting = boost;

            ProcessMovement();

            if(area.trainingMode)
            {
                //Small negative reward every step for faster movement
                AddReward(-1f / MaxStep);

                // To make sure we havent run out of time if training
                if(StepCount > nextStepTimeout)
                {
                    AddReward(-.5f);
                    EndEpisode();
                }

                Vector3 localCheckpointDir = vectorToNextCheckpoint();
                if (localCheckpointDir.magnitude < Academy.Instance.EnvironmentParameters.GetWithDefault("checkpoint_radius", 0f))
                {
                    GotCheckpoint();
                }
            }
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            //observe the aircraft velocity (1 vector3 = 3 values)
            sensor.AddObservation(transform.InverseTransformDirection(rb.velocity));

            // Where is the next checkpoint? (1vect3 = 3values)
            sensor.AddObservation(vectorToNextCheckpoint());

            //orientation of the next checkpoint (1vec3 = 3values)
            Vector3 nextCheckpointForward = area.checkPoints[NextCheckPointIndex].transform.forward;
            sensor.AddObservation(transform.InverseTransformDirection(nextCheckpointForward));

            //Total observation = 3+3+3 = 9
        }


        public override void Heuristic(in ActionBuffers actionsOut)
        {
            Debug.LogError("Heuristic was called on " + gameObject.name +
                " Only AircraftPlayer has heuristic");
        }

        public void FreezAgent()
        {
            Debug.Assert(area.trainingMode == false, "Freeze not support in training ");
            frozen = true;
            rb.Sleep();
            trail.emitting = false;
        }

        public void ThawAgent()
        {
            Debug.Assert(area.trainingMode == false, "Thaw not support in training ");
            frozen = false;
            rb.WakeUp();
        }

        private void GotCheckpoint()
        {
            //Next CheckPoint reached
            NextCheckPointIndex = (NextCheckPointIndex + 1) % area.checkPoints.Count;
            if(area.trainingMode)
            {
                AddReward(0.5f);
                nextStepTimeout = StepCount + stepTimeout;
            }
        }

        private Vector3 vectorToNextCheckpoint()
        {
            Vector3 nextCheckpointDir = area.checkPoints[NextCheckPointIndex].transform.position - transform.position;
            Vector3 localCheckpointDir = transform.InverseTransformDirection(nextCheckpointDir);
            return localCheckpointDir;
        }

        private void ProcessMovement()
        {
            //Calculate boost
            float boostModifier = boost ? boostMultipler : 1f;

            //Apply forward thrust
            rb.AddForce(transform.forward * thrust * boostModifier, ForceMode.Force);

            //Get the current Rotation
            Vector3 curRot = transform.rotation.eulerAngles;

            //Calculate the roll angle between -180 and 180
            float rollAngle = curRot.z > 180f ? curRot.z - 360f : curRot.z;
            if(yawChange == 0f)
            {
                //Not turning; smoothlu roll toward center
                rollChange = -rollAngle / maxRollAngle;
            }
            else
            {
                //Turning; roll in opposite direction of turn
                rollChange = -yawChange;
            }

            // Calculate smooth deltas 
            smoothPitchChange = Mathf.MoveTowards(smoothPitchChange, pitchChange, 2f * Time.fixedDeltaTime);
            smoothYawChange = Mathf.MoveTowards(smoothYawChange, yawChange, 2f * Time.fixedDeltaTime);
            smoothRollChange = Mathf.MoveTowards(smoothRollChange, rollChange, 2f * Time.deltaTime);

            // Claculate new pitch, yaw and roll. Clamp pitch and roll.
            float pitch = curRot.x + smoothPitchChange * Time.fixedDeltaTime * pitchSpeed;
            if (pitch > 180f) pitch -= 360f;
            pitch = Mathf.Clamp(pitch, -maxPitchAngle, maxPitchAngle);

            float yaw = curRot.y + smoothYawChange * Time.fixedDeltaTime * yawSpeed;

            float roll = curRot.z + smoothRollChange * Time.fixedDeltaTime * rollSpeed;
            if (roll > 180f) roll -= 360f;
            roll = Mathf.Clamp(roll, -maxRollAngle, maxRollAngle);

            //Set the new rotation
            transform.rotation = Quaternion.Euler(pitch, yaw, roll);
        }

        private void OnTriggerEnter(Collider other)
        {
            if(other.transform.CompareTag("checkpoint") &&
                other.gameObject == area.checkPoints[NextCheckPointIndex])
            {
                GotCheckpoint();
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if(!collision.transform.CompareTag("agent"))
            {
                if(area.trainingMode)
                {
                    AddReward(-1f);
                    EndEpisode();
                    return;
                }
                else
                {
                    StartCoroutine(ExplosionReset());
                }
            }
        }

        private IEnumerator ExplosionReset()
        {
            FreezAgent();

            //Disable  mesh
            meshObject.SetActive(false);
            explosionEffect.SetActive(true);
            yield return new WaitForSeconds(2f);

            //disable explosion, re-enable 
            meshObject.SetActive(true);
            explosionEffect.SetActive(false);

            area.ResetAgentPosition(this);
            yield return new WaitForSeconds(1f);

            ThawAgent();
        }
    }
}



