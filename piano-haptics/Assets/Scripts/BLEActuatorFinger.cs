using System;
using System.Collections;
using UnityEngine;


public class BLEActuatorFinger : MonoBehaviour
{
    public string uuidCharacteristic;
    public BLEPianoHandActuator handActuator;

    public void Vibrate()
    {
        if (handActuator != null && uuidCharacteristic != null)
        {
            handActuator.Write(new Guid(uuidCharacteristic), 1);
        }
    }
}
