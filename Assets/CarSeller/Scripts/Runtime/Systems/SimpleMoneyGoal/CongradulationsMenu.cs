using UnityEngine;
using UnityEngine.UI;

public class CongradulationsMenu : GlobalSingletonBehaviour<CongradulationsMenu>
{
    protected override CongradulationsMenu GlobalInstance { get => G.CongradulationsMenu; set => G.CongradulationsMenu = value; }

    public Canvas Canvas { get; private set; }
    public CanvasGroup CanvasGroup { get; private set; }
    //[SerializeField] TMPro.TextMeshProUGUI goalAmountText;
    Button closeButton;

    protected override void Awake()
    {
        base.Awake();
        if (!IsActiveSingleton) return;

        Canvas = GetComponent<Canvas>();
        CanvasGroup = GetComponent<CanvasGroup>();
        closeButton = GetComponentInChildren<Button>();
        closeButton.onClick.AddListener(Hide);
    }

    private void Start()
    {
        Hide();
    }
    public void Show()
    {
        G.BlockUIManager.Block(Canvas, Hide);
        CanvasGroup.alpha = 1;
        CanvasGroup.blocksRaycasts = true;
    }
    public void Hide()
    {
        G.BlockUIManager.Unblock(Canvas);
        CanvasGroup.alpha = 0;
        CanvasGroup.blocksRaycasts = false;
    }
}