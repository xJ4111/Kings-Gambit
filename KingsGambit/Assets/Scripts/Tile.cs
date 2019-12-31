using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public int PosX, PosY = 0;
    public Light l;
    public Piece Occupier;
    public bool Safe;
    public Piece onner;

    void Start()
    {
        Safe = true;
        FindPosition();
    }

    // Update is called once per frame
    void Update()
    {
       
    }

    void FindPosition()
    {
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (transform.position == Grid.M.GridPos[i, j])
                {
                    PosX = i;
                    PosY = j;
                    Grid.M.Tiles[i, j] = this;
                }
            }
        }
    }

    public void Enter(Piece piece)
    {
        Occupier = piece.GetComponent<Piece>();
        Occupier.posX = PosX;
        Occupier.posY = PosY;
    }

    public void Exit()
    {
        Occupier = null;
    }

    private void OnMouseOver()
    {
        if(Input.GetMouseButtonDown(0))
        {
            if (Game.M.Selected && l.enabled)
            {
                Game.M.TargetTile = this;
            }
        }

    }
}
