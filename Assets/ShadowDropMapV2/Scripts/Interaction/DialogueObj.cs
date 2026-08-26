using UnityEngine;
using System.Collections.Generic;

public class DialogueObj : MonoBehaviour
{
    [SerializeField] private TextAsset dialogue;
    [SerializeField] private int index; 

    void Start()
    {
        if (TryGetComponent<InteractionPoint2D>(out var interactionPoint))
        {
            interactionPoint.onInteract.AddListener(() => Trigger());
        }
    }
    public void Trigger()
    {
        DialogueManager.instance.InitDialogue(DataManager.instance.ParseDialogueData(dialogue.text, index));
        index++;
    }
}
