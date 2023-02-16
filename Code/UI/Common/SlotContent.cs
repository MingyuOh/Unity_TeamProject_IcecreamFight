using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Communication;

public class SlotContent : MonoBehaviour
{
    //슬롯의 루트
    public Transform slotRoot;
    //슬롯 리스트
    private List<Slot> slots;

    //아이템 DB에 접근하기 위한 user인스턴스
    private UserAuth userAuth;

    private void Awake()
    {
        userAuth = UserAuth.Instance;
    }

    void Start()
    {
        slots = new List<Slot>();

        int slotCnt = slotRoot.childCount;
        //보유아이템 DB에서 목록을 끌어온다
        if (slotRoot.CompareTag("SkinContent"))
        {
            //스킨슬롯창일때

        }
        else if (slotRoot.CompareTag("FaceContent"))
        {
            //페이스슬롯창일때

        }
        else if (slotRoot.gameObject.CompareTag("AccContent"))
        {
            //악세사리슬롯창일때

            //악세사리 리스트
            List<string> HearAccs = userAuth.GetHeadAccessoryList();
            List<string> ChestAccs = userAuth.GetChestAccessoryList();

            int iCnt;
            int ItemCnt;

            for (ItemCnt = 0, iCnt = 0; ItemCnt < HearAccs.Count; ItemCnt++, iCnt++)
            {
                if (iCnt > slotCnt)
                {
                    Debug.Log("아이템갯수 > 슬롯 오류");
                    break;
                }
                var slot = slotRoot.GetChild(iCnt).GetComponent<Slot>();
                //슬롯 활성화 및 아이템 이미지 추가
                slot.SetItem();
                Debug.Log("iCnt번슬롯에 Hear악세사리 아이템 추가");
                slots.Add(slot);
            }

            for (ItemCnt = 0; ItemCnt < ChestAccs.Count; ItemCnt++, iCnt++)
            {
                if (iCnt > slotCnt)
                {
                    Debug.Log("아이템갯수 > 슬롯 오류");
                    break;
                }

                var slot = slotRoot.GetChild(iCnt).GetComponent<Slot>();
                //슬롯 활성화 및 아이템 이미지 추가
                slot.SetItem();
                Debug.Log("iCnt번슬롯에 Chest악세사리 아이템 추가");
                slots.Add(slot);
            }
        }
        else
        {
            Debug.Log("잘못된 태그이름입니다");
        }
    }

    public void OnclickSlot()
    {

    }
}
