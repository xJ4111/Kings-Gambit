using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleLoader : MonoBehaviour
{
    //Script used to load particle effects at certain positions during runtime

    public static ParticleLoader M;
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

    public GameObject Energy;
    public GameObject SmallEnergy;
    public GameObject Health;
    public GameObject Blood;
    public GameObject Teleport;
    public GameObject Spark;
    public GameObject Explosion;

    public void Spawn(string effect, Tile location)
    {
        GameObject temp = Instantiate(Load(effect), location.transform);
        if (location.Occupier)
            temp.transform.rotation = location.Occupier.transform.rotation;
        Destroy(temp, temp.GetComponentInChildren<ParticleSystem>().main.duration);
    }

    GameObject Load(string effect)
    {
        switch(effect)
        {
            case "Energy":
                return Energy;
            case "Health":
                return Health;
            case "Blood":
                return Blood;
            case "Teleport":
                return Teleport;
            case "Spark":
                return Spark;
            case "Explosion":
                return Explosion;
        }

        return null;
    }
}
