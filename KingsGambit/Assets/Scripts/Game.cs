using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    public static Game M;

    [Header("Selection")]
    public CustomPiece Selected;
    public Tile TargetTile;

    [Header("Turn Management")]
    public string Turn = "White";
    private int turnVal = 0;

    [Header("Piece Management")]
    public GameObject WhiteParentObj;
    private CustomPiece WhiteKing;
    private CustomPiece[] WhitePieces;

    public GameObject BlackParentObj;
    private CustomPiece BlackKing;
    private CustomPiece[] BlackPieces;

    public bool InCheck;

    public Animation CamPivotAnim;

    private void Awake()
    {
        if (M == null)
        {
            M = this;
        }
        else if (M != this)
        {
            Destroy(this);
        }

        Turn = "White";
    }

    // Start is called before the first frame update
    void Start()
    {
        WhitePieces = WhiteParentObj.GetComponentsInChildren<CustomPiece>();
        foreach (CustomPiece piece in WhitePieces)
            if (piece.Type == "King")
                WhiteKing = piece;

        BlackPieces = BlackParentObj.GetComponentsInChildren<CustomPiece>();
        foreach (CustomPiece piece in BlackPieces)
            if (piece.Type == "King")
                BlackKing = piece;
    }

    // Update is called once per frame
    void Update()
    {
        if(Selected && TargetTile)
        {
            Selected.Move();
            NextTurn();

            if (Turn == "White")
                CheckStatus(WhiteKing);
            if (Turn == "Black")
                CheckStatus(BlackKing);
        }


        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            foreach (CustomPiece piece in BlackPieces)
            {
                if (piece.Type == "Pawn")
                    piece.PawnShowAttack();
                else
                    piece.Highlight(true);
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            foreach (CustomPiece piece in BlackPieces)
            {
                piece.Restart();
            }
        }


        if (Input.GetKeyDown(KeyCode.S))
        {
            CheckStatus(WhiteKing);
            CheckStatus(BlackKing);
        }
    }

    void NextTurn()
    {
        if (turnVal + 1 < 2)
            turnVal++;
        else
            turnVal = 0;

        switch (turnVal)
        {
            case 0:
                Turn = "White";
                CamPivotAnim["CameraPivot"].time = 1f;
                CamPivotAnim["CameraPivot"].speed = -1;
                CamPivotAnim.Play();
                break;
            case 1:
                Turn = "Black";
                CamPivotAnim["CameraPivot"].time = 0;
                CamPivotAnim["CameraPivot"].speed = 1;
                CamPivotAnim.Play();
                break;
        }

        UI.M.TurnText.text = Turn + "'s Turn";
    }

    #region Check/Checkmate State

    void CheckStatus(CustomPiece King)
    {
        CustomPiece[] Enemies = null;

        switch (King.Side)
        {
            case "White":
                Enemies = BlackPieces;
                break;
            case "Black":
                Enemies = WhitePieces;
                break;
        }

        foreach (CustomPiece piece in Enemies)
        {
            if (piece.Type == "Pawn")
                piece.PawnShowAttack();
            else
                piece.Highlight(true);
        }

        InCheck = Grid.M.Tiles[King.posX, King.posY].l.enabled;

        foreach (CustomPiece piece in Enemies)
        {
            piece.Restart();
        }
    }

    #endregion
}