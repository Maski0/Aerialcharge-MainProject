using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Aircraft {


    public class Rotate : MonoBehaviour
    {
        [Tooltip("Speed of the rotation")]
        public Vector3 rotateSpeed;

        [Tooltip("To randomize the start position")]
        public bool randomize = false;

        private void Start()
        {
            if (randomize) transform.Rotate(rotateSpeed.normalized * UnityEngine.Random.Range(0f, 360f));
        }

        private void Update()
        {
            transform.Rotate(rotateSpeed * Time.deltaTime, Space.Self);
        }

    }
}
