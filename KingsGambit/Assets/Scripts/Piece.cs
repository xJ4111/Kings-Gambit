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

    //PAWN ONLY
    public List<Tile> PawnAttackTiles = new List<Tile>();

    public bool Guarded;

    [HideInInspector] public bool CanBlock;

    [Header("Position")]
    public Tile Pos;
    public int PosY, PosX;
    protected Tile[,] Tiles;
    public int activeTiles;

    protected bool pawnFirst = true;
    [HideInInspector] public bool[] hit = new bool[4];

    public void SetPos()
    {
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (transform.position == Grid.M.Tiles[i, j].transform.position)
                {
                    PosY = i;
                    PosX = j;
                    Pos = Grid.M.Tiles[i, j];
                    Pos.Enter(this);
                }
            }
        }
    }

    #region PathProcessing

    public void CheckPinning()
    {
        bool KingInPath = false;
        CustomPiece PieceInPath = null;

        foreach (Tile t in FullPath())
        {
            if (t.Occupier && t.Occupier.Side != Side)
            {
                if (t.Occupier.Type == "King")
                    KingInPath = true;
                else
                {
                    if (!PieceInPath)
                        PieceInPath = t.Occupier;
                }
            }
        }

        if (KingInPath && PieceInPath)
        {
            CustomPiece EnemyKing = null;
            switch (Side)
            {
                case "White":
                    EnemyKing = Game.M.BlackKing;
                    break;
                case "Black":
                    EnemyKing = Game.M.WhiteKing;
                    break;
            }

            PieceInPath.PinnedMovement(LineOfSight(EnemyKing));
        }
    }

    public void PinnedMovement(List<Tile> LOS)
    {
        List<Tile> tempPath = Clone(Path);

        if (LOS.Count > 0)
        {
            foreach(Tile t in tempPath)
            {
                if(!LOS.Contains(t))
                {
                    Path.Remove(t);
                }
            }
        }
    }
    List<Tile> Clone(List<Tile> list)
    {
        List<Tile> copy = new List<Tile>();

        foreach(Tile t in list)
        {
            copy.Add(t);
        }

        return copy;
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
        switch(Type)
        {
            case "Rook":
                return RookLOS(King);
            case "Bishop":
                return BishopLOS(King);
            case "Queen":
                return QueenLOS(King);
        }

        return null;
    }

    public List<Tile> RookLOS(CustomPiece King)
    {
        List<Tile> path = new List<Tile>();
        int x = PosX;
        int y = PosY;

        if (King.Pos.PosY != PosY && King.Pos.PosX != PosX)
        {
            return null;
        }
        else if(King.Pos.PosY == PosY)
        {
            if(King.Pos.PosX > PosX)
            {
                while(Tiles[y,x] != King.Pos)
                {
                    path.Add(Tiles[y, x]);
                    x++;
                }

                return path;
            }
            if (King.Pos.PosX < PosX)
            {
                while (Tiles[y, x] != King.Pos)
                {
                    path.Add(Tiles[y, x]);
                    x--;
                }

                return path;
            }
        }
        else if (King.Pos.PosX == PosX)
        {
            if (King.Pos.PosY < PosY)
            {
                while (Tiles[y, x] != King.Pos)
                {
                    path.Add(Tiles[y, x]);
                    y--;
                }

                return path;
            }
            if (King.Pos.PosY > PosY)
            {
                while (Tiles[y, x] != King.Pos)
                {
                    path.Add(Tiles[y, x]);
                    y++;
                }

                return path;
            }
        }

        return path;
    }
    public List<Tile> BishopLOS(CustomPiece King)
    {
        List<Tile> path = new List<Tile>();
        int x = PosX;
        int y = PosY;

        if (King.Pos.PosY == PosY || King.Pos.PosX == PosX)
        {
            return null;
        }
        else if (King.Pos.PosY > PosY) //below
        {
            if (King.Pos.PosX > PosX) //right
            {
                while (Tiles[y, x] != King.Pos)
                {
                    path.Add(Tiles[y, x]);
                    x++;
                    y++;
                }

                return path;
            }
            else if (King.Pos.PosX < PosX) //left
            {
                while (Tiles[y, x] != King.Pos)
                {
                    path.Add(Tiles[y, x]);
                    x--;
                    y++;

                    Debug.Log(x + " " + y);
                }

                return path;
            }
        }
        else if (King.Pos.PosY < PosY) //above
        {
            if (King.Pos.PosX > PosX) //right
            {
                while (Tiles[y, x] != King.Pos)
                {
                    path.Add(Tiles[y, x]);
                    x++;
                    y--;
                }

                return path;
            }
            else if (King.Pos.PosX < PosX) //left
            {
                while (Tiles[y, x] != King.Pos)
                {
                    path.Add(Tiles[y, x]);
                    x--;
                    y--;
                }

                return path;
            }
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
    #endregion

    #endregion

    #region Movement Checks
    protected void CheckMove(int posX, int posY)
    {
        if (InRange(posX, posY))
        {
            if (!Tiles[posX, posY].Occupier)
            {
                Path.Add(Tiles[posX, posY]);
                activeTiles++;
            }
        }
    }
    protected void CheckAttack(int posX, int posY)
    {
        if (InRange(posX, posY))
        {
            if (Tiles[posX, posY] && Tiles[posX, posY].Occupier)
            {
                if(Tiles[posX, posY].Occupier.Side != Side)
                {
                    Path.Add(Tiles[posX, posY]);
                    activeTiles++;
                }
                else if(Tiles[posX, posY].Occupier.Side == Side)
                {
                    Tiles[posX, posY].Occupier.Guarded = true;
                }
            }
        }
    }

    protected void CheckMoveAttack(int posX, int posY)
    {
        if (InRange(posX, posY))
        {
            if (Tiles[posX, posY])
            {
                if((Tiles[posX, posY].Occupier && Tiles[posX, posY].Occupier.Side != Side) || !Tiles[posX, posY].Occupier)
                {
                    Path.Add(Tiles[posX, posY]);
                    activeTiles++;
                }
                else if(Tiles[posX, posY].Occupier && Tiles[posX, posY].Occupier.Side == Side)
                {
                    Tiles[posX, posY].Occupier.Guarded = true;
                }
                else if(!Tiles[posX, posY].Occupier)
                    Path.Add(Tiles[posX, posY]);
            }
        }
    }

    protected void ShowMoveAttack(int posX, int posY)
    {
        if (InRange(posX, posY))
        {
            if (Tiles[posX, posY])
            {
                if ((Tiles[posX, posY].Occupier && Tiles[posX, posY].Occupier.Side != Side) || !Tiles[posX, posY].Occupier)
                {
                    PawnAttackTiles.Add(Tiles[posX, posY]);
                    activeTiles++;
                }
                else if (Tiles[posX, posY].Occupier && Tiles[posX, posY].Occupier.Side == Side)
                {
                    Tiles[posX, posY].Occupier.Guarded = true;
                }
            }
        }
    }

    protected int Offset(int pos, int offset)
    {
        if(Side == "White")
        {
            if (pos - offset < 8 && pos - offset >= 0)
                return pos - offset;
        }
        else if(Side == "Black")
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
    protected virtual void PawnMove()
    {
        //Front 2 Spaces
        CheckMove(Offset(PosY, 1), PosX);

        if (pawnFirst && Path.Contains(Tiles[Offset(PosY, 1), PosX]))
        {
            CheckMove(Offset(PosY, 2), PosX);
        }
    }

    protected virtual void PawnAttack()
    {
        //Diagonal Left
        if ((Offset(PosY, 1) != PosY && Offset(PosX, 1) != PosX))
            CheckAttack(Offset(PosY, 1), Offset(PosX, 1));

        //Diagonal Right
        if ((Offset(PosY, 1) != PosY && Offset(PosX, -1) != PosX))
            CheckAttack(Offset(PosY, 1), Offset(PosX, -1));
    }

    public virtual void PawnShowAttack()
    {
        //Diagonal Left
        if ((Offset(PosY, 1) != PosY && Offset(PosX, 1) != PosX))
            ShowMoveAttack(Offset(PosY, 1), Offset(PosX, 1));

        //Diagonal Right
        if ((Offset(PosY, 1) != PosY && Offset(PosX, -1) != PosX))
            ShowMoveAttack(Offset(PosY, 1), Offset(PosX, -1));
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
            if (Tiles[Offset(PosY, i), PosX].Occupier)
            {
                if (Tiles[Offset(PosY, i), PosX].Occupier.Side == Side)
                {
                    Potential.Add(Tiles[Offset(PosY, i), PosX]);
                    hit = true;
                }
                else if (Tiles[Offset(PosY, i), PosX].Occupier.Side != Side)
                {
                    hit = true;
                    CheckMoveAttack(Offset(PosY, i), PosX);
                }
            }
            else if (!Tiles[Offset(PosY, i), PosX].Occupier)
            {
                CheckMoveAttack(Offset(PosY, i), PosX);
            }
        }
        else if (hit)
            Potential.Add(Tiles[Offset(PosY, i), PosX]);
    }

    protected virtual void RookMovementX(int i, ref bool hit)
    {
        if (!hit)
        {
            if (Tiles[PosY, Offset(PosX, i)].Occupier)
            {
                if (Tiles[PosY, Offset(PosX, i)].Occupier.Side == Side)
                {
                    Potential.Add(Tiles[PosY, Offset(PosX, i)]);
                    hit = true;
                }
                else if (Tiles[PosY, Offset(PosX, i)].Occupier.Side != Side)
                {
                    hit = true;
                    CheckMoveAttack(PosY, Offset(PosX, i));
                }
            }
            else if (!Tiles[PosY, Offset(PosX, i)].Occupier)
            {
                CheckMoveAttack(PosY, Offset(PosX, i));
            }
        }
        else if (hit)
            Potential.Add(Tiles[PosY, Offset(PosX, i)]);
    }

    protected virtual void Bishop()
    {
        for (int i = 1; i < 8; i++)
        {
            //Top Left
            BishopMovement(i, i, ref hit[0]);

            //Top Right
            BishopMovement(i, -i, ref hit[1]);

            //Bottom Left
            BishopMovement(-i, i, ref hit[2]);

            //Bottom Right
            BishopMovement(-i, -i, ref hit[3]);
        } 
    }

    protected virtual void BishopMovement(int x, int y, ref bool hit)
    {
        if(!hit && (Offset(PosY, x) != PosY && Offset(PosX, y) != PosX))
        {
            if(Tiles[Offset(PosY, x), Offset(PosX, y)].Occupier)
            {
                if(Tiles[Offset(PosY, x), Offset(PosX, y)].Occupier.Side != Side)
                {
                    CheckMoveAttack(Offset(PosY, x), Offset(PosX, y));
                    hit = true;
                }
                else if(Tiles[Offset(PosY, x), Offset(PosX, y)].Occupier.Side == Side)
                {
                    CheckMoveAttack(Offset(PosY, x), Offset(PosX, y));
                    Potential.Add(Tiles[Offset(PosY, x), Offset(PosX, y)]);
                    hit = true;
                }
            }
            else
                CheckMoveAttack(Offset(PosY, x), Offset(PosX, y));
        }
        else if (hit && (Offset(PosY, x) != PosY && Offset(PosX, y) != PosX))
            Potential.Add(Tiles[Offset(PosY, x), Offset(PosX, y)]);
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
        if (Offset(PosY, x) != PosY && Offset(PosX, y) != PosX)
        {
            CheckMoveAttack(Offset(PosY, x), Offset(PosX, y));
        }
    }

    protected virtual void King()
    {
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (InRange(PosY + i, PosX + j))
                {
                    Tile t = Grid.M.Tiles[PosY + i, PosX + j];
                    if (!t.Occupier && t.Safe)
                        CheckMoveAttack(PosY + i, PosX + j);
                    else if(t.Occupier && !t.Occupier.Guarded)
                        CheckMoveAttack(PosY + i, PosX + j);
                }
            }
        }
    }

    #endregion
}