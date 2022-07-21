using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchHint : MonoBehaviour
{
    private TMPro.TextMeshProUGUI text;
    private string prefix = "匹配";
    private string suffix;
    private string dot = "";
    private float nextFlushDot;

    private void Start()
    {
        text = GetComponent<TMPro.TextMeshProUGUI>();
        nextFlushDot = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if(Time.time >= nextFlushDot)
        {
            if (dot.Length > 3)
                dot = ".";
            else
                dot += ".";

            text.text = prefix + suffix + dot;
            nextFlushDot = Time.time + 1f;
        }
    }

    public void SetMatchHint(int side)
    {
        nextFlushDot = Time.time;
        dot = "";

        if (side == 0)
            suffix = "[白色方]";
        else if (side == 1)
            suffix = "[黑色方]";
        else
            suffix = "[随机方]";
    }
}
