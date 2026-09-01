using UnityEngine;
using System.Collections.Generic;

public class TriggerObj : MonoBehaviour
{
    bool isTrigger = false;

    void Start()
    {
        if (TryGetComponent<InteractionPoint2D>(out var interactionPoint))
        {
            interactionPoint.onInteract.AddListener(() => Trigger());
        }
    }

    public void Trigger()
    {
        if (isTrigger) return;
        isTrigger = true;
        if (TryGetComponent(out StoryTarget storyTarget))
        {
            StoryManager.instance.TryPlayStory(storyTarget.targetId); 
        }
        gameObject.SetActive(false);
    }
}
