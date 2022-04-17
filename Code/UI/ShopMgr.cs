using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class ShopMgr : MonoSingleton<ShopMgr> {

    //상점 패널
    [SerializeField]
    private RectTransform ShopGoldPanel;
    [SerializeField]
    private RectTransform ShopDiaPanel;
    [SerializeField]
    private RectTransform ShopCostumePanel;

    public void ActivateShopPanel()
    {
        //상점패널 활성화시 일어나야할 로직
        CommonGUI.Instance.ShowBackButton();
        CommonGUI.Instance.ShowGold();
        CommonGUI.Instance.ShowDia();

        //뒤로가기 버튼 이벤트 추가
        BackButtonEvent.InformOnClickBackButton += InShopBackButtonEvent;
    }

    private void InShopBackButtonEvent()
    {
        BackButtonEvent.InformOnClickBackButton -= InShopBackButtonEvent;
        //Shop패널 비활성화
        this.gameObject.SetActive(false);
    }

    ////private
    //private int Gacha_Range;            //가챠를 진행할 범위
    //private CanvasGroup GetItem;
    public void OnClickGoldBtn()
    {
        ShopGoldPanel.gameObject.SetActive(true);

        ShopDiaPanel.gameObject.SetActive(false);
        ShopCostumePanel.gameObject.SetActive(false);
    }
    public void OnClickDiaBtn()
    {
        ShopDiaPanel.gameObject.SetActive(true);

        ShopGoldPanel.gameObject.SetActive(false);
        ShopCostumePanel.gameObject.SetActive(false);
    }
    public void OnClickCostumeBtn()
    {
        ShopCostumePanel.gameObject.SetActive(true);

        ShopDiaPanel.gameObject.SetActive(false);
        ShopGoldPanel.gameObject.SetActive(false);
    }
    //public void OnClickGacha_Skin()
    //{
    //    //데이터에서 가차 범위를 설정
    //    int Gacha_Range = GameInfoData.Instance.Face.Count;

    //    //랜덤함수
    //    int RandomNumber = Random.Range(0, Gacha_Range - 1);

    //    //해당 스킨의 스트링
    //    string RandomSkin = GameInfoData.Instance.Face[RandomNumber];

    //    //인벤토리 데이터에 추가
    //    transform.GetChild(1).GetComponentInChildren<Text>().text += RandomSkin;
    //    ItemList.Instance.AddItem_Skin(RandomSkin);
    //    ShowGetItemPanel(true);
    //}

    //public void OnClickGetItPanel_X_Button()
    //{
    //    ShowGetItemPanel(false);
    //}

    //public void ShowGetItemPanel(bool isOpened)
    //{
    //    GetItem.alpha = (isOpened) ? 1.0f : 0.0f;
    //    GetItem.interactable = isOpened;
    //    GetItem.blocksRaycasts = isOpened;
    //}
}

