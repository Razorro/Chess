using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sync;
using System;

public class MouseMove : MonoBehaviour
{
    public GameObject hand;
    public GameObject touch = null;
    public Vector3 lastPos, holdOffset;

    public GridCalc calculator;

    public LayerMask chessLayer, boardLayer;

    public float handOffset = 3f;
    
    public Camera mainCamera;

    public bool blocked = false;

    public AudioClip put, pick;
    private AudioSource player;

    public NetPacket codec;

    private void Start()
    {
        player = GetComponent<AudioSource>();

        Input.simulateMouseWithTouches = true;

        codec.OnRecvMove += MoveOpponentPiece;
        codec.OnRecvPromote += SyncPromoteFrom;
    }

    void MoveOpponentPiece(GameObject piece, Tuple<int, int> putUnit, Tuple<int, int> origin)
    {
        var moveLogic = GetMoveLogic(piece);
        var axis = calculator.GetAxisScaleFromUnit(putUnit.Item1, putUnit.Item2);
        if (moveLogic.CheckPutRule(new Vector3(axis.Item1, axis.Item2, 0), true) && calculator.CheckActColor(moveLogic.GetColor()))
        {
            var originAxis = calculator.GetAxisScaleFromUnit(origin.Item1, origin.Item2);
            var endGame = calculator.PutPieceInGrid(piece.transform, new Vector3(originAxis.Item1, originAxis.Item2, 0), putUnit);
            moveLogic.SetStatus(Status.Normal);
            if (endGame)
                return;

            moveLogic.PutCallback(true);
            moveLogic.ClearTiles();
            moveLogic.GenMovableGrid();
            calculator.UpdatePieceStatus(moveLogic.GetColor());

            player.PlayOneShot(put);
        }
        else
            Debug.Log("invalid opponent movement");
    }

    // Update is called once per frame
    void Update()
    {
        if (blocked)
            return;

        if(Input.GetMouseButtonDown(0))
        {
            if (touch != null)
            {
                touch.transform.position += Vector3.up * 5f;

                var moveLogic = GetMoveLogic(touch);
                if (moveLogic != null)
                    moveLogic.SetStatus(Status.PickUp);

                player.PlayOneShot(pick);
            }
        }
        if(Input.GetMouseButtonUp(0))
        {
            if (touch)
            {
                var moveLogic = GetMoveLogic(touch);
                if (calculator.CheckActColor(moveLogic.GetColor()) && calculator.CheckPlayerColor(moveLogic.GetColor()) && moveLogic.CheckPutRule(touch.transform.localPosition))
                {
                    var endGame = calculator.PutPieceInGrid(touch.transform, lastPos);
                    touch.transform.position -= Vector3.up * 5f;

                    var origin = calculator.GetUnitFromAxisScale(lastPos.x, lastPos.y);
                    var later = calculator.GetUnitFromAxisScale(touch.transform.localPosition.x, touch.transform.localPosition.y);

                    codec.SyncMove(origin, later, moveLogic.GetColor());

                    if (endGame)
                    {
                        moveLogic.SetStatus(Status.Normal);
                        holdOffset = Vector3.zero;
                        return;
                    }

                    moveLogic.PutCallback();
                    moveLogic.ClearTiles();
                    moveLogic.GenMovableGrid();
                    calculator.UpdatePieceStatus(moveLogic.GetColor());
                }
                else
                    touch.transform.localPosition = lastPos;

                player.PlayOneShot(put);

                moveLogic.SetStatus(Status.Normal);
                holdOffset = Vector3.zero;
            }
        }
        else if(Input.GetMouseButton(0))
        {
            if(touch != null)
            {
                var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, chessLayer))
                {
                    if(holdOffset == Vector3.zero)
                        holdOffset = touch.transform.position - hit.point;

                    touch.transform.position = new Vector3(hit.point.x + holdOffset.x, touch.transform.position.y, hit.point.z + holdOffset.z);
                    hand.transform.position = new Vector3(touch.transform.position.x, hand.transform.position.y, touch.transform.position.z + handOffset);
                }
            }
        }
        else
        {
            var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit raycastHit, Mathf.Infinity, boardLayer))
            {
                touch = raycastHit.transform.gameObject;
                lastPos = touch.transform.localPosition;
                hand.transform.position = new Vector3(touch.transform.position.x, hand.transform.position.y, touch.transform.position.z + handOffset);
            }
            else
                touch = null;
        }
    }

    public static IPieceController GetMoveLogic(GameObject piece)
    {
        switch(piece.tag)
        {
            case "bishop":
                return piece.GetComponent<BishopMove>();
            case "rook":
                return piece.GetComponent<RookMove>();
            case "king":
                return piece.GetComponent<KingMove>();
            case "queen":
                return piece.GetComponent<QueenMove>();
            case "pawn":
                return piece.GetComponent<PawnMove>();
            case "knight":
                return piece.GetComponent<KnightMove>();
            default:
                return null;
        }
    }
    public void PawnPromote(string tag)
    {
        if (touch == null)
        {
            Debug.LogError("None piece is selected");
            return;
        }

        if(touch.CompareTag("pawn") == false)
        {
            Debug.LogError("Current piece is not pawn");
            return;
        }

        calculator.PawnPromote(tag, touch);

        blocked = false;
    }

    public void SyncPromoteFrom(Promote promote)
    {
        var posUnit = new Tuple<int, int>(promote.X, promote.Y);
        if(calculator.boardPieces.ContainsKey(posUnit) == false)
        {
            Debug.Log("invild promote piece pos");
            return;
        }

        var piece = calculator.boardPieces[posUnit];
        calculator.PawnPromote(promote.Tag, piece, true);
    }

    public void SyncPromoteTo(string tag, GameObject piece)
    {
        codec.SyncPromoteAct(tag, calculator.GetUnitFromAxisScale(piece.transform));
    }

    private void MoveOpponentPiece(MovePiece movement)
    {
        if (!calculator.CheckActColor(movement.Color == Sync.Color.White ? UnityEngine.Color.white : UnityEngine.Color.black))
        {
            Debug.Log("invalid opponent movement");
            return;
        }

        Debug.Log("opponent movement data: " + movement);

        var originUnit = new Tuple<int, int>(movement.OriginX, movement.OriginY);
        var piece = calculator.boardPieces[originUnit];
        Debug.Log("move piece from opponent: " + piece.name);
        MoveOpponentPiece(piece, new Tuple<int, int>(movement.X, movement.Y), originUnit);
    }
}
