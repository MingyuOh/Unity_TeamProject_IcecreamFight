using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Communication
{
    public class InvenManager : MonoSingleton<InvenManager>
    {
        private CommonGUI commonGUI;

        //창고패널의 종속패널
        [SerializeField]
        private RectTransform InvenFacePanel;
        [SerializeField]
        private RectTransform InvenAccPanel;
        [SerializeField]
        private RectTransform InvenSkinPanel;

        private void Awake()
        {
            commonGUI = CommonGUI.Instance;
            DontDestroyOnLoad(this.gameObject);
        }

        public void ActivateInvenPanel()
        {
            this.gameObject.SetActive(true);
            //창고패널 활성화시 일어나야할 로직
            if (commonGUI != null)
            {
                commonGUI.ShowGold();
                commonGUI.ShowDia();
                commonGUI.ShowMail();
                commonGUI.ShowSetting();
                commonGUI.ShowChat();
                commonGUI.ShowBackButton();

                //뒤로가기 버튼 이벤트 추가
                BackButtonEvent.InformOnClickBackButton += InInvenBackButtonEvent;
            }

            //초기상태
            InvenFacePanel.gameObject.SetActive(true);
            InvenAccPanel.gameObject.SetActive(false);
            InvenSkinPanel.gameObject.SetActive(false);
        }

        public void DeActivateInvenPanel()
        {
            this.gameObject.SetActive(false);
        }

        private void InInvenBackButtonEvent()
        {
            BackButtonEvent.InformOnClickBackButton -= InInvenBackButtonEvent;
            //Inven패널 비활성화
            MainManager.Instance.ActivateMainPanel();
            DeActivateInvenPanel();
        }

        ///////////////////////버튼 클릭 이벤트/////////////////////////
        public void onClickFaceBtn()
        {
            InvenFacePanel.gameObject.SetActive(true);
            InvenAccPanel.gameObject.SetActive(false);
            InvenSkinPanel.gameObject.SetActive(false);
        }

        public void onClickAccBtn()
        {
            InvenAccPanel.gameObject.SetActive(true);
            InvenFacePanel.gameObject.SetActive(false);
            InvenSkinPanel.gameObject.SetActive(false);
        }

        public void onClickSkinBtn()
        {
            InvenFacePanel.gameObject.SetActive(false);
            InvenAccPanel.gameObject.SetActive(false);
            InvenSkinPanel.gameObject.SetActive(true);
        }
    }
}
