using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI : MonoBehaviour
{
    //Global UI handler script

    public static UI M;
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

    public Animation FadePanel;

    [Header("Info Display")]
    public Animation WhiteCrown;
    public Animation BlackCrown;
    public Animation WhiteCheck;
    public Animation BlackCheck;

    public TextMeshProUGUI Info;

    [Header ("Selected Piece Info")]
    public Image SelectImage;
    public Animation SelectAnim;
    public TextMeshProUGUI SelectedText;
    public Button SelectedButton;
    public TextMeshProUGUI SelectedExpand;
    public Button AbilityButton;

    [Header("Promotion Panel")]
    public GameObject PromoPanel;
    public Image PromoImage;
    public TextMeshProUGUI[] PromoTexts;
    public Button ConfirmButton;

    [HideInInspector] public CustomPiece PromotionTarget;

    [Header("Game State")]
    public GameObject InGameMenuPanel;
    private TextMeshProUGUI[] MenuTexts;
    private Button[] MenuButtons;

    [Header("Tooltip")]
    public GameObject Tooltips;
    public GameObject TooltipPrefab;
    private int ttCount = 0;

    private void Start()
    {
        StartCoroutine(SceneLoader.M.FadeOut());

        MenuTexts = InGameMenuPanel.GetComponentsInChildren<TextMeshProUGUI>();
        MenuButtons = InGameMenuPanel.GetComponentsInChildren<Button>();

        UpdateTurn();

        Abilities.Load();
    }

    private void Update()
    {
        Instructions();
        InGameMenu();
    }

    public void UpdateTurn()
    {
        switch (Game.M.Turn)
        {
            case "White":
                PlayAnim(WhiteCrown, true);
                PlayAnim(BlackCrown, false);
                break;
            case "Black":
                PlayAnim(WhiteCrown, false);
                PlayAnim(BlackCrown, true);
                break;
        }

        if(WhiteCheck.GetComponent<Image>().color.a == 1)
        {
            PlayAnim(WhiteCheck, false);
        }

        if (BlackCheck.GetComponent<Image>().color.a == 1)
        {
            PlayAnim(BlackCheck, false);
        }

        if (Game.M.InCheck)
        {
            switch (Game.M.Turn)
            {
                case "White":
                    PlayAnim(WhiteCheck, true);
                    break;
                case "Black":
                    PlayAnim(BlackCheck, true);
                    break;
            }
        }

        WhiteCrown.GetComponentsInChildren<TextMeshProUGUI>()[0].text = Game.White.DefencePower.ToString();
        WhiteCrown.GetComponentsInChildren<TextMeshProUGUI>()[1].text = Game.White.InvasionPower.ToString();

        BlackCrown.GetComponentsInChildren<TextMeshProUGUI>()[0].text = Game.Black.DefencePower.ToString();
        BlackCrown.GetComponentsInChildren<TextMeshProUGUI>()[1].text = Game.Black.InvasionPower.ToString();
    }

    #region Selection Info
    public void Selected(CustomPiece Target)
    {
        SelectImage.gameObject.SetActive(true);

        SelectImage.sprite = Resources.Load<Sprite>(Target.name.Split(' ')[0] + "/" + Target.name.Split(' ')[1]);
        PlayAnim(SelectAnim, true);

        SelectedText.text = Target.Ability;

        if (Target.Ability == "")
        {
            SelectedButton.gameObject.SetActive(false);
        }
        else
        {
            SelectedText.gameObject.SetActive(true);

            SelectedButton.gameObject.SetActive(true);
            SelectedButton.onClick.RemoveAllListeners();
            SelectedButton.onClick.AddListener(() => ToggleExpand());

            if(Target.Ability == "")
                SelectedExpand.text = Abilities.Movements[Target.Type];
            else
                SelectedExpand.text = Abilities.AbilityDescriptions[Target.Ability];


            AbilityButton.gameObject.SetActive(Target.OnClick());
        }
    }

    public void UnSelected()
    {
        PlayAnim(SelectAnim, false);
    }

    public void ToggleAbilityButton(UnityEngine.Events.UnityAction call, CustomPiece piece)
    {
        AbilityButton.gameObject.SetActive(true);
        AbilityButton.onClick.RemoveAllListeners();
        AbilityButton.onClick.AddListener(call);
        AbilityButton.onClick.AddListener(() => AbilityButton.gameObject.SetActive(false));
    }

    void ToggleExpand()
    {
        if(SelectedExpand.enabled)
        {
            SelectedExpand.enabled = false;
            SelectedButton.GetComponentInChildren<TextMeshProUGUI>().text = "Expand";
        }
        else
        {
            SelectedExpand.enabled = true;
            SelectedButton.GetComponentInChildren<TextMeshProUGUI>().text = "Close";
        }

    }

    void PlayAnim(Animation anim, bool forward)
    {
        if (forward)
        {
            anim[anim.clip.name].time = 0;
            anim[anim.clip.name].speed = 1f;
        }
        else
        {
            anim[anim.clip.name].time = anim[anim.clip.name].length;
            anim[anim.clip.name].speed = -1;
        }

        anim.Play(anim.clip.name);
    }
    #endregion

    #region Events
    public void TogglePromoPanel(bool toggle)
    {
        PromoPanel.SetActive(toggle);

        PromoImage.sprite = Resources.Load<Sprite>("White/Knight");
        PromoImage.color = new Color(1,1,1,0);
        PromoTexts[0].text = "";
        PromoTexts[1].text = "";
    }

    public void PromotionPanel(string Type)
    {
        PromoImage.sprite = Resources.Load<Sprite>(PromotionTarget.Side + "/" + Type);
        PromoImage.color = new Color(1, 1, 1, 1);

        PromoTexts[0].text = Type;
        PromoTexts[1].text = Abilities.Movements[Type];

        ConfirmButton.onClick.RemoveAllListeners();
        ConfirmButton.onClick.AddListener(() => PromotionTarget.Promote(Type));
        ConfirmButton.onClick.AddListener(() => TogglePromoPanel(false));

    }

    public void InGameMenu()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            InGameMenuPanel.SetActive(!InGameMenuPanel.activeSelf);

            MenuTexts[0].text = "Menu";
            MenuTexts[1].text = "";
            MenuTexts[2].text = "";

            MenuButtons[0].onClick.RemoveAllListeners();
            MenuButtons[1].onClick.RemoveAllListeners();
            MenuButtons[2].onClick.RemoveAllListeners();

            MenuButtons[0].onClick.AddListener(() => SceneLoader.M.LoadScene("PlayScene"));
            MenuButtons[1].onClick.AddListener(() => SceneLoader.M.LoadScene("AbilityScene"));
            MenuButtons[2].onClick.AddListener(() => SceneLoader.M.LoadScene("MainMenu"));
            MenuButtons[3].gameObject.SetActive(true);
            MenuButtons[3].onClick.AddListener(() => InGameMenuPanel.SetActive(!InGameMenuPanel.activeSelf));
        }
    }

    public void GameOver(string winner, string loser, string method)
    {
        InGameMenuPanel.SetActive(true);

        switch (method)
        {
            case "Checkmate":
                MenuTexts[1].text = winner + " Won By " + method;
                MenuTexts[2].text = loser + "'s King was being attacked by a " + winner + " piece and had no move to escape the check and the check could not be blocked. " + winner + " won by " + method + ".";
                break;
            case "Invasion":
                MenuTexts[1].text = winner + " Won By " + method;
                MenuTexts[2].text = winner + " had more power pieces on " + loser + "'s side of the field than vice versa. " + winner + " Wins By " + method + ".";
                break;
        }

        MenuButtons[0].onClick.AddListener(() => SceneLoader.M.LoadScene("PlayScene"));
        MenuButtons[1].onClick.AddListener(() => SceneLoader.M.LoadScene("AbilityScene"));
        MenuButtons[2].onClick.AddListener(() => SceneLoader.M.LoadScene("MainMenu"));
        MenuButtons[3].gameObject.SetActive(false);
    }

    #endregion

    #region Player Info
    public void Tooltip(string message)
    {
        bool exists = false;

        foreach (TextMeshProUGUI child in Tooltips.GetComponentsInChildren<TextMeshProUGUI>())
        {
            if (child.text == message)
                exists = true;
        }

        if(!exists)
        {
            foreach (Transform child in Tooltips.transform)
            {
                if (child != Tooltips.transform)
                    child.localPosition = new Vector3(child.localPosition.x, child.localPosition.y - 30, child.localPosition.z);
            }

            StartCoroutine(SpawnTT(message));
        }
        
    }

    IEnumerator SpawnTT(string message)
    {
        GameObject temp = Instantiate(TooltipPrefab, Tooltips.transform);
        temp.GetComponentInChildren<TextMeshProUGUI>().text = message;
        ttCount++;
        Destroy(temp, 3f);
        yield return new WaitForSeconds(2.5f);
        PlayAnim(temp.GetComponent<Animation>(), false);
        ttCount--;
    }

    void Instructions()
    {
        if(!Game.M.Selected)
        {
            Info.text = "- Select a Piece";
        }
        else
        {
            Info.text = "";

            if(Game.M.Selected.Path.Count > 0)
            {
                Info.text += "- Select a Highlighted Target Position \n";
            }

            if(Game.M.Selected.OnClick())
            {
                Info.text += "- Use ''" + Game.M.Selected.Ability + "'' \n";
            }

            if(Game.M.SearchingPiece)
            {
                Info.text = "- Select a Target for ''" + Game.M.Selected.Ability + "''";
            }

            if (Game.M.SearchingPos)
            {
                Info.text = "- Select a Target Position for ''" + Game.M.Selected.Ability + "";
            }
        }

    }

    #endregion
}