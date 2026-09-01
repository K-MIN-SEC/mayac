using UnityEngine;
using DG.Tweening;

public class InGame : StoryManager
{
    [SerializeField] Transform player;
    [SerializeField] Vector3 playerpos;

    [SerializeField] SmartphoneUIManager phone;
    bool i;

    protected override void Start()
    {
        base.Start();
        FadeInOut(false);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (!isNext || isFading) return;

            if (!isDialogue && string.IsNullOrEmpty(triggerId)) isDialogue = Trigger();
            else isDialogue = DialogueManager.instance.TriggerDialogue();
        }
    }

    public override void SetupNextPhase()
    {
        isFading = true;
        fadeImage.color = new Color(0, 0, 0, 0);
        Sequence sequence = DOTween.Sequence();
        sequence.Append(fadeImage.DOFade(1f, 1.5f));
        
        dialogueIndex++;
        dialogueTextIndex = 0;
        indexWeight = 1;
        triggerId = "";
        Event("Init");

        isNext = true;
        isDialogue = false;
        isTransitioning = false;
        sequence.AppendInterval(0.5f).OnComplete(() => { player.position = playerpos; });
        phone.ClosePhone();
        sequence.Append(fadeImage.DOFade(0f, 1.5f)).SetEase(Ease.Linear);
        sequence.OnComplete(() => { isFading = false; });
    }
}