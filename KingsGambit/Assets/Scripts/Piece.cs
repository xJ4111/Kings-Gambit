using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour
{
    public string Side;

    private int posX, posY;
    private Tile[,] Tiles;
    private int activeTiles;



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
    }

    public void SetPos(int x, int y)
    {
        posX = x;
        posY = y;
    }

    void Highlight(bool toggle)
    {
        if(name.Contains("Pawn"))
        {
            Pawn(toggle);
        }
    }
    void Check(int posX, int posY, bool toggle)
    {
        if (!Tiles[posX, posY].Occupier)
        {
            Tiles[posX, posY].l.enabled = toggle;
            activeTiles++;
        }
    }

    void Attack(int posX, int posY, bool toggle)
    {
        if (Tiles[posX, posY].Occupier && Tiles[posX, posY].Occupier.Side != Side)
        {
            Tiles[posX, posY].l.enabled = toggle;
            activeTiles++;
        }

    }

    int Offset(int pos, int offset)
    {
        switch (Side)
        {
            case "White": return pos - offset;
            case "Black": return pos + offset;
        }

        return 0;
    }


    void Movement()
    {
        if(Game.M.Selected == this && Game.M.TargetTile)
        {
            Highlight(false);

            Restart();
        }
    }

    void Move()
    {
        //Attack
        if (Game.M.TargetTile.Occupier)
            Destroy(Game.M.TargetTile.Occupier.gameObject);

        //Move
        transform.position = Game.M.TargetTile.transform.position;
    }

    void Restart()
    {
        Game.M.Selected = null;
        Game.M.TargetTile = null;
        activeTiles = 0;
    }

    private void OnMouseOver()
    {
        if(!Game.M.Selected)
        {
            Highlight(true);

            if (Input.GetMouseButtonDown(1) && activeTiles > 0)
            {
                Game.M.Selected = this;
            }
        }
    }

    private void OnMouseExit()
    {
        if(Game.M.Selected != this)
        {
            Highlight(false);
        }
    }

    #region Piece Movement

    void Pawn(bool toggle)
    {
        //Front 2 Spaces
        Check(Offset(posX, 1), posY, toggle);

        if (toggle && Tiles[Offset(posX, 1), posY].l.enabled)
            Check(Offset(posX, 2), posY, toggle);
        else if (!toggle)
            Check(Offset(posX, 2), posY, toggle);

        //Diagonal Left
        Attack(Offset(posX, 1), Offset(posY, 1), toggle);

        //Diagonal Right
        Attack(Offset(posX, 1), Offset(posY, -1), toggle);
    }

    #endregion
}