using System.Collections;
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

        EPTake = true;
        FirstMove = true;
        Guarded = false;
        CancelEP = false;
    }

    protected void OnMouseOver()
    {
        if(!UI.M.PromoPanel.activeSelf)
        {
            if (Game.M.Turn == Side && !Game.M.Selected)
            {
                if (!Game.M.InCheck)
                    Highlight(true);
                else if (Game.M.InCheck && (Type == "King" || CanBlock))
                    Highlight(true);

                if (Input.GetMouseButtonDown(1) && activeTiles > 0)
                {
                    Game.M.Selected = this;
                }
            }

            if (Game.M.Selected && Game.M.Selected != this && Tiles[PosX, PosY].l.enabled)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    Game.M.TargetTile = Tiles[PosX, PosY];
                }
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

        Attack();
        Castle();

        MoveTo(Game.M.TargetTile);

        if (Type == "Pawn")
        {
            PawnStateCheck();
        }

        FirstMove = false;
    }


    public void Highlight(bool b)
    {
        foreach (Tile t in Path)
        {
            t.l.enabled = b;
        }
    }

    public void CheckPath()
    {
        Path.Clear();
        Potential.Clear();

        switch (Type)
        {
            case "Pawn":
                Pawn();
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

    protected override void Pawn()
    {
        if (Ability == "")
            base.Pawn();
        else
        {
            EPCheck();

            switch (Ability)
            {
                case "Zeal":
                    Vulnerable = true;
                    CheckMove(PosX, Offset(PosY, 1));
                    if(Path.Contains(Tiles[PosX, Offset(PosY, 1)]))
                        CheckMove(PosX, Offset(PosY, 2));

                    CheckAttack(Offset(PosX, 1), Offset(PosY, 1));
                    if (!Path.Contains(Tiles[Offset(PosX, 1), Offset(PosY, 1)]))
                        CheckAttack(Offset(PosX, 2), Offset(PosY, 2));

                    CheckAttack(Offset(PosX, -1), Offset(PosY, 1));
                    if (!Path.Contains(Tiles[Offset(PosX, -1), Offset(PosY, 1)]))
                        CheckAttack(Offset(PosX, -2), Offset(PosY, 2));

                    break;
                case "Focus":
                    Vulnerable = false;

                    if (FirstMove)
                    {
                        CheckMoveAttack(PosX, Offset(PosY, 1));
                        CheckMoveAttack(PosX, Offset(PosY, 2));
                    }
                    else
                    {
                        CheckMoveAttack(PosX, Offset(PosY, 1));
                    }
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
                case "Shield Wall":
                    break;
                case "Reckless":
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
                case "Combat Medic":
                    break;
                case "Charge":
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
                case "Warp":
                    break;
                case "Arcane Connection":
                    break;
            }
        }
    }

    protected override void Queen()
    {
        if (Ability == "")
            base.Queen();
        else
        {
            switch (Ability)
            {
                case "Deploy":
                    break;
                case "Arcane Connection":
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
