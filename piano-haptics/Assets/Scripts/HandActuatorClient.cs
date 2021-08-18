using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class HandActuatorClient : MonoBehaviour
{

    public string urlOfHand;
  

    public void Vibrate(string finger)
    {
        StartCoroutine(SendRequestVibrate(finger));
    }

    private IEnumerator SendRequestVibrate(string finger)
    {
        UnityWebRequest requestToTriggerFinger = UnityWebRequest.Post($"{urlOfHand}/{finger}","");
        yield return requestToTriggerFinger.SendWebRequest();

        requestToTriggerFinger.Dispose();

        yield break;

    }
}
