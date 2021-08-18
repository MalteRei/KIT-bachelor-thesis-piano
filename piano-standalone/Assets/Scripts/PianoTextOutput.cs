using System.Collections;
using TMPro;
using UnityEngine;


public class PianoTextOutput : MonoBehaviour
{
    [SerializeField]
    protected TextMeshPro textMesh = null;

   

   

    public void SetText(string text)
    {
        textMesh.text = text;
    }
}
