using NCMB;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Communication;
using UI;

public class Main : MonoSingleton<Main>
{
    public PermanentUI permanentUI;
    public int titleSceneIndex = 0;

    public bool HasBadWord(string message)
    {
        //if(badWordList != null)
        //{
        //    string[] badwords = badWordList.words;
        //    return badwords.Any(word => word.Contains(message));
        //}
        return false;
    }

	protected override void Awake()
	{
		base.Awake();

		permanentUI = (PermanentUI)FindObjectOfType(typeof(PermanentUI));
	}

	private void Start()
    {
		//HideLoadingPanel();

#if UNITY_EDITOR

		if (SceneManager.GetSceneByName("Title").IsValid())
        {
        }
        else if (SceneManager.GetSceneByName("Battle").IsValid())
        {
            ShowLoadingPanel(LoadingStr.BASCICLOADING);
        }
        else if (SceneManager.GetSceneByName("Menu").IsValid())
        {
            ShowLoadingPanel(LoadingStr.BASCICLOADING);
        }
        else
        {
            //OnTitle();
        }
#endif
    }

    public void OnTitle()
    {
        //SoundManager.Instance.ClearAudioListenerPos();
        SceneManager.LoadScene(SceneClass.TITLE_SCENE, LoadSceneMode.Single);
    }

    public void OnBattle()
    {
        ShowLoadingPanel(LoadingStr.BASCICLOADING);
        //SoundManager.Instance.ClearAudioListenerPos();
        SceneManager.LoadScene("Battle", LoadSceneMode.Single);
    }

    public void OnMenu()
    {
        // SoundManager.Instance.ClearAudioListenerPos();
        SceneManager.LoadScene(SceneClass.MENU_SCENE, LoadSceneMode.Single);
    }

    //로딩패널
    public void ShowLoadingPanel(string str)
    {
        permanentUI.ShowLoading(str);
    }

    public void HideLoadingPanel()
    {
        permanentUI.HideLoading();
    }

    //페이드인
    public void ShowFadeinPanel()
    {
        permanentUI.ShowFadeIn();
    }

    public void HideFadeinPanel()
    {
        permanentUI.HideFadeIn();
    }

    //페이드아웃
    public void ShowFadeOutPanel()
    {
        permanentUI.ShowFadeOut();
    }

    public void HideFadeOutPanel()
    {
        permanentUI.HideFadeOut();
    }

    //닉네임패널
    public void ShowInputNickNamePanel()
    {
        permanentUI.ShowNickNamePanel();
    }

    public void HideInputNickNamePanel()
    {
        permanentUI.HideNickNamePanel();
    }

    //게임종료패널
    public void ShowEndPanel()
    {
        permanentUI.ShowEndPanel();
    }

    public void HideEndPanel()
    {
        permanentUI.HideEndPanel();
    }

    //확정패널
    public void ShowConfirmPanel()
    {
        permanentUI.ShowConfirmPanel();
    }

    public void HideConfirmPanel()
    {
        permanentUI.HideConfirmPanel();
    }

    //public string BuildNumber
    //{
    //    get
    //    {
    //        var buildManifest = CloudBuildManifest.Load();
    //        return buildManifest == null
    //            ? "Not Cloud Build"
    //            : "Build Number " + buildManifest.BuildNumber;
    //    }
    //}


    public void ForceToTitle(NCMBException exeption = null)
    {
        if (exeption != null)
        {
            //permanentUI.ShowOK(GetErrorMessageFromEroorCode(exeption));
        }
        Debug.Log("함수에러");
        //OnTitle();
    }

    public void ForceToTitle(string message)
    {
        Debug.Log("함수에러");
        //permanentUI.ShowOK(message);
        //OnTitle();
    }

    public string GetErrorMessageFromEroorCode(NCMBException exeption)
    {
        if (exeption.ErrorCode == NCMBException.INCORRECT_PASSWORD)
        {
            return "ID와 비밀번호가 일치하지 않습니다.";
        }
        else if (exeption.ErrorCode == NCMBException.DUPPLICATION_ERROR)
        {
            return "해당 ID는 이미 사용중입니다.";
        }
        else if (exeption.ErrorCode == NCMBException.INVALID_FORMAT)
        {
            return "메일 형식이 올바르지 않습니다.";
        }
        else if (exeption.ErrorCode == NCMBException.REQUEST_OVERLOAD)
        {
            //실제로 월 이용 한도를 넘은 상황//
            return "서버 점검중입니다.";
        }
        else if (exeption.ErrorCode == "E503001")
        {
            //실제로는 503//
            return "503 서버 점검중입니다.";
        }
        else
        {
            return "네트워크 오류가 발생했습니다.";
        }
    }

}