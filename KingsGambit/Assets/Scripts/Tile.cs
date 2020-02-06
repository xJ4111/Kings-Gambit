using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public int PosY, PosX = 0;
    public Light l;
    public CustomPiece Occupier;
    public bool Safe;

    public List<CustomPiece> Graves;

    void Awake()
    {
        Safe = true;
        StartCoroutine(FindPosition());
    }
    IEnumerator FindPosition()
    {
        yield return new WaitForEndOfFrame();
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
        Occupier = piece.GetComponent<CustomPiece>();
        Occupier.Pos = this;
        Occupier.PosY = PosY;
        Occupier.PosX = PosX;
    }

    public void Exit()
    {
        Occupier = null;
    }

    private void OnMouseOver()
    {
        if(Input.GetMouseButtonDown(0))
        {
            if (Game.M.Selected && !Game.M.TargetTile && l.enabled)
            {
                Game.M.TargetTile = this;
            }
        }

    }
}
