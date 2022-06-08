using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

namespace Aircraft
{
    public class HUDController : MonoBehaviour
    {
        [Tooltip("place in the race ")]
        public TextMeshProUGUI placeText;

        [Tooltip("time remaining to reach next checkpoint (sec)")]
        public TextMeshProUGUI TimeText;

        [Tooltip("Current Lap")]
        public TextMeshProUGUI LapText;

        [Tooltip("Icon for the next checkpoint")]
        public Image CheckPointIcon;

        [Tooltip("Arrow pointing towards checkpoint")]
        public Image CheckPointArrow;

        [Tooltip("Icon Boundaries or limits")]
        public float indicatorLimit = 0.7f;

        public AircraftAgent FollowAgent { get; internal set; }

        RaceManager raceManager;

        private void Awake()
        {
            raceManager = FindObjectOfType<RaceManager>();
        }

        private void Update()
        {
            if(FollowAgent != null)
            {
                UpdatePlaceText();
                UpdateTimeText();
                UpdateLapText();
                UpdateArrow();
            }
        }

        private void UpdatePlaceText()
        {
            string place = raceManager.GetAgentPlace(FollowAgent);
            placeText.text = place;
        }

        private void UpdateTimeText()
        {
            float Time = raceManager.GetAgentTime(FollowAgent);
            TimeText.text = "Time " + Time.ToString("0.0");
        }

        private void UpdateLapText()
        {
            int lap = raceManager.GetAgentLap(FollowAgent);
            LapText.text = "Lap " + lap + "/" + raceManager.numLaps;
        }

        private void UpdateArrow()
        {
            //Finding the checkpoint in ViewPort
            Transform nextCheckpoint = raceManager.GetAgentNextCheckpoint(FollowAgent);
            Vector3 viewportPoint = raceManager.ActiverCamera.WorldToViewportPoint(nextCheckpoint.transform.position);
            bool behindCam = viewportPoint.z < 0;
            viewportPoint.z = 0f;
            //position calculations
            Vector3 viewportCentre = new Vector3(0.5f, 0.5f, 0);
            Vector3 fromCentre = viewportPoint - viewportCentre;
            float halfLimit = indicatorLimit / 2f;
            bool showArrow = false;

            if (behindCam)
            {
                // limit the distace from center 
                fromCentre = -fromCentre.normalized * halfLimit;
                showArrow = true;
            }
            else
            {
                if(fromCentre.magnitude > halfLimit)
                {
                    fromCentre = fromCentre.normalized * halfLimit;
                    showArrow = true;
                }
            }

            //Updating the position
            CheckPointArrow.gameObject.SetActive(showArrow);
            CheckPointArrow.rectTransform.rotation = Quaternion.FromToRotation(Vector3.up, fromCentre);
            CheckPointIcon.rectTransform.position = raceManager.ActiverCamera.ViewportToScreenPoint(fromCentre + viewportCentre);
        }
    }
}


