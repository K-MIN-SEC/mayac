using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement;

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
        DataManager.instance.curNpcDialogueData = npcDialogue[dialogueIndex];
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
        indexWeight = 1;
        triggerId = "";
        DataManager.instance.curNpcDialogueData = npcDialogue[dialogueIndex];

        isNext = true;
        isDialogue = false;
        isTransitioning = false;

        sequence.AppendInterval(0.5f);
        sequence.AppendCallback(() =>
        {
            player.position = playerpos;
            phone.ClosePhone();
            Event("Init");
        });
        
        sequence.Append(fadeImage.DOFade(0f, 1.5f)).SetEase(Ease.Linear);
        sequence.OnComplete(() =>
        {
            dialogueTextIndex = 0;
            isFading = false;
        });
    }

    public override void SetupNextScene()
    {
        isFading = true;
        fadeImage.color = new Color(0, 0, 0, 0);
        Sequence sequence = DOTween.Sequence();
        sequence.Append(fadeImage.DOFade(1f, 1.5f));

        sequence.AppendInterval(0.5f);
        sequence.AppendCallback(() =>
        {
            SceneManager.LoadScene(3);
        });
    }
}