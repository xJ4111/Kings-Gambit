using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public static Grid M;

    public Vector3[,] GridPos = new Vector3[8,8];
    public Tile[,] Tiles = new Tile[8, 8];

    private void Awake()
    {
        if (M == null)
        {
            M = this;
        }
        else if (M != this)
        {
            Destroy(this);
        }

        SetGridCoords();
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void SetGridCoords()
    {
        Vector3 tempCoord = new Vector3(-14f,0f,14f);

        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                GridPos[i,j] = tempCoord;
                tempCoord.x += 4f;
            }
            tempCoord.x = -14f;
            tempCoord.z -= 4f;
        }
    }
}