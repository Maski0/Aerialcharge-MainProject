using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Aircraft
{
    public class GameOverUIController : MonoBehaviour
    {
        public TextMeshProUGUI PlaceText;

        private RaceManager raceManager;

        private void Awake()
        {
            raceManager = FindObjectOfType<RaceManager>();
        }

        private void OnEnable()
        {
            if(GameManager.Instance != null &&
                GameManager.Instance.GameState == GameState.GameOver)
            {
                // Gets place and update once
                string place = raceManager.GetAgentPlace(raceManager.followAgent);
                this.PlaceText.text = place + " Place";
            }
        }

        public void MainMenuButtonClicke()
        {
            GameManager.Instance.LoadLevel("MainMenu", GameState.MainMenu);
        }
    }
}
