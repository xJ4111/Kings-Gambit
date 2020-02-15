using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class UI : MonoBehaviour
{
    public static UI M;

    [Header("Info Display")]
    public TextMeshProUGUI TurnText, SelectedText;

    [Header("Promotion Panel")]
    public GameObject PromoPanel;
    public Image PromoImage;
    public TextMeshProUGUI[] PromoTexts;
    public Button ConfirmButton;
    public Button AbilityButton;
    private TextMeshProUGUI AbilityText;

    [HideInInspector] public CustomPiece PromotionTarget;

    [Header("Game State")]
    public GameObject GameOverPanel;
    private TextMeshProUGUI[] GOTexts;
    private Button[] GOButtons;

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
        AbilityText = AbilityButton.GetComponentsInChildren<TextMeshProUGUI>()[1];
        GOTexts = GameOverPanel.GetComponentsInChildren<TextMeshProUGUI>();
        GOButtons = GameOverPanel.GetComponentsInChildren<Button>();
    }

    void UpdateText()
    {
        TurnText.text = Game.M.Turn;
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
        PromoTexts[1].text = AbilityManager.M.Descriptions[Type];

        ConfirmButton.onClick.RemoveAllListeners();
        ConfirmButton.onClick.AddListener(() => PromotionTarget.Promote(Type));
        ConfirmButton.onClick.AddListener(() => TogglePromoPanel(false));
    }

    public void ToggleAbilityButton(UnityEngine.Events.UnityAction call, CustomPiece piece)
    {
        AbilityButton.gameObject.SetActive(true);
        AbilityButton.onClick.RemoveAllListeners();
        AbilityButton.onClick.AddListener(call);
        AbilityButton.onClick.AddListener(() => ToggleAbilityButton());

        if (AbilityManager.M.Abilities[piece.Type].Item1.Name == piece.Ability)
        {
            AbilityButton.GetComponentInChildren<TextMeshProUGUI>().text = AbilityManager.M.Abilities[piece.Type].Item1.Name;
            AbilityText.text = AbilityManager.M.Abilities[piece.Type].Item1.Description;
        }
        else
        {
            AbilityButton.GetComponentInChildren<TextMeshProUGUI>().text = AbilityManager.M.Abilities[piece.Type].Item2.Name;
            AbilityText.text = AbilityManager.M.Abilities[piece.Type].Item2.Description;
        }

    }
    public void ToggleAbilityButton()
    {
        AbilityButton.gameObject.SetActive(false);
    }

    public void GameOver(string winner, string loser, string method)
    {
        GameOverPanel.SetActive(true);

        switch (method)
        {
            case "Checkmate":
                GOTexts[1].text = winner + " Won By " + method;
                GOTexts[2].text = loser + "'s King was being attacked by a " + winner + " piece and had no move to escape the check and the check could not be blocked. " + winner + " won by " + method + ".";
                break;
            case "Invasion":
                GOTexts[1].text = winner + " Won By " + method;
                GOTexts[2].text = winner + " had more power pieces on " + loser + "'s side of the field than vice versa. " + winner + " Wins By " + method + ".";
                break;
        }

        GOButtons[0].onClick.AddListener(() => SceneManager.LoadScene("PlayScene"));
        GOButtons[1].onClick.AddListener(() => SceneManager.LoadScene("AbilityScene"));
        GOButtons[2].onClick.AddListener(() => SceneManager.LoadScene("MainMenu"));
    }
}