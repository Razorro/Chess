using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class KingMove : PieceMove
{
    public KingStatus moveStatus;
    private HashSet<Tuple<int, int>> castlingUnits = new HashSet<Tuple<int, int>>();

    private List<Tuple<int, int>> whiteRookCheck = new List<Tuple<int, int>> {
        new Tuple<int, int>(-3, -3),
        new Tuple<int, int>(4, -3)
    };

    private List<Tuple<int, int>> blackRookCheck = new List<Tuple<int, int>> {
        new Tuple<int, int>(-3, 4),
        new Tuple<int, int>(4, 4)
    };

    // Start is called before the first frame update
    void Start()
    {
        base.Start();

        moveStatus = KingStatus.First;
    }

    public override void ResetStatus()
    {
        base.ClearTiles();

        moveStatus = KingStatus.First;
        castlingUnits.Clear();
    }

    /// <summary>
    /// For kings' available path, we should notice the castling judgment, every 
    /// 
    /// </summary>
    /// <param name="newStatus"></param>
    public override void SetStatus(Status newStatus, bool forceRefresh = false)
    {
        base.SetStatus(newStatus, true);
    }

    public override void GenMovableGrid()
    {
        if (availableGridTiles.Count > 0)
            return;

        var possibleTiles = new List<Tuple<float, float>>();
        Tuple<float, float> tileAxis;
        for (int i = -1; i <= 1; ++i)
        {
            for (int j = -1; j <= 1; ++j)
            {
                if (i == 0 && j == 0)
                    continue;

                tileAxis = new Tuple<float, float>(transform.localPosition.x + 2 * i * calculator.xScaleUnit, transform.localPosition.y + 2 * j * calculator.yScaleUnit);
                if (calculator.CheckValidAxis(tileAxis.Item1, tileAxis.Item2, color))
                    possibleTiles.Add(tileAxis);
            }
        }

        CheckCastling(possibleTiles);

        if (possibleTiles.Count > 0)
        {
            //Debug.Log("King possible move: " + possibleTiles.Count);
            InstantiateTiles(possibleTiles);
        }
    }

    private bool CheckRookCastlingCond(int dir, Color color)
    {
        if(color == Color.white)
        {
            if (calculator.boardPieces.ContainsKey(whiteRookCheck[dir]))
            {
                var moveLogic = calculator.boardPieces[whiteRookCheck[dir]].GetComponent<RookMove>();
                if (moveLogic != null && moveLogic.color == color && moveLogic.moveStatus == RookStatus.First && !moveLogic.IsFromPromote())
                    return true;
            }
        }
        else
        {
            if (calculator.boardPieces.ContainsKey(blackRookCheck[dir]))
            {
                var moveLogic = calculator.boardPieces[blackRookCheck[dir]].GetComponent<RookMove>();
                if (moveLogic != null && moveLogic.color == color && moveLogic.moveStatus == RookStatus.First && !moveLogic.IsFromPromote())
                    return true;
            }
        }

        return false;
    }

    private Tuple<int, int> GetCastlingRookPos(Color color, int dir)
    {
        if (color == Color.white)
            return whiteRookCheck[dir];
        else
            return blackRookCheck[dir];
    }

    private void CheckCastling(List<Tuple<float, float>> possibleAxis)
    {
        if (calculator.CheckCastlingChange(color) == false)
            return;

        if (moveStatus != KingStatus.First)
            return;

        var attaker = color == Color.white ? Color.black : Color.white;
        if (calculator.IsAttacked(calculator.GetUnitFromAxisScale(transform.localPosition.x, transform.localPosition.y), attaker))
            return;

        var curPosUnit = calculator.GetUnitFromAxisScale(transform.localPosition.x, transform.localPosition.y);
        if (color == Color.white && (curPosUnit.Item1 != 0 || curPosUnit.Item2 != -3))
            return;
        if (color == Color.black && (curPosUnit.Item1 != 0 || curPosUnit.Item2 != 4))
            return;

        bool leftShift = false, rightShift = false;
        // rook status check
        leftShift = CheckRookCastlingCond(0, color);
        rightShift = CheckRookCastlingCond(1, color);

        // empty path check
        int i = leftShift ? -2 : curPosUnit.Item1 + 1;
        int j = rightShift ? 3 : curPosUnit.Item1 - 1;
        //Debug.LogFormat("from left {0} to rigint {1}", i, j);
        for (; i <= j; ++i)
        {
            if(calculator.IsAttacked(new Tuple<int, int>(i, curPosUnit.Item2), attaker) || !calculator.CheckGridEmpty(new Tuple<int, int>(i, curPosUnit.Item2)))
            {
                if (i < curPosUnit.Item1)
                {
                    i = curPosUnit.Item1 + 1;
                    leftShift = false;
                    continue;
                }
                else
                {
                    rightShift = false;
                    break;
                }
            }
        }

        Tuple<float, float> validAxis;
        if(leftShift)
        {
            validAxis = calculator.GetAxisScaleFromUnit(-2, curPosUnit.Item2);
            possibleAxis.Add(validAxis);

            castlingUnits.Add(calculator.GetUnitFromAxisScale(validAxis.Item1, validAxis.Item2));
        }

        if(rightShift)
        {
            validAxis = calculator.GetAxisScaleFromUnit(3, curPosUnit.Item2);
            possibleAxis.Add(validAxis);

            castlingUnits.Add(calculator.GetUnitFromAxisScale(validAxis.Item1, validAxis.Item2));
        }  
    }

    public override void PutCallback(bool isOpponent=false)
    {
        base.PutCallback();

        moveStatus = KingStatus.Normal;

        if(castlingUnits.Count > 0)
        {
            var posUnit = calculator.GetUnitFromAxisScale(transform.localPosition.x, transform.localPosition.y);
            if(castlingUnits.Contains(posUnit))
            {
                GameObject rookPiece;
                if (posUnit.Item1 == 3)
                {
                    rookPiece = calculator.boardPieces[GetCastlingRookPos(color, 1)];
                    rookPiece.transform.localPosition += new Vector3(-4 * calculator.xScaleUnit, 0, 0);
                }
                else
                {
                    rookPiece = calculator.boardPieces[GetCastlingRookPos(color, 0)];
                    rookPiece.transform.localPosition += new Vector3(4 * calculator.xScaleUnit, 0, 0);
                }
                
                calculator.PutPieceInGrid(rookPiece.transform, Vector3.zero);
                calculator.UsingCastling(color);
            }

            castlingUnits.Clear();
        }
    }
}
