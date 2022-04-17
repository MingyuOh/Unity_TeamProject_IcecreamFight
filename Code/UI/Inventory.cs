using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour {

    //공개
    public List<GameObject> AllSlot;        //모든 슬롯을 관리해줄 리스트
    public RectTransform InvenRect;         //인벤토리의 Rect
    public GameObject OriginSlot;           //오리지널 슬롯

    public float slotSize;                  //슬롯의 사이즈
    public float slotGap;                   //슬롯간 간격
    public float slotCountX;                //슬롯의 가로 개수
    public float slotCountY;                //슬롯의 세로 개수

    //비공개
    private float InvenWidth;               //인벤토리 가로길이
    private float InvenHeight;              //인벤토리 세로길이
    private float EmptySlot;                //빈 슬롯의 개수

    void Awake()
    {
        //인벤토리 이미지 가로, 세로 사이즈 셋팅
        InvenWidth = (slotCountX * slotSize) + (slotCountX * slotGap) + slotGap;
        InvenHeight = (slotCountY * slotSize) + (slotCountY * slotGap) + slotGap;

        //셋팅된 사이즈로 크기 설정
        InvenRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, InvenWidth);
        InvenRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, InvenHeight);

        //슬롯 생성
        for (int y = 0; y < slotCountY; y++)
        {
            for (int x = 0; x < slotCountX; x++)
            {
                //슬롯을 복사
                GameObject slot = Instantiate(OriginSlot) as GameObject;

                //슬롯의 RectTransform
                RectTransform slotRect = slot.GetComponent<RectTransform>();

                //슬롯의 자식인 투명이미지의 RectTransform을 가져온다.
                RectTransform item = slot.transform.GetChild(0).GetComponent<RectTransform>();

                slot.name = "slot_" + y + "_" + x;  //슬롯 이름
                slot.transform.parent = transform;  //부모 설정 (Inventory)

                //슬롯이 생성될 위치 설정
                slotRect.localPosition = new Vector3((slotSize * x) + (slotGap * (x + 1)),
                                                        -((slotSize * y) + (slotGap * (y + 1))), 0);

                //슬롯의 사이즈 설정
                slotRect.localScale = Vector3.one;
                slotRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, slotSize);
                slotRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, slotSize);

                //슬롯의 자식인 투명이미지의 사이즈 설정
                item.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, slotSize - slotSize * 0.3f);
                item.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, slotSize - slotSize * 0.3f);

                AllSlot.Add(slot);
            }
        }

        //빈 슬롯 = 슬롯의 숫자
        EmptySlot = AllSlot.Count;
    }
}
