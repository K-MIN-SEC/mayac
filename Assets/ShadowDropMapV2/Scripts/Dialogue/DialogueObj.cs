using UnityEngine;
using System.Collections.Generic;

public class DialogueObj : MonoBehaviour
{
    [SerializeField] private TextAsset dialogue;
    [SerializeField] private int index; 

    void Start()
    {
        TryGetComponent<InteractionPoint2D>(out var interactionPoint);
        Debug.Log(interactionPoint == null);
        if (interactionPoint != null)
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
