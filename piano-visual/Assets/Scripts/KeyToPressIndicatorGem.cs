using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyToPressIndicatorGem : MonoBehaviour
{
    public event EventHandler<KeyToPressIndicatorGem> GemDestroyed;

    public event EventHandler<KeyToPressIndicatorGem> GemInTargetArea;

    /*private readonly float targetAreaLowerBoundY = -0.31f;
    private readonly float targetAreaUpperBoundY = -0.275f;
    */

    // private readonly float targetAreaLowerBoundY = -0.250f;
    private readonly float targetAreaLowerBoundY = -0.225f;
    private readonly float targetAreaUpperBoundY = -0.15f;
    // private readonly float targetAreaUpperBoundY = -0.225f;
    public NoteToPlay NoteThisGemRepresents { get; set; }


    private GameObject visualFxTemplate_OnHit = null;

    private bool enteredRangeAllowedToPlay = false;


    private void Start()
    {
        /*   GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
           cube.transform.localScale = Vector3.one * 0.005f;

           cube.transform.position = transform.position;*/
        visualFxTemplate_OnHit = transform.Find("GemExplosion").gameObject;
    }
    // Update is called once per frame
    void FixedUpdate()
    {


        if (!enteredRangeAllowedToPlay && IsInTargetArea())
        {

            MakeNoteEnterRangeAllowedToPlay();
        }
        if (transform.position.y < targetAreaLowerBoundY)
        {

            Destroy();
        }
    }

    private void MakeNoteEnterRangeAllowedToPlay()
    {
        enteredRangeAllowedToPlay = true;

        GemInTargetArea?.Invoke(this, this);

        /* if (NoteThisGemRepresents != null)
         {
             NoteThisGemRepresents.EnterRangeAllowedToPlay();
         }*/
    }

    public bool IsInTargetArea()
    {
        bool result =
                transform.position.y < targetAreaUpperBoundY && transform.position.y > targetAreaLowerBoundY;

        return result;
    }

    public void PianoKeyHit()
    {

        if (IsInTargetArea())
        {

            PlayAnimationOnHit();
            Destroy();
        }
    }

    private void Destroy()
    {
        GemDestroyed?.Invoke(this, this);
        /* if (NoteThisGemRepresents != null)
         {
             NoteThisGemRepresents.Done();
         }*/

        // GemDestroyed?.Invoke(this, this);
        Destroy(gameObject);

    }

    private void PlayAnimationOnHit()
    {
        if (visualFxTemplate_OnHit != null)
        {
            GameObject visfx = Instantiate(visualFxTemplate_OnHit, transform.position, transform.rotation);
            visfx.SetActive(true);
            Destroy(visfx, 2);
        }
    }


}
