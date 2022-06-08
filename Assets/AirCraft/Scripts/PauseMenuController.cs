using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Aircraft
{
    public class PauseMenuController : MonoBehaviour
    {
        void Start()
        {
            GameManager.Instance.OnStateChange += OnstateChange;
        }

        private void OnstateChange()
        {
            if(GameManager.Instance.GameState == GameState.Playing)
            {
                gameObject.SetActive(false);
            }
        }

        public void ResumeButtonClicked()
        {
            GameManager.Instance.GameState = GameState.Playing;
        }

        public void MainMenuButtonClicked()
        {
            GameManager.Instance.LoadLevel("MainMenu", GameState.MainMenu);
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null) GameManager.Instance.OnStateChange -= OnstateChange;
        }
    }

}