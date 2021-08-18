using System.Collections;
using UnityEngine;

    public class MoveLeft : MonoBehaviour
    {

    private float speed = 0.02f;


    // Update is called once per frame
    void Update()
    {

        transform.Translate(Vector3.left * Time.deltaTime * speed);

    }
}
