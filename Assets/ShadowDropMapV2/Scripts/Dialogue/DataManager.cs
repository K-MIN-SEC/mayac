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
            if (string.IsNullOrWhiteSpace(rows[i])) continue;

            string[] columns = rows[i].Split(',');
            if (columns.Length < 8) continue;
            if (string.IsNullOrEmpty(columns[0])) continue;
            if (index != int.Parse(columns[0])) continue;

            var newText = new Dialogue()
            {
                index = index,
                name = columns[1].Trim(),
                text = columns[2].Trim(),
                triggerType = columns[3].Trim(),
                startEventName = columns[4].Trim(),
                endEventName = columns[5].Trim(),
            };
            newText.text = newText.text.Replace("\\", "\n").Replace("|", ",");

            if (!string.IsNullOrEmpty(columns[6]))
            {
                newText.isOption = true;
                newText.OptionA = columns[6].Trim();
                newText.OptionB = columns[7].Trim();
            }

            dialogBox.Enqueue(newText);
        }
        return dialogBox;
    }

    public List<MessengerMessageData> ParseMessageData(string data, int index)
    {
        List<MessengerMessageData> messageBox = new();
        string[] rows = data.Split('\n');
        for (int i = 1; i < rows.Length; i++)
        {
            string[] columns = rows[i].Split(',');
            if (columns.Length == 0) continue;
            if (columns[0] == "") continue;
            if (index != int.Parse(columns[0])) continue;

            MessengerMessageData message = new()
            {
                name = columns[1].Trim(),
                text = columns[2].Trim(),
            };
            message.text = message.text.Replace("\\", "\n").Replace("|", ",");
            if (!string.IsNullOrEmpty(columns[3]))
            {
                message.replyOptions = columns[3].Trim().Split(']');
                if(string.IsNullOrEmpty(message.text)) message.isFromMe = true;
            }
            if(!string.IsNullOrEmpty(columns[4])) message.photoName = columns[4].Trim();

            messageBox.Add(message);
        }
        return messageBox;
    }
}
