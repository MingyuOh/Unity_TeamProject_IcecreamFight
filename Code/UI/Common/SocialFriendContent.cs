using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Communication;

public class SocialFriendContent : MonoBehaviour
{
    //슬롯의 루트
    public Transform slotRoot;

    [SerializeField]
    private Text numofFriend;

    //슬롯 리스트
    private List<GameObject> slots;


    //친구목록 DB에 접근하기 위한 user인스턴스
    FriendSystemManager friendsystemManager;

    public void Awake()
    {
        friendsystemManager = FriendSystemManager.Instance;

        slots = new List<GameObject>();   //리스트 생성
    }

    public void OnDisable()
    {
        DeleteSlotAll();
    }

    //친구 목록 갱신 코루틴
    public void RenewalFriendList()
    {
        DeleteSlotAll();
        //친구 수 세팅
        numofFriend.text = friendsystemManager.friendPlayerInfoList.Count.ToString();

        //친구 카운트만큼 슬롯 추가
        for (int FCnt = 0; FCnt < friendsystemManager.friendPlayerInfoList.Count; FCnt++)
        {
            PlayerInfo playerinfo = friendsystemManager.friendPlayerInfoList[FCnt];
            FriendItem friendItem = friendsystemManager.friendItemDictionary[playerinfo.userId];
            AddFriendContent(FCnt+1, playerinfo.nickName, playerinfo.tier, friendItem.userStatus);
        }

    }

    //목록에 친구정보를 받아서 새로 추가하는 함수 (추후 인자 추가)
    public void AddFriendContent(int order, string nickname, string tier, int userstatus)
    {
        Vector3 originScale = slotRoot.localScale;

        GameObject prefab = Resources.Load("Prefab/UI/SocialFriendContent") as GameObject;
        GameObject SFS = MonoBehaviour.Instantiate(prefab) as GameObject;

        SFS.name = "socailfriendslot";
        SFS.transform.SetParent(slotRoot);
        SFS.transform.localScale = originScale;
        SFS.GetComponent<SocialFriendSlot>().SetSlot(order, nickname, tier, userstatus);

        slots.Add(SFS);
    }

    //public void DeleteSocialfriendSlot(string userid)
    //{
    //    for (int iCnt = 0; iCnt < slots.Count; iCnt++)
    //    {
    //        if (slots[iCnt].GetComponent<SocialFriendSlot>().GetuserID().CompareTo(userid) == 0)
    //        {
    //            Destroy(slots[iCnt]);
    //            slots.RemoveAt(iCnt);

    //            break;
    //        }
    //    }
    //}

    public void DeleteSlotAll()
    {
        if (slots.Count == 0)
            return;
        for (int iCnt = slots.Count - 1; iCnt >= 0; iCnt--)
        {
            Destroy(slots[iCnt]);
            slots.RemoveAt(iCnt);
        }
    }
}
