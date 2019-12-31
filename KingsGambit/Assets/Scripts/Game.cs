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
    public GameObject PieceParentObj;
    [SerializeField] private CustomPiece[] AllPieces;

    [SerializeField] private CustomPiece WhiteKing;
    private List<CustomPiece> WhitePawns;
    private List<CustomPiece> WhitePieces;

    [SerializeField] private CustomPiece BlackKing;
    private List<CustomPiece> BlackPawns;
    private List<CustomPiece> BlackPieces;

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
        AllPieces = PieceParentObj.GetComponentsInChildren<CustomPiece>();
        WhitePawns = new List<CustomPiece>();
        WhitePieces = new List<CustomPiece>();
        BlackPawns = new List<CustomPiece>();
        BlackPieces = new List<CustomPiece>();

        StartCoroutine(Initialise());
    }

    // Update is called once per frame
    void Update()
    {
        if(Selected && TargetTile)
        {
            Selected.Move();
            NextTurn();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Selected)
                Selected.Unselect();
        }
    }

    IEnumerator Initialise()
    {
        yield return new WaitForEndOfFrame();

        foreach (CustomPiece p in AllPieces)
        {
            if (p.name.Contains("White"))
            {
                WhitePieces.Add(p);

                if (p.name.Contains("Pawn"))
                    WhitePawns.Add(p);

                if (p.name.Contains("King"))
                    WhiteKing = p;
            }

            if (p.name.Contains("Black"))
            {
                BlackPieces.Add(p);

                if (p.name.Contains("Pawn"))
                    BlackPawns.Add(p);

                if (p.name.Contains("King"))
                    BlackKing = p;
            }

            p.SetPos();
        }

        CalcMovePaths();
        CheckStatus(WhiteKing);
    }

    #region Turn Management
    void NextTurn()
    {
        Selected = null;
        TargetTile = null;

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

        CalcMovePaths();

        if (Turn == "White")
            CheckStatus(WhiteKing);
        if (Turn == "Black")
            CheckStatus(BlackKing);
    }

    void CalcMovePaths()
    {
        foreach (CustomPiece p in AllPieces)
        {
            p.Guarded = false;
            p.ResetHit();
        }

        foreach (CustomPiece p in AllPieces)
        {
            p.CheckPath();
        }
    }
    #endregion

    #region Check/Checkmate State
    void CheckStatus(CustomPiece King)
    {
        List<CustomPiece> Enemies = new List<CustomPiece>();
        InCheck = false;

        switch (King.Side)
        {
            case "White":
                Enemies = BlackPieces;
                break;
            case "Black":
                Enemies = WhitePieces;
                break;
        }

        foreach (Tile t in Grid.M.Tiles)
            t.Safe = true;

        foreach (CustomPiece piece in Enemies)
        {
            if(piece.Type == "Pawn")
            {
                foreach (Tile t in piece.PawnAttackTiles)
                {
                    t.Safe = false;

                    if (!InCheck && t == Grid.M.Tiles[King.posX, King.posY])
                        InCheck = true;
                }
            }
            else
            {
                foreach (Tile t in piece.Path)
                {
                    t.Safe = false;

                    if (!InCheck && t == Grid.M.Tiles[King.posX, King.posY])
                        InCheck = true;
                }
            }
        }

        King.CheckPath();
    }
    #endregion
}