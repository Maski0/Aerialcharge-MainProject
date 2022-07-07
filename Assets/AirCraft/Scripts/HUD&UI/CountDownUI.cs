using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Aircraft
{
    public class CountDownUI : MonoBehaviour
    {

        public TextMeshProUGUI coundownText;

        public IEnumerator StartCountdown()
        {
            // 321 GO!

            coundownText.text = "3";
            yield return new WaitForSeconds(1f);
            coundownText.text = string.Empty;
            yield return new WaitForSeconds(0.5f);

            coundownText.text = "2";
            yield return new WaitForSeconds(1f);
            coundownText.text = string.Empty;
            yield return new WaitForSeconds(0.5f);

            coundownText.text = "1";
            yield return new WaitForSeconds(1f);
            coundownText.text = string.Empty;
            yield return new WaitForSeconds(0.5f);

            coundownText.text = "GO!";
            yield return new WaitForSeconds(1f);
            coundownText.text = string.Empty;
        }
    }

}