﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomPiece : Piece
{
    public string Ability = "";

    // Start is called before the first frame update
    void Start()
    {
        Tiles = Grid.M.Tiles;

        string[] names = name.Split(' ');

        Side = names[0];
        Type = names[1];

        pawnFirst = true;
        Guarded = false;
    }

    protected void OnMouseOver()
    {
        if (Game.M.Turn == Side && !Game.M.Selected)
        {
            Highlight(true);

            if (Input.GetMouseButtonDown(1) && activeTiles > 0)
            {
                Game.M.Selected = this;
            }
        }

        if (Game.M.Selected && Game.M.Selected != this)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if(Tiles[posX, posY].l.enabled)
                    Game.M.TargetTile = Tiles[posX, posY];
            }
        }
    }

    protected void OnMouseExit()
    {
        if (!Game.M.Selected)
        {
            Highlight(false);

            for (int i = 0; i < 4; i++)
                hit[i] = false;
        }
    }

    public void Move()
    {
        Highlight(false);

        //Attack
        if (Game.M.TargetTile.Occupier)
            Destroy(Game.M.TargetTile.Occupier.gameObject);
        //Move
        transform.position = Game.M.TargetTile.transform.position;

        Tiles[posX, posY].Exit();
        Game.M.TargetTile.Enter(this);

        pawnFirst = false;
    }

    public void Highlight(bool b)
    {
        foreach (Tile t in Path)
            t.l.enabled = b;
    }

    public void CheckPath()
    {
        Path.Clear();

        switch (Type)
        {
            case "Pawn":
                PawnMove();
                PawnAttack();
                PawnShowAttack();
                break;
            case "Rook":
                Rook();
                break;
            case "Knight":
                Knight();
                break;
            case "Bishop":
                Bishop();
                break;
            case "Queen":
                Rook();
                ResetHit();
                Bishop();
                break;
            case "King":
                King();
                break;
        }
    }

    public void Unselect()
    {
        Highlight(false);
        Game.M.Selected = null;
    }

    protected override void PawnMove()
    {
        if (Ability == "")
            base.PawnMove();
        else
        {
            switch(Ability)
            {
                case "Endurance":
                    #region Endurance
                    //Front 2 Spaces
                    CheckMove(Offset(posX, 1), posY);
                    CheckMove(Offset(posX, 2), posY);

                    if (pawnFirst && Tiles[Offset(posX, 1), posY].l.enabled)
                    {
                        CheckMove(Offset(posX, 3), posY);
                        CheckMove(Offset(posX, 4), posY);
                    }

                    //Diagonal Left
                    for (int i = 1; i <= 2; i++)
                    {
                        if ((Offset(posX, i) != posX && Offset(posY, i) != posY))
                        {
                            CheckAttack(Offset(posX, i), Offset(posY, i));
                        }
                    }

                    //Diagonal Right
                    for (int i = 1; i <= 2; i++)
                    {
                        if ((Offset(posX, i) != posX && Offset(posY, -i) != posY))
                        {
                            CheckAttack(Offset(posX, i), Offset(posY, -i));
                        }
                    }
                    #endregion
                    break;
            }
        }

    }
    protected override void Rook()
    {
        if (Ability == "")
            base.Rook();
        else
        {
            switch (Ability)
            {
                case "Endurance":
                    break;
            }
        }
    }
    protected override void Knight()
    {
        if (Ability == "")
            base.Knight();
        else
        {
            switch (Ability)
            {
                case "Endurance":
                    break;
            }
        }
    }

    protected override void Bishop()
    {
        if (Ability == "")
            base.Bishop();
        else
        {
            switch (Ability)
            {
                case "Endurance":
                    break;
            }
        }
    }

    protected override void King()
    {
        if (Ability == "")
            base.King();
        else
        {
            switch (Ability)
            {
                case "Endurance":
                    break;
            }
        }
    }
}
