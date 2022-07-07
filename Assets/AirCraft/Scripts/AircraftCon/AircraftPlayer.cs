using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents.Actuators;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Aircraft {

    public class AircraftPlayer : AircraftAgent
    {
        [Header("Input Binding")]
        public InputAction pitchInput;
        public InputAction yawInput;
        public InputAction boostInput;
        public InputAction pauseInput;

        public override void Initialize()
        {
            base.Initialize();
            pitchInput.Enable();
            yawInput.Enable();
            boostInput.Enable();
            pauseInput.Enable();
        }

     

        public override void Heuristic(in ActionBuffers actionsOut)
        {
            // Pitch: 1 == up , 0 == none , -1 == down
            float pitchValue = Mathf.Round(pitchInput.ReadValue<float>());

            // Yaw: 1 == right, 0 == none, -1 == left
            float yawValue = Mathf.Round(yawInput.ReadValue<float>());

            //Boost: 1 == boost, 0 == no boost
            float boostValue = Mathf.Round(boostInput.ReadValue<float>());

            //convert -1(down) to discrete value 2
            if (pitchValue == -1f) pitchValue = 2f;

            // convert -1(left) to discrete value 2
            if (yawValue == -1f) yawValue = 2f;

            var discreateActions = actionsOut.DiscreteActions;
            discreateActions[0] = (int)pitchValue;
            discreateActions[1] = (int)yawValue;
            discreateActions[2] = (int)boostValue;
        }

        private void OnDestroy()
        {
            pitchInput.Disable();
            yawInput.Disable();
            boostInput.Disable();
            pauseInput.Disable();
        }

    }
}

