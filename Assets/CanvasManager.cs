using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sync;

public class CanvasManager : MonoBehaviour
{
    public List<Canvas> UI = new List<Canvas>();
    public GameObject board, handMark;
    public NetPacket codec;

    // Start is called before the first frame update
    void Start()
    {
        board.SetActive(false);
        handMark.SetActive(false);

        codec.OnRecvGameSetupNotify += (GameSetupNotify notify) => { SelectSide((int)notify.Side); };

        SwitchUI(0);
    }

    public void SwitchUI(int index)
    {
        for(int i = 0; i < UI.Count; i++)
        {
            if(i == index)
                UI[i].enabled = true;
            else
                UI[i].enabled = false;
        }
    }

    public void SelectSide(int side)
    {
        board.SetActive(true);
        var calc = board.GetComponentInChildren<GridCalc>();
        calc.playerSide = side == 0 ? UnityEngine.Color.white : UnityEngine.Color.black;

        handMark.SetActive(true);

        SwitchUI(1);
        UI[1].gameObject.GetComponentInChildren<ChangeView>().ChangeViewDir(side);
    }

    public void ShowGameEndInfo(bool isWinner)
    {
        if (isWinner)
            UI[3].GetComponentInChildren<TMPro.TextMeshProUGUI>().text = "ƒ„”Æ¡À!";
        else
            UI[3].GetComponentInChildren<TMPro.TextMeshProUGUI>().text = "ƒ„ ‰¡À!";
    }
}
