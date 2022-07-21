using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class PromoteClick : MonoBehaviour
{
    public MouseMove mark;

    private Button btn;
    private TextMeshProUGUI text;
    
    // Start is called before the first frame update
    void Start()
    {
        btn = GetComponent<Button>();
        text = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void Click(string tag)
    {
        mark.PawnPromote(tag);

        mark.SyncPromoteTo(tag, mark.touch);
    }
}
