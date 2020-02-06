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
    }

    public static class Black
    {
        public static List<CustomPiece> All = new List<CustomPiece>();
        public static CustomPiece King;
        public static CustomPiece Queen;
    }

    [Header("Game State")]
    public int RoundCount = 1;

    public bool InCheck;
    public int CheckCount;
    public bool CheckBlockable;
    public List<Tile> AttackPath;
    public GameObject WhiteSide;
    private int whiteSideCount;
    public GameObject BlackSide;
    private int blackSideCount;

    public Dictionary<Tile, CustomPiece> EPTiles = new Dictionary<Tile, CustomPiece>();

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
                }
            }

            p.SetPos();
        }

        CalcMovePaths();
    }

    #region Turn Management
    void NextTurn()
    {
        RoundCount++;
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
        EPTiles.Clear();

        foreach (CustomPiece p in AllPieces)
        {
            p.Guarded = false;
            p.ResetHit();

            if (p.Injured && RoundCount - p.RoundInjured == 3)
                p.Injured = false;

            if(p.CancelEP)
                p.EPTake = false;

            if(!p.FirstMove)
                p.CancelEP = true;
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

        if(InCheck)
        {
            if(White.King.Checker)
            {
                foreach (CustomPiece p in White.All)
                {
                    p.CheckBlock();
                }
            }

            if (Black.King.Checker)
            {
                foreach (CustomPiece p in Black.All)
                {
                    p.CheckBlock();
                }
            }
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
                foreach (Tile t in piece.Path)
                {
                    t.Safe = false;
                }

                List<Tile> path = piece.LineOfSight(King);
    
                if (piece.Path.Contains(King.Pos))
                {
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

    #region Piece Management
    public void Kill(CustomPiece p)
    {
        switch (p.Side)
        {
            case "White":
                if (White.All.Contains(p))
                    White.All.Remove(p);

                p.gameObject.transform.parent = WhiteSide.transform;
                whiteSideCount++;
                p.gameObject.transform.localPosition = SidePos(whiteSideCount, 1);
                break;

            case "Black":
                if (Black.All.Contains(p))
                    Black.All.Remove(p);

                p.gameObject.transform.parent = BlackSide.transform;
                blackSideCount++;
                p.gameObject.transform.localPosition = SidePos(blackSideCount, -1);
                break;
        }
    }

    Vector3 SidePos(int count, int offset)
    {
        float x;
        float z;

        if (count % 2 == 0)
            x = 1.5f;
        else
            x = -1.5f;

        if (count > 2)
            z = (offset * 3.0f) * (count / 2);
        else
            z = 0;

        return new Vector3(x, 0, z);
    }

    #endregion
}