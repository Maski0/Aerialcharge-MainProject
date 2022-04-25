using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Cinemachine;

namespace Aircraft 
{
    public class AircraftArea : MonoBehaviour
    {
        [Tooltip("The path For the Race, Cinemachine component)")]
        public CinemachineSmoothPath racePath;


        [Tooltip("Prefab to use the CheckPoints")]
        public GameObject checkpointPrefab;

        [Tooltip("Prefab to Use for Start & End Checkpoint")]
        public GameObject finishCheckpointPrefab;

        [Tooltip("True for Training mode")]
        public bool trainingMode;

        public List<AircraftAgent> AircraftAgents { get; private set; }
        public List<GameObject> checkPoints { get; private set; }


        private void Awake()
        {
            if(AircraftAgents == null) FindAircraftAgents();
        }

        private void Start()
        {
            if(checkPoints == null) CreatsCheckpoints();
        }

       

        private void FindAircraftAgents()
        {
            //Find all the aircraft agents
            AircraftAgents = transform.GetComponentsInChildren<AircraftAgent>().ToList();
            Debug.Assert(AircraftAgents.Count > 0, "No AircraftsAgents Found");
        }
        private void CreatsCheckpoints()
        {
            Debug.Assert(racePath != null, "Race path is not set");
            checkPoints = new List<GameObject>();
            int numCheckpoints = (int)racePath.MaxUnit(CinemachinePathBase.PositionUnits.PathUnits);
            for (int i = 0; i < numCheckpoints; i++)
            {
                //Initiating Finish line or checkpoints 
                GameObject checkpoint;
                if (i == numCheckpoints - 1) checkpoint = Instantiate<GameObject>(finishCheckpointPrefab);
                else checkpoint = Instantiate<GameObject>(checkpointPrefab);

                //set the parent, pos, rot
                checkpoint.transform.SetParent(racePath.transform);
                checkpoint.transform.localPosition = racePath.m_Waypoints[i].position;
                checkpoint.transform.localRotation = racePath.EvaluateOrientationAtUnit(i, CinemachinePathBase.PositionUnits.PathUnits);

                //Add the checkpoint to list
                checkPoints.Add(checkpoint);
            }
        }

        public void ResetAgentPosition(AircraftAgent agent, bool Randomize = false)
        {


            if (AircraftAgents == null) FindAircraftAgents();
            if (checkPoints == null) CreatsCheckpoints();


            if (Randomize)
            {
                //Pick a new checkpoint at random
                agent.NextCheckPointIndex = Random.Range(0, checkPoints.Count);

            }
            // set Start position to previous checkpoint
            int previousCheckPointIndex = agent.NextCheckPointIndex - 1;
            if (previousCheckPointIndex == -1) previousCheckPointIndex = checkPoints.Count - 1;

            float startPosition = racePath.FromPathNativeUnits(previousCheckPointIndex, CinemachinePathBase.PositionUnits.PathUnits);

            // Convert the position on the race path to a position in 3d space
            Vector3 basePosition = racePath.EvaluatePosition(startPosition);

            // To get the orientation at the position of the path
            Quaternion orientation = racePath.EvaluateOrientation(startPosition);

            //calculate a Horizontal offset so that agents are spread out  ,To always spacing 10 mtrs apart
            Vector3 positionOffset = Vector3.right * (AircraftAgents.IndexOf(agent) - AircraftAgents.Count / 2f) * Random.Range(9f, 10f);

            //set aircraft pos and rot
            agent.transform.position = basePosition + orientation * positionOffset;
            agent.transform.rotation = orientation;
        }
    }

}

