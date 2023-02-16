using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Communication;
using System;
using UnityEngine.UI;

namespace Menu
{
	public class MenuContorller : MonoBehaviour
	{
        //메뉴씬 패널
        [SerializeField]
        private RectTransform MainPanel;        //메뉴패널
        private RectTransform ShopPanel;        //상점패널
        private RectTransform InventoryPanel;   //창고패널
        
        //User정보로부터 받아오는것
        [SerializeField]
        private Text Coin;
        [SerializeField]
        private Text Dia;
        [SerializeField]
        private Text UserNickName;
        [SerializeField]
        private Text UserScore;
        

        private UserAuth userAuth;
		private PlayerInfoManager playerInfoManger;

        //매니저
		private ShopManager shopManager;
        private MainManager mainManager;
        private InvenManager invenManager;
        //
		private IEnumerator currentCoroutine;

		private void Awake()
		{
			userAuth = UserAuth.Instance;
			playerInfoManger = PlayerInfoManager.Instance;
			shopManager = ShopManager.Instance;
            mainManager = MainManager.Instance;
            invenManager = InvenManager.Instance;
        }

		private void Start()
		{
            //메뉴씬으로 전환됐을때 페이드아웃 숨김
            Main.Instance.HideFadeOutPanel();

            StartCoroutine(OnMenuSequence());

            mainManager.ActivateMainPanel();
            shopManager.DeActivateShopPanel();
            invenManager.DeActivateInvenPanel();

            //페이드인 시작
            StartCoroutine(MenuFadeIn());
		}

        //메뉴씬 페이드인코루틴
        private IEnumerator MenuFadeIn()
        {
            Main.Instance.ShowFadeinPanel();
            yield return ImageFadeIn.Instance.PlayFadeIn();

            Main.Instance.HideFadeinPanel();
        }

		// 메뉴 화면 시작 시퀀스
		private IEnumerator OnMenuSequence()
		{
            //Main.Instance.ShowLoadingPanel();

            // 회원 관리로부터 닉네임 가져와 UI에 뿌림
            UserNickName.text = userAuth.GetNickName();

            // 회원 관리로부터 코인 개수 가져옴
            Coin.text = userAuth.GetCoinCount().ToString();

            // 회원 관리로부터 다이아 개수 가져옴
            Dia.text = userAuth.GetDiamondCount().ToString();

			// 회원관리로부터 티어 가져옴
			userAuth.GetTier();

            // 회원관리로부터 점수 가져옴
            UserScore.text = userAuth.GetPointCount().ToString();

			// 회원의 공개 데이터 가져옴
			//yield return playerInfoManger.FetchOwnDataCoroutine(Main.Instance.ForceToTitle);

			// 데일리 보너스 체크
			IEnumerator coroutine = userAuth.FetchLoginDateSpan(Main.Instance.ForceToTitle);

			yield return StartCoroutine(coroutine);

			int day = (int)coroutine.Current;

			if(day == 0)
			{
				// 데일리 보너스 UI Hide
			}
			else
			{
				// 데일리 보너스 UI Show
			}

			//Main.Instance.HideLoadingPanel();
		}

		// 데일리 보너스 UI 터치 시 동작
		public void OnButtonGetDailyBonus()
		{
			StartCoroutine(GetDailyBonusCoroutine());
		}

		public IEnumerator GetDailyBonusCoroutine()
		{
			// 그날의 데일리 보상 부여
			// 코인 / 다이아 / 일반가챠 / 스페셜가챠 등등..

			// 획득 후 UI 갱신

			// 데이터베이스에 저장
			yield return userAuth.SaveAsyncCurrentUserCoroutine(Main.Instance.ForceToTitle);

			// 데일리 보너스 UI Hide

			// 메인 UI Show
		}

		////////////////////////////////////////////////////////////////////////
		// 이 스크립트에서 각종 버튼에 대한 이벤트 함수 등록해야한다.
		////////////////////////////////////////////////////////////////////////

		// 싱글플레이 버튼 함수
		public void OnClickSingleplay()
		{
			// 매치메이킹 시작
			NetworkManagerCustom.StartMatch(NetworkMode.Offline);

			StartCoroutine(HandleTimeout());
		}

		// 멀티플레이 버튼 함수
		public void OnClickMultiplay()
        {
            // 매치메이킹 시작
            NetworkManagerCustom.StartMatch(NetworkMode.Online);

			// 플레이
			NetworkManagerCustom.GetInstance().Play();

			StartCoroutine(HandleTimeout());
        }

        // 경기 참여 전 10초 기다리는 코루틴
        IEnumerator HandleTimeout()
        {
            yield return new WaitForSeconds(10.0f);

			// 타임 아웃이 지나면 게임에 참여하지 않음
			PhotonNetwork.Disconnect();

            // 화면에 연결되지 않은 이유를 표시
            OnConnectionError();
        }

		// 이 함수를 이용하여 연결되지 않은 이유를 UI로 표시 
		void OnConnectionError()
        {
            StopCoroutine(HandleTimeout());
            //loadingWindow.SetActive(false);
            //connectionErrorWindow.SetActive(true);
        }
    }
}