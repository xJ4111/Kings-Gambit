using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour
{
    private int posX, posY;
    public Vector3 targetPos = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Move();
    }

    public void SetPos(int x, int y)
    {
        posX = x;
        posY = y;
    }

    void Highlight(bool toggle)
    {
        if(name.Contains("White"))
        {
            Grid.M.Tiles[posX - 1, posY].l.enabled = toggle;
            Grid.M.Tiles[posX - 2, posY].l.enabled = toggle;
        }
        
        if(name.Contains("Black"))
        {
            Grid.M.Tiles[posX + 1, posY].l.enabled = toggle;
            Grid.M.Tiles[posX + 2, posY].l.enabled = toggle;
        }

    }

    void Move()
    {
        if(Game.M.Selected == this && Game.M.TargetTile)
        {
            transform.position = Game.M.TargetTile.transform.position;
            Game.M.Selected = null;
            Game.M.TargetTile = null;
            Highlight(false);
        }
    }


    private void OnMouseOver()
    {
        Highlight(true);

        if (Input.GetMouseButtonDown(1))
        {
            Game.M.Selected = this;
        }
    }

    private void OnMouseExit()
    {
        if(Game.M.Selected != this)
        {
            Highlight(false);
        }
    }
}
