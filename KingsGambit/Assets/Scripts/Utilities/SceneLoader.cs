using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    //Script used to load scenes with a fade in/fade out effect.

    public static SceneLoader M;

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

        DontDestroyOnLoad(this);
    }

    Animation FadePanel;

    public void LoadScene(string scene)
    {
        StartCoroutine(Load(scene));
    }

    IEnumerator Load(string scene)
    {
        FadeIn();
        yield return new WaitForSeconds(1);
        SceneManager.LoadScene(scene);
    }

    void FadeIn()
    {
        FadePanel.gameObject.SetActive(true);
        FadePanel.Play("FadeIn");
    }

    public IEnumerator FadeOut()
    {
        if (!FadePanel)
            FadePanel = GameObject.Find("Fade").GetComponentInChildren<Animation>();

        FadePanel.Play("FadeOut");
        yield return new WaitForSeconds(1);
        FadePanel.gameObject.SetActive(false);
    }
}
