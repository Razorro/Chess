using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public enum Mode { Sole, Network };

public class GridCalc : MonoBehaviour
{
    public GameObject white_rook, white_knight, white_bishop, white_queen, white_king, white_pawn;
    public GameObject black_rook, black_knight, black_bishop, black_queen, black_king, black_pawn;

    public Dictionary<Tuple<float, float>, GameObject> whitePieces = new Dictionary<Tuple<float, float>, GameObject>();
    public Dictionary<Tuple<float, float>, GameObject> blackPieces = new Dictionary<Tuple<float, float>, GameObject>();
    private List<GameObject> promotePieces = new List<GameObject>();

    public Dictionary<Tuple<int, int>, GameObject> boardPieces = new Dictionary<Tuple<int, int>, GameObject>();
    public HashSet<Tuple<int, int>> attackByWhite = new HashSet<Tuple<int, int>>();
    public HashSet<Tuple<int, int>> attackByBlack = new HashSet<Tuple<int, int>>();

    public Mode mode;

    public float scaleRatio = 0.48f;
    public float xScaleUnit, yScaleUnit;

    public bool whiteCastling = true, blackCastling = true;

    private float xMinScale = -0.24f, xMaxScale = 0.24f, yMinScale = -0.24f, yMaxScale = 0.24f;

    private Color actSide = Color.white;
    public Color playerSide;

    public MouseMove handMark;
    public CanvasManager manager;

    public void AddAttackGrid(Tuple<int, int> attackAxis, Color color)
    {
        if(color == Color.white)
            attackByWhite.Add(attackAxis);
        else
            attackByBlack.Add(attackAxis);
    }

    public void RemoveAttackGrid(Tuple<int, int> attackAxis, Color color)
    {
        if (color == Color.white)
            attackByWhite.Remove(attackAxis);
        else
            attackByBlack.Remove(attackAxis);
    }

    public void SetMode(int pickMode)
    {
        mode = (Mode)pickMode;
    }

    public bool CheckValidAxis(float xScale, float yScale, Color color)
    {
        if (xScale < xMinScale || xScale > xMaxScale)
            return false;

        if (yScale < yMinScale || yScale > yMaxScale)
            return false;

        var unitTuple = GetUnitFromAxisScale(xScale, yScale);
        if (boardPieces.ContainsKey(unitTuple) && boardPieces[unitTuple].GetComponent<MeshRenderer>().material.color == color)
            return false;

        return true;
    }

    public bool CheckCastlingChange(Color color)
    {
        if (color == Color.white)
            return whiteCastling;
        else
            return blackCastling;
    }

    public void UsingCastling(Color color)
    {
        if(color == Color.white)
            whiteCastling = false;
        else
            blackCastling = false;
    }

    public bool CheckGridEmpty(float xAxis, float yAxis)
    {
        var gridIndex = GetUnitFromAxisScale(xAxis, yAxis);
        if (boardPieces.ContainsKey(gridIndex))
            return false;

        return true;
    }

    public bool CheckGridEmpty(Vector3 curPos)
    {
        var gridIndex =  GetUnitFromAxisScale(curPos.x, curPos.y);
        if (boardPieces.ContainsKey(gridIndex))
            return false;

        return true;
    }

    public bool IsAttacked(Tuple<int, int> axis, Color color)
    {
        if (color == Color.white)
        {
            //Debug.LogFormat("check axis: {0}, attacker color: {1}, attack range: {2}", axis, color, String.Join(" ", attackByWhite));
            return attackByWhite.Contains(axis);
        }
        else
        {
            //Debug.LogFormat("check axis: {0}, attacker color: {1}, attack range: {2}", axis, color, String.Join(" ", attackByBlack));
            return attackByBlack.Contains(axis);
        }
            
    }

    public bool CheckGridEmpty(Tuple<int, int> unitTuple)
    {
        return boardPieces.ContainsKey(unitTuple) == false;
    }

    public bool CheckPutAction(GameObject go)
    {
        var gridIdx = GetUnitFromAxisScale(go.transform.localPosition.x, go.transform.localPosition.y);
        if (boardPieces.ContainsKey(gridIdx) == false)
            return true;

        var curGridPiece = boardPieces[gridIdx];
        if (curGridPiece.GetComponent<MeshRenderer>().material.color != go.GetComponent<MeshRenderer>().material.color)
            return true;

        return false;
    }

    public Vector3 GetGridCentralPos(Vector3 curPos)
    {
        //var xUnit = Mathf.Round((curPos.x + xScaleUnit) / xScaleUnit / 2);
        //var yUnit = Mathf.Round((curPos.y + yScaleUnit) / yScaleUnit / 2);

        var scaleTuple = GetUnitFromAxisScale(curPos.x, curPos.y);

        var newPos = new Vector3(2 * scaleTuple.Item1 * xScaleUnit - xScaleUnit, 2 * scaleTuple.Item2 * yScaleUnit - yScaleUnit, curPos.z);
        
        return newPos;
    }

    public void ClearBoardPiece(Tuple<int, int> clearUnit)
    {
        boardPieces[clearUnit].SetActive(false);
        boardPieces.Remove(clearUnit);
    }

    public bool CheckActColor(Color color)
    {
        return actSide == color;
    }

    public bool CheckPlayerColor(Color color)
    {
        if (mode == Mode.Sole)
            return true;

        return playerSide == color;
    }

    public bool PutPieceInGrid(Transform piece, Vector3 lastPos, Tuple<int, int> putUnit = null)
    {
        var endGame = false;
        if(putUnit == null)
            putUnit = GetUnitFromAxisScale(piece.localPosition.x, piece.localPosition.y);
        
        if (boardPieces.ContainsKey(putUnit))
        {
            //Debug.Log("attacked piece: " + boardPieces[putUnit].tag);
            if (boardPieces[putUnit].CompareTag("king"))
            {
                manager.ShowGameEndInfo(MouseMove.GetMoveLogic(boardPieces[putUnit]).GetColor() != playerSide);
                manager.SwitchUI(3);
                handMark.blocked = true;
                endGame = true;
            }

            if (boardPieces[putUnit] != piece.gameObject)
                ClearBoardPiece(putUnit);
        }

        var axis = GetAxisScaleFromUnit(putUnit.Item1, putUnit.Item2);
        piece.localPosition = new Vector3(axis.Item1, axis.Item2, piece.localPosition.z);
        
        boardPieces.Remove(GetUnitFromAxisScale(lastPos.x, lastPos.y));
        boardPieces.Add(GetUnitFromAxisScale(piece.localPosition.x, piece.localPosition.y), piece.gameObject);

        return endGame;
    }

    public void SetActColor(Color curAct)
    {
        actSide = curAct;

        manager.UI[1].GetComponentInChildren<Image>().color = curAct;
    }

    public void UpdatePieceStatus(Color color)
    {
        var updatePieces = color == Color.white ? blackPieces : whitePieces;
        foreach(var item in updatePieces)
        {
            if (item.Value.CompareTag("pawn"))
            {
                var pawnLogic = item.Value.GetComponent<PawnMove>();
                if (pawnLogic.moveStatus == PawnStatus.EnPassant)
                    pawnLogic.moveStatus = PawnStatus.Normal;
            }
            else
            {
                var logic = MouseMove.GetMoveLogic(item.Value);
                logic.ClearTiles();
                logic.GenMovableGrid();
            }
        }
    }

    private void Awake()
    {
        xScaleUnit = scaleRatio / 16;
        yScaleUnit = scaleRatio / 16;

        ResetChessPosition();
    }

    public Tuple<float, float> GetAxisScaleFromUnit(int xUnit, int yUnit)
    {
        return new Tuple<float, float>(2 * xUnit * xScaleUnit - xScaleUnit, 2 * yUnit * yScaleUnit - yScaleUnit);
    }

    public Tuple<int, int> GetUnitFromAxisScale(float xScale, float yScale)
    {
        var xUnit = Mathf.Round((xScale + xScaleUnit) / xScaleUnit / 2);
        var yUnit = Mathf.Round((yScale + yScaleUnit) / yScaleUnit / 2);

        return new Tuple<int, int>((int)xUnit, (int)yUnit);
    }

    public Tuple<int, int> GetUnitFromAxisScale(Transform trans)
    {
        var xUnit = Mathf.Round((trans.localPosition.x + xScaleUnit) / xScaleUnit / 2);
        var yUnit = Mathf.Round((trans.localPosition.y + yScaleUnit) / yScaleUnit / 2);

        return new Tuple<int, int>((int)xUnit, (int)yUnit);
    }

    GameObject InstantiatePiece(GameObject piece, int x, int y, Color color, Action<GameObject> specialAdjust = null, bool firstInit = true)
    {
        var axisScale = GetAxisScaleFromUnit(x, y);
        var go = Instantiate(piece, transform);
        go.transform.Rotate(new Vector3(90, 0, 0));
        go.transform.localPosition = new Vector3(axisScale.Item1, axisScale.Item2, 0);
        go.transform.localScale /= 100;

        specialAdjust?.Invoke(go);

        boardPieces[new Tuple<int, int>(x, y)] = go;

        if (firstInit)
        {
            if (color == Color.white)
                whitePieces[axisScale] = go;
            else
                blackPieces[axisScale] = go;
        }

        return go;
    }

    public void ResetChessPosition()
    {
        SetActColor(Color.white);

        if(promotePieces.Count > 0)
        {
            foreach(var promoted in promotePieces)
                Destroy(promoted);

            promotePieces.Clear();
        }

        if(whitePieces.Count == 0)
        {
            for (int i = -3; i <= 4; i++)
                InstantiatePiece(white_pawn, i, -2, Color.white, (GameObject go) => { go.transform.localPosition += Vector3.forward * 0.01f; });

            var whiteRookPos = new List<Tuple<int, int>> {
                new Tuple<int, int>(-3, -3),
                new Tuple<int, int>(4, -3),
            };
            foreach (var rookPos in whiteRookPos)
                InstantiatePiece(white_rook, rookPos.Item1, rookPos.Item2, Color.white);

            var whiteKnightPos = new List<Tuple<int, int>> {
                new Tuple<int, int>(-2, -3),
                new Tuple<int, int>(3, -3)
            };
            foreach (var knightPos in whiteKnightPos)
                InstantiatePiece(white_knight, knightPos.Item1, knightPos.Item2, Color.white, (GameObject go) => { go.transform.Rotate(new Vector3(0, 0, 90)); });

            var whiteBishopPos = new List<Tuple<int, int>> {
                new Tuple<int, int>(-1, -3),
                new Tuple<int, int>(2, -3),
            };
            foreach (var bishopPos in whiteBishopPos)
                InstantiatePiece(white_bishop, bishopPos.Item1, bishopPos.Item2, Color.white);

            InstantiatePiece(white_king, 0, -3, Color.white);
            InstantiatePiece(white_queen, 1, -3, Color.white);

            for (int i = -3; i <= 4; i++)
                InstantiatePiece(black_pawn, i, 3, Color.black, (GameObject go) => { go.transform.localPosition += Vector3.forward * 0.01f; });

            var blackRookPos = new List<Tuple<int, int>> {
                new Tuple<int, int>(-3, 4),
                new Tuple<int, int>(4, 4),
            };
            foreach (var rookPos in blackRookPos)
                InstantiatePiece(black_rook, rookPos.Item1, rookPos.Item2, Color.black);

            var blackKnightPos = new List<Tuple<int, int>> {
                new Tuple<int, int>(-2, 4),
                new Tuple<int, int>(3, 4)
            };
            foreach (var knightPos in blackKnightPos)
                InstantiatePiece(black_knight, knightPos.Item1, knightPos.Item2, Color.black, (GameObject go) => { go.transform.Rotate(new Vector3(0, 0, -90)); });

            var blackBishopPos = new List<Tuple<int, int>> {
                new Tuple<int, int>(-1, 4),
                new Tuple<int, int>(2, 4),
            };
            foreach (var bishopPos in blackBishopPos)
                InstantiatePiece(black_bishop, bishopPos.Item1, bishopPos.Item2, Color.black);

            InstantiatePiece(black_king, 0, 4, Color.black);
            InstantiatePiece(black_queen, 1, 4, Color.black);
        }
        else
        {
            boardPieces.Clear();

            foreach (var item in whitePieces)
            {
                item.Value.transform.localPosition = new Vector3(item.Key.Item1, item.Key.Item2, item.Value.transform.localPosition.z);
                var posUnit = GetUnitFromAxisScale(item.Key.Item1, item.Key.Item2);
                //Debug.LogFormat("white piece pos unit: {0}, {1}", posUnit.Item1, posUnit.Item2);
                boardPieces[posUnit] = item.Value;
                MouseMove.GetMoveLogic(item.Value).ResetStatus();
                item.Value.SetActive(true);
            }

            foreach (var item in blackPieces)
            {
                item.Value.transform.localPosition = new Vector3(item.Key.Item1, item.Key.Item2, item.Value.transform.localPosition.z);
                boardPieces[GetUnitFromAxisScale(item.Key.Item1, item.Key.Item2)] = item.Value;
                MouseMove.GetMoveLogic(item.Value).ResetStatus();
                item.Value.SetActive(true);
            }
        }

        handMark.touch = null;
        handMark.blocked = false;
    }
    public void PawnPromote(string tag, GameObject pawn, bool fromOpponent=false)
    {
        var color = pawn.GetComponent<PawnMove>().color;
        GameObject prefab = tag switch
        {
            "queen" => color == Color.white ? white_queen : black_queen,
            "knight" => color == Color.white ? white_knight : black_knight,
            "bishop" => color == Color.white ? white_bishop : black_bishop,
            "rook" => color == Color.white ? white_rook : black_rook,
            _ => null,
        };
        var posUnit = GetUnitFromAxisScale(pawn.transform.localPosition.x, pawn.transform.localPosition.y);
        ClearBoardPiece(posUnit);
        var go = InstantiatePiece(prefab, posUnit.Item1, posUnit.Item2, color, null, false);
        MouseMove.GetMoveLogic(go).MarkPromote();
        promotePieces.Add(go);

        SetActColor(color == Color.white ? Color.black : Color.white);

        if(fromOpponent == false)
            manager.SwitchUI(1);
    }
}
