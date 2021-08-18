using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class NextTestButton : MonoBehaviour
{
    public PianoTracker pianoTracker;
  
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
    }

    public void HandlePressEvent()
    {
        if(pianoTracker != null)
        {
            pianoTracker.NextSession();
        }
    }

}
