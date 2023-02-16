using NCMB;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Communication;
using UI;
using Facebook.Unity;

public class TitleController : MonoBehaviour
{
    public RectTransform LogoCanvas;
    public RectTransform TtileCanvas;

	private UserAuth userAuth;
	private FacebookAuthManager faceBookAuth;
	private PlayerInfoManager playerInfoManager;
	private FriendSystemManager friendSystemManager;
	private GameSettingManager gameSettingManager;
	private NetworkManagerCustom networkManager;
	private NetworkSocialManager networkSocialManager;
    private Main main;

    //private ImageFadeIn imagefadein;
    //private ImageFadeOut imagefadeout;

    private bool isTest = false;

    //닉네임 입력완료
    private bool isNickNameInputComplete;

    private void Awake()
	{
        Screen.SetResolution(Screen.width, Screen.width * 9 / 16, false);

		LogoCanvas.gameObject.SetActive(true);
        TtileCanvas.gameObject.SetActive(false);

        isNickNameInputComplete = false;

        //Main.Instance.ShowLoadingPanel();

        //imagefadein = ImageFadeIn.Instance;
        //imagefadeout = ImageFadeOut.Instance;
    }

	private void Start()
	{
		userAuth = UserAuth.Instance;
		faceBookAuth = FacebookAuthManager.Instance;
		playerInfoManager = PlayerInfoManager.Instance;
		friendSystemManager = FriendSystemManager.Instance;
		gameSettingManager = GameSettingManager.Instance;
		networkManager = NetworkManagerCustom.GetInstance();
		networkSocialManager = NetworkSocialManager.Instance;

        main = Main.Instance;
		//imagefadein = ImageFadeIn.Instance;
		//imagefadeout = ImageFadeOut.Instance;

		//초기 페이드인 코루틴 시작
		StartCoroutine(ShowLogo());
    }

    private IEnumerator ShowLogo()
    {
        //로고 페이드인 코루틴 시작
        main.ShowFadeinPanel();
        yield return StartCoroutine(ImageFadeIn.Instance.PlayFadeIn());
        
        main.HideFadeinPanel();

        //로고화면을 비추는 코루틴 시작
        yield return StartCoroutine(LogoCanvasShow());

        //로고 페이드아웃 코루틴 시작
        main.ShowFadeOutPanel();
        yield return StartCoroutine(ImageFadeOut.Instance.PlayFadeOut());

        main.HideFadeOutPanel();

        LogoCanvas.gameObject.SetActive(false);
        TtileCanvas.gameObject.SetActive(true);

        //타이틀화면 페이드인 코루틴 시작
        main.ShowFadeinPanel();
        yield return StartCoroutine(ImageFadeIn.Instance.PlayFadeIn());

        main.HideFadeinPanel();

        //게임셋 로드
        yield return StartCoroutine(LoadGameSetting());
    }

    //로고화면을 비추는 코루틴
    private IEnumerator LogoCanvasShow()
    {
        //로고화면 1.5초 지속
        yield return new WaitForSeconds(1.5f);
    }

    //타이틀화면 페이드아웃 코루틴
    private IEnumerator TitleFadeOut()
    {
        yield return StartCoroutine(ImageFadeOut.Instance.PlayFadeOut());

        main.OnMenu();
    }


	// 게임 계속 버튼에서 호출
	public void FacebookLogin()
	{
		if (gameSettingManager.IsLoaded)
		{
			if (CheckGameValid() == false)
				return;

			StartCoroutine(AutoLoginSequence());
		}
		else
		{
			StartCoroutine(LoadGameSetting());
		}
	}

	// 데이터 스토어로부터 게임 설정을 로드
	public IEnumerator LoadGameSetting()
	{
		if(Application.internetReachability == NetworkReachability.NotReachable)
		{
			//Main.Instance.ShowErrorDialogue("연결된 네트워크가 없습니다.");
			yield break;
		}

        main.ShowLoadingPanel(LoadingStr.BASCICLOADING);

		yield return gameSettingManager.FetchGameSetting();

		string updateDateStr = gameSettingManager.GetUpdateDateString();

		if (string.IsNullOrEmpty(updateDateStr))
		{
			//settingFileDateText.text = "No Setting Data Found";
		}
		else
		{
			//settingFileDateText.text = updateDateStr;
		}

		// 서비스 중인지 확인
		if (gameSettingManager.IsServiceEnable() == false)
		{
			//Main.Instance.ShowErrorDialogue("서버 점검중입니다.");
		}

        main.HideLoadingPanel();
	}

	// 자동로그인 결과 코루틴
	private IEnumerator AutoLoginSequence()
	{
        main.ShowLoadingPanel(LoadingStr.BASCICLOADING);
        ///////////////////////////////////////////////////////////////////////
        // SNS와 연결되어있는지 체크(facebook과 twitter만 지원)
        // PC에서는 이 조건문 만족안함 빌드에서 테스트
        ///////////////////////////////////////////////////////////////////////
        // if(userAuth.CheckConnectwithSNS(SNSClass.FACEBOOK))
		if(isTest == true)
        {
			yield return userAuth.LogInCoroutine("AohuVKNlzS", "123123", Main.Instance.ForceToTitle);
		}
		else
		{
			// 플랫폼 연동 버튼 활성화
			// 이게 else 부분의 플랫폼 버튼 코루틴에서 선택되어야 함
			yield return faceBookAuth.FacebookAutoSignUpAndLoginCoroutine();
		}

        // 닉네임 여부 체크(코루틴 콜백으로 대기시켜야한다) - 근데 콜백이 있나..
        // 콜백이 존재하지않으면 bool 변수로 밑에 로딩패널 부분 처리해야할듯
        // 아래 함수로는 처리할 수 없음
        // 닉네임 패널 코루틴 대기시키면 될 듯
        main.HideLoadingPanel();
        yield return NickNameCheckCoroutine(Main.Instance.ForceToTitle);

        main.ShowLoadingPanel(LoadingStr.BASCICLOADING);
        yield return userAuth.SaveAsyncCurrentUserCoroutine(Main.Instance.ForceToTitle);

		// =========================== 데이터 로드 =========================== // 
		// 포톤 서버 연결
		networkManager.AwakePhotonConnect();

		// 포톤 채팅 연결
		networkSocialManager.Connect();

		// 유저 정보 Load(추후 수정)
		userAuth.LoadUserInfo();

		// PlayerInfo 데이터 로드
		yield return playerInfoManager.FetchOwnDataCoroutine(Main.Instance.ForceToTitle);

		// FriendInfo 데이터 로드
		yield return friendSystemManager.FetchOwnFriendInfoDataCoroutine(Main.Instance.ForceToTitle);

		// Friend System 초기화
		yield return friendSystemManager.InitFriendSystemDataCoroutine();

		main.HideLoadingPanel();

		// 이전 게임이 존재하는지 체크
		IEnumerator coroutine = networkManager.CheckIsGamePlayingBeforeAppTerminatedCoroutine();
		yield return StartCoroutine(coroutine);

		if (coroutine.Current != null)
		{
			// 이전 게임이 존재한다면 해당 게임에 참가하므로
			// 현재 함수 종료
			bool isRejoinRoom = (bool)coroutine.Current;
			if (isRejoinRoom == true)
				yield break;
		}

		//타이틀씬에서의 모든작업을 끝내고
		//메뉴씬으로 이동하기전 페이드아웃이 일어난다
		main.ShowFadeOutPanel();

        StartCoroutine(TitleFadeOut());
	}

    //닉네임 입력완료
    public void IsNickNameInputComplete()
    {
        isNickNameInputComplete = true;
    }

    // 닉네임 체크 코루틴
    private IEnumerator NickNameCheckCoroutine(UnityAction<NCMBException> errorCallback)
	{
        InputNickName.InformComplete += IsNickNameInputComplete;

        if (NCMBUser.CurrentUser != null)
		{
			string nickName = NCMBUser.CurrentUser[UserKey.NICKNAME] as string;
			if (nickName == string.Empty)
			{
                // 닉네임 생성 패널 띄워야함
                // yield return 패널코루틴;
                main.ShowInputNickNamePanel();

                while (isNickNameInputComplete == false)
                {
                    yield return null;
                }

				// 닉네임 DB에 저장
				nickName = NCMBUser.CurrentUser[UserKey.NICKNAME] as string;

                InputNickName.InformComplete -= IsNickNameInputComplete;
                main.HideInputNickNamePanel();

				// 닉네임 공개데이터에 추가(리턴 받아서 처리해야함)
				yield return playerInfoManager.SaveNicknameToOwnData(nickName, errorCallback);
				yield return friendSystemManager.SaveNicknameToOwnData(nickName, errorCallback);
			}
		}

	}

	// 게임이 유효한지 조사하고 NG인 경우는 대화상자 표시
	private bool CheckGameValid()
	{
		if(Application.internetReachability == NetworkReachability.NotReachable)
		{
			Main.Instance.ForceToTitle("연결된 네트워크가 없습니다.");
			return false;
		}

		if(gameSettingManager.IsServiceEnable() == false)
		{
            main.ForceToTitle("서버 점검중입니다.");
			return false;
		}

		return true;
	}

    public void OnClickFaceBookLoginButton()
    {
        StartCoroutine(AutoLoginSequence());
    }

    public void OnClickNaverLoginButton()
    {

    }

    public void OnClickGoogleLoginButton()
    {

    }

	public void OnClickTestUserLoginButton()
	{
		isTest = true;
		StartCoroutine(AutoLoginSequence());
	}
}
