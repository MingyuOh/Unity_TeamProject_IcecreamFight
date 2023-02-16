using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleMgr : MonoSingleton<TitleMgr>
{
    //public

    //private
    private CanvasGroup Panel_Frist;
    private CanvasGroup Panel_Second;

    ////////////////////////디버깅용////////////////////////
    private CanvasGroup Panel_Error;
    private WaitForSeconds ws;
    private bool isWait = true;
    ////////////////////////////////////////////////////////

    void Start () {

    ////////////////////////디버깅용////////////////////////
    ws = new WaitForSeconds(1.0f);
    ////////////////////////////////////////////////////////

    //처음 FadeIn
    //ImageFadeIn.Instance.StartFadeAnim();

        //캔버스그룹 컴포넌트
        Panel_Frist = transform.GetChild(0).GetChild(1).GetComponent<CanvasGroup>();
        Panel_Second = transform.GetChild(0).GetChild(2).GetComponent<CanvasGroup>();
        Panel_Error = transform.GetChild(0).GetChild(3).GetComponent<CanvasGroup>();

        ShowFirstPanel();

        ShowGetItemPanel(false, Panel_Error);

    }
	
	// Update is called once per frame
	void Update () {
        if (Panel_Second.blocksRaycasts == true)
        {
            //if (Input.GetMouseButton(0))
            //{
            //    FadeOut
            //    ImageFadeOut.Instance.StartFadeAnim();
            //}
            //if (ImageFadeOut.Instance.isPlayingEnd)
            //    Title.Instance.OnMain();
        }
    }

    public void ShowFirstPanel()
    {
        //첫번째 로그인패널 Show
        ShowGetItemPanel(true, Panel_Frist);
        //두번째 화면깜빡임패널 Not Show
        ShowGetItemPanel(false, Panel_Second);
    }

    public void ShowSecondPanel()
    {
        //첫번째 로그인패널 Show
        ShowGetItemPanel(true, Panel_Second);
        //두번째 화면깜빡임패널 Not Show
        ShowGetItemPanel(false, Panel_Frist);
    }

    ////////////////////////디버깅용////////////////////////
    public void ShowErrorPanel()
    {
        StartCoroutine(ShowError());
    }

    public IEnumerator ShowError()
    {
        if (isWait == false)
        {
            isWait = true;
            ShowGetItemPanel(false, Panel_Error);
            yield break;
        }
        ShowGetItemPanel(true, Panel_Error);
        isWait = false;
        yield return ws;
    }
    ////////////////////////////////////////////////////////

    public void ShowGetItemPanel(bool isOpened, CanvasGroup PNL)
    {
        PNL.alpha = (isOpened) ? 1.0f : 0.0f;
        PNL.interactable = isOpened;
        PNL.blocksRaycasts = isOpened;
    }
}
