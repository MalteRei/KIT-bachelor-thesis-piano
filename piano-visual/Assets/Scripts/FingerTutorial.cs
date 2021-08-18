using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class FingerTutorial : MonoBehaviour
{
    private readonly List<KeyToPressIndicatorGem> notesToDisplay = new List<KeyToPressIndicatorGem>();

    public List<GameObject> prefabsForNotes;

    private GameObject currentNoteDisplayed = null;



    public void AddNextNote(KeyToPressIndicatorGem noteGem)
    {
        noteGem.GemInTargetArea += HandleNoteEnteredRangeAllowedToPlayEvent;
        //note.NoteEnteredRangeAllowedToPlay += HandleNoteEnteredRangeAllowedToPlayEvent;
    }

    private void PushNextNoteToDisplay(KeyToPressIndicatorGem noteGem)
    {
        if (noteGem.NoteThisGemRepresents.PianoKeyId < prefabsForNotes.Count)
        {
            noteGem.GemDestroyed += HandleNotePlayedEvent;
            //note.NoteDone += HandleNotePlayedEvent;
            notesToDisplay.Add(noteGem);
            DisplayNextNote();

        }
    }
    public void HandleNoteEnteredRangeAllowedToPlayEvent(object sender, KeyToPressIndicatorGem args)
    {
       
        PushNextNoteToDisplay(args);
    }


    public void HandleNotePlayedEvent(object sender, KeyToPressIndicatorGem args)
    {
        //  args.NoteEnteredRangeAllowedToPlay -= HandleNoteEnteredRangeAllowedToPlayEvent;
        notesToDisplay.Remove(args);
        UpdateDisplayedNoteRemoved();
    }

    private void UpdateDisplayedNoteRemoved()
    {
        if (currentNoteDisplayed != null && currentNoteDisplayed != gameObject)
        {

            Destroy(currentNoteDisplayed);


            currentNoteDisplayed = null;
            DisplayNextNote();
        }
    }

    public void hideOrShowFingerTutorial(bool show)
    {
        if (currentNoteDisplayed != null)
        {
            currentNoteDisplayed.SetActive(show);
        }

    }

    private void DisplayNextNote()
    {
        if (currentNoteDisplayed == null && notesToDisplay.Count > 0)
        {
            KeyToPressIndicatorGem noteToDisplay = notesToDisplay.ElementAtOrDefault(0);
            if (noteToDisplay == null)
            {
                notesToDisplay.RemoveAll(note => note == null);
                DisplayNextNote();
            }
            else
            {

                GameObject prefabToRepresentNote = prefabsForNotes.ElementAtOrDefault(noteToDisplay.NoteThisGemRepresents.PianoKeyId);
                if (prefabToRepresentNote != null)
                {
                    GameObject instantiateNoteRepresentation = Instantiate(prefabToRepresentNote);
                    instantiateNoteRepresentation.transform.SetParent(transform);
                    instantiateNoteRepresentation.transform.localPosition = Vector3.zero;
                    currentNoteDisplayed = instantiateNoteRepresentation;

                }
                else
                {
                    notesToDisplay.Remove(noteToDisplay);
                    DisplayNextNote();
                }

            }
        }
    }
}
