using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI : MonoBehaviour
{
    public static UI M;

    [Header("Info Display")]
    public Text TurnText, SelectedText;

    [Header("Promotion Panel")]
    public GameObject PromoPanel;
    public Image PromoImage;
    public TextMeshProUGUI[] PromoTexts;
    public Button ConfirmButton;
    public Button AbilityButton;

    [HideInInspector] public CustomPiece PromotionTarget;

    private Dictionary<string, string> Descriptions = new Dictionary<string, string>();

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

    private void Start()
    {
        LoadDescriptions();
    }

    void UpdateText()
    {
        TurnText.text = Game.M.Turn;
    }

    void LoadDescriptions()
    {
        TextAsset data = Resources.Load<TextAsset>("Descriptions");

        string[] lines = data.text.Split('\n');

        for (int i = 1; i < lines.Length; i++)
        {
            string[] cells = lines[i].Split(',');
            Descriptions.Add(cells[0], cells[1]);
        }
    }

    public void TogglePromoPanel(bool toggle)
    {
        PromoPanel.SetActive(toggle);

        PromoImage.sprite = Resources.Load<Sprite>("Knight");
        PromoImage.color = new Color(1,1,1,0);
        PromoTexts[0].text = "";
        PromoTexts[1].text = "";
    }

    public void PromotionPanel(string Type)
    {
        PromoImage.sprite = Resources.Load<Sprite>(Type);
        PromoImage.color = new Color(1, 1, 1, 1);

        PromoTexts[0].text = Type;
        PromoTexts[1].text = Descriptions[Type];

        ConfirmButton.onClick.RemoveAllListeners();
        ConfirmButton.onClick.AddListener(() => PromotionTarget.Promote(Type));
        ConfirmButton.onClick.AddListener(() => TogglePromoPanel(false));
    }

    public void ToggleAbilityButton(UnityEngine.Events.UnityAction call)
    {
        AbilityButton.gameObject.SetActive(true);
        AbilityButton.onClick.RemoveAllListeners();
        AbilityButton.onClick.AddListener(call);
        AbilityButton.onClick.AddListener(() => ToggleAbilityButton());
    }
    public void ToggleAbilityButton()
    {
        AbilityButton.gameObject.SetActive(false);
    }
}