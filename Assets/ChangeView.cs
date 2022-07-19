using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeView : MonoBehaviour
{
    public Camera cam;
    public GameObject handMark;
    public TMPro.TMP_Dropdown dropdown;

    private void Start()
    {
        dropdown.onValueChanged.AddListener((int e) => { ChangeViewDir(dropdown); });
    }

    public void ChangeViewDir(TMPro.TMP_Dropdown dropdown)
    {
        cam.transform.transform.RotateAround(cam.transform.position, Vector3.up, 180);
        handMark.transform.RotateAround(handMark.transform.position, Vector3.up, 180);

        cam.transform.position = new Vector3(-cam.transform.position.x, cam.transform.position.y, -cam.transform.position.z);
    }

    public void ChangeViewDir(int side)
    {
        if(side == 0)
        {
            cam.transform.transform.RotateAround(cam.transform.position, Vector3.up, 180);
            handMark.transform.RotateAround(handMark.transform.position, Vector3.up, 180);

            cam.transform.position = new Vector3(-cam.transform.position.x, cam.transform.position.y, -cam.transform.position.z);

            dropdown.options.Reverse();
            dropdown.captionText.text = dropdown.options[0].text;
        }
    }
}
