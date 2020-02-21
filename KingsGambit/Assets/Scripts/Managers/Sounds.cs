using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sounds : MonoBehaviour
{
    //Script used to play sounds during runtime

    public static Sounds M;
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
    }

    public AudioClip MaleHit;
    public AudioClip FemaleHit;
    public AudioClip Sword;
    public AudioClip Punch;
    public AudioClip Magic;
    public AudioClip Heal;
    public AudioClip Fire;
    public AudioClip Bow;
    public AudioClip BowDrawn;

    public void Play(string effect, Tile location)
    {
        location.Audio.clip = Load(effect);
        location.Audio.Play();
    }

    AudioClip Load(string effect)
    {
        switch (effect)
        {
            case "Male Hit":
                return MaleHit;
            case "Female Hit":
                return FemaleHit;
            case "Sword":
                return Sword;
            case "Punch":
                return Punch;
            case "Magic":
                return Magic;
            case "Heal":
                return Heal;
            case "Fire":
                return Fire;
            case "Bow":
                return Bow;
            case "Bow Drawn":
                return BowDrawn;
        }

        return null;
    }
}
