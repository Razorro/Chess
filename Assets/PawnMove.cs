using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PawnMove : PieceMove
{
    public Tuple<int, int> posUnit;
    public PawnStatus moveStatus = PawnStatus.First;

    private CanvasManager manager;

    private void Start()
    {
        base.Start();

        posUnit = calculator.GetUnitFromAxisScale(transform.localPosition.x, transform.localPosition.y);
        manager = GameObject.Find("CanvasManager").GetComponent<CanvasManager>();   
    }

    public override void ResetStatus()
    {
        base.ResetStatus();

        moveStatus = PawnStatus.First;
        posUnit = calculator.GetUnitFromAxisScale(transform.localPosition.x, transform.localPosition.y);
    }

    public override void PutCallback(bool isOpponent=false)
    {
        // En passant check
        var newPosUnit = calculator.GetUnitFromAxisScale(transform.localPosition.x, transform.localPosition.y);
        if (Math.Abs(newPosUnit.Item2 - posUnit.Item2) == 2)
        {
            moveStatus = PawnStatus.EnPassant;
            //Debug.Log("En passant status");
        }
        else
        {
            moveStatus = PawnStatus.Normal;
            //Debug.Log("Normal status");
        }

        Tuple<int, int> enPassantUnit;
        if(color == Color.white)
            enPassantUnit = calculator.GetUnitFromAxisScale(transform.localPosition.x, transform.localPosition.y - 2 * calculator.yScaleUnit);
        else
            enPassantUnit = calculator.GetUnitFromAxisScale(transform.localPosition.x, transform.localPosition.y + 2 * calculator.yScaleUnit);

        if (calculator.boardPieces.ContainsKey(enPassantUnit))
        {
            var piece = calculator.boardPieces[enPassantUnit];
            if(piece.CompareTag(transform.tag) && 
                piece.GetComponent<PawnMove>().color != color &&
                piece.GetComponent<PawnMove>().moveStatus == PawnStatus.EnPassant)
            {
                piece.SetActive(false);
                calculator.ClearBoardPiece(enPassantUnit);
            }
        }

        // Promotion check
        if (color == Color.white)
        {
            if(calculator.GetUnitFromAxisScale(transform.localPosition.x, transform.localPosition.y).Item2 == 4)
            {
                if (isOpponent == false)
                {
                    manager.SwitchUI(2);
                    mark.blocked = true;
                }
            }
            else
                base.PutCallback();
        }
        else 
        {
            if (calculator.GetUnitFromAxisScale(transform.localPosition.x, transform.localPosition.y).Item2 == -3)
            {
                if (isOpponent == false)
                {
                    manager.SwitchUI(2);
                    mark.blocked = true;
                }
            }  
            else
                base.PutCallback();
        }
    }

    private void CheckEnPassent(List<Tuple<float, float>> possibleAxis)
    {
        var checkAdjacent = new List<Tuple<int, int>>{
            calculator.GetUnitFromAxisScale(transform.localPosition.x - 2 * calculator.xScaleUnit, transform.localPosition.y),
            calculator.GetUnitFromAxisScale(transform.localPosition.x + 2 * calculator.xScaleUnit, transform.localPosition.y),
        };
        
        foreach(var adjacent in checkAdjacent)
        {
            if (calculator.boardPieces.ContainsKey(adjacent))
            {
                var adjacentPiece = calculator.boardPieces[adjacent];
                if (adjacentPiece.CompareTag(transform.tag) &&
                    adjacentPiece.GetComponent<PawnMove>().color != color &&
                    adjacentPiece.GetComponent<PawnMove>().moveStatus == PawnStatus.EnPassant)
                {
                    if (color == Color.white)
                        possibleAxis.Add(new Tuple<float, float>(adjacentPiece.transform.localPosition.x, adjacentPiece.transform.localPosition.y + 2 * calculator.yScaleUnit));
                    else
                        possibleAxis.Add(new Tuple<float, float>(adjacentPiece.transform.localPosition.x, adjacentPiece.transform.localPosition.y - 2 * calculator.yScaleUnit));
                }
            }
        }
    }

    public override void GenMovableGrid()
    {
        if (availableGridTiles.Count > 0)
            return;

        var possibleAxis = new List<Tuple<float, float>>();
        bool emptyFlag;
        if (color == Color.white)
        {
            // advance forward
            emptyFlag = calculator.CheckGridEmpty(transform.localPosition + new Vector3(0, 2 * calculator.yScaleUnit, 0));
            if (emptyFlag && calculator.CheckValidAxis(transform.localPosition.x, transform.localPosition.y + 2 * calculator.yScaleUnit, color))
                possibleAxis.Add(new Tuple<float, float>(transform.localPosition.x, transform.localPosition.y + 2 * calculator.yScaleUnit));

            if (moveStatus == PawnStatus.First)
            {
                emptyFlag = emptyFlag && calculator.CheckGridEmpty(transform.localPosition + new Vector3(0, 4 * calculator.yScaleUnit, 0));
                if (emptyFlag && calculator.CheckValidAxis(transform.localPosition.x, transform.localPosition.y + 4 * calculator.yScaleUnit, color))
                    possibleAxis.Add(new Tuple<float, float>(transform.localPosition.x, transform.localPosition.y + 4 * calculator.yScaleUnit));
            }

            // advance diagonal
            emptyFlag = calculator.CheckGridEmpty(transform.localPosition + new Vector3(2 * calculator.xScaleUnit, 2 * calculator.yScaleUnit));
            if (!emptyFlag && calculator.CheckValidAxis(transform.localPosition.x + 2 * calculator.xScaleUnit, transform.localPosition.y + 2 * calculator.yScaleUnit, color))
                possibleAxis.Add(new Tuple<float, float>(transform.localPosition.x + 2 * calculator.xScaleUnit, transform.localPosition.y + 2 * calculator.yScaleUnit));

            emptyFlag = calculator.CheckGridEmpty(transform.localPosition + new Vector3(-2 * calculator.xScaleUnit, 2 * calculator.yScaleUnit));
            if (!emptyFlag && calculator.CheckValidAxis(transform.localPosition.x - 2 * calculator.xScaleUnit, transform.localPosition.y + 2 * calculator.yScaleUnit, color))
                possibleAxis.Add(new Tuple<float, float>(transform.localPosition.x - 2 * calculator.xScaleUnit, transform.localPosition.y + 2 * calculator.yScaleUnit));
        }
        else
        {
            emptyFlag = calculator.CheckGridEmpty(transform.localPosition + new Vector3(0, -2 * calculator.yScaleUnit, 0));
            if (emptyFlag && calculator.CheckValidAxis(transform.localPosition.x, transform.localPosition.y - 2 * calculator.yScaleUnit, color))
                possibleAxis.Add(new Tuple<float, float>(transform.localPosition.x, transform.localPosition.y - 2 * calculator.yScaleUnit));

            if (moveStatus == PawnStatus.First)
            {
                emptyFlag = emptyFlag && calculator.CheckGridEmpty(transform.localPosition + new Vector3(0, -4 * calculator.yScaleUnit, 0));
                if (emptyFlag && calculator.CheckValidAxis(transform.localPosition.x, transform.localPosition.y - 4 * calculator.yScaleUnit, color))
                    possibleAxis.Add(new Tuple<float, float>(transform.localPosition.x, transform.localPosition.y - 4 * calculator.yScaleUnit));
            }

            emptyFlag = calculator.CheckGridEmpty(transform.localPosition + new Vector3(2 * calculator.xScaleUnit, -2 * calculator.yScaleUnit));
            if (!emptyFlag && calculator.CheckValidAxis(transform.localPosition.x + 2 * calculator.xScaleUnit, transform.localPosition.y - 2 * calculator.yScaleUnit, color))
                possibleAxis.Add(new Tuple<float, float>(transform.localPosition.x + 2 * calculator.xScaleUnit, transform.localPosition.y - 2 * calculator.yScaleUnit));

            emptyFlag = calculator.CheckGridEmpty(transform.localPosition + new Vector3(-2 * calculator.xScaleUnit, -2 * calculator.yScaleUnit));
            if (!emptyFlag && calculator.CheckValidAxis(transform.localPosition.x - 2 * calculator.xScaleUnit, transform.localPosition.y - 2 * calculator.yScaleUnit, color))
                possibleAxis.Add(new Tuple<float, float>(transform.localPosition.x - 2 * calculator.xScaleUnit, transform.localPosition.y - 2 * calculator.yScaleUnit));
        }
        //Debug.Log("Pawn normal move: " + possibleAxis.Count);

        CheckEnPassent(possibleAxis);

        if (possibleAxis.Count > 0)
        {
            //Debug.Log("Pawn can move: " + possibleAxis.Count);
            InstantiateTiles(possibleAxis);
        }
    }
}
