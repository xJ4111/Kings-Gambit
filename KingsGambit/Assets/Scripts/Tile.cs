using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public string gridPos = "00";
    public Light l;

    void Start()
    {
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
                    Debug.Log(transform.position.ToString("F3") + " : " + Grid.M.GridPos[i, j].ToString("F3") + (transform.position == Grid.M.GridPos[i, j]));
                    gridPos = i + "" + j;
                }
            }
        }
    }

    private void OnMouseOver()
    {
        l.enabled = true;
    }

    private void OnMouseExit()
    {
        l.enabled = false;
    }
}
