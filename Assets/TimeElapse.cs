using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeElapse : MonoBehaviour
{
    // Start is called before the first frame update

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(0, 10 * Time.deltaTime, 0, Space.World);
    }
}
