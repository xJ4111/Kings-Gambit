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
    public List<CustomPiece> AllPieces;
    public static class White
    {
        public static List<CustomPiece> All = new List<CustomPiece>();
        public static CustomPiece King;
        public static CustomPiece Queen;
        public static List<CustomPiece> Pawns = new List<CustomPiece>();
        public static List<CustomPiece> Rooks = new List<CustomPiece>();
        public static List<CustomPiece> Bishops = new List<CustomPiece>();
        public static List<CustomPiece> Knights = new List<CustomPiece>();
    }

    public static class Black
    {
        public static List<CustomPiece> All = new List<CustomPiece>();
        public static CustomPiece King;
        public static CustomPiece Queen;
        public static List<CustomPiece> Pawns = new List<CustomPiece>();
        public static List<CustomPiece> Rooks = new List<CustomPiece>();
        public static List<CustomPiece> Bishops = new List<CustomPiece>();
        public static List<CustomPiece> Knights = new List<CustomPiece>();
    }

    public bool InCheck;
    public int CheckCount;
    public bool CheckBlockable;
    public List<Tile> AttackPath;

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
        foreach (CustomPiece p in PieceParentObj.GetComponentsInChildren<CustomPiece>())
            AllPieces.Add(p);

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

        if (Input.GetKeyDown(KeyCode.R))
        {
            NextTurn();
        }
    }

    IEnumerator Initialise()
    {
        yield return new WaitForEndOfFrame();

        foreach (CustomPiece p in AllPieces)
        {
            if (p.name.Contains("White"))
            {
                White.All.Add(p);

                switch (p.name.Split(' ')[1])
                {
                    case "King":
                        White.King = p;
                        break;
                    case "Queen":
                        White.Queen = p;
                        break;
                    case "Pawn":
                        White.Pawns.Add(p);
                        break;
                    case "Bishop":
                        White.Bishops.Add(p);
                        break;
                    case "Knight":
                        White.Knights.Add(p);
                        break;
                    case "Rook":
                        White.Rooks.Add(p);
                        break;
                }
            }

            if (p.name.Contains("Black"))
            {
                Black.All.Add(p);

                switch (p.name.Split(' ')[1])
                {
                    case "King":
                        Black.King = p;
                        break;
                    case "Queen":
                        Black.Queen = p;
                        break;
                    case "Pawn":
                        Black.Pawns.Add(p);
                        break;
                    case "Bishop":
                        Black.Bishops.Add(p);
                        break;
                    case "Knight":
                        Black.Knights.Add(p);
                        break;
                    case "Rook":
                        Black.Rooks.Add(p);
                        break;
                }
            }

            p.SetPos();
        }

        CalcMovePaths();
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

        foreach (CustomPiece p in AllPieces)
        {
            p.CheckPinning();
        }

        if (Turn == "White")
        {
            CheckStatus(White.King);
            White.King.CheckCastling();
        }
        if (Turn == "Black")
        {
            CheckStatus(Black.King);
            Black.King.CheckCastling();
        }

        foreach (CustomPiece p in AllPieces)
        {
            p.CheckBlock();
        }

    }
    #endregion

    #region Check/Checkmate State
    void CheckStatus(CustomPiece King)
    {
        List<CustomPiece> Enemies = new List<CustomPiece>();
        King.Checker = null;
        InCheck = false;
        CheckCount = 0;
        AttackPath = null;

        switch (King.Side)
        {
            case "White":
                Enemies = Black.All;
                break;
            case "Black":
                Enemies = White.All;
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

                    if(t == Grid.M.Tiles[King.PosX, King.PosY])
                    {
                        InCheck = true;
                        CheckCount++;

                        if (!King.Checker)
                            King.Checker = piece;
                    }
                }
            }
            else
            {
                List<Tile> path = piece.LineOfSight(King);
    
                if (path.Count > 0 && piece.Path.Contains(King.Pos))
                {
                    foreach (Tile t in path)
                    {
                        t.Safe = false;
                    }

                    InCheck = true;
                    CheckCount++;

                    if (!King.Checker)
                        King.Checker = piece;
                }
                else
                {
                    foreach (Tile t in piece.Path)
                    {
                        t.Safe = false;
                    }
                }
            }
        }

        CheckBlockable = CheckCount <= 1;

        if(InCheck && CheckBlockable)
        {
            AttackPath = King.Checker.LineOfSight(King);
        }

        King.CheckPath();

        if(InCheck && !CheckBlockable && King.Path.Count == 0)
        {
            Debug.Log("CHECKMATE!");
        }
    }
    #endregion
}