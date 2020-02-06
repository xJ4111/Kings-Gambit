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
        OnMove();
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
        if(b)
        {
            if (!Injured)
            {
                foreach (Tile t in Path)
                {
                    t.l.enabled = b;
                }
            }
        }
        else
        {
            foreach (Tile t in Path)
            {
                t.l.enabled = b;
            }
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

    #region Abilities
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

    #region Rook
    protected override void Rook()
    {
        if (Ability == "")
            base.Rook();
        else
        {
            switch (Ability)
            {
                case "Shield Wall":
                    Invincible = true;

                    for (int i = 1; i < 3; i++)
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

                    break;
                case "Reckless":
                    base.Rook();
                    break;
            }
        }
    }

    protected override void RookMovementX(int i, ref bool hit)
    {
        if (Ability == "")
            base.RookMovementX(i, ref hit);
        else
        {
            switch (Ability)
            {
                case "Shield Wall":

                    if (!hit)
                    {
                        if (Tiles[Offset(PosX, i), PosY].Occupier)
                        {
                            hit = true;
                        }
                        else if (!Tiles[Offset(PosX, i), PosY].Occupier)
                        {
                            CheckMove(Offset(PosX, i), PosY);
                        }
                    }

                    break;
                case "Reckless":
                    base.RookMovementX(i, ref hit);
                    break;
            }
        }
    }

    protected override void RookMovementY(int i, ref bool hit)
    {
        if (Ability == "")
            base.RookMovementY(i, ref hit);
        else
        {
            switch (Ability)
            {
                case "Shield Wall":
                    if (!hit)
                    {
                        if (Tiles[PosX, Offset(PosY, i)].Occupier)
                        {
                            hit = true;
                        }
                        else if (!Tiles[PosX, Offset(PosY, i)].Occupier)
                        {
                            CheckMove(PosX, Offset(PosY, i));
                        }
                    }
                    break;
                case "Reckless":
                    base.RookMovementY(i, ref hit);
                    break;
            }
        }
    }
    #endregion
    protected override void Knight()
    {
        if (Ability == "")
            base.Knight();
        else
        {
            switch (Ability)
            {
                case "Combat Medic":
                    base.Knight();
                    break;
                case "Charge":
                    base.Knight();
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

    #endregion

    #region On Move Effects
    void OnMove()
    {
        switch (Type)
        {
            case "Pawn":
                break;
            case "Rook":
                switch (Ability)
                {
                    case "Shield Wall":
                        break;
                    case "Reckless":
                        Reckless();
                        break;
                }
                break;
            case "Knight":
                switch (Ability)
                {
                    case "Combat Medic":
                        Revive();
                        break;
                    case "Charge":
                        Charge();
                        break;
                }
                break;
            case "Bishop":
                break;
            case "Queen":
                break;
            case "King":
                break;
        }
    }

    void Reckless()
    {
        CustomPiece temp = Game.M.TargetTile.Occupier;
        List<CustomPiece> surrounding = new List<CustomPiece>();

        if(temp)
        {
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (!(i == 0 && j == 0))
                    {
                        if(InRange(temp.PosX + i, temp.PosY + j))
                        {
                            Tile t = Tiles[temp.PosX + i, temp.PosY + j];
                            if (t.Occupier && t.Occupier.Side != Side && !t.Occupier.Invincible)
                                surrounding.Add(t.Occupier);
                        }
                    }
                }
            }

            if(surrounding.Count >= 1)
            {
                CustomPiece target = surrounding[Random.Range(0, surrounding.Count)];
                target.Injured = true;
                target.RoundInjured = Game.M.RoundCount;
            }

            Injured = true;
            RoundInjured = Game.M.RoundCount;
        }
    }

    void Revive()
    {
        CustomPiece revive = null;

        Game.M.TargetTile.Graves.Reverse();

        foreach (CustomPiece p in Game.M.TargetTile.Graves)
        {
            if(p.Side == Side)
            {
                revive = p;
            }
        }

        revive.enabled = true;
        revive.MoveTo(Pos);
        Game.M.TargetTile.Graves.Remove(revive);

        Game.M.TargetTile.Graves.Reverse();
    }

    void Charge()
    {
        List<CustomPiece> inPath = new List<CustomPiece>();
 
        int x = Game.M.TargetTile.PosX - PosX;
        int y = Game.M.TargetTile.PosY - PosY;

        if(x > 0)
        {
            for(int i = 0; i <= x; i++)
            {
                if(y > 0)
                {
                    for(int j = 0; j <= y; j++)
                    {
                        if(!(i == 0 && j == 0))
                        {
                            if(Tiles[PosX + i, PosY + j].Occupier && Tiles[PosX + i, PosY + j].Occupier.Side != Side && !Tiles[PosX + i, PosY + j].Occupier.Invincible)
                            {
                                inPath.Add(Tiles[PosX + i, PosY + j].Occupier);
                            }
                        }
                    }
                }
                else
                {
                    for (int j = 0; j >= y; j--)
                    {
                        if (!(i == 0 && j == 0))
                        {
                            if (Tiles[PosX + i, PosY + j].Occupier && Tiles[PosX + i, PosY + j].Occupier.Side != Side && !Tiles[PosX + i, PosY + j].Occupier.Invincible)
                            {
                                inPath.Add(Tiles[PosX + i, PosY + j].Occupier);
                            }
                        }
                    }
                }
            }
        }
        else
        {
            for (int i = 0; i >= x; i--)
            {
                if (y > 0)
                {
                    for (int j = 0; j <= y; j++)
                    {
                        if (!(i == 0 && j == 0))
                        {
                            if (Tiles[PosX + i, PosY + j].Occupier && Tiles[PosX + i, PosY + j].Occupier.Side != Side && !Tiles[PosX + i, PosY + j].Occupier.Invincible)
                            {
                                inPath.Add(Tiles[PosX + i, PosY + j].Occupier);
                            }
                        }
                    }
                }
                else
                {
                    for (int j = 0; j >= y; j--)
                    {
                        if (!(i == 0 && j == 0))
                        {
                            if (Tiles[PosX + i, PosY + j].Occupier && Tiles[PosX + i, PosY + j].Occupier.Side != Side && !Tiles[PosX + i, PosY + j].Occupier.Invincible)
                            {
                                inPath.Add(Tiles[PosX + i, PosY + j].Occupier);
                            }
                        }
                    }
                }
            }
        }

        if(Game.M.TargetTile.Occupier)
            inPath.Remove(Game.M.TargetTile.Occupier);

        foreach(CustomPiece p in inPath)
        {
            if (p.Type == "Pawn")
                p.Injured = true;
        }
    }

    #endregion
}