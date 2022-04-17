using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

/// <summary>
/// 모든 씬에 걸쳐 존재하는 UI의 관리 클래스. '...Loading', '네트워크 오류' 표시 //
/// </summary>

public class PermanentUI : MonoBehaviour
{
    //캔버스
    //public CanvasGroup loadingCanvas;
    //public CanvasGroup FadeCanvas;
    //public CanvasGroup DialogueCanvas;

    //패널
    public RectTransform FadeInPanel;
    public RectTransform FadeOutPanel;
    public RectTransform InputNickNamePanel;
    public RectTransform EndPanel;
    public RectTransform ConfirmPanel;
    public RectTransform LoadingPanel;
    
    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        LoadingPanel.gameObject.SetActive(false);
        FadeInPanel.gameObject.SetActive(false);
        FadeOutPanel.gameObject.SetActive(false);
        InputNickNamePanel.gameObject.SetActive(false);
        EndPanel.gameObject.SetActive(false);
        ConfirmPanel.gameObject.SetActive(false);
    }

    public void ShowLoading(string str)
    {
        LoadingPanel.gameObject.SetActive(true);
        //메세지 세팅
        Loading.Instance.LoadingTextSetting(str);
    }

    public void HideLoading()
    {
        LoadingPanel.gameObject.SetActive(false);
    }

    public void ShowFadeIn()
    {
        FadeInPanel.gameObject.SetActive(true);
        //imagefadeIn.StartFadeAnim();
    }

    public void HideFadeIn()
    {
        FadeInPanel.gameObject.SetActive(false);
    }

    public void ShowFadeOut()
    {
        FadeOutPanel.gameObject.SetActive(true);
        //imagefadeout.StartFadeAnim();
    }

    public void HideFadeOut()
    {
        FadeOutPanel.gameObject.SetActive(false);
    }

    public void ShowNickNamePanel()
    {
        InputNickNamePanel.gameObject.SetActive(true);
    }

    public void HideNickNamePanel()
    {
        InputNickNamePanel.gameObject.SetActive(false);
    }

    public void ShowEndPanel()
    {
        EndPanel.gameObject.SetActive(true);
    }

    public void HideEndPanel()
    {
        EndPanel.gameObject.SetActive(false);
    }

    public void ShowConfirmPanel()
    {
        ConfirmPanel.gameObject.SetActive(true);
    }

    public void HideConfirmPanel()
    {
        ConfirmPanel.gameObject.SetActive(false);
    }
}