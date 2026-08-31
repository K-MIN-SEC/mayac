using UnityEngine;
using System.Collections.Generic;

public class ExitDay : MonoBehaviour
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
        StoryManager.instance.SetupNextPhase();
        gameObject.SetActive(false);
    }
}
