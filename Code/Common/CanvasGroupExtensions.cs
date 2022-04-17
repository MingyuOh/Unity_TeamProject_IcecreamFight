using UnityEngine;

/// <summary>
/// Canvas Groupに表示、非表示の便利メソッドを付加する//
/// </summary>

public static class CanvasGroupExtensions
{
    public static void Show(this CanvasGroup canvasGroup)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
    }

    public static void Hide(this CanvasGroup canvasGroup)
    {
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
    }
}