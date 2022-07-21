using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BishopMove : PieceMove
{
    // Start is called before the first frame update
    void Start()
    {
        base.Start();
    }

    public override void GenMovableGrid()
    {
        var possibleAxis = new List<Tuple<float, float>>();
        GetDiagonal(possibleAxis);

        if (possibleAxis.Count > 0)
        {
            //Debug.Log("Rook possbile move grid: " + possibleAxis.Count);
            InstantiateTiles(possibleAxis);
        }
    }

    protected void GetDiagonal(List<Tuple<float, float>> possibleAxis)
    {
        for (int i = 2, j = 2; calculator.CheckValidAxis(transform.localPosition.x + i * calculator.xScaleUnit, transform.localPosition.y + j * calculator.yScaleUnit, color); i += 2, j += 2)
        {
            var gridUnit = calculator.GetUnitFromAxisScale(transform.localPosition.x + i * calculator.xScaleUnit, transform.localPosition.y + j * calculator.yScaleUnit);
            if(calculator.CheckGridEmpty(gridUnit))
                possibleAxis.Add(new Tuple<float, float>(transform.localPosition.x + i * calculator.xScaleUnit, transform.localPosition.y + j * calculator.yScaleUnit));
            else if (calculator.boardPieces[gridUnit].GetComponent<MeshRenderer>().material.color != color)
            {
                possibleAxis.Add(new Tuple<float, float>(transform.localPosition.x + i * calculator.xScaleUnit, transform.localPosition.y + j * calculator.yScaleUnit));
                break;
            }
            else
                break;
        }

        for (int i = -2, j = 2; calculator.CheckValidAxis(transform.localPosition.x + i * calculator.xScaleUnit, transform.localPosition.y + j * calculator.yScaleUnit, color); i -= 2, j += 2)
        {
            
            var gridUnit = calculator.GetUnitFromAxisScale(transform.localPosition.x + i * calculator.xScaleUnit, transform.localPosition.y + j * calculator.yScaleUnit);
            if (calculator.CheckGridEmpty(gridUnit))
                possibleAxis.Add(new Tuple<float, float>(transform.localPosition.x + i * calculator.xScaleUnit, transform.localPosition.y + j * calculator.yScaleUnit));
            else if (calculator.boardPieces[gridUnit].GetComponent<MeshRenderer>().material.color != color)
            {
                possibleAxis.Add(new Tuple<float, float>(transform.localPosition.x + i * calculator.xScaleUnit, transform.localPosition.y + j * calculator.yScaleUnit));
                break;
            }
            else
                break;
        }

        for (int i = 2, j = -2; calculator.CheckValidAxis(transform.localPosition.x + i * calculator.xScaleUnit, transform.localPosition.y + j * calculator.yScaleUnit, color); i += 2, j -= 2)
        {
            
            var gridUnit = calculator.GetUnitFromAxisScale(transform.localPosition.x + i * calculator.xScaleUnit, transform.localPosition.y + j * calculator.yScaleUnit);
            if (calculator.CheckGridEmpty(gridUnit))
                possibleAxis.Add(new Tuple<float, float>(transform.localPosition.x + i * calculator.xScaleUnit, transform.localPosition.y + j * calculator.yScaleUnit));
            else if (calculator.boardPieces[gridUnit].GetComponent<MeshRenderer>().material.color != color)
            {
                possibleAxis.Add(new Tuple<float, float>(transform.localPosition.x + i * calculator.xScaleUnit, transform.localPosition.y + j * calculator.yScaleUnit));
                break;
            }
            else
                break;
        }

        for (int i = -2, j = -2; calculator.CheckValidAxis(transform.localPosition.x + i * calculator.xScaleUnit, transform.localPosition.y + j * calculator.yScaleUnit, color); i -= 2, j -= 2)
        {
            
            var gridUnit = calculator.GetUnitFromAxisScale(transform.localPosition.x + i * calculator.xScaleUnit, transform.localPosition.y + j * calculator.yScaleUnit);
            if (calculator.CheckGridEmpty(gridUnit))
                possibleAxis.Add(new Tuple<float, float>(transform.localPosition.x + i * calculator.xScaleUnit, transform.localPosition.y + j * calculator.yScaleUnit));
            else if (calculator.boardPieces[gridUnit].GetComponent<MeshRenderer>().material.color != color)
            {
                possibleAxis.Add(new Tuple<float, float>(transform.localPosition.x + i * calculator.xScaleUnit, transform.localPosition.y + j * calculator.yScaleUnit));
                break;
            }
            else
                break;
        }
    }
}