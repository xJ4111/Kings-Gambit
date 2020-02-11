using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
public class MainMenu : MonoBehaviour
{
    public GameObject[] Pages;
    private Button[] bFront, bPlay, bSettings, bVideo, bAudio;
    private TextMeshProUGUI[] tFront, tPlay, tSettings;

    // Start is called before the first frame update
    void Start()
    {
        bFront = Pages[0].GetComponentsInChildren<Button>();
        tFront = Pages[0].GetComponentsInChildren<TextMeshProUGUI>();

        bPlay = Pages[1].GetComponentsInChildren<Button>();
        tPlay = Pages[1].GetComponentsInChildren<TextMeshProUGUI>();

        bSettings = Pages[2].GetComponentsInChildren<Button>();
        tSettings = Pages[2].GetComponentsInChildren<TextMeshProUGUI>();

        bVideo = Pages[3].GetComponentsInChildren<Button>();
        bAudio = Pages[4].GetComponentsInChildren<Button>();

        SetupFront();
        SetupPlay();
        SetupSettings();
        SetupVideo();
        SetupAudio();
    }

    void TogglePage(int page)
    {
        for(int i = 0; i < Pages.Length; i++)
        {
            if (i == page)
                Pages[i].SetActive(true);
            else
                Pages[i].SetActive(false);
        }
    }

    void SetupFront()
    {
        foreach (Button b in bFront)
            b.onClick.RemoveAllListeners();

        bFront[0].onClick.AddListener(() => TogglePage(1));
        bFront[1].onClick.AddListener(() => TogglePage(2));
        bFront[2].onClick.AddListener(() => Application.Quit());
    }

    void SetupPlay()
    {
        foreach (Button b in bPlay)
            b.onClick.RemoveAllListeners();

        bPlay[0].onClick.AddListener(() => SceneManager.LoadScene("AbilityScene"));
        bPlay[1].onClick.AddListener(() => SceneManager.LoadScene("PlayScene"));
        bPlay[2].onClick.AddListener(() => TogglePage(0));
    }

    void SetupSettings()
    {
        foreach (Button b in bSettings)
            b.onClick.RemoveAllListeners();

        bSettings[0].onClick.AddListener(() => TogglePage(3));
        bSettings[1].onClick.AddListener(() => TogglePage(4));
        bSettings[2].onClick.AddListener(() => TogglePage(0));
    }

    void SetupVideo()
    {
        foreach (Button b in bVideo)
            b.onClick.RemoveAllListeners();

        //bVideo[0].onClick.AddListener(() => ApplySettings());

        bVideo[1].onClick.AddListener(() => TogglePage(2));

    }

    void SetupAudio()
    {
        foreach (Button b in bAudio)
            b.onClick.RemoveAllListeners();

        //bAudio[0].onClick.AddListener(() => ApplySettings());

        bAudio[1].onClick.AddListener(() => TogglePage(2));

    }
}
