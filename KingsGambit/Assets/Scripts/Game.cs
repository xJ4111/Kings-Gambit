using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    public static Game M;

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

    [Header("Selection")]
    public CustomPiece Selected;
    public Tile TargetTile;

    [Header("Ability Targeting")]
    public bool SearchingPiece;
    public CustomPiece AbilityTarget;

    public bool NeedPos;
    public bool SearchingPos;
    public Tile AbilityPosition;

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

        public static CustomPiece FTKTarget;
        public static int FTKRound;

        public static int DefencePower;
        public static int InvasionPower;
    }

    public static class Black
    {
        public static List<CustomPiece> All = new List<CustomPiece>();
        public static CustomPiece King;
        public static CustomPiece Queen;

        public static CustomPiece FTKTarget;
        public static int FTKRound;

        public static int DefencePower;
        public static int InvasionPower;
    }

    [Header("Game State")]
    public int RoundCount = 1;

    public bool InCheck;
    public int CheckCount;
    public bool CheckBlockable;
    public List<Tile> AttackPath;
    public GameObject WhiteSide;
    public GameObject BlackSide;

    private int whiteSideCount;
    private int blackSideCount;

    public Dictionary<Tile, CustomPiece> EPTiles = new Dictionary<Tile, CustomPiece>();
    public Animation CamPivotAnim;

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

        if(Selected && AbilityTarget)
        {
            if(NeedPos && AbilityPosition)
            {
                Selected.UseAbility();
            }
            else if(!NeedPos)
            {
                Selected.UseAbility();
            }
        }

        if (Input.GetKeyDown(KeyCode.C))
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

        White.All.Clear();
        Black.All.Clear();

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
    public void NextTurn()
    {
        RoundCount++;

        Clear();

        if (turnVal + 1 < 2)
            turnVal++;
        else
            turnVal = 0;

        CameraFlip();

        CalcMovePaths();

        UI.M.UpdateTurn();
    }

    public void Clear()
    {
        if(Selected)
            Selected.Unselect();

        TargetTile = null;
        AbilityTarget = null;
        AbilityPosition = null;
        NeedPos = false;
        SearchingPiece = false;
        SearchingPos = false;
    }

    void CameraFlip()
    {
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
    }

    public void CalcMovePaths()
    {
        FTKState();

        EPTiles.Clear();

        foreach (CustomPiece p in AllPieces)
        {
            p.Guarded = false;
            p.ResetHit();

            if(p.Type == "Pawn")
                p.EPCheck();

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

        InvasionCheck();
    }
    #endregion

    #region Check/Checkmate State
    void CheckStatus(CustomPiece King)
    {
        List<CustomPiece> Enemies = new List<CustomPiece>();
        string enemy = "";
        King.Checker = null;
        InCheck = false;
        CheckCount = 0;
        AttackPath = null;

        switch (King.Side)
        {
            case "White":
                enemy = "Black";
                Enemies = Black.All;
                break;
            case "Black":
                enemy = "White";
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

        if(InCheck)
        {
            AttackPath = King.Checker.LineOfSight(King);
        }

        King.CheckPath();

        if(InCheck && King.Path.Count == 0)
        {
            BlockCheck(King);

            if(!CheckBlockable)
            {
                Debug.Log("Checkmate");
                UI.M.GameOver(enemy, King.Side, "Checkmate");
            }
            else
            {
                Debug.Log("Blockable");
            }
        }
    }

    void BlockCheck(CustomPiece King)
    {
        CheckBlockable = false;
        List<CustomPiece> temp = null;

        switch (King.Side)
        {
            case "White":
                temp = White.All;
                break;
            case "Black":
                temp = Black.All;
                break;
        }

        foreach(CustomPiece p in temp)
        {
            foreach(Tile t in p.Path)
            {
                if (AttackPath.Contains(t))
                    CheckBlockable = true;
            }
        }
    }
    #endregion

    #region Piece Management
    public void Kill(CustomPiece p)
    {
        AllPieces.Remove(p);
        p.Pos.Exit(p);

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

        p.Pos.Graves.Add(p);
        p.enabled = false;
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

    #region Game State Checks
    void FTKState()
    {
        if(White.FTKTarget)
        {
            if (RoundCount - White.FTKRound == 2)
            {
                White.FTKTarget.Demote();
                White.FTKTarget = null;
            }
        }


        if(Black.FTKTarget)
        {
            if (RoundCount - Black.FTKRound == 2)
            {
                Black.FTKTarget.Demote();
                Black.FTKTarget = null;
            }
        }
    }

    void InvasionCheck()
    {
        White.DefencePower = 0;
        White.InvasionPower = 0;

        foreach (CustomPiece p in White.All)
        {
            if (p.Type != "Pawn")
            {
                if (p.PosY >= 4)
                    White.DefencePower++;
                else
                    White.InvasionPower++;
            }
        }

        Black.DefencePower = 0;
        Black.InvasionPower = 0;

        foreach (CustomPiece p in Black.All)
        {
            if (p.Type != "Pawn")
            {
                if (p.PosY <= 3)
                    Black.DefencePower++;
                else
                    Black.InvasionPower++;
            }
        }

        if (White.InvasionPower > Black.DefencePower)
            UI.M.GameOver("White", "Black", "Invasion");

        if (Black.InvasionPower > White.DefencePower)
            UI.M.GameOver("Black", "White", "Invasion");
    }
    #endregion
}