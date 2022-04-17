using NCMB;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
namespace Communication
{
	[RequireComponent(typeof(DataStoreCoroutine))]
	public class ShopManager : MonoSingleton<ShopManager>
	{
        //상점패널의 종속패널
        [SerializeField]
        private RectTransform ShopGoldPanel;
        [SerializeField]
        private RectTransform ShopDiaPanel;
        [SerializeField]
        private RectTransform ShopCostumePanel;

        [SerializeField]
        private Scrollbar GoldPanelBar;
        [SerializeField]
        private Scrollbar DiaPanelBar;
        [SerializeField]
        private Scrollbar CostumePanelBar;

        private CommonGUI commonGUI;
        private DataStoreCoroutine dataStoreCoroutine;
		private ShopInfo currentShopInfo;

		public bool IsLoaded
		{
			get
			{
				return currentShopInfo != null;
			}
		}

		protected override void Awake()
		{
            base.Awake();

            commonGUI = CommonGUI.Instance;
            dataStoreCoroutine = GetComponent<DataStoreCoroutine>();
			currentShopInfo = new ShopInfo();

			DontDestroyOnLoad(this.gameObject);
		}


        public IEnumerator FetchShopSetting()
		{
			NCMBQuery<NCMBObject> query
				= new NCMBQuery<NCMBObject>(DataStoreClass.SHOPSETTING);

			query.Limit = 1;

			IEnumerator coroutine
				= dataStoreCoroutine.FindAsyncCoroutine(query, Main.Instance.ForceToTitle);

			yield return StartCoroutine(coroutine);

			List<NCMBObject> ncmbObjectList = (List<NCMBObject>)coroutine.Current;

			if (!ncmbObjectList.Any())
			{
				// 데이터 스토어에 없는 경우는 ShopInfo 클래스의 초기값으로 작동
				currentShopInfo = new ShopInfo();
				yield break;
			}
			else
			{
				currentShopInfo = new ShopInfo(ncmbObjectList.First());
			}
		}

			public int GetGeneralGachaPrice()
		{
			return currentShopInfo.generalGachaPrice;
		}

		public int GetSpecialGachaPrice()
		{
			return currentShopInfo.specialGachaPrice;
		}

        public void ActivateShopPanel()
        {
            this.gameObject.SetActive(true);
            //상점패널 활성화시 일어나야할 로직
            if (commonGUI != null)
            {
                commonGUI.ShowBackButton();
                commonGUI.ShowGold();
                commonGUI.ShowDia();
                commonGUI.HideMail();
                commonGUI.HideChat();
                commonGUI.HideSetting();
                //뒤로가기 버튼 이벤트 추가
                BackButtonEvent.InformOnClickBackButton += InShopBackButtonEvent;
            }
            //초기상태
            OnClickGoldBtn();
        }

        public void DeActivateShopPanel()
        {
            this.gameObject.SetActive(false);
        }

        private void InShopBackButtonEvent()
        {
            BackButtonEvent.InformOnClickBackButton -= InShopBackButtonEvent;
            //Shop패널 비활성화
            MainManager.Instance.ActivateMainPanel();
            DeActivateShopPanel();
        }

        ///////////////////////버튼 클릭 이벤트/////////////////////////
        public void OnClickGoldBtn()
        {
            ShopGoldPanel.gameObject.SetActive(true);
            GoldPanelBar.value = 0.0f;
            ShopDiaPanel.gameObject.SetActive(false);
            ShopCostumePanel.gameObject.SetActive(false);
        }
        public void OnClickDiaBtn()
        {
            ShopDiaPanel.gameObject.SetActive(true);
            DiaPanelBar.value = 0.0f;
            ShopGoldPanel.gameObject.SetActive(false);
            ShopCostumePanel.gameObject.SetActive(false);
        }
        public void OnClickCostumeBtn()
        {
            ShopCostumePanel.gameObject.SetActive(true);
            CostumePanelBar.value = 0.0f;
            ShopDiaPanel.gameObject.SetActive(false);
            ShopGoldPanel.gameObject.SetActive(false);
        }
        ///////////////////////////////////////////////////////////////
    }
}