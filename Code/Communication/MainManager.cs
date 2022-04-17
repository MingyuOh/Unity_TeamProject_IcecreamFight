using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace Communication
{
    public class MainManager : MonoSingleton<MainManager>
    {
        private CommonGUI commonGUI;

        //메인패널의 종속 패널
        private SocialPanel socialPanel;
        private RankPanel rankPanel;
        private ChatPanel chatPanel;

        //소셜버튼 요청이미지
        [SerializeField]
        private Image socialReqImg;
        [SerializeField]
        private Text socialReqText;

        public void Start()
        {
            commonGUI = CommonGUI.Instance;
            socialPanel = SocialPanel.Instance;
            rankPanel = RankPanel.Instance;
            chatPanel = ChatPanel.Instance;

            DontDestroyOnLoad(this.gameObject);
        }

        public void ActivateMainPanel()
        {
            this.gameObject.SetActive(true);
            socialPanel.DeActivateSocialPanel();
            chatPanel.DeActivateChatPanel();
            //메인패널 활성화시 일어나야할 로직
            if (commonGUI != null)
            {
                commonGUI.ShowGold();
                commonGUI.ShowDia();
                commonGUI.ShowMail(); 
                commonGUI.ShowSetting();
                commonGUI.ShowChat();
                commonGUI.HideBackButton();
            }

        }

        public void DeActivateMainPanel()
        {
            socialPanel.DeActivateSocialPanel();
            this.gameObject.SetActive(false);
        }

        public void ActiveSocialReqImg()
        {
            socialReqImg.gameObject.SetActive(true);
        }
        public void SetSicalReqText(int num)
        {
            socialReqText.text = num.ToString();
        }

        ///////////////////////버튼 클릭 이벤트/////////////////////////
        public void OnClickShopButton()
        {
            ShopManager.Instance.ActivateShopPanel();
            DeActivateMainPanel();
        }

        public void OnClickInvenButton()
        {
            InvenManager.Instance.ActivateInvenPanel();
            DeActivateMainPanel();
        }

        public void OnClickSocialButton()
        {
            socialPanel.ActivateSocialPanel();
            RequestImg.Instance.DeActivateRequestImg();

        }

        public void OnClickRankButton()
        {
            rankPanel.ActiveRankPanel();
        }

        public void OnClickChatButton()
        {
            chatPanel.ActivateChatPanel();
        }
    }
}
