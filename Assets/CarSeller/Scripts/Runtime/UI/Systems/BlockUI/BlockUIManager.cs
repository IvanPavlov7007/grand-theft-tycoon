using UnityEngine;
using System;
using UnityEngine.UI;

// TODO: add stacking of blocking UIs for modular UI Menues, i a Pause Menu can open an Options Menu on top of it
// which also blocks the underlying Pause Menu, but unblocking the Options Menu returns to the Pause Menu blocking state

/// <summary>
///  
/// </summary>
public class BlockUIManager : GlobalSingletonBehaviour<BlockUIManager>
{
    protected override BlockUIManager GlobalInstance { get => G.BlockUIManager; set => G.BlockUIManager = value; }

    CanvasGroup canvasGroup;
    Canvas canvas;
    Button button;

    public bool IsBlocking => focusCanvas != null;

    Canvas focusCanvas;
    int initialTargetSortingOrder;

    protected override void Awake()
    {
        base.Awake();
        if (!IsActiveSingleton) return;

        canvas = GetComponent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();
        button = GetComponentInChildren<Button>();
    }

    public void Block(Canvas focusCanvas, Action onClick)
    {
        if (this.focusCanvas != null)
        {
            Debug.LogWarning($"BlockUIManager: Blocking UI with focus canvas {this.focusCanvas.name}");
            // invoke previous before locking
            onClick?.Invoke();
            // do nothing if still locks
            if (this.focusCanvas != null)
            {
                Debug.LogWarning($"BlockUIManager: Still blocking UI with focus canvas {this.focusCanvas.name}, cannot block with {focusCanvas.name}");
                return;
            }
        }
        if(focusCanvas == null)
            return;
        
        this.focusCanvas = focusCanvas;
        initialTargetSortingOrder = focusCanvas.sortingOrder;
        focusCanvas.sortingOrder = canvas.sortingOrder + 1;
        setBlock(true, onClick);
    }

    public void Unblock(Canvas key)
    {
        if(focusCanvas != key)
        {
            Debug.LogWarning($"BlockUIManager: Cannot unblock UI with focus canvas {focusCanvas?.name} using key canvas {key.name}");
            return;
        }
        if (focusCanvas != null)
        {
            focusCanvas.sortingOrder = initialTargetSortingOrder;
            setBlock(false);
        }
        focusCanvas = null;
    }

    void setBlock(bool blocking, Action action = null)
    {
        if(blocking)
        {
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                if (action != null)
                {
                    button.onClick.AddListener(() => action());
                }
            }
        }
        else
            button.onClick.RemoveAllListeners();
        canvasGroup.alpha = blocking ? 1 : 0;
        canvasGroup.blocksRaycasts = blocking;
    }

}