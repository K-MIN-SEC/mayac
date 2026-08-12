using UnityEngine;
using System.Collections.Generic;

public class DataManager : MonoBehaviour
{
    public static DataManager instance { get; private set; }
    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            instance = this;
        }
        DontDestroyOnLoad(gameObject);
    }

    public Queue<Dialogue> ParseDialogueData(string data, int index)
    {
        Queue<Dialogue> dialogBox = new();

        string[] rows = data.Split('\n');
        for (int i = 1; i < rows.Length; i++)
        {
            string[] columns = rows[i].Split(',');
            if(columns.Length == 0) continue;
            if(columns[0] == "") continue;
            if(index != int.Parse(columns[0])) continue;
            //Debug.Log($"[Dialogue] {columns[0]} {columns[1]} {columns[2]}");
            Debug.Log($"{rows[i]}");
            var newText = new Dialogue()
            {
                name = columns[1],
                text = columns[2],
            };
            if (columns[3] != "")
            {
                newText.isOption = true;
                newText.OptionA = columns[3];
                newText.OptionB = columns[4];
            }
            dialogBox.Enqueue(newText);
        }
        return dialogBox;
    }
}
