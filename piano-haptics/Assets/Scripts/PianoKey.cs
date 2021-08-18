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
    private Transform spawnPoint;
    public KeyToPressIndicatorGem highlightNotePrefab;

    private readonly List<KeyToPressIndicatorGem> highlightNoteGems = new List<KeyToPressIndicatorGem>();

    public AudioSource audioSourceForKey;

    public AudioSource audioSourceForKeyPressIncorrect;

    private TrackedHandJoint currentFingerPressingKey = TrackedHandJoint.None;

    private PressableButton pressableButton;
    private readonly HandTrackingInputEventData handTrackingInputEvent = new HandTrackingInputEventData(EventSystem.current);


    public PianoTextOutput pianoTextOutput;

    // Start is called before the first frame update
    void Start()
    {
        spawnPoint = transform.Find("SpawnPointNote");
        pressableButton = GetComponent<PressableButton>();
    }

    // Update is called once per frame
    void Update()
    {
        KeyTouchUpdated();
    }

    public KeyToPressIndicatorGem HighlightKey(NoteToPlay note)
    {
        GameObject highlightNote = Instantiate(highlightNotePrefab.gameObject, spawnPoint.position, highlightNotePrefab.transform.rotation);
        KeyToPressIndicatorGem keyToPressIndicatorGem = highlightNote.GetComponent<KeyToPressIndicatorGem>();
        if (keyToPressIndicatorGem != null)
        {
            keyToPressIndicatorGem.NoteThisGemRepresents = note;
            keyToPressIndicatorGem.GemDestroyed += HandleGemDestroyedEvent;
            highlightNoteGems.Add(keyToPressIndicatorGem);
        }
        else
        {
            Destroy(highlightNote);
        }
        return keyToPressIndicatorGem;


    }

    public void KeyPressed()
    {
        if (highlightNoteGems.Count > 0)
        {
            KeyToPressIndicatorGem lowestGem = highlightNoteGems.First();
            if (lowestGem == null)
            {
                highlightNoteGems.RemoveAll(gem => gem == lowestGem);
                KeyPressed();
            }
            else
            {

                if (lowestGem.IsInTargetArea())
                {
                    pianoTextOutput.IncreaseCounter();
                    audioSourceForKey.Play();
                    lowestGem.PianoKeyHit();
                    return;
                }
            }
        }
        audioSourceForKeyPressIncorrect.Play();

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
            if (mixedRealityHand != null && pose != null)
            {
                handTrackingInputEvent.Initialize(mixedRealityHand.InputSource, mixedRealityHand, Handedness.Right, pose.Position);

            } else
            {
                ((IMixedRealityTouchHandler)pressableButton).OnTouchCompleted(handTrackingInputEvent);
                currentFingerPressingKey = TrackedHandJoint.None;

            }
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


    public void HandleGemDestroyedEvent(object sender, KeyToPressIndicatorGem args)
    {
        highlightNoteGems.Remove(args);
      /*  int gemToDestroyIndex = highlightNoteGems.FindIndex(gem => gem == args);
        if (gemToDestroyIndex != -1)
        {
            highlightNoteGems.RemoveAt(gemToDestroyIndex);
        }*/
    }


}
