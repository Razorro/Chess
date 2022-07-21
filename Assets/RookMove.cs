using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class RookMove : PieceMove
{
    public RookStatus moveStatus;

    private void Start()
    {
        base.Start();
    }

    public override void ResetStatus()
    {
        moveStatus = RookStatus.First;
    }

    public void GetCrossAxis(List<Tuple<float, float>> possibleAxis)
    {
        for (int i = 2; calculator.CheckValidAxis(transform.localPosition.x + i * calculator.xScaleUnit, transform.localPosition.y, color); i += 2)
        {
            
            var gridUnit = calculator.GetUnitFromAxisScale(transform.localPosition.x + i * calculator.xScaleUnit, transform.localPosition.y);
            if (calculator.CheckGridEmpty(gridUnit))
                possibleAxis.Add(new Tuple<float, float>(transform.localPosition.x + i * calculator.xScaleUnit, transform.localPosition.y));
            else if (calculator.boardPieces[gridUnit].GetComponent<MeshRenderer>().material.color != color)
            {
                possibleAxis.Add(new Tuple<float, float>(transform.localPosition.x + i * calculator.xScaleUnit, transform.localPosition.y));
                break;
            }
            else
                break;
        }

        for (int i = -2; calculator.CheckValidAxis(transform.localPosition.x + i * calculator.xScaleUnit, transform.localPosition.y, color); i -= 2)
        {
            var gridUnit = calculator.GetUnitFromAxisScale(transform.localPosition.x + i * calculator.xScaleUnit, transform.localPosition.y);
            if(calculator.CheckGridEmpty(gridUnit))
                possibleAxis.Add(new Tuple<float, float>(transform.localPosition.x + i * calculator.xScaleUnit, transform.localPosition.y));
            else if (calculator.boardPieces[gridUnit].GetComponent<MeshRenderer>().material.color != color)
            {
                possibleAxis.Add(new Tuple<float, float>(transform.localPosition.x + i * calculator.xScaleUnit, transform.localPosition.y));
                break;
            }
            else
                break;
        }

        for (int j = 2; calculator.CheckValidAxis(transform.localPosition.x, transform.localPosition.y + j * calculator.yScaleUnit, color); j += 2)
        {
            
            var gridUnit = calculator.GetUnitFromAxisScale(transform.localPosition.x, transform.localPosition.y + j * calculator.yScaleUnit);
            if (calculator.CheckGridEmpty(gridUnit))
                possibleAxis.Add(new Tuple<float, float>(transform.localPosition.x, transform.localPosition.y + j * calculator.yScaleUnit));
            else if (calculator.boardPieces[gridUnit].GetComponent<MeshRenderer>().material.color != color)
            {
                possibleAxis.Add(new Tuple<float, float>(transform.localPosition.x, transform.localPosition.y + j * calculator.yScaleUnit));
                break;
            }
            else
                break;
        }

        for (int j = -2; calculator.CheckValidAxis(transform.localPosition.x, transform.localPosition.y + j * calculator.yScaleUnit, color); j -= 2)
        {
            
            var gridUnit = calculator.GetUnitFromAxisScale(transform.localPosition.x, transform.localPosition.y + j * calculator.yScaleUnit);
            if (calculator.CheckGridEmpty(gridUnit))
                possibleAxis.Add(new Tuple<float, float>(transform.localPosition.x, transform.localPosition.y + j * calculator.yScaleUnit));
            else if (calculator.boardPieces[gridUnit].GetComponent<MeshRenderer>().material.color != color)
            {
                possibleAxis.Add(new Tuple<float, float>(transform.localPosition.x, transform.localPosition.y + j * calculator.yScaleUnit));
                break;
            }
            else
                break;
        }
    }

    public override void GenMovableGrid()
    {
        if (availableGridTiles.Count > 0)
            return;

        var possibleAxis = new List<Tuple<float, float>>();
        GetCrossAxis(possibleAxis);

        if (possibleAxis.Count > 0)
        {
            //Debug.Log("Rook possbile move grid: " + possibleAxis.Count);
            InstantiateTiles(possibleAxis);
        }
    }

    public override void PutCallback(bool isOpponent)
    {
        base.PutCallback(isOpponent);

        moveStatus = RookStatus.Normal;
    }
}
