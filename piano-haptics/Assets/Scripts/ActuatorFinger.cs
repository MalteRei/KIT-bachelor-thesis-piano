using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActuatorFinger : MonoBehaviour
{
    public HandActuatorClient actuatorClient;
    public string nameOfFinger;
    
    public void Vibrate()
    {
        if(actuatorClient != null && nameOfFinger != null)
        {
            actuatorClient.Vibrate(nameOfFinger);
        }
    }
}
