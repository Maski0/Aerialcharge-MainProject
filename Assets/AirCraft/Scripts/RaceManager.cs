using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Barracuda;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Aircraft
{
    public class RaceManager : MonoBehaviour
    {
        [Tooltip("Number of Laps for the race")]
        public int numLaps = 3;

        [Tooltip("Bonus seconds to give upon reaching checkPoint")]
        public float checkPointBonusTime = 15f;

        [Serializable]
        public struct DifficultyModel
        {
            public GameDifficulty difficulty;
            public NNModel model;
        }

        public List<DifficultyModel> difficultyModels;

        //The Agent Being followed by the camera
        public AircraftAgent followAgent { get; private set; }
        public Camera ActiverCamera { get; private set; }

        private CinemachineVirtualCamera VirtualCamera;
        private CountDownUI countDownUI;
        private PauseMenuController pauseMenu;
        private HUDController hud;
        private GameOverUIController gameOverUI;
        private AircraftArea aircraftArea;
        private AircraftPlayer aircraftPlayer;
        private List<AircraftAgent> sortedAircraftAgents;

        //Pause Timers
        private float lastResumeTime = 0f;
        private float previouslyElapsedTime = 0f;

        private float lastPlaceUpdate = 0f;
        private Dictionary<AircraftAgent, AircraftStatus> aircraftStatues;

        private class AircraftStatus
        {
            public int checkpointIndex = 0;
            public int lap = 0;
            public int place = 0;
            public float timeRemaining = 0f;
        }

        public float RaceTime
        {
            get
            {
                if(GameManager.Instance.GameState == GameState.Playing)
                {
                    return previouslyElapsedTime + Time.time - lastResumeTime;
                }
                else if(GameManager.Instance.GameState == GameState.Paused)
                {
                    return previouslyElapsedTime;
                }
                else
                {
                    return 0;
                }
            }
        }

        //Get the agent's next checkpoint's transform
        public Transform GetAgentNextCheckpoint(AircraftAgent agent)
        {
            return aircraftArea.checkPoints[aircraftStatues[agent].checkpointIndex].transform;
        }
        //Get Agents lap count
        public int GetAgentLap(AircraftAgent agent)
        {
            return aircraftStatues[agent].lap;
        }
        //Gets the race place for an agent (i.e. 1st, 2nd, 3rd, etc)
        public string GetAgentPlace(AircraftAgent agent)
        {
            int place = aircraftStatues[agent].place;
            if (place <= 0)
            {
                return string.Empty;
            }

            if (place >= 11 && place <= 13) return place.ToString() + "th";

            switch (place % 10)
            {
                case 1:
                    return place.ToString() + "st";
                case 2:
                    return place.ToString() + "nd";
                case 3:
                    return place.ToString() + "rd";
                default:
                    return place.ToString() + "th";
            }
        }
        public float GetAgentTime(AircraftAgent agent)
        {
            return aircraftStatues[agent].timeRemaining;
        }

        private void Awake()
        {
            hud = FindObjectOfType<HUDController>();
            countDownUI = FindObjectOfType<CountDownUI>();
            pauseMenu = FindObjectOfType<PauseMenuController>();
            gameOverUI = FindObjectOfType<GameOverUIController>();
            VirtualCamera = FindObjectOfType<CinemachineVirtualCamera>();
            aircraftArea = FindObjectOfType<AircraftArea>();
            ActiverCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        }

        private void Start()
        {
            GameManager.Instance.OnStateChange += OnStateChange;

            followAgent = aircraftArea.AircraftAgents[0];
            foreach (AircraftAgent agent in aircraftArea.AircraftAgents)
            {
                agent.FreezAgent();
                if(agent.GetType() == typeof(AircraftPlayer))
                {
                    followAgent = agent;
                    aircraftPlayer = (AircraftPlayer)agent;
                    aircraftPlayer.pauseInput.performed += PauseInputPerformed;
                }
                else
                {
                    agent.SetModel(GameManager.Instance.GameDifficulty.ToString(), difficultyModels.Find(x => x.difficulty == GameManager.Instance.GameDifficulty).model);
                }
            }
            // To make camera follow and HUD to follow
            Debug.Assert(VirtualCamera != null, "Virtual Camera was not specified");
            VirtualCamera.Follow = followAgent.transform;
            VirtualCamera.LookAt = followAgent.transform;
            hud.FollowAgent = followAgent;

            // Hide UI
            hud.gameObject.SetActive(false);
            pauseMenu.gameObject.SetActive(false);
            countDownUI.gameObject.SetActive(false);
            gameOverUI.gameObject.SetActive(false);

            // Start the race
            StartCoroutine(StartRace());

        }

        private IEnumerator StartRace()
        {
            // Show countdown
            countDownUI.gameObject.SetActive(true);
            yield return countDownUI.StartCountdown();

            // Initialize agent status tracking
            aircraftStatues = new Dictionary<AircraftAgent, AircraftStatus>();
            foreach (AircraftAgent agent in aircraftArea.AircraftAgents)
            {
                AircraftStatus status = new AircraftStatus();
                status.lap = 1;
                status.timeRemaining = checkPointBonusTime;
                aircraftStatues.Add(agent, status);
            }

            // Begin playing
            GameManager.Instance.GameState = GameState.Playing;
        }

        private void PauseInputPerformed(InputAction.CallbackContext obj)
        {
            if(GameManager.Instance.GameState == GameState.Playing)
            {
                GameManager.Instance.GameState = GameState.Paused;
                pauseMenu.gameObject.SetActive(true);
            }
        }

        private void OnStateChange()
        {
            if(GameManager.Instance.GameState == GameState.Playing)
            {
                // Start/resume game time, show the HUD, thaw the agents
                lastResumeTime = Time.time;
                hud.gameObject.SetActive(true);
                foreach (AircraftAgent agent in aircraftArea.AircraftAgents) agent.ThawAgent();
            }
            else if (GameManager.Instance.GameState == GameState.Paused)
            {
                // Pause the game time, freeze the agents
                previouslyElapsedTime += Time.time - lastResumeTime;
                foreach (AircraftAgent agent in aircraftArea.AircraftAgents) agent.FreezAgent();
            }
            else if (GameManager.Instance.GameState == GameState.GameOver)
            {
                // Pause game time, hide the HUD, freeze the agents
                previouslyElapsedTime += Time.time - lastResumeTime;
                hud.gameObject.SetActive(false);
                foreach (AircraftAgent agent in aircraftArea.AircraftAgents) agent.FreezAgent();

                // Show game over screen
                gameOverUI.gameObject.SetActive(true);
            }
            else
            {
                // Reset time
                lastResumeTime = 0f;
                previouslyElapsedTime = 0f;
            }
        }
        private void FixedUpdate()
        {
            if(GameManager.Instance.GameState == GameState.Playing)
            {
                //Updates the place list every half second
                if(lastPlaceUpdate + .5f < Time.fixedTime)
                {
                    lastPlaceUpdate = Time.fixedTime;
                    if(sortedAircraftAgents == null)
                    {
                        // getting the copy of agents
                        sortedAircraftAgents = new List<AircraftAgent>(aircraftArea.AircraftAgents);
                    }
                    //Recalculate race places
                    sortedAircraftAgents.Sort((a, b) => PlaceComapere(a, b));
                    for(int i = 0; i < sortedAircraftAgents.Count; i++)
                    {
                        aircraftStatues[sortedAircraftAgents[i]].place = i + 1;
                    }
                }

                foreach(AircraftAgent agent in aircraftArea.AircraftAgents)
                {
                    AircraftStatus status = aircraftStatues[agent];

                    //Update Lap of agent
                    if(status.checkpointIndex != agent.NextCheckPointIndex)
                    {
                        status.checkpointIndex = agent.NextCheckPointIndex;
                        status.timeRemaining = checkPointBonusTime;

                        if(status.checkpointIndex == 0)
                        {
                            status.lap++;
                            if(agent == followAgent && status.lap > numLaps)
                            {
                                GameManager.Instance.GameState = GameState.GameOver;
                            }    
                        }
                    }

                    // Update agent time remainig
                    status.timeRemaining = Mathf.Max(0f, status.timeRemaining - Time.fixedDeltaTime);
                    if(status.timeRemaining == 0f)
                    {
                        aircraftArea.ResetAgentPosition(agent);
                        status.timeRemaining = checkPointBonusTime;
                    }
                }
            }
        }

        //Compares the RacePace
        private int PlaceComapere(AircraftAgent a, AircraftAgent b)
        {
            AircraftStatus statusA = aircraftStatues[a];
            AircraftStatus statusB = aircraftStatues[b];
            int checkpointA = statusA.checkpointIndex + (statusA.lap - 1) * aircraftArea.checkPoints.Count;
            int checkpointB = statusB.checkpointIndex + (statusB.lap - 1) * aircraftArea.checkPoints.Count;
            if (checkpointA == checkpointB)
            {
                // Compare distances to the next checkpoint
                Vector3 nextCheckpointPosition = GetAgentNextCheckpoint(a).position;
                int compare = Vector3.Distance(a.transform.position, nextCheckpointPosition)
                    .CompareTo(Vector3.Distance(b.transform.position, nextCheckpointPosition));
                return compare;
            }
            else
            {
                // Compare number of checkpoints hit. The agent with more checkpoints is
                // ahead (lower place), so we flip the compare
                int compare = -1 * checkpointA.CompareTo(checkpointB);
                return compare;
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null) GameManager.Instance.OnStateChange -= OnStateChange;
            if (aircraftPlayer != null) aircraftPlayer.pauseInput.performed -= PauseInputPerformed;
        }
    }
}