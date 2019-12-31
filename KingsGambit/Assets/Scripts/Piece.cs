using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour
{
    [Header("Piece Info")]
    public string Side;
    public string Type;
    public List<Tile> Path = new List<Tile>();
    public List<Tile> PawnAttackTiles = new List<Tile>();
    public bool Guarded;

    [Header("Position")]
    public int posX, posY;
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
                    posX = i;
                    posY = j;
                    Grid.M.Tiles[i, j].Enter(this);
                }
            }
        }
    }

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
        CheckMove(Offset(posX, 1), posY);

        if (pawnFirst && Path.Contains(Tiles[Offset(posX, 1), posY]))
        {
            CheckMove(Offset(posX, 2), posY);
        }
    }

    protected virtual void PawnAttack()
    {
        //Diagonal Left
        if ((Offset(posX, 1) != posX && Offset(posY, 1) != posY))
            CheckAttack(Offset(posX, 1), Offset(posY, 1));

        //Diagonal Right
        if ((Offset(posX, 1) != posX && Offset(posY, -1) != posY))
            CheckAttack(Offset(posX, 1), Offset(posY, -1));
    }

    public virtual void PawnShowAttack()
    {
        //Diagonal Left
        if ((Offset(posX, 1) != posX && Offset(posY, 1) != posY))
            ShowMoveAttack(Offset(posX, 1), Offset(posY, 1));

        //Diagonal Right
        if ((Offset(posX, 1) != posX && Offset(posY, -1) != posY))
            ShowMoveAttack(Offset(posX, 1), Offset(posY, -1));
    }

    protected virtual void Rook()
    {
        
        for (int i = 1; i < 8; i++)
        {
            //Forward
            RookMovementX(i, ref hit[0]);

            //Backwards
            RookMovementX(-i, ref hit[1]);

            //Forward
            RookMovementY(i, ref hit[2]);

            //Backwards
            RookMovementY(-i, ref hit[3]);
        }
    }

    protected virtual void RookMovementX(int i, ref bool hit)
    {
        if (!hit)
        {
            if (Tiles[Offset(posX, i), posY].Occupier)
            {
                if (Tiles[Offset(posX, i), posY].Occupier.Side == Side)
                    hit = true;
                else if (Tiles[Offset(posX, i), posY].Occupier.Side != Side)
                {
                    hit = true;
                    CheckMoveAttack(Offset(posX, i), posY);
                }
            }
            else if (!Tiles[Offset(posX, i), posY].Occupier)
            {
                CheckMoveAttack(Offset(posX, i), posY);
            }
        }
    }

    protected virtual void RookMovementY(int i, ref bool hit)
    {
        if (!hit)
        {
            if (Tiles[posX, Offset(posY, i)].Occupier)
            {
                if (Tiles[posX, Offset(posY, i)].Occupier.Side == Side)
                    hit = true;
                else if (Tiles[posX, Offset(posY, i)].Occupier.Side != Side)
                {
                    hit = true;
                    CheckMoveAttack(posX, Offset(posY, i));
                }
            }
            else if (!Tiles[posX, Offset(posY, i)].Occupier)
            {
                CheckMoveAttack(posX, Offset(posY, i));
            }
        }
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
        if(!hit && (Offset(posX, x) != posX && Offset(posY, y) != posY))
        {
            if(Tiles[Offset(posX, x), Offset(posY, y)].Occupier)
            {
                if(Tiles[Offset(posX, x), Offset(posY, y)].Occupier.Side != Side)
                {
                    CheckMoveAttack(Offset(posX, x), Offset(posY, y));
                    hit = true;
                }
                else if(Tiles[Offset(posX, x), Offset(posY, y)].Occupier.Side == Side)
                {
                    CheckMoveAttack(Offset(posX, x), Offset(posY, y));
                    hit = true;
                }
            }
            else
                CheckMoveAttack(Offset(posX, x), Offset(posY, y));
        }
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
        if (Offset(posX, x) != posX && Offset(posY, y) != posY)
        {
            CheckMoveAttack(Offset(posX, x), Offset(posY, y));
        }
    }

    protected virtual void King()
    {
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (InRange(posX + i, posY + j))
                {
                    Tile t = Grid.M.Tiles[posX + i, posY + j];
                    if (!t.Occupier && t.Safe)
                        CheckMoveAttack(posX + i, posY + j);
                    else if(t.Occupier && !t.Occupier.Guarded)
                        CheckMoveAttack(posX + i, posY + j);
                }
            }
        }
    }

    #endregion
}