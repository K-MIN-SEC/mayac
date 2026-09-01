using UnityEngine;
using System.Collections.Generic;

public class TriggerObj : MonoBehaviour
{
    void Start()
    {
        if (TryGetComponent<InteractionPoint2D>(out var interactionPoint))
        {
            interactionPoint.onInteract.AddListener(() => Trigger());
        }
    }
    
    public void Trigger()
    {
        if (TryGetComponent(out StoryTarget storyTarget))
            {
                StoryManager.instance.TryPlayStory(storyTarget.targetId);
                gameObject.SetActive(false);
            }
    }
}
