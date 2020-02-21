using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Abilities
{
    //script used to handle ability data, read in from Excel documents

    public static bool Loaded;

    public static Dictionary<string, string> Movements = new Dictionary<string, string>();
    public static Dictionary<string, System.Tuple<Ability, Ability>> AllAbilities = new Dictionary<string, System.Tuple<Ability, Ability>>();//Used to query possible abilities for pieces
    public static Dictionary<string, string> AbilityDescriptions = new Dictionary<string, string>(); //Used to directly query ability descriptions


    public class Ability
    {
        public string Name;
        public string Lore;
        public string Description;

        public Ability() { }

        public Ability(string n, string l, string d)
        {
            Name = n;
            Lore = l;
            Description = d;
        }
    }

    public static void Load()
    {
        if(!Loaded)
        {
            LoadMovementsCSV();
            LoadAbilitiesCSV();
            Loaded = true;
        }
    }

    public static void LoadMovementsCSV()
    {
        TextAsset data = Resources.Load<TextAsset>("Movements");

        string[] lines = data.text.Split('\n');

        for (int i = 1; i < lines.Length; i++)
        {
            string[] cells = lines[i].Split(',');
            Movements.Add(cells[0], cells[1]);
        }
    }

    public static void LoadAbilitiesCSV()
    {
        TextAsset file = Resources.Load<TextAsset>("Abilities");
        string[] lines = file.text.Split('\n');

        for (int i = 1; i < lines.Length - 1; i++)
        {
            string[] cells = lines[i].Split(',');

            if (!AllAbilities.ContainsKey(cells[0]))
            {
                AllAbilities.Add(cells[0], new System.Tuple<Ability, Ability>(new Ability(cells[1], cells[2], cells[3]), new Ability()));
            }
            else
            {
                Ability temp = AllAbilities[cells[0]].Item1;
                AllAbilities[cells[0]] = new System.Tuple<Ability, Ability>(temp, new Ability(cells[1], cells[2], cells[3]));
            }

            AbilityDescriptions.Add(cells[1], cells[3]);
        }
    }
}
