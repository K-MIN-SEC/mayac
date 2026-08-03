using UnityEngine;
using System.Collections.Generic;

public class ReadCSV : MonoBehaviour
{
    [SerializeField] private List<TextAsset> NpcDialogueCSV = new List<TextAsset>();

    void ParseDialogueData(string data)
    {
        Debug.Log("ReadDialogue");

        //var d = DataManager.instance;
        string[] rows = data.Split('\n');
        for (int i = 1; i < rows.Length; i++)
        {
            string[] columns = rows[i].Split(',');

            
        }
    }
}
