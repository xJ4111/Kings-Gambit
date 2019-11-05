using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour
{
    public string Side;

    private int posX, posY;
    private Tile[,] Tiles;
    private int activeTiles;

    private bool pawnFirst = true;
    public bool[] hit = new bool[4];

    private bool toggle;


    // Start is called before the first frame update
    void Start()
    {
        Tiles = Grid.M.Tiles;

        if (name.Contains("White"))
            Side = "White";
        else if (name.Contains("Black"))
            Side = "Black";
    }

    // Update is called once per frame
    void Update()
    {
        Movement();

        if(Input.GetKeyDown(KeyCode.Escape))
        {
            Restart();
        }
    }

    public void SetPos(int x, int y)
    {
        posX = x;
        posY = y;
    }

    void Movement()
    {
        if (Game.M.Selected == this && Game.M.TargetTile)
        {
            Highlight(false);
            Move();
            Restart();
        }
    }

    void Highlight(bool b)
    {
        toggle = b;

        if(name.Contains("Pawn"))
            Pawn();

        if (name.Contains("Rook"))
            Rook();

        if (name.Contains("Bishop"))
            Bishop();

        if (name.Contains("Knight"))
            Knight();

        if (name.Contains("Queen"))
        {
            Debug.Log("quenn");
            Rook();
            ResetHit();
            Bishop();
        }

        if (name.Contains("King"))
            King();
    }
    void CheckMove(int posX, int posY)
    {
        if (InRange(posX, posY))
        {
            if (!Tiles[posX, posY].Occupier)
            {
                Tiles[posX, posY].l.enabled = toggle;
                if (toggle)
                    activeTiles++;
            }
        }
    }
    void CheckAttack(int posX, int posY)
    {
        if (InRange(posX, posY))
        {
            if (Tiles[posX, posY] && Tiles[posX, posY].Occupier && Tiles[posX, posY].Occupier.Side != Side)
            {
                Tiles[posX, posY].l.enabled = toggle;
                if (toggle)
                    activeTiles++;
            }
        }
    }

    void CheckMoveAttack(int posX, int posY)
    {
        if (InRange(posX, posY))
        {
            if (!Tiles[posX, posY].Occupier)
            {
                Tiles[posX, posY].l.enabled = toggle;
                if(toggle)
                    activeTiles++;
            }

            if (Tiles[posX, posY] && Tiles[posX, posY].Occupier && Tiles[posX, posY].Occupier.Side != Side)
            {
                Tiles[posX, posY].l.enabled = toggle;
                if (toggle)
                    activeTiles++;
            }
        }
    }

    int Offset(int pos, int offset)
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

    void Move()
    {
        //Attack
        if (Game.M.TargetTile.Occupier)
            Destroy(Game.M.TargetTile.Occupier.gameObject);

        //Move
        transform.position = Game.M.TargetTile.transform.position;

        pawnFirst = false;
    }

    void Restart()
    {
        Highlight(false);
        Game.M.Selected = null;
        Game.M.TargetTile = null;
        ResetHit();
        activeTiles = 0;
    }

    bool InRange(int posX, int posY)
    {
        return posX < 8 && posX >= 0 && posY < 8 && posY >= 0;
    }

    void ResetHit()
    {
        for (int i = 0; i < 4; i++)
            hit[i] = false;
    }

    private void OnMouseOver()
    {
        if(!Game.M.Selected)
        {
            Highlight(true);

            if (Input.GetMouseButtonDown(1) && activeTiles > 0)
            {
                Debug.Log(activeTiles);
                Game.M.Selected = this;
            }
        }
    }

    private void OnMouseExit()
    {
        if(Game.M.Selected != this)
        {
            Highlight(false);

            for (int i = 0; i < 4; i++)
                hit[i] = false;
        }
    }

    #region Piece Movement
    void Pawn()
    {
        //Front 2 Spaces
        CheckMove(Offset(posX, 1), posY);

        if (toggle && pawnFirst && Tiles[Offset(posX, 1), posY].l.enabled)
        {
            CheckMove(Offset(posX, 2), posY);
        }
        else if(!toggle)
            CheckMove(Offset(posX, 2), posY);

        //Diagonal Left
        CheckAttack(Offset(posX, 1), Offset(posY, 1));

        //Diagonal Right
        CheckAttack(Offset(posX, 1), Offset(posY, -1));
    }

    void Rook()
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

    void RookMovementX(int i, ref bool hit)
    {
        if (toggle && !hit)
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
        else if (!toggle)
            CheckMoveAttack(Offset(posX, i), posY);
    }

    void RookMovementY(int i, ref bool hit)
    {
        if (toggle && !hit)
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
        else if (!toggle)
            CheckMoveAttack(posX, Offset(posY, i));
    }

    void Bishop()
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

    void BishopMovement(int x, int y, ref bool hit)
    {
        if(toggle && !hit && (Offset(posX, x) != posX && Offset(posY, y) != posY))
        {
            if(Tiles[Offset(posX, x), Offset(posY, y)].Occupier)
            {
                if(Tiles[Offset(posX, x), Offset(posY, y)].Occupier.Side != Side)
                {
                    CheckMoveAttack(Offset(posX, x), Offset(posY, y));
                    Debug.Log("Hit");
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
        else if(!toggle)
            CheckMoveAttack(Offset(posX, x), Offset(posY, y));
    }

    void Knight()
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

    void KnightMove(int x, int y)
    {
        if (Offset(posX, x) != posX && Offset(posY, y) != posY)
        {
            CheckMoveAttack(Offset(posX, x), Offset(posY, y));
        }
    }

    void King()
    {
        //Front
        CheckMoveAttack(Offset(posX, 1), posY);

        //Back
        CheckMoveAttack(Offset(posX, -1), posY);

        //Front
        CheckMoveAttack(posX, Offset(posY, 1));

        CheckMoveAttack(posX, Offset(posY, -1));

        CheckMoveAttack(Offset(posX, 1), Offset(posY, 1));

        CheckMoveAttack(Offset(posX, -1), Offset(posY, 1));

        CheckMoveAttack(Offset(posX, 1), Offset(posY, -1));

        CheckMoveAttack(Offset(posX, -1), Offset(posY, -1));
    }

    #endregion
}