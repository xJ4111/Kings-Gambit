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

        firstMove = true;
        Guarded = false;
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

        //Attack
        if (Game.M.TargetTile.Occupier)
        {
            Game.M.Kill(Game.M.TargetTile.Occupier);
        }

        //En Passant Attack
        if(Game.M.TargetTile == EPTile)
        {
            Game.M.Kill(EPTarget);
        }

        if(Type == "King")
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

        MoveTo(Game.M.TargetTile);

        if (Type == "Pawn")
        {
            PawnStateCheck();
        }

        firstMove = false;
    }

    public void Highlight(bool b)
    {
        foreach (Tile t in Path)
        {
            t.l.enabled = b;
        }

        if(EPTile && EPTarget)
        {
            EPTile.l.enabled = b;
        }
    }

    public void CheckPath()
    {
        Path.Clear();
        Potential.Clear();

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
