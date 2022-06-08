using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;


namespace Aircraft
{
    public class MainMenuController : MonoBehaviour
    {
        [Tooltip("The List of Levels That is used to load")]
        public List<string> levels;

        [Tooltip("DropDown for Level selection")]
        public TMP_Dropdown levelDropDown;
        [Tooltip("DropDown for Difficulty selection")]
        public TMP_Dropdown DifficultyDropDown;

        private string selectedLevel;
        private GameDifficulty selectedDifficulty;


        private void Start()
        {
            Debug.Assert(levels.Count > 0, "No levels Available ");
            levelDropDown.ClearOptions();
            levelDropDown.AddOptions(levels);
            selectedLevel = levels[0];

            DifficultyDropDown.ClearOptions();
            DifficultyDropDown.AddOptions(Enum.GetNames(typeof(GameDifficulty)).ToList());
            selectedDifficulty = GameDifficulty.Normal;
        }

        public void SetLevel(int levelIndex)
        {
            selectedLevel = levels[levelIndex];
        }

        public void SetDifficulty(int difficultyIndex)
        {
            selectedDifficulty = (GameDifficulty)difficultyIndex;
        }

        public void StartButtonClicked()
        {
            // Sets the Difficulty
            GameManager.Instance.GameDifficulty = selectedDifficulty;
            // Starts level in preparing mode
            GameManager.Instance.LoadLevel(selectedLevel, GameState.Preparing);
        }

        public void QuitButtonClicked()
        {
            Application.Quit();
        }
    }
}

