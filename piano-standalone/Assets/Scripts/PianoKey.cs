using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEngine.EventSystems;

public class PianoKey : MonoBehaviour
{
 
    public AudioSource audioSourceForKey;
    public PianoTracker pianoTracker;

    private TrackedHandJoint currentFingerPressingKey = TrackedHandJoint.None;

    private PressableButton pressableButton;
    private readonly HandTrackingInputEventData handTrackingInputEvent = new HandTrackingInputEventData(EventSystem.current);





    // Start is called before the first frame update
    void Start()
    {
        
        pressableButton = GetComponent<PressableButton>();
    }

    // Update is called once per frame
    void Update()
    {
        KeyTouchUpdated();
    }

    
   
    public void KeyPressed()
    {
        audioSourceForKey.Play();
        pianoTracker.RegisterKeyPress(transform.name);
    }

   
    private void OnTriggerEnter(Collider other)
    {
        switch (other.name)
        {
            case "PinkyTip Proxy Transform":
                fingerEntered(TrackedHandJoint.PinkyTip);
                break;
            case "RingTip Proxy Transform":
                fingerEntered(TrackedHandJoint.RingTip);
                break;
            case "MiddleTip Proxy Transform":
                fingerEntered(TrackedHandJoint.MiddleTip);
                break;
            case "IndexTip Proxy Transform":
                fingerEntered(TrackedHandJoint.IndexTip);
                break;
            case "ThumbTip Proxy Transform":
                fingerEntered(TrackedHandJoint.ThumbTip);
                break;
        }
    }

    private void OnTriggerExit(Collider other)
    { 
        switch (other.name)
        {
            case "PinkyTip Proxy Transform":
                fingerExit(TrackedHandJoint.PinkyTip);
                break;
            case "RingTip Proxy Transform":
                fingerExit(TrackedHandJoint.RingTip);
                break;
            case "MiddleTip Proxy Transform":
                fingerExit(TrackedHandJoint.MiddleTip);
                break;
            case "IndexTip Proxy Transform":
                fingerExit(TrackedHandJoint.IndexTip);
                break;
            case "ThumbTip Proxy Transform":
                fingerExit(TrackedHandJoint.ThumbTip);
                break;
        }
    }

    private void UpdateHandTrackingInputEvent()
    {
        if (currentFingerPressingKey != TrackedHandJoint.None)
        {
            HandJointUtils.TryGetJointPose(currentFingerPressingKey, Handedness.Right, out MixedRealityPose pose);
            IMixedRealityHand mixedRealityHand = HandJointUtils.FindHand(Handedness.Right);
            handTrackingInputEvent.Initialize(mixedRealityHand.InputSource, mixedRealityHand, Handedness.Right, pose.Position);
        }
    }

    private void KeyTouchStarted()
    {
        if (currentFingerPressingKey != TrackedHandJoint.None)
        {
            UpdateHandTrackingInputEvent();
            ((IMixedRealityTouchHandler)pressableButton).OnTouchStarted(handTrackingInputEvent);
        }
    }

    private void KeyTouchUpdated()
    {
        if (currentFingerPressingKey != TrackedHandJoint.None)
        {
            UpdateHandTrackingInputEvent();
            ((IMixedRealityTouchHandler)pressableButton).OnTouchUpdated(handTrackingInputEvent);
        }
    }

    private void KeyTouchCompleted()
    {
        if (currentFingerPressingKey != TrackedHandJoint.None)
        {
            UpdateHandTrackingInputEvent();
            ((IMixedRealityTouchHandler)pressableButton).OnTouchCompleted(handTrackingInputEvent);
        }
    }

    private void fingerEntered(TrackedHandJoint fingerThatEntered)
    {
        if (currentFingerPressingKey == TrackedHandJoint.None)
        {
            currentFingerPressingKey = fingerThatEntered;
            KeyTouchStarted();
        }
    }

    private void fingerExit(TrackedHandJoint finger)
    {
        if (currentFingerPressingKey == finger)
        {
            KeyTouchCompleted();
            currentFingerPressingKey = TrackedHandJoint.None;
        }
    }
   

   
}
