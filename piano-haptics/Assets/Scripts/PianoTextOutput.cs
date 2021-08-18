using System.Collections;
using TMPro;
using UnityEngine;


public class PianoTextOutput : MonoBehaviour
{
    [SerializeField]
    protected TextMeshPro textMesh = null;

    private int counter = 0;

    // Use this for initialization
    void Start()
    {
        UpdateCounterText();
    }

 

    public void ResetCounter()
    {
        counter = 0;
        textMesh.text = "Starting song";
        UpdateCounterText();
    }

    public void IncreaseCounter()
    {
        counter++;
        UpdateCounterText();
    }

    private void UpdateCounterText()
    {   
        if(counter > 0)
        {
            textMesh.text = counter.ToString();
        }        
    }

    public void SetText(string text)
    {
        textMesh.text = text;
    }
}
