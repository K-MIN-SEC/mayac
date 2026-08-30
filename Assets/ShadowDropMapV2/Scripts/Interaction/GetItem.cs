using System;
using DG.Tweening;
using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;

public class GetItem : MonoBehaviour
{
    [SerializeField] RectTransform image;

    void Start()
    {
        if (TryGetComponent<InteractionPoint2D>(out var interactionPoint))
        {
            interactionPoint.onInteract.AddListener(() => Trigger());
        }
    }
    public void Trigger()
    {
        Sequence sequence = DOTween.Sequence();

        image.rotation = Quaternion.identity;

        sequence.Append(image.DOAnchorPos(Vector2.zero, 0.5f));
        sequence.Join(image.DORotate(new Vector3(0, 0, 360), 0.5f, RotateMode.FastBeyond360));

        sequence.AppendInterval(1.0f);

        sequence.Append(image.DOAnchorPos(new Vector2(0, -1000f), 0.5f));
        sequence.Join(image.DORotate(new Vector3(0, 0, 360), 0.5f, RotateMode.FastBeyond360));

        sequence.OnComplete(() =>
        {
            if (TryGetComponent(out StoryTarget storyTarget))
            {
                StoryManager.instance.TryPlayStory(storyTarget.targetId);
                gameObject.SetActive(false);
            }
        });
    }
}
