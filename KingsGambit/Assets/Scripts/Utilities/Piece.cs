using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour
{
    [Header("Piece Info")]
    public string Side;
    public string Type;

    public List<Tile> Path = new List<Tile>();
    public List<Tile> Potential = new List<Tile>();

    public bool Pinned;
    public bool Guarded;
    public bool CanBlock;

    [Header("Position")]
    public Tile Pos;
    public int PosX, PosY;
    protected Tile[,] Tiles;
    public int activeTiles;


    [Header("State")]
    public bool FirstMove = true;
    public bool CancelEP = false;
    [HideInInspector] public bool[] hit = new bool[4];
    public bool Invincible = false;
    public bool Injured = false;
    public int RoundInjured = 0;

    [Header("Pawn Only")]
    public List<Tile> PawnAttackTiles = new List<Tile>();
    public bool Vulnerable = true;
    public bool EPTake = false;
    protected int startingY;

    [Header("King Related")]
    public CustomPiece AllyKing;
    public CustomPiece AllyQueen;
    public CustomPiece Checker;
    protected int startingX;
    public System.Tuple<CustomPiece, List<Tile>> CastleLeft;
    public System.Tuple<CustomPiece, List<Tile>> CastleRight;
    public bool FTKUsed = false;

    [Header("Movement Controller")]
    public Movement MC;

    public void SetPos()
    {
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (transform.position == Grid.M.Tiles[i, j].transform.position)
                {
                    PosX = i;
                    PosY = j;
                    startingX = i;
                    startingY = j;
                    Pos = Grid.M.Tiles[i, j];
                    Pos.Enter(this);
                }
            }
        }
    }

    protected void Attack()
    {
        //Attack
        if (Game.M.TargetTile.Occupier)
        {
            MC.TargetPiece = Game.M.TargetTile.Occupier;
        }

        //En Passant Attack
        if(Game.M.EPTiles.ContainsKey(Game.M.TargetTile) && Game.M.EPTiles[Game.M.TargetTile].Side != Side)
        {
            MC.TargetPiece = Game.M.EPTiles[Game.M.TargetTile];
        }
    }

    #region PathProcessing

    public void CheckPinning()
    {
        bool KingInPath = false;
        CustomPiece PieceInPath = null;
        int piecesInPath = 0;
        CustomPiece EnemyKing = null;

        switch (Side)
        {
            case "White":
                EnemyKing = Game.Black.King;
                break;
            case "Black":
                EnemyKing = Game.White.King;
                break;
        }

        List<Tile> TempList = LineOfSight(EnemyKing);

        foreach (Tile t in FullPath())
        {
            if (t.Occupier && t.Occupier.Side != Side)
            {
                if (t.Occupier.Type == "King")
                    KingInPath = true;
                else
                {
                    if (TempList.Contains(t))
                    {
                        PieceInPath = t.Occupier;
                        piecesInPath++;
                    }
                }
            }
        }

        if (KingInPath && PieceInPath && piecesInPath == 1)
        {
            PieceInPath.PinnedMovement(TempList);
        }
    }

    public void PinnedMovement(List<Tile> LOS)
    {
        List<Tile> tempPath = Clone(Path);

        if (LOS.Count > 0)
        {
            foreach (Tile t in tempPath)
            {
                if (!LOS.Contains(t))
                {
                    Path.Remove(t);
                }
            }
        }
    }
    List<Tile> Clone(List<Tile> list)
    {
        List<Tile> copy = new List<Tile>();

        foreach (Tile t in list)
        {
            copy.Add(t);
        }

        return copy;
    }

    public void CheckBlock()
    {
        if(Type != "King")
        {
            CanBlock = false;

            if (Game.M.InCheck)
            {
                foreach (Tile t in Game.M.AttackPath)
                {
                    if (Path.Contains(t))
                    {
                        CanBlock = true;
                    }
                }

                if (Path.Contains(AllyKing.Checker.Pos))
                    CanBlock = true;

                Tile EPCheckBlock = null;

                foreach (KeyValuePair<Tile, CustomPiece> temp in Game.M.EPTiles)
                {
                    if (temp.Value == AllyKing.Checker)
                    {
                        if (Path.Contains(temp.Key))
                        {
                            CanBlock = true;
                            EPCheckBlock = temp.Key;
                        }
                    }
                }

                if (CanBlock)
                {
                    List<Tile> copy = Clone(Path);
                    foreach (Tile t in copy)
                    {
                        if (!Game.M.AttackPath.Contains(t) && t != AllyKing.Checker.Pos && t != EPCheckBlock)
                        {
                            Path.Remove(t);
                        }
                    }
                }
            }
        }
    }

    public List<Tile> FullPath()
    {
        //Pinner Line of Sight
        List<Tile> all = new List<Tile>();

        foreach (Tile t in Path)
            all.Add(t);
        foreach (Tile t in Potential)
            all.Add(t);

        return all;
    }

    #region LineOfSight
    public List<Tile> LineOfSight(CustomPiece King)
    {
        switch (Type)
        {
            case "Rook":
                return RookLOS(King);
            case "Bishop":
                return BishopLOS(King);
            case "Queen":
                return QueenLOS(King);
        }

        return new List<Tile>();
    }

    public List<Tile> RookLOS(CustomPiece King)
    {
        List<Tile> path = new List<Tile>();

        if (King.Pos.PosY != PosY && King.Pos.PosX != PosX)
        {
            return path;
        }
        else if (King.Pos.PosY == PosY)
        {
            if (King.Pos.PosX > PosX)
            {
                path = SeePath(1, 0);
            }
            if (King.Pos.PosX < PosX)
            {
                path = SeePath(-1, 0);
            }
        }
        else if (King.Pos.PosX == PosX)
        {
            if (King.Pos.PosY < PosY)
            {
                path = SeePath(0, -1);
            }
            if (King.Pos.PosY > PosY)
            {
                path = SeePath(0, 1);
            }
        }

        if (path.Contains(King.Pos))
        {
            return path;
        }
        else
        {
            path.Clear();
        }

        return path;
    }
    public List<Tile> BishopLOS(CustomPiece King)
    {
        List<Tile> path = new List<Tile>();

        if (King.Pos.PosY == PosY || King.Pos.PosX == PosX)
        {
            return path;
        }
        else if (King.Pos.PosY > PosY) //below
        {
            if (King.Pos.PosX > PosX) //right
            {
                path = SeePath(1, 1);
            }
            else if (King.Pos.PosX < PosX) //left
            {
                path = SeePath(-1, 1);
            }
        }
        else if (King.Pos.PosY < PosY) //above
        {
            if (King.Pos.PosX > PosX) //right
            {
                path = SeePath(1, -1);
            }
            else if (King.Pos.PosX < PosX) //left
            {
                path = SeePath(-1, -1);
            }
        }

        if (path.Contains(King.Pos))
        {
            return path;
        }
        else
        {
            path.Clear();
        }

        return path;
    }

    public List<Tile> QueenLOS(CustomPiece King)
    {
        if (King.PosX == PosX || King.PosY == PosY)
            return RookLOS(King);
        else
            return BishopLOS(King);
    }

    List<Tile> SeePath(int xDir, int yDir)
    {
        List<Tile> path = new List<Tile>();
        int x = PosX;
        int y = PosY;

        while (InRange(x, y))
        {
            path.Add(Tiles[x, y]);
            x += xDir;
            y += yDir;
        }

        return path;
    }
    #endregion

    #endregion

    #region Movement Checks
    public void MoveTo(Tile TargetTile)
    {
        MC.TargetTile = TargetTile;
        MC.LastPos = Pos;

        if (Type != "Knight")
            MC.Move(true);
        else
            MC.Move(false);

        Tiles[PosX, PosY].Exit(this);
        TargetTile.Enter(this);
    }
    protected void CheckMove(int x, int y)
    {
        if (InRange(x, y))
        {
            if (!Tiles[x, y].Occupier)
            {
                Path.Add(Tiles[x, y]);
                activeTiles++;
            }
        }
    }
    protected void CheckAttack(int x, int y)
    {
        if (InRange(x, y))
        {
            EnPassant(x, y);

            if (Tiles[x, y] && Tiles[x, y].Occupier)
            {
                if (Tiles[x, y].Occupier.Side != Side)
                {
                    if (Type == "Pawn")
                        PawnAttackTiles.Add(Tiles[x, y]);

                    Path.Add(Tiles[x, y]);
                }
                else if (Tiles[x, y].Occupier.Side == Side)
                {
                    Tiles[x, y].Occupier.Guarded = true;
                }
            }
        }
    }

    protected void CheckMoveAttack(int x, int y)
    {
        if (InRange(x, y))
        {
            EnPassant(x, y);

            if (Tiles[x, y])
            {
                if ((Tiles[x, y].Occupier && Tiles[x, y].Occupier.Side != Side) || !Tiles[x, y].Occupier)
                {
                    if (Type == "Pawn")
                        PawnAttackTiles.Add(Tiles[x, y]);

                    Path.Add(Tiles[x, y]);
                }
                else if (Tiles[x, y].Occupier.Side == Side)
                {
                    Tiles[x, y].Occupier.Guarded = true;
                }
            }
        }
    }

    protected void ShowMoveAttack(int x, int y)
    {
        if (InRange(x, y))
        {
            if (Tiles[x, y])
            {
                if ((Tiles[x, y].Occupier && Tiles[x, y].Occupier.Side != Side && !Tiles[x, y].Occupier.Invincible) || !Tiles[x, y].Occupier)
                {
                    PawnAttackTiles.Add(Tiles[x, y]);
                }
                else if (Tiles[x, y].Occupier.Side == Side)
                {
                    Tiles[x, y].Occupier.Guarded = true;
                }
            }
        }
    }

    protected int Offset(int pos, int offset)
    {
        if (Side == "White")
        {
            if (pos - offset < 8 && pos - offset >= 0)
                return pos - offset;
        }
        else if (Side == "Black")
        {
            if (pos + offset < 8 && pos + offset >= 0)
                return pos + offset;
        }

        return pos;
    }


    protected bool InRange(int posX, int posY)
    {
        return posX < 8 && posX >= 0 && posY < 8 && posY >= 0;
    }

    public void ResetHit()
    {
        for (int i = 0; i < 4; i++)
            hit[i] = false;
    }

    #endregion

    #region Piece Movement
    protected virtual void Pawn()
    {
        EPCheck();
        PawnMove();
        PawnAttack();
        PawnShowAttack();
    }

    protected virtual void PawnMove()
    {
        //Front 2 Spaces
        CheckMove(PosX, Offset(PosY, 1));

        if (FirstMove && Path.Contains(Tiles[PosX, Offset(PosY, 1)]))
        {
            CheckMove(PosX, Offset(PosY, 2));
        }
    }

    protected virtual void PawnAttack()
    {
        //Diagonal Left
        if ((Offset(PosX, 1) != PosX) && (Offset(PosY, 1) != PosY))
            CheckAttack(Offset(PosX, 1), Offset(PosY, 1));

        //Diagonal Right
        if ((Offset(PosX, -1) != PosX && Offset(PosY, 1) != PosY))
            CheckAttack(Offset(PosX, -1), Offset(PosY, 1));
    }

    public void EPCheck()
    {
        Tile temp = Tiles[PosX, Offset(PosY, -1)];

        if (!Vulnerable)
        {
            if (EPTake)
            {
                if (Mathf.Abs(PosY - startingY) == 2)
                {
                    if (!Game.M.EPTiles.ContainsKey(temp))
                    {
                        Game.M.EPTiles.Add(temp, (CustomPiece)this);
                    }
                }
            }
        }
        else
        {
            if(!temp.Occupier)
            {
                if (!Game.M.EPTiles.ContainsKey(temp))
                {
                    Game.M.EPTiles.Add(temp, (CustomPiece)this);
                }
            }
            else
            {
                Game.M.EPTiles.Remove(temp);
            }
        }
    }

    protected void EnPassant(int x, int y)
    {
        if (Game.M.EPTiles.ContainsKey(Tiles[x, y]))
        {
            if ((Type == "Pawn" || Game.M.EPTiles[Tiles[x, y]].Vulnerable) && Game.M.EPTiles[Tiles[x, y]].Side != Side)
            {
                Path.Add(Tiles[x, y]);
            }
        }
    }

    public virtual void PawnShowAttack()
    {
        //Diagonal Left
        if ((Offset(PosX, 1) != PosX && Offset(PosY, 1) != PosY))
            ShowMoveAttack(Offset(PosX, 1), Offset(PosY, 1));

        //Diagonal Right
        if ((Offset(PosX, -1) != PosX && Offset(PosY, 1) != PosY))
            ShowMoveAttack(Offset(PosX, -1), Offset(PosY, 1));
    }

    protected virtual void PawnStateCheck()
    {
        if ((Side == "White" && PosY == 0) || (Side == "Black" && PosY == 8))
        {
            UI.M.TogglePromoPanel(true);
            UI.M.PromotionTarget = (CustomPiece)this;
        }
    }

    protected virtual void Rook()
    {
        for (int i = 1; i < 8; i++)
        {
            //Forward
            RookMovementY(i, ref hit[0]);

            //Backwards
            RookMovementY(-i, ref hit[1]);

            //Forward
            RookMovementX(i, ref hit[2]);

            //Backwards
            RookMovementX(-i, ref hit[3]);
        }
    }

    protected virtual void RookMovementY(int i, ref bool hit)
    {
        if (!hit)
        {
            if (Tiles[PosX, Offset(PosY, i)].Occupier)
            {
                if (Tiles[PosX, Offset(PosY, i)].Occupier.Side == Side || Tiles[PosX, Offset(PosY, i)].Occupier.Invincible)
                {
                    Potential.Add(Tiles[PosX, Offset(PosY, i)]);
                    CheckMoveAttack(PosX, Offset(PosY, i));
                    hit = true;
                }
                else if (Tiles[PosX, Offset(PosY, i)].Occupier.Side != Side)
                {
                    hit = true;
                    CheckMoveAttack(PosX, Offset(PosY, i));
                }
            }
            else if (!Tiles[PosX, Offset(PosY, i)].Occupier)
            {
                CheckMoveAttack(PosX, Offset(PosY, i));
            }
        }
        else if (hit)
            Potential.Add(Tiles[PosX, Offset(PosY, i)]);
    }

    protected virtual void RookMovementX(int i, ref bool hit)
    {
        if (!hit)
        {
            if (Tiles[Offset(PosX, i), PosY].Occupier)
            {
                if (Tiles[Offset(PosX, i), PosY].Occupier.Side == Side)
                {
                    Potential.Add(Tiles[Offset(PosX, i), PosY]);
                    CheckMoveAttack(Offset(PosX, i), PosY);
                    hit = true;
                }
                else if (Tiles[Offset(PosX, i), PosY].Occupier.Side != Side)
                {
                    hit = true;
                    CheckMoveAttack(Offset(PosX, i), PosY);
                }
            }
            else if (!Tiles[Offset(PosX, i), PosY].Occupier)
            {
                CheckMoveAttack(Offset(PosX, i), PosY);
            }
        }
        else if (hit)
            Potential.Add(Tiles[Offset(PosX, i), PosY]);
    }

    protected virtual void Bishop()
    {
        for (int i = 1; i < 8; i++)
        {
            BishopMovement(i, i, ref hit[0]);
            BishopMovement(i, -i, ref hit[1]);
            BishopMovement(-i, i, ref hit[2]);
            BishopMovement(-i, -i, ref hit[3]);
        }
    }

    protected virtual void BishopMovement(int x, int y, ref bool hit)
    {
        if (!hit && Offset(PosX, x) != PosX && Offset(PosY, y) != PosY)
        {
            if (Tiles[Offset(PosX, x), Offset(PosY, y)].Occupier)
            {
                if (Tiles[Offset(PosX, x), Offset(PosY, y)].Occupier.Side != Side)
                {
                    CheckMoveAttack(Offset(PosX, x), Offset(PosY, y));
                    hit = true;
                }
                else if (Tiles[Offset(PosX, x), Offset(PosY, y)].Occupier.Side == Side)
                {
                    CheckMoveAttack(Offset(PosX, x), Offset(PosY, y));
                    Potential.Add(Tiles[Offset(PosX, x), Offset(PosY, y)]);
                    hit = true;
                }
            }
            else
                CheckMoveAttack(Offset(PosX, x), Offset(PosY, y));
        }
        else if (hit && (Offset(PosX, x) != PosX && Offset(PosY, y) != PosY))
            Potential.Add(Tiles[Offset(PosX, x), Offset(PosY, y)]);
    }

    protected virtual void Knight()
    {
        KnightMove(1, 2);
        KnightMove(1, -2);

        KnightMove(2, -1);
        KnightMove(2, 1);

        KnightMove(-1, 2);
        KnightMove(-1, -2);

        KnightMove(-2, 1);
        KnightMove(-2, -1);
    }

    protected virtual void KnightMove(int x, int y)
    {
        if (Offset(PosX, y) != PosX && Offset(PosY, x) != PosY)
        {
            CheckMoveAttack(Offset(PosX, y), Offset(PosY, x));
        }
    }

    protected virtual void Queen()
    {
        Rook();
        ResetHit();
        Bishop();
    }

    protected virtual void King()
    {
        List<Tile> temp = new List<Tile>();

        if (Checker)
            temp = Checker.FullPath();

        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (InRange(PosX + i, PosY + j))
                {
                    Tile t = Grid.M.Tiles[PosX + i, PosY + j];

                    if (Checker && t == Checker.Pos)
                    {
                        if (t.Safe && !Checker.Guarded)
                            CheckMoveAttack(PosX + i, PosY + j);
                    }

                    if(!temp.Contains(t))
                    {
                        if (!t.Occupier && t.Safe)
                            CheckMoveAttack(PosX + i, PosY + j);
                        else if (t.Occupier && !t.Occupier.Guarded)
                            CheckMoveAttack(PosX + i, PosY + j);
                    }
                }
            }
        }
    }

    protected void Castle()
    {
        if (Type == "King")
        {
            if (CastleLeft != null && Game.M.TargetTile == CastleLeft.Item2[0])
            {
                CastleLeft.Item1.MoveTo(CastleLeft.Item2[1]);
            }

            if (CastleRight != null && Game.M.TargetTile == CastleRight.Item2[0])
            {
                CastleRight.Item1.MoveTo(CastleRight.Item2[1]);
            }

            CastleLeft = null;
            CastleRight = null;
        }
    }

    public void CheckCastling()
    {
        if(FirstMove && !Game.M.InCheck)
        {
            switch (Side)
            {
                case "White":
                    foreach (CustomPiece piece in Game.White.All)
                    {
                        if (piece.Type == "Rook")
                            Castling(piece);
                    }
                    break;
                case "Black":
                    foreach (CustomPiece piece in Game.Black.All)
                    {
                        if(piece.Type == "Rook")
                            Castling(piece);
                    }
                    break;
            }
        }
    }

    bool Castling(CustomPiece Rook)
    {
        if (Rook.PosX != Rook.startingX)
        {
            return false;
        }


        foreach (Tile t in Rook.RookLOS((CustomPiece)this))
        {
            if (t.Occupier && t.Occupier.Type != "Rook" && t.Occupier != this)
            {
                return false;
            }
        }

        List<Tile> temp = new List<Tile>();
        bool left = Rook.PosX < PosX;

        for (int i = -2; i < 0; i++)
        {
            if(left)
                temp.Add(Tiles[PosX + i, PosY]);
            else
                temp.Add(Tiles[PosX - i, PosY]);
        }

        foreach (Tile t in temp)
        {
            if (!t.Safe)
            {
                return false;
            }
        }

        Path.Add(temp[0]);
        if(left)
        {
            CastleLeft = new System.Tuple<CustomPiece, List<Tile>>(Rook,temp);
        }
        else if (!left)
        {
            CastleRight = new System.Tuple<CustomPiece, List<Tile>>(Rook, temp);
        }

        return true;
    }

    #endregion
}