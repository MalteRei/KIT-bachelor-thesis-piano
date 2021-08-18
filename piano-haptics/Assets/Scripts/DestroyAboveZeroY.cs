using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAboveZeroY : MonoBehaviour
{
    private readonly float boundaryY = -0.28f;

    // Update is called once per frame
    void Update()
    {
        if(transform.position.y < boundaryY)
        {
            Destroy(gameObject);
        }
    }
}
