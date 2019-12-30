using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    public static UI M;

    [Header("Info Display")]
    public Text TurnText, SelectedText;

    // Start is called before the first frame update
    void Awake()
    {
        if (M == null)
        {
            M = this;
        }
        else if (M != this)
        {
            Destroy(this);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    void UpdateText()
    {
        TurnText.text = Game.M.Turn;
    }

}
