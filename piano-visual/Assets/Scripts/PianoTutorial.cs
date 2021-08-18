using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Microsoft.MixedReality.Toolkit.Utilities;
using System;

public class NoteToPlay
{
    public int PianoKeyId { get; }

    public Guid Id { get; } = System.Guid.NewGuid();

    public TrackedHandJoint FingerToPressKeyWithIndex { get; }
    /*
        public event EventHandler<NoteToPlay> NoteDone;

        public event EventHandler<NoteToPlay> NoteEnteredRangeAllowedToPlay;
    */

    public NoteToPlay(int pianoKeyId, TrackedHandJoint fingerToPressKeyWithIndex)
    {
        PianoKeyId = pianoKeyId;
        FingerToPressKeyWithIndex = fingerToPressKeyWithIndex;
    }
    /*
        public void Done()
        {
            NoteDone?.Invoke(this, this);
        }

        public void EnterRangeAllowedToPlay()
        {
            NoteEnteredRangeAllowedToPlay?.Invoke(this, this);
        }*/
}

public interface ITimeStepInSong
{
    public float GetSecondsFromPreviousTimeStep();

    public void Play(IEnumerable<PianoKey> pianoKeys, FingerTutorial[] fingers);


}

public class SingleNoteStep : ITimeStepInSong
{
    private readonly float _SecondsFromPreviousTimeStep;

    private readonly NoteToPlay _Note;

    public float GetSecondsFromPreviousTimeStep()
    {
        return _SecondsFromPreviousTimeStep;
    }



    public void Play(IEnumerable<PianoKey> pianoKeys, FingerTutorial[] fingers)
    {
        PianoKey toPlay = pianoKeys.ElementAtOrDefault(_Note.PianoKeyId);
        KeyToPressIndicatorGem keyToPressIndicatorGemForNote = null;
        if (toPlay != null)
        {
            keyToPressIndicatorGemForNote = toPlay.HighlightKey(_Note);
        }

        if (keyToPressIndicatorGemForNote != null)
        {
            int indexOfFinger = (int)_Note.FingerToPressKeyWithIndex;
            if (_Note.FingerToPressKeyWithIndex != TrackedHandJoint.None && indexOfFinger >= 0 && indexOfFinger < fingers.Length)
            {
                FingerTutorial fingerToUseToPlayThisNote = fingers[(int)_Note.FingerToPressKeyWithIndex];
                if (fingerToUseToPlayThisNote != null)
                {
                    fingerToUseToPlayThisNote.AddNextNote(keyToPressIndicatorGemForNote);
                }
            }
        }
    }


    public SingleNoteStep(NoteToPlay note, float secondsFromPreviousTimeStep)
    {
        _Note = note;
        _SecondsFromPreviousTimeStep = secondsFromPreviousTimeStep;
    }
}

public class MultipleNotesStep : ITimeStepInSong
{
    private readonly float _SecondsFromPreviousTimeStep;

    private readonly IEnumerable<NoteToPlay> _Notes;

    public float GetSecondsFromPreviousTimeStep()
    {
        return _SecondsFromPreviousTimeStep;
    }



    public void Play(IEnumerable<PianoKey> pianoKeys, FingerTutorial[] fingers)
    {
        foreach (var note in _Notes)
        {
            PianoKey toPlay = pianoKeys.ElementAtOrDefault(note.PianoKeyId);
            KeyToPressIndicatorGem keyToPressIndicatorGemForNote = null;
            if (toPlay != null)
            {
                keyToPressIndicatorGemForNote = toPlay.HighlightKey(note);
            }

            if (keyToPressIndicatorGemForNote != null)
            {
                int indexOfFinger = (int)note.FingerToPressKeyWithIndex;
                if (note.FingerToPressKeyWithIndex != TrackedHandJoint.None && indexOfFinger >= 0 && indexOfFinger < fingers.Length)
                {
                    FingerTutorial fingerToUseToPlayThisNote = fingers[(int)note.FingerToPressKeyWithIndex];
                    if (fingerToUseToPlayThisNote != null)
                    {
                        fingerToUseToPlayThisNote.AddNextNote(keyToPressIndicatorGemForNote);
                    }
                }
            }

        }

    }

    public MultipleNotesStep(IEnumerable<NoteToPlay> notes, float secondsFromPreviousTimeStep)
    {
        _Notes = notes;
        _SecondsFromPreviousTimeStep = secondsFromPreviousTimeStep;
    }
}

public class PianoTutorial : MonoBehaviour
{

    private IEnumerable<ITimeStepInSong> song;
    private int nextStepIndex = 0;

    public List<PianoKey> pianoKeys;

    private PianoTextOutput pianoTextOutput;

    public FingerTutorial[] fingers = new FingerTutorial[27];


    private IEnumerable<ITimeStepInSong> createBasicSong()
    {
        List<ITimeStepInSong> basicSequenceOfSomeNotes = new List<ITimeStepInSong>();

        basicSequenceOfSomeNotes.Add(new MultipleNotesStep(new List<NoteToPlay>() { new NoteToPlay(0, TrackedHandJoint.ThumbTip), new NoteToPlay(2, TrackedHandJoint.IndexTip) }, 3));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(0, TrackedHandJoint.ThumbTip), 0));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(0, TrackedHandJoint.ThumbTip), 2));
        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(1, TrackedHandJoint.IndexTip), 2));
        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(2, TrackedHandJoint.MiddleTip), 2));
        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(3, TrackedHandJoint.RingTip), 2));
        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(4, TrackedHandJoint.PinkyTip), 2));
        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(5, TrackedHandJoint.RingTip), 2));
        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(6, TrackedHandJoint.ThumbTip), 2));
        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(7, TrackedHandJoint.PinkyTip), 2));
        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(8, TrackedHandJoint.ThumbTip), 2));
        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(9, TrackedHandJoint.ThumbTip), 2));
        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(10, TrackedHandJoint.ThumbTip), 2));
        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(11, TrackedHandJoint.ThumbTip), 2));

        return basicSequenceOfSomeNotes;
    }

    private IEnumerable<ITimeStepInSong> createRandomSongForSecondStudy()
    {

        List<ITimeStepInSong> randomSequenceNotes = new List<ITimeStepInSong>();

        const float beatPerMinute = 60;
        const float quarterNum = 1.0f / 4;
        const float speed = 60.0f / quarterNum / beatPerMinute;
        const float quarter = (1.0f / 4) * speed;
        const float half = (1.0f / 2) * speed;


        randomSequenceNotes.Add(new SingleNoteStep(new NoteToPlay(0, TrackedHandJoint.ThumbTip), quarter));
        randomSequenceNotes.Add(new SingleNoteStep(new NoteToPlay(2, TrackedHandJoint.IndexTip), half));
        randomSequenceNotes.Add(new SingleNoteStep(new NoteToPlay(0, TrackedHandJoint.ThumbTip), quarter));
        randomSequenceNotes.Add(new SingleNoteStep(new NoteToPlay(2, TrackedHandJoint.IndexTip), quarter));
        randomSequenceNotes.Add(new SingleNoteStep(new NoteToPlay(7, TrackedHandJoint.PinkyTip), half));
        randomSequenceNotes.Add(new SingleNoteStep(new NoteToPlay(2, TrackedHandJoint.IndexTip), quarter));
        randomSequenceNotes.Add(new SingleNoteStep(new NoteToPlay(5, TrackedHandJoint.RingTip), quarter));
        randomSequenceNotes.Add(new SingleNoteStep(new NoteToPlay(4, TrackedHandJoint.MiddleTip), half));
        randomSequenceNotes.Add(new SingleNoteStep(new NoteToPlay(7, TrackedHandJoint.PinkyTip), quarter));
        randomSequenceNotes.Add(new SingleNoteStep(new NoteToPlay(4, TrackedHandJoint.MiddleTip), quarter));
        randomSequenceNotes.Add(new SingleNoteStep(new NoteToPlay(2, TrackedHandJoint.IndexTip), quarter));
        randomSequenceNotes.Add(new SingleNoteStep(new NoteToPlay(4, TrackedHandJoint.MiddleTip), half));/*
        randomSequenceNotes.Add(new SingleNoteStep(new NoteToPlay(0, TrackedHandJoint.ThumbTip), half));
        randomSequenceNotes.Add(new SingleNoteStep(new NoteToPlay(0, TrackedHandJoint.ThumbTip), quarter));
        randomSequenceNotes.Add(new SingleNoteStep(new NoteToPlay(7, TrackedHandJoint.PinkyTip), quarter));
        randomSequenceNotes.Add(new SingleNoteStep(new NoteToPlay(2, TrackedHandJoint.IndexTip), quarter));
        randomSequenceNotes.Add(new SingleNoteStep(new NoteToPlay(7, TrackedHandJoint.PinkyTip), quarter));
        randomSequenceNotes.Add(new SingleNoteStep(new NoteToPlay(0, TrackedHandJoint.ThumbTip), half));
        randomSequenceNotes.Add(new SingleNoteStep(new NoteToPlay(4, TrackedHandJoint.MiddleTip), quarter));
        randomSequenceNotes.Add(new SingleNoteStep(new NoteToPlay(4, TrackedHandJoint.MiddleTip), quarter));
        randomSequenceNotes.Add(new SingleNoteStep(new NoteToPlay(2, TrackedHandJoint.IndexTip), quarter));
        randomSequenceNotes.Add(new SingleNoteStep(new NoteToPlay(7, TrackedHandJoint.PinkyTip), quarter));
        randomSequenceNotes.Add(new SingleNoteStep(new NoteToPlay(7, TrackedHandJoint.PinkyTip), half));
        randomSequenceNotes.Add(new SingleNoteStep(new NoteToPlay(5, TrackedHandJoint.RingTip), quarter));
        randomSequenceNotes.Add(new SingleNoteStep(new NoteToPlay(2, TrackedHandJoint.IndexTip), quarter));*/

        return randomSequenceNotes;
    }


    private IEnumerable<ITimeStepInSong> createOdeToJoyEasy()
    {
        List<ITimeStepInSong> basicSequenceOfSomeNotes = new List<ITimeStepInSong>();

        const float beatPerMinute = 60;
        const float quarter = 1.0f / 4;
        const float speed = 60.0f / quarter / beatPerMinute;
        const float quarterNoteDurationSeconds = (1.0f / 4) * speed;
        const float threeEighthNoteDurationSeconds = (3.0f / 8) * speed;
        const float eighthNoteDurationSeconds = (1.0f / 8) * speed;
        const float halfNoteDurationSeconds = (1.0f / 2) * speed;


        Debug.Log(quarterNoteDurationSeconds);
        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(4, TrackedHandJoint.MiddleTip), 0));
        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(4, TrackedHandJoint.MiddleTip), quarterNoteDurationSeconds));
        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(5, TrackedHandJoint.RingTip), quarterNoteDurationSeconds));
        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(7, TrackedHandJoint.PinkyTip), quarterNoteDurationSeconds));
        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(7, TrackedHandJoint.PinkyTip), quarterNoteDurationSeconds));
        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(5, TrackedHandJoint.RingTip), quarterNoteDurationSeconds));
        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(4, TrackedHandJoint.MiddleTip), quarterNoteDurationSeconds));
        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(2, TrackedHandJoint.IndexTip), quarterNoteDurationSeconds));
        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(0, TrackedHandJoint.ThumbTip), quarterNoteDurationSeconds));
        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(0, TrackedHandJoint.ThumbTip), quarterNoteDurationSeconds));
        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(2, TrackedHandJoint.IndexTip), quarterNoteDurationSeconds));
        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(4, TrackedHandJoint.MiddleTip), quarterNoteDurationSeconds));
        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(4, TrackedHandJoint.MiddleTip), quarterNoteDurationSeconds));
        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(2, TrackedHandJoint.IndexTip), threeEighthNoteDurationSeconds));
        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(2, TrackedHandJoint.IndexTip), eighthNoteDurationSeconds));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(4, TrackedHandJoint.MiddleTip), halfNoteDurationSeconds));
        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(4, TrackedHandJoint.MiddleTip), quarterNoteDurationSeconds));
        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(5, TrackedHandJoint.RingTip), quarterNoteDurationSeconds));
        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(7, TrackedHandJoint.PinkyTip), quarterNoteDurationSeconds));
        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(7, TrackedHandJoint.PinkyTip), quarterNoteDurationSeconds));
        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(5, TrackedHandJoint.RingTip), quarterNoteDurationSeconds));
        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(4, TrackedHandJoint.MiddleTip), quarterNoteDurationSeconds));
        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(2, TrackedHandJoint.IndexTip), quarterNoteDurationSeconds));
        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(0, TrackedHandJoint.ThumbTip), quarterNoteDurationSeconds));
        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(0, TrackedHandJoint.ThumbTip), quarterNoteDurationSeconds));
        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(2, TrackedHandJoint.IndexTip), quarterNoteDurationSeconds));
        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(4, TrackedHandJoint.MiddleTip), quarterNoteDurationSeconds));
        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(2, TrackedHandJoint.IndexTip), quarterNoteDurationSeconds));
        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(0, TrackedHandJoint.ThumbTip), threeEighthNoteDurationSeconds));
        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(0, TrackedHandJoint.ThumbTip), eighthNoteDurationSeconds));

        return basicSequenceOfSomeNotes;

    }
    private IEnumerable<ITimeStepInSong> createOdeToJoy()
    {
        float factor = 1f;
        List<ITimeStepInSong> basicSequenceOfSomeNotes = new List<ITimeStepInSong>();

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(4, TrackedHandJoint.MiddleTip), 9 * factor));
        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(4, TrackedHandJoint.MiddleTip), 1 * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(5, TrackedHandJoint.RingTip), 0.19f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(7, TrackedHandJoint.PinkyTip), 0.88f * factor));
        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(7, TrackedHandJoint.PinkyTip), 0.17f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(5, TrackedHandJoint.RingTip), 0.91f * factor));
        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(4, TrackedHandJoint.MiddleTip), 0.87f * factor));
        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(2, TrackedHandJoint.IndexTip), 0.18f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(0, TrackedHandJoint.ThumbTip), 0.86f * factor));
        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(0, TrackedHandJoint.ThumbTip), 0.2f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(2, TrackedHandJoint.IndexTip), 1.07f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(4, TrackedHandJoint.MiddleTip), 0.88f * factor));
        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(4, TrackedHandJoint.MiddleTip), 0.15f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(2, TrackedHandJoint.IndexTip), 1.07f * factor));
        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(2, TrackedHandJoint.IndexTip), 0.19f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(4, TrackedHandJoint.MiddleTip), 1.76f * factor));
        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(4, TrackedHandJoint.MiddleTip), 0.17f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(5, TrackedHandJoint.RingTip), 0.87f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(7, TrackedHandJoint.PinkyTip), 0.2f * factor));
        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(7, TrackedHandJoint.PinkyTip), 0.88f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(5, TrackedHandJoint.RingTip), 0.85f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(4, TrackedHandJoint.MiddleTip), 0.22f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(2, TrackedHandJoint.IndexTip), 0.86f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(0, TrackedHandJoint.ThumbTip), 0.18f * factor));
        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(0, TrackedHandJoint.ThumbTip), 0.88f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(1, TrackedHandJoint.ThumbTip), 0.88f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(4, TrackedHandJoint.MiddleTip), 0.17f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(2, TrackedHandJoint.IndexTip), 0.88f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(0, TrackedHandJoint.ThumbTip), 0.18f * factor));
        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(0, TrackedHandJoint.ThumbTip), 0.88f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(4, TrackedHandJoint.MiddleTip), 10.94f * factor));
        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(4, TrackedHandJoint.MiddleTip), 0.19f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(5, TrackedHandJoint.RingTip), 0.85f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(7, TrackedHandJoint.PinkyTip), 0.92f * factor));
        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(7, TrackedHandJoint.PinkyTip), 0.18f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(5, TrackedHandJoint.RingTip), 0.87f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(4, TrackedHandJoint.MiddleTip), 0.16f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(2, TrackedHandJoint.IndexTip), 0.87f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(0, TrackedHandJoint.ThumbTip), 0.9f * factor));
        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(0, TrackedHandJoint.ThumbTip), 0.19f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(2, TrackedHandJoint.IndexTip), 0.87f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(4, TrackedHandJoint.MiddleTip), 0.18f * factor));
        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(4, TrackedHandJoint.MiddleTip), 0.91f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(2, TrackedHandJoint.IndexTip), 0.85f * factor));
        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(2, TrackedHandJoint.IndexTip), 0.21f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(4, TrackedHandJoint.MiddleTip), 1.03f * factor));
        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(4, TrackedHandJoint.MiddleTip), 0.9f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(5, TrackedHandJoint.RingTip), 0.86f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(7, TrackedHandJoint.PinkyTip), 0.2f * factor));
        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(7, TrackedHandJoint.PinkyTip), 0.88f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(5, TrackedHandJoint.RingTip), 0.18f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(4, TrackedHandJoint.MiddleTip), 0.88f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(2, TrackedHandJoint.IndexTip), 0.88f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(0, TrackedHandJoint.ThumbTip), 0.18f * factor));
        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(0, TrackedHandJoint.ThumbTip), 0.88f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(2, TrackedHandJoint.IndexTip), 0.19f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(4, TrackedHandJoint.MiddleTip), 0.88f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(2, TrackedHandJoint.IndexTip), 0.88f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(0, TrackedHandJoint.ThumbTip), 0.2f * factor));
        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(0, TrackedHandJoint.ThumbTip), 0.88f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(4, TrackedHandJoint.MiddleTip), 10.94f * factor));
        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(4, TrackedHandJoint.MiddleTip), 0.18f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(5, TrackedHandJoint.RingTip), 0.93f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(7, TrackedHandJoint.PinkyTip), 0.11f * factor));
        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(7, TrackedHandJoint.PinkyTip), 0.91f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(5, TrackedHandJoint.RingTip), 0.88f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(4, TrackedHandJoint.MiddleTip), 0.16f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(2, TrackedHandJoint.IndexTip), 0.9f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(0, TrackedHandJoint.ThumbTip), 0.18f * factor));
        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(0, TrackedHandJoint.ThumbTip), 0.9f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(2, TrackedHandJoint.IndexTip), 0.85f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(4, TrackedHandJoint.MiddleTip), 0.18f * factor));
        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(4, TrackedHandJoint.MiddleTip), 0.88f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(2, TrackedHandJoint.IndexTip), 0.91f * factor));
        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(2, TrackedHandJoint.IndexTip), 0.11f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(4, TrackedHandJoint.MiddleTip), 1.1f * factor));
        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(4, TrackedHandJoint.MiddleTip), 0.91f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(5, TrackedHandJoint.RingTip), 0.16f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(7, TrackedHandJoint.PinkyTip), 0.88f * factor));
        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(7, TrackedHandJoint.PinkyTip), 0.88f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(5, TrackedHandJoint.RingTip), 0.16f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(4, TrackedHandJoint.MiddleTip), 0.88f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(2, TrackedHandJoint.IndexTip), 0.18f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(0, TrackedHandJoint.ThumbTip), 0.88f * factor));
        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(0, TrackedHandJoint.ThumbTip), 0.88f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(2, TrackedHandJoint.IndexTip), 0.21f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(4, TrackedHandJoint.MiddleTip), 0.88f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(2, TrackedHandJoint.IndexTip), 0.17f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(0, TrackedHandJoint.ThumbTip), 0.88f * factor));
        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(0, TrackedHandJoint.ThumbTip), 0.88f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(4, TrackedHandJoint.MiddleTip), 10.23f * factor));
        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(4, TrackedHandJoint.MiddleTip), 0.9f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(5, TrackedHandJoint.RingTip), 0.88f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(7, TrackedHandJoint.PinkyTip), 0.19f * factor));
        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(7, TrackedHandJoint.PinkyTip), 0.88f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(5, TrackedHandJoint.RingTip), 0.18f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(4, TrackedHandJoint.MiddleTip), 0.85f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(2, TrackedHandJoint.IndexTip), 0.87f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(0, TrackedHandJoint.ThumbTip), 0.21f * factor));
        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(0, TrackedHandJoint.ThumbTip), 0.88f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(2, TrackedHandJoint.IndexTip), 0.17f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(4, TrackedHandJoint.MiddleTip), 0.87f * factor));
        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(4, TrackedHandJoint.MiddleTip), 0.9f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(2, TrackedHandJoint.IndexTip), 0.15f * factor));
        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(2, TrackedHandJoint.IndexTip), 0.91f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(4, TrackedHandJoint.MiddleTip), 1.04f * factor));
        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(4, TrackedHandJoint.MiddleTip), 0.89f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(5, TrackedHandJoint.RingTip), 0.16f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(7, TrackedHandJoint.PinkyTip), 0.89f * factor));
        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(7, TrackedHandJoint.PinkyTip), 0.2f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(5, TrackedHandJoint.RingTip), 0.86f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(4, TrackedHandJoint.ThumbTip), 0.89f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(2, TrackedHandJoint.IndexTip), 0.18f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(0, TrackedHandJoint.ThumbTip), 0.87f * factor));
        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(0, TrackedHandJoint.ThumbTip), 0.91f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(2, TrackedHandJoint.IndexTip), 0.15f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(4, TrackedHandJoint.ThumbTip), 0.87f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(2, TrackedHandJoint.IndexTip), 0.19f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(0, TrackedHandJoint.ThumbTip), 0.89f * factor));
        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(0, TrackedHandJoint.ThumbTip), 0.19f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(4, TrackedHandJoint.ThumbTip), 10.94f * factor));
        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(4, TrackedHandJoint.ThumbTip), 0.88f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(5, TrackedHandJoint.RingTip), 0.89f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(7, TrackedHandJoint.PinkyTip), 0.16f * factor));
        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(7, TrackedHandJoint.PinkyTip), 0.91f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(5, TrackedHandJoint.RingTip), 0.15f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(4, TrackedHandJoint.ThumbTip), 0.87f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(2, TrackedHandJoint.IndexTip), 0.2f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(0, TrackedHandJoint.ThumbTip), 0.89f * factor));
        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(0, TrackedHandJoint.ThumbTip), 0.87f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(2, TrackedHandJoint.IndexTip), 0.17f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(4, TrackedHandJoint.ThumbTip), 0.89f * factor));
        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(4, TrackedHandJoint.ThumbTip), 0.89f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(2, TrackedHandJoint.IndexTip), 0.17f * factor));
        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(2, TrackedHandJoint.IndexTip), 0.89f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(4, TrackedHandJoint.ThumbTip), 1.03f * factor));
        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(4, TrackedHandJoint.ThumbTip), 0.94f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(5, TrackedHandJoint.RingTip), 0.17f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(7, TrackedHandJoint.PinkyTip), 0.9f * factor));
        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(7, TrackedHandJoint.PinkyTip), 0.18f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(5, TrackedHandJoint.RingTip), 0.87f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(4, TrackedHandJoint.ThumbTip), 0.89f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(2, TrackedHandJoint.IndexTip), 0.18f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(0, TrackedHandJoint.ThumbTip), 0.89f * factor));
        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(0, TrackedHandJoint.ThumbTip), 0.18f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(2, TrackedHandJoint.IndexTip), 0.88f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(4, TrackedHandJoint.ThumbTip), 0.88f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(2, TrackedHandJoint.IndexTip), 0.15f * factor));

        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(0, TrackedHandJoint.ThumbTip), 0.89f * factor));
        basicSequenceOfSomeNotes.Add(new SingleNoteStep(new NoteToPlay(0, TrackedHandJoint.ThumbTip), 0.17f * factor));
        return basicSequenceOfSomeNotes;
    }

    private Coroutine runningSong;

    // Start is called before the first frame update
    void Start()
    {
        pianoTextOutput = GetComponent<PianoTextOutput>();

        // song = createBasicSong();

        song = createRandomSongForSecondStudy();
        runningSong = StartCoroutine(PlaySong());

    }






    private IEnumerator PlaySong()
    {
        ITimeStepInSong nextStepToPlay;
        while ((nextStepToPlay = song.ElementAtOrDefault(nextStepIndex)) != null)
        {

            nextStepIndex++;
            yield return new WaitForSeconds(nextStepToPlay.GetSecondsFromPreviousTimeStep());
            nextStepToPlay.Play(pianoKeys, fingers);
        }

        yield return new WaitForSeconds(5);
        for (int i = 10; i > 0; i--)
        {
            pianoTextOutput.SetText("Restarting in " + i);
            yield return new WaitForSeconds(1);
        }


        RestartSong();
    }

    private void RestartSong()
    {

        pianoTextOutput.ResetCounter();
        StopCoroutine(runningSong);

        nextStepIndex = 0;
        runningSong = StartCoroutine(PlaySong());

    }
}
