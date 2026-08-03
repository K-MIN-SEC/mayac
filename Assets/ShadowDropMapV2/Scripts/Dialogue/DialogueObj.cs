using UnityEngine;

public class DialogueObj : MonoBehaviour
{
    [SerializeField] private TextAsset dialogue;
    [SerializeField] private QuarterViewWalkableNavigator2D navigator;
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
        navigator.dialogueBox = DataManager.instance.ParseDialogueData(dialogue.text, index);
        index++;
    }
}
