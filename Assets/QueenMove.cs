using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class QueenMove : PieceMove
{
    // Start is called before the first frame update
    void Start()
    {
        base.Start();
    }

    protected void GetDiagonal(List<Tuple<float, float>> possibleAxis)
    {
        for (int i = 2, j = 2; calculator.CheckValidAxis(transform.localPosition.x + i * calculator.xScaleUnit, transform.localPosition.y + j * calculator.yScaleUnit, color); i += 2, j += 2)
        {
            possibleAxis.Add(new Tuple<float, float>(transform.localPosition.x + i * calculator.xScaleUnit, transform.localPosition.y + j * calculator.yScaleUnit));
            var gridUnit = calculator.GetUnitFromAxisScale(transform.localPosition.x + i * calculator.xScaleUnit, transform.localPosition.y + j * calculator.yScaleUnit);
            if (calculator.CheckGridEmpty(gridUnit) == false && calculator.boardPieces[gridUnit].GetComponent<MeshRenderer>().material.color != color)
                break;
        }

        for (int i = -2, j = 2; calculator.CheckValidAxis(transform.localPosition.x + i * calculator.xScaleUnit, transform.localPosition.y + j * calculator.yScaleUnit, color); i -= 2, j += 2)
        {
            possibleAxis.Add(new Tuple<float, float>(transform.localPosition.x + i * calculator.xScaleUnit, transform.localPosition.y + j * calculator.yScaleUnit));
            var gridUnit = calculator.GetUnitFromAxisScale(transform.localPosition.x + i * calculator.xScaleUnit, transform.localPosition.y + j * calculator.yScaleUnit);
            if (calculator.CheckGridEmpty(gridUnit) == false && calculator.boardPieces[gridUnit].GetComponent<MeshRenderer>().material.color != color)
                break;
        }

        for (int i = 2, j = -2; calculator.CheckValidAxis(transform.localPosition.x + i * calculator.xScaleUnit, transform.localPosition.y + j * calculator.yScaleUnit, color); i += 2, j -= 2)
        {
            possibleAxis.Add(new Tuple<float, float>(transform.localPosition.x + i * calculator.xScaleUnit, transform.localPosition.y + j * calculator.yScaleUnit));
            var gridUnit = calculator.GetUnitFromAxisScale(transform.localPosition.x + i * calculator.xScaleUnit, transform.localPosition.y + j * calculator.yScaleUnit);
            if (calculator.CheckGridEmpty(gridUnit) == false && calculator.boardPieces[gridUnit].GetComponent<MeshRenderer>().material.color != color)
                break;
        }

        for (int i = -2, j = -2; calculator.CheckValidAxis(transform.localPosition.x + i * calculator.xScaleUnit, transform.localPosition.y + j * calculator.yScaleUnit, color); i -= 2, j -= 2)
        {
            possibleAxis.Add(new Tuple<float, float>(transform.localPosition.x + i * calculator.xScaleUnit, transform.localPosition.y + j * calculator.yScaleUnit));
            var gridUnit = calculator.GetUnitFromAxisScale(transform.localPosition.x + i * calculator.xScaleUnit, transform.localPosition.y + j * calculator.yScaleUnit);
            if (calculator.CheckGridEmpty(gridUnit) == false && calculator.boardPieces[gridUnit].GetComponent<MeshRenderer>().material.color != color)
                break;
        }
    }

    public void GetCrossAxis(List<Tuple<float, float>> possibleAxis)
    {
        for (int i = 2; calculator.CheckValidAxis(transform.localPosition.x + i * calculator.xScaleUnit, transform.localPosition.y, color); i += 2)
        {
            possibleAxis.Add(new Tuple<float, float>(transform.localPosition.x + i * calculator.xScaleUnit, transform.localPosition.y));
            var gridUnit = calculator.GetUnitFromAxisScale(transform.localPosition.x + i * calculator.xScaleUnit, transform.localPosition.y);
            if (calculator.CheckGridEmpty(gridUnit) == false && calculator.boardPieces[gridUnit].GetComponent<MeshRenderer>().material.color != color)
                break;
        }

        for (int i = -2; calculator.CheckValidAxis(transform.localPosition.x + i * calculator.xScaleUnit, transform.localPosition.y, color); i -= 2)
        {
            possibleAxis.Add(new Tuple<float, float>(transform.localPosition.x + i * calculator.xScaleUnit, transform.localPosition.y));
            var gridUnit = calculator.GetUnitFromAxisScale(transform.localPosition.x + i * calculator.xScaleUnit, transform.localPosition.y);
            if (calculator.CheckGridEmpty(gridUnit) == false && calculator.boardPieces[gridUnit].GetComponent<MeshRenderer>().material.color != color)
                break;
        }

        for (int j = 2; calculator.CheckValidAxis(transform.localPosition.x, transform.localPosition.y + j * calculator.yScaleUnit, color); j += 2)
        {
            possibleAxis.Add(new Tuple<float, float>(transform.localPosition.x, transform.localPosition.y + j * calculator.yScaleUnit));
            var gridUnit = calculator.GetUnitFromAxisScale(transform.localPosition.x, transform.localPosition.y + j * calculator.yScaleUnit);
            if (calculator.CheckGridEmpty(gridUnit) == false && calculator.boardPieces[gridUnit].GetComponent<MeshRenderer>().material.color != color)
                break;
        }

        for (int j = -2; calculator.CheckValidAxis(transform.localPosition.x, transform.localPosition.y + j * calculator.yScaleUnit, color); j -= 2)
        {
            possibleAxis.Add(new Tuple<float, float>(transform.localPosition.x, transform.localPosition.y + j * calculator.yScaleUnit));
            var gridUnit = calculator.GetUnitFromAxisScale(transform.localPosition.x, transform.localPosition.y + j * calculator.yScaleUnit);
            if (calculator.CheckGridEmpty(gridUnit) == false && calculator.boardPieces[gridUnit].GetComponent<MeshRenderer>().material.color != color)
                break;
        }
    }

    public override void GenMovableGrid()
    {
        var possibleAxis = new List<Tuple<float, float>>();
        GetDiagonal(possibleAxis);
        GetCrossAxis(possibleAxis);

        if (possibleAxis.Count > 0)
        {
            //Debug.Log("Queen can move: " + possibleAxis.Count);
            InstantiateTiles(possibleAxis);
        }
    }
}
