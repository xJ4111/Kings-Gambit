using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AbilityManager : MonoBehaviour
{
    public static AbilityManager M;
    private void Awake()
    {
        if (M == null)
        {
            M = this;
        }
        else if (M != this)
        {
            Destroy(gameObject);
            Destroy(this);
        }
    }

    [Header("UI")]
    public TextMeshProUGUI Title;
    public GameObject DetailPanel;
    public TextMeshProUGUI Current;

    public GameObject Ability1;
    public GameObject Ability2;

    public TextMeshProUGUI Normal;

    private TextMeshProUGUI ab1name, ab1text, ab2name, ab2text;
    private Button ab1button, ab2button;

    public Button prev, next, overview;

    public GameObject OverviewPanel;
    public GameObject[] OverviewSlots;

    public GameObject FinalizePanel;
    public GameObject[] FinalizeSlots;

    [Header("Camera")]
    public GameObject Cam;
    public float MoveSpeed;
    private Vector3 targetPos;
    public float RotSpeed;
    private Quaternion targetRot;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(SceneLoader.M.FadeOut());
        Setup();
        Abilities.Load();
        LoadPage(0);
    }

    // Update is called once per frame
    void Update()
    {
        if(Cam)
        {
            Cam.transform.position = Vector3.Lerp(Cam.transform.position, targetPos, MoveSpeed * Time.deltaTime);
            Cam.transform.rotation = Quaternion.Slerp(Cam.transform.rotation, targetRot, RotSpeed * Time.deltaTime);
        }
    }


    void Setup()
    {
        ab1name = Ability1.GetComponentsInChildren<TextMeshProUGUI>()[0];
        ab2name = Ability2.GetComponentsInChildren<TextMeshProUGUI>()[0];

        ab1text = Ability1.GetComponentsInChildren<TextMeshProUGUI>()[1];
        ab2text = Ability2.GetComponentsInChildren<TextMeshProUGUI>()[1];

        ab1button = Ability1.GetComponentInChildren<Button>();
        ab2button = Ability2.GetComponentInChildren<Button>();

        //Selected.Clear();

        for (int i = 0; i < 2; i++)
        {
            string side;

            if (i == 0)
                side = "White";
            else
                side = "Black";

            Abilities.Selected.Add(side + " Pawn", "");
            Abilities.Selected.Add(side + " Rook", "");
            Abilities.Selected.Add(side + " Knight", "");
            Abilities.Selected.Add(side + " Bishop", "");
            Abilities.Selected.Add(side + " Queen", "");
        }


    }

    void LoadPage(int pageNum)
    {
        Title.transform.parent.gameObject.SetActive(true);

        if (pageNum < 5)
        {
            targetPos = new Vector3(0, 5, 0);
            targetRot = Quaternion.Euler(0, 72 * pageNum, 0);
            Title.text = "White Side \n Abilities";
        }
        else
        {
            targetPos = new Vector3(0, 5, -100);
            targetRot = Quaternion.Euler(0, 72 * (pageNum - 1), 0);
            Title.text = "Black Side \n Abilities";
        }


        switch (pageNum)
        {
            case 0:
                Page("Pawn", 0, pageNum);
                break;
            case 1:
                Page("Rook", 1, pageNum);
                break;
            case 2:
                Page("Knight", 2, pageNum);
                break;
            case 3:
                Page("Bishop", 3, pageNum);
                break;
            case 4:
                Page("Queen", 4, pageNum);
                break;
            case 5:
                targetRot = Quaternion.Euler(90 - 15, 0, 0);
                targetPos = new Vector3(0, 50, 0);
                Overview("White", pageNum);
                break;
            case 6:
                Page("Pawn", 5, pageNum);
                break;
            case 7:
                Page("Rook", 6, pageNum);
                break;
            case 8:
                Page("Knight", 7, pageNum);
                break;
            case 9:
                Page("Bishop", 8, pageNum);
                break;
            case 10:
                Page("Queen", 9, pageNum);
                break;
            case 11:
                targetRot = Quaternion.Euler(90 - 15, 0, 0);
                targetPos = new Vector3(0, 50, -100);
                Overview("Black", pageNum);
                break;
            case 12:
                targetRot = Quaternion.Euler(90 - 15, 90, 0);
                targetPos = new Vector3(0, 75, -50);
                Finalize(pageNum);
                break;
        }

        overview.onClick.RemoveAllListeners();
        overview.gameObject.SetActive(true);

        if (pageNum < 5)
            overview.onClick.AddListener(() => LoadPage(5));
        else if (pageNum == 5)
            overview.gameObject.SetActive(false);
        else if (pageNum < 11)
            overview.onClick.AddListener(() => LoadPage(11));
        else if (pageNum == 11)
            overview.gameObject.SetActive(false);
    }
     
    void Page(string piece, int index, int pageNum)
    {
        DetailPanel.SetActive(true);
        OverviewPanel.SetActive(false);
        FinalizePanel.SetActive(false);

        if (index == 0)
            prev.gameObject.SetActive(false); 
        else
        {
            prev.gameObject.SetActive(true);
            prev.onClick.RemoveAllListeners();
            prev.onClick.AddListener(() => LoadPage(pageNum - 1));
        }

        string side;
        if (index < 5)
            side = "White";
        else
            side = "Black";

        Current.text = piece;
        if (Abilities.Selected.ContainsKey(side + " " + piece))
            Normal.text = Abilities.Movements[piece] + "\n\nAbility: " + Abilities.Selected[side + " " + piece];
        else
            Normal.text = Abilities.Movements[piece] + "\n\nNo Ability Selected";

        ab1button.onClick.RemoveAllListeners();
        ab1button.onClick.AddListener(() => SetSelected(side + " " + piece, Abilities.AllAbilities[piece].Item1.Name));
        ab1button.onClick.AddListener(() => LoadPage(pageNum));

        ab1name.text = Abilities.AllAbilities[piece].Item1.Name;
        ab1text.text = "''" + Abilities.AllAbilities[piece].Item1.Lore + "''" + "\n\n" + Abilities.AllAbilities[piece].Item1.Description;

        ab2button.onClick.RemoveAllListeners();
        ab2button.onClick.AddListener(() => SetSelected(side + " " + piece, Abilities.AllAbilities[piece].Item2.Name));
        ab2button.onClick.AddListener(() => LoadPage(pageNum));

        ab2name.text = Abilities.AllAbilities[piece].Item2.Name;
        ab2text.text = "''" + Abilities.AllAbilities[piece].Item2.Lore + "''" + "\n\n" + Abilities.AllAbilities[piece].Item2.Description;

        next.GetComponentInChildren<TextMeshProUGUI>().text = "Next";
        next.onClick.RemoveAllListeners();
        next.onClick.AddListener(() => LoadPage(pageNum + 1));
    }

    void SetSelected(string piece, string ability)
    {
        if (Abilities.Selected.ContainsKey(piece))
            Abilities.Selected[piece] = ability;
        else
            Abilities.Selected.Add(piece, ability);
    }

    void Overview(string side, int pageNum)
    {
        prev.gameObject.SetActive(true);
        prev.onClick.RemoveAllListeners();
        prev.onClick.AddListener(() => LoadPage(pageNum - 1));

        DetailPanel.SetActive(false);
        OverviewPanel.SetActive(true);
        FinalizePanel.SetActive(false);

        Title.transform.parent.gameObject.SetActive(true);
        Title.text = side + " Side \n Overview";

        foreach (GameObject slot in OverviewSlots)
        {
            TextMeshProUGUI t1 = slot.GetComponentsInChildren<TextMeshProUGUI>()[0];
            TextMeshProUGUI t2 = slot.GetComponentsInChildren<TextMeshProUGUI>()[1];
            Button b1 = slot.GetComponentsInChildren<Button>()[0];
            Button b2 = slot.GetComponentsInChildren<Button>()[1];

            System.Tuple<Abilities.Ability, Abilities.Ability> temp = Abilities.AllAbilities[t1.text];

            if (Abilities.Selected[side + " " + t1.text] != "")
                t2.text = Abilities.Selected[side + " " + t1.text];
            else
                t2.text = "No Ability Selected";

            b1.GetComponentInChildren<TextMeshProUGUI>().text = temp.Item1.Name;
            b1.onClick.RemoveAllListeners();
            b1.onClick.AddListener(() => SetSelected(side + " " +t1.text, temp.Item1.Name));
            b1.onClick.AddListener(() => Overview(side, pageNum));

            b2.GetComponentInChildren<TextMeshProUGUI>().text = temp.Item2.Name;
            b2.onClick.RemoveAllListeners();
            b2.onClick.AddListener(() => SetSelected(side + " " + t1.text, temp.Item2.Name));
            b2.onClick.AddListener(() => Overview(side, pageNum));
        }

        next.onClick.RemoveAllListeners();
        next.GetComponentInChildren<TextMeshProUGUI>().text = "Next";
        next.onClick.AddListener(() => LoadPage(pageNum + 1));

        if (pageNum == 11)
            next.GetComponentInChildren<TextMeshProUGUI>().text = "Finalize";
    }

    void Finalize(int pageNum)
    {
        Title.transform.parent.gameObject.SetActive(false);

        prev.onClick.RemoveAllListeners();
        prev.onClick.AddListener(() => LoadPage(pageNum - 1));

        overview.gameObject.SetActive(false);

        DetailPanel.SetActive(false);
        OverviewPanel.SetActive(false);
        FinalizePanel.SetActive(true);

        for (int i = 0; i < Abilities.Selected.Count; i++)
        {
            TextMeshProUGUI t1 = FinalizeSlots[i].GetComponentsInChildren<TextMeshProUGUI>()[0];
            TextMeshProUGUI t2 = FinalizeSlots[i].GetComponentsInChildren<TextMeshProUGUI>()[1];
            Button b1 = FinalizeSlots[i].GetComponentsInChildren<Button>()[0];
            Button b2 = FinalizeSlots[i].GetComponentsInChildren<Button>()[1];

            System.Tuple<Abilities.Ability, Abilities.Ability> temp = Abilities.AllAbilities[t1.text];
            string side;

            if (i < 5)
                side = "White";
            else
                side = "Black";

            if (Abilities.Selected[side + " " + t1.text] != "")
                t2.text = Abilities.Selected[side + " " + t1.text];
            else
                t2.text = "No Ability Selected";

            b1.GetComponentInChildren<TextMeshProUGUI>().text = temp.Item1.Name;
            b1.onClick.RemoveAllListeners();
            b1.onClick.AddListener(() => SetSelected(side + " " + t1.text, temp.Item1.Name));
            b1.onClick.AddListener(() => Finalize(pageNum));

            b2.GetComponentInChildren<TextMeshProUGUI>().text = temp.Item2.Name;
            b2.onClick.RemoveAllListeners();
            b2.onClick.AddListener(() => SetSelected(side + " " + t1.text, temp.Item2.Name));
            b2.onClick.AddListener(() => Finalize(pageNum));

            next.onClick.RemoveAllListeners();
            next.GetComponentInChildren<TextMeshProUGUI>().text = "Start Game";
            next.onClick.AddListener(() => StartGame());
        }
    }

    void StartGame()
    {
        Abilities.Selected.Add("White King", "For The King");
        Abilities.Selected.Add("Black King", "For The King");

        SceneLoader.M.LoadScene("PlayScene");
    }
}
