using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Slot : MonoBehaviour {

    //public
    //public ItemProperty item;

    public Image Itemimage;     //아이템 이미지
    public Image WearingImg;    //아이템 장착이미지

    public void Awake()
    {
        //초기에 버튼컴포넌트 비활성화
        this.GetComponent<UnityEngine.UI.Button>().interactable = false;
        WearingImg.gameObject.SetActive(false);
    }

    ///////////////////////////아이템 추가함수//////////////////////////////
    public void SetItem()
    {
        this.GetComponent<UnityEngine.UI.Button>().interactable = true;

        //테스트용
        WearingImg.gameObject.SetActive(true);
    }
    ////public Stack<Item> slot;              //슬롯을 스택으로
    //public Text     text;                   //아이템의 개수를 표현해줄 텍스트.
    //public Sprite   DefaultImg;             //슬롯이 비어있을때 표시해줄 이미지(투명)
    //public Sprite   EegeI  mgae;              //아이템을 장착했을시 표시해줄 테두리이미지
    //public bool     isEmpty;                //현재 슬롯이 비어있는가?

    ////private
    //private Image   ItemImage;              //슬롯에 표시할 아이템의 이미지

    //void Start()
    //{
    //    ItemImage = transform.GetChild(0).GetComponent<Image>();
    //    isEmpty = true;
    //}

    //public void AddItem(Sprite AddImage)
    //{
    //    ItemImage.sprite = AddImage;
    //    isEmpty = false;
    //}
}
