using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Aircraft
{
    public class FreeRomeController : MonoBehaviour
    {
        public AircraftAgent followAgent { get; private set; }
        public Camera ActiverCamera { get; private set; }
        private AircraftPlayer aircraftPlayer;

        private void Awake()
        {
            aircraftPlayer = FindObjectOfType<AircraftPlayer>();
        }
        void Start()
        {
            GameManager.Instance.OnStateChange += OnStateChange;
            followAgent = aircraftPlayer;
        }

        private void OnStateChange()
        {
            if (GameManager.Instance.GameState == GameState.Playing)
            {
                // Start/resume game time, show the HUD, thaw the agents
                //hud.gameObject.SetActive(true);
                aircraftPlayer.ThawAgent();
            }
            else if (GameManager.Instance.GameState == GameState.Paused)
            {
                // Pause the game time, freeze the agents
                //previouslyElapsedTime += Time.time - lastResumeTime;
                aircraftPlayer.FreezAgent();
            }
            else if (GameManager.Instance.GameState == GameState.GameOver)
            {
                // Pause game time, hide the HUD, freeze the agents
                //previouslyElapsedTime += Time.time - lastResumeTime;
                //hud.gameObject.SetActive(false);
                aircraftPlayer.FreezAgent();

                // Show game over screen
                //gameOverUI.gameObject.SetActive(true);
            }
        }
        void Update()
        {

        }


        private void OnDestroy()
        {
            if (GameManager.Instance != null) GameManager.Instance.OnStateChange += OnStateChange;
        }
    }

}
