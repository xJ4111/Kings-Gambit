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

        pawnFirst = true;
        Guarded = false;
    }

    private void Update()
    {
        
    }

    protected void OnMouseOver()
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

        if (Game.M.Selected && Game.M.Selected != this)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (Tiles[PosX, PosY].l.enabled)
                    Game.M.TargetTile = Tiles[PosY, PosX];
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

        //Move
        transform.position = Game.M.TargetTile.transform.position;

        //Attack
        if (Game.M.TargetTile.Occupier)
        {
            Game.M.AllPieces.Remove(Game.M.TargetTile.Occupier);
            Destroy(Game.M.TargetTile.Occupier.gameObject);
        }

        //En Passant Attack
        if(Game.M.TargetTile == EPTile)
        {
            Game.M.AllPieces.Remove(EPTarget);
            Destroy(EPTarget.gameObject);
        }

        Tiles[PosX, PosY].Exit();
        Game.M.TargetTile.Enter(this);

        if(Type == "Pawn")
        {
            PawnStateCheck();
        }
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
                    #region Endurance
                    //Front 2 Spaces
                    CheckMove(Offset(PosY, 1), PosX);
                    CheckMove(Offset(PosY, 2), PosX);

                    if (pawnFirst && Tiles[Offset(PosY, 1), PosX].l.enabled)
                    {
                        CheckMove(Offset(PosY, 3), PosX);
                        CheckMove(Offset(PosY, 4), PosX);
                    }


                    for (int i = 1; i <= 2; i++)
                    {
                        //Diagonal Left
                        if ((Offset(PosY, i) != PosY && Offset(PosX, i) != PosX))
                        {
                            if(!hit[0])
                            {
                                if (Tiles[Offset(PosY, i), Offset(PosX, i)].Occupier && Tiles[Offset(PosY, i), Offset(PosX, i)].Occupier.Side != Side)
                                {
                                    hit[0] = true;
                                    CheckAttack(Offset(PosY, i), Offset(PosX, i));
                                }
                                else
                                {
                                    Potential.Add(Tiles[Offset(PosY, i), Offset(PosX, i)]);
                                }
                            }
                            else if (hit[0])
                            {
                                Potential.Add(Tiles[Offset(PosY, i), Offset(PosX, i)]);
                            }
                        }

                        //Diagonal Right
                        if ((Offset(PosY, i) != PosY && Offset(PosX, -i) != PosX))
                        {
                            if (!hit[1])
                            {
                                if (Tiles[Offset(PosY, i), Offset(PosX, -i)].Occupier && Tiles[Offset(PosY, i), Offset(PosX, -i)].Occupier.Side != Side)
                                {
                                    hit[1] = true;
                                    CheckAttack(Offset(PosY, i), Offset(PosX, -i));
                                }
                                else
                                {
                                    Potential.Add(Tiles[Offset(PosY, i), Offset(PosX, -i)]);
                                }
                            }
                            else if (hit[0])
                            {
                                Potential.Add(Tiles[Offset(PosY, i), Offset(PosX, -i)]);
                            }
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
