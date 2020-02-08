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

    private void Update()
    {
        if(!AllyKing)
            StartCoroutine(GetAllyKing());
    }

    IEnumerator GetAllyKing()
    {
        yield return new WaitForEndOfFrame();

        switch (Side)
        {
            case "White":
                AllyKing = Game.White.King;
                break;
            case "Black":
                AllyKing = Game.Black.King;
                break;
        }
    }

    protected void OnMouseOver()
    {
        if(!UI.M.PromoPanel.activeSelf)
        {
            Selection();
            Targeting();
        }
    }

    protected void OnMouseExit()
    {
        if (!Game.M.Selected)
        {
            Highlight(false);
            UI.M.ToggleAbilityButton();

            for (int i = 0; i < 4; i++)
                hit[i] = false;
        }
    }
    
    void Selection()
    {
        if (Game.M.Turn == Side && !Game.M.Selected)
        {
            if (!Game.M.InCheck)
                Highlight(true);
            else if (Game.M.InCheck)
            {
                CheckBlock();

                if(Type == "King" || CanBlock)
                    Highlight(true);
            }


            OnClick();

            if (Input.GetMouseButtonDown(1) && (OnClick() || activeTiles > 0))
            {
                Game.M.Selected = this;
            }
        }
    }

    void Targeting()
    {
        if (Game.M.Selected && Game.M.Selected != this)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if(Tiles[PosX, PosY].l.enabled)
                    Game.M.TargetTile = Tiles[PosX, PosY];

                if (Game.M.SearchingPiece)
                {
                    if(Game.M.Selected.TargetValid(this) == "Valid")
                    {
                        Game.M.AbilityTarget = this;
                        GetTargetPosition();
                    }
                    else
                    {
                        Debug.Log(Game.M.Selected.TargetValid(this));
                    }
                }
            }
        }

    }

    public void Move()
    {
        Highlight(false);

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
                    if (t.Occupier)
                    {
                        if (AllyKing.Checker && t.Occupier == AllyKing.Checker)
                            t.l.enabled = b;
                        else if(!t.Occupier.Invincible)
                            t.l.enabled = b;
                    }
                    else
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
        PawnAttackTiles.Clear();

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
                Queen();
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
        UI.M.ToggleAbilityButton();
    }

    #region Abilities
    protected new void Pawn()
    {
        if (Ability == "")
            base.Pawn();
        else
        {
            switch (Ability)
            {
                case "Caution":
                    Vulnerable = true;

                    if (FirstMove)
                    {
                        if ((Offset(PosY, -1) != PosY))
                            CheckMove(PosX, Offset(PosY, -1));

                        if ((Offset(PosY, 1) != PosY))
                            CheckMove(PosX, Offset(PosY, 1));

                        if(Path.Contains(Tiles[PosX, Offset(PosY, 1)]))
                            if ((Offset(PosY, 1) != PosY))
                                CheckMove(PosX, Offset(PosY, 2));
                    }
                    else
                    {
                        if ((Offset(PosY, -1) != PosY))
                            CheckMove(PosX, Offset(PosY, -1));

                        if ((Offset(PosY, 1) != PosY))
                            CheckMove(PosX, Offset(PosY, 1));
                    }

                    if (Offset(PosX, 1) != PosX && Offset(PosY, 1) != PosY)
                        CheckAttack(Offset(PosX, 1), Offset(PosY, 1));

                    if (Offset(PosX, -1) != PosX && Offset(PosY, 1) != PosY)
                        CheckAttack(Offset(PosX, -1), Offset(PosY, 1));

                    break;
                case "Focus":
                    Vulnerable = false;

                    if (FirstMove)
                    {
                        if (Offset(PosY, 1) != PosY)
                            CheckMoveAttack(PosX, Offset(PosY, 1));

                        if (Path.Contains(Tiles[PosX, Offset(PosY, 1)]))
                            if (Offset(PosY, 2) != PosY)
                                CheckMoveAttack(PosX, Offset(PosY, 2));
                    }
                    else
                    {
                        if (Offset(PosY, 1) != PosY)
                            CheckMoveAttack(PosX, Offset(PosY, 1));
                    }
                    break;
            }
        }
    }

    #region Rook
    protected new void Rook()
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

    protected new void RookMovementX(int i, ref bool hit)
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

    protected new void RookMovementY(int i, ref bool hit)
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
    protected new void Knight()
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

    protected new void Bishop()
    {
        if (Ability == "")
            base.Bishop();
        else
        {
            switch (Ability)
            {
                case "Blast":
                    base.Bishop();
                    break;
                case "Arcane Connection":
                    base.Bishop();
                    break;
            }
        }
    }

    protected new void Queen()
    {
        switch (Ability)
        {
            case "":
                base.Queen();
                break;
            case "Deploy":
                base.Queen();
                break;
            case "Arcane Connection":
                base.Queen();
                break;
        }
    }

    protected new void King()
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

    #region Effects

    bool OnClick()
    {
        switch (Type)
        {
            case "Pawn":
                return false;
            case "Rook":
                return false;
            case "Knight":
                return false;
            case "Bishop":
                switch (Ability)
                {
                    case "Blast":
                        Game.M.NeedPos = false;
                        UI.M.ToggleAbilityButton(() => GetTargetPiece("In Path"));
                        break;
                    case "Arcane Connection":
                        UI.M.ToggleAbilityButton(() => ACBishop());
                        break;
                }
                return true;
            case "Queen":
                switch (Ability)
                {
                    case "Deploy":
                        Game.M.NeedPos = true;
                        UI.M.ToggleAbilityButton(() => GetTargetPiece("Any"));
                        break;
                    case "Arcane Connection":
                        Game.M.NeedPos = false;
                        UI.M.ToggleAbilityButton(() => GetTargetPiece("Any"));
                        break;
                }
                return true;
            case "King":
                Game.M.NeedPos = false;
                UI.M.ToggleAbilityButton(() => GetTargetPiece("Any"));
                return true;
        }

        return false;
    }

    public void ACQueen(CustomPiece bishop)
    {
        SwapPos(bishop);
    }

    public void ACBishop()
    {
        Highlight(false);

        switch (Side)
        {
            case "White":
                SwapPos(Game.White.Queen);
                break;
            case "Black":
                SwapPos(Game.Black.Queen);
                break;
        }

        Game.M.NextTurn();
    }

    void SwapPos(CustomPiece target)
    {
        int x = PosX;
        int y = PosY;

        MoveTo(target.Pos);
        target.MoveTo(Tiles[x, y]);
    }

    void OnMove()
    {
        switch (Type)
        {
            case "Pawn":
                Attack();
                break;
            case "Rook":
                switch (Ability)
                {
                    case "":
                        Attack();
                        break;
                    case "Shield Wall":
                        break;
                    case "Reckless":
                        Attack();
                        Reckless();
                        break;
                }
                break;
            case "Knight":
                switch (Ability)
                {
                    case "":
                        Attack();
                        break;
                    case "Combat Medic":
                        Attack();
                        Revive();
                        break;
                    case "Charge":
                        Attack();
                        Charge();
                        break;
                }
                break;
            case "Bishop":
                switch (Ability)
                {
                    case "":
                        Attack();
                        break;
                    case "Blast":
                        Attack();
                        break;
                    case "Arcane Connection":
                        Attack();
                        break;
                }
                break;
            case "Queen":
                Attack();
                break;
            case "King":
                Attack();
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
        if(Game.M.TargetTile.Graves.Count > 0)
        {
            CustomPiece revive = null;

            Game.M.TargetTile.Graves.Reverse();

            foreach (CustomPiece p in Game.M.TargetTile.Graves)
            {
                if (p.Side == Side)
                {
                    revive = p;
                }
            }

            revive.enabled = true;
            revive.MoveTo(Pos);
            Game.M.TargetTile.Graves.Remove(revive);

            Game.M.TargetTile.Graves.Reverse();
        }

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

    public void UseAbility()
    {
        switch (Type)
        {
            case "Pawn":
                break;
            case "Rook":
                break;
            case "Knight":
                break;
            case "Bishop":
                switch (Ability)
                {
                    case "Blast":
                        Blast();
                        break;
                    case "Arcane Connection":
                        break;
                }

                break;
            case "Queen":
                switch(Ability)
                {
                    case "Deploy":
                        Deploy();
                        break;
                    case "Arcane Connection":
                        ACQueen();
                        break;
                }
                break;
        }

        Highlight(false);

        if(Ability == "For The King")
        {
            ForTheKing();
            Game.M.Clear();
        }
        else
        {
            Game.M.NextTurn();
        }
    }

    public string TargetValid(CustomPiece target)
    {
        switch (Type)
        {
            case "Pawn":
                return "";
            case "Rook":
                return "";
            case "Knight":
                return "";
            case "Bishop":
                switch (Ability)
                {
                    case "Blast":
                        if(target.Side != Side)
                        {
                            if (Path.Contains(target.Pos))
                            {
                                if (!target.Invincible)
                                {
                                    if (!target.Injured)
                                    {
                                        return "Valid";
                                    }
                                    else
                                    {
                                        return "Target Already Injured";
                                    }
                                }
                                else
                                {
                                    return "Target Invincible";
                                }
                            }
                            else
                                return "Target Is Not In Path";
                        }
                        else
                            return "Target Is Not An Enemy";

                    case "Arcane Connection":
                        break;
                }

                return "";
            case "Queen":
                switch (Ability)
                {
                    case "Deploy":
                        if (Grid.M.Distance(Pos, target.Pos) == 1)
                            return "Valid";
                        else
                            return "Too Far Away";

                    case "Arcane Connection":

                        if (target.Side == Side)
                        {
                            if (target.Type == "Bishop")
                                return "Valid";
                            else
                                return "Target is not a Bishop";
                        }
                        else
                            return "Target is not an Ally";
                }
                return "";
            case "King":
                if(Ability == "For The King")
                {
                    if (target.Side == Side)
                    {
                        if (target.Type == "Pawn")
                            return "Valid";
                        else
                            return "Target is not a Pawn";
                    }
                    else
                        return "Target is not an Ally";
                }
                return "";
        }

        return "";
    }

    void GetTargetPiece(string Type)
    {
        Highlight(false);

        switch (Type)
        {
            case "Any":
                Highlight(false);
                break;
            case "In Path":
                foreach (Tile t in Path)
                    if (t.Occupier)
                        t.l.enabled = true;
                break;
        }

        Game.M.SearchingPiece = true;
    }

    void GetTargetPosition()
    {
        Game.M.SearchingPiece = false;

        if (Game.M.NeedPos)
        {
            Game.M.Selected.Highlight(true);
            Game.M.SearchingPos = true;
        }
    }

    void Blast()
    {
        Game.M.AbilityTarget.Injured = true;
    }

    void Deploy()
    {
        Game.M.AbilityTarget.MoveTo(Game.M.AbilityPosition);
    }

    void ACQueen()
    {
        SwapPos(Game.M.AbilityTarget);
    }

    void ForTheKing()
    {
        UI.M.PromotionTarget = Game.M.AbilityTarget;
        UI.M.TogglePromoPanel(true);
        Game.M.AbilityTarget.Invincible = true;

        switch (Side)
        {
            case "White":
                Game.White.FTKTarget = Game.M.AbilityTarget;
                Game.White.FTKRound = Game.M.RoundCount;
                break;
            case "Black":
                Game.Black.FTKTarget = Game.M.AbilityTarget;
                Game.Black.FTKRound = Game.M.RoundCount;
                break;
        }

        FTKUsed = true;
    }

    #endregion

    public void Promote(string NewType)
    {
        Type = NewType;
        name = Side + " " + Type;

        switch (Side)
        {
            case "White":

                foreach(CustomPiece p in Game.White.All)
                {
                    if (Type == p.Type)
                        Ability = p.Ability;
                }

                break;
            case "Black":

                foreach (CustomPiece p in Game.Black.All)
                {
                    if (Type == p.Type)
                        Ability = p.Ability;
                }
                break;
        }

        Game.M.CalcMovePaths();
    }

    public void Demote()
    {
        Type = "Pawn";
        name = Side + " " + Type;

        switch (Side)
        {
            case "White":

                foreach (CustomPiece p in Game.White.All)
                {
                    if (Type == p.Type)
                        Ability = p.Ability;
                }

                break;
            case "Black":

                foreach (CustomPiece p in Game.Black.All)
                {
                    if (Type == p.Type)
                        Ability = p.Ability;
                }
                break;
        }

        Invincible = false;
    }
}