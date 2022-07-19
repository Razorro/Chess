using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Floating : MonoBehaviour
{
    public float floatingRange = 3f;
    public float offset = 0f;
    public float speed = 10f;
    public float originY = 0f;
    public int dir = 1;

    // Start is called before the first frame update
    void Start()
    {
        originY = transform.position.y;
    }

    // Update is called once per frame
    void Update()
    {
        var distance = dir * speed * Time.deltaTime;
        offset += distance;
        transform.position = transform.position + new Vector3(0, distance, 0);

        if (offset < -floatingRange || offset > floatingRange)
        {
            dir *= -1;
            offset = 0f;
        }
    }
}
