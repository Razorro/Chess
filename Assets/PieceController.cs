using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum Status { Normal, PickUp };
public enum PawnStatus { First, EnPassant, Normal };
public enum KingStatus { First, Normal};
public enum RookStatus { First, Normal};

public interface IPieceController 
{
    public void ShowPath();
    public void SetStatus(Status state, bool forceRefresh=false);
    public bool CheckPutRule(Vector3 curPos, bool isOpponent=false);
    public void PutCallback(bool isOpponent=false);
    public Color GetColor();
    public void ClearTiles();
    public void GenMovableGrid();
    public void MarkPromote();
    public bool IsFromPromote();
    public void ResetStatus();
}
