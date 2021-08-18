using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveBack : MonoBehaviour
{
    private float speed = 0.02f;


    // Update is called once per frame
    void Update()
    {
        
        transform.Translate(Vector3.back * Time.deltaTime * speed);

    }
}
