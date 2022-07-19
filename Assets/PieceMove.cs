using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PieceMove : MonoBehaviour, IPieceController
{
    public GridCalc calculator;
    public MouseMove mark;
    public GameObject tile;

    public Color color;
    public Status status;

    protected List<GameObject> availableGridTiles = new List<GameObject>();
    protected Vector3 pickUpPos;

    public bool fromPromote = false;

    protected void Start()
    {
        calculator = GameObject.Find("Chess Board/Chess Board Play Surface").GetComponent<GridCalc>();
        mark = GameObject.Find("StylizedHand").GetComponent<MouseMove>();
        color = GetComponent<MeshRenderer>().material.color;
        pickUpPos = transform.localPosition;

        GenMovableGrid();
    }

    public virtual void ResetStatus() { ClearTiles(); }

    public Color GetColor() {  return color; }

    public virtual void GenMovableGrid() { }

    public virtual void SetStatus(Status newStatus, bool forceRefresh = false)
    {
        if (status != newStatus)
        {
            status = newStatus;
            switch (status)
            {
                case Status.Normal:
                    if (availableGridTiles.Count > 0)
                        HidePath();

                    break;

                case Status.PickUp:
                    pickUpPos = transform.localPosition;

                    ClearTiles();
                    GenMovableGrid();

                    ShowPath();

                    break;
            }
        }
    }
    public virtual void ShowPath()
    {
        foreach (var tile in availableGridTiles)
            tile.SetActive(true);
    }

    public virtual void HidePath()
    {
        foreach (var tile in availableGridTiles)
            tile.SetActive(false);
    }

    public bool CheckPutRule(Vector3 curPos, bool opponent=false)
    {
        if (opponent)
        {
            ClearTiles();
            GenMovableGrid();
        }

        if (availableGridTiles.Count == 0)
            return false;

        var gridUnit = calculator.GetUnitFromAxisScale(curPos.x, curPos.y);
        foreach (var tile in availableGridTiles)
        {
            var tileUnit = calculator.GetUnitFromAxisScale(tile.transform.localPosition.x, tile.transform.localPosition.y);
            if (tileUnit.Item1 == gridUnit.Item1 && tileUnit.Item2 == gridUnit.Item2)
                return true;
        }

        return false;
    }

    protected void InstantiateTiles(List<Tuple<float, float>> possibleAxis)
    {
        foreach (var item in possibleAxis)
        {
            var putTile = Instantiate(tile, calculator.transform, false);
            putTile.transform.localPosition = calculator.GetGridCentralPos(new Vector3(item.Item1, item.Item2, 0.0001f));
            putTile.transform.localScale /= 16;
            putTile.transform.Rotate(90, 0, 0);
            putTile.SetActive(false);

            availableGridTiles.Add(putTile);

            calculator.AddAttackGrid(calculator.GetUnitFromAxisScale(item.Item1, item.Item2), color);
        }
    }

    public void ClearTiles()
    {
        foreach (var tile in availableGridTiles)
        {
            calculator.RemoveAttackGrid(calculator.GetUnitFromAxisScale(tile.transform.localPosition.x, tile.transform.localPosition.y), color);
            Destroy(tile);
        }

        availableGridTiles.Clear();
    }

    public virtual void PutCallback(bool isOpponent=false)
    {
        calculator.SetActColor(color == Color.white ? Color.black : Color.white);
    }

    public void MarkPromote()
    {
        fromPromote = true;
    }

    public bool IsFromPromote()
    {
        return fromPromote;
    }
}
