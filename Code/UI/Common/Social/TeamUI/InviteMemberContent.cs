using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Communication;
using ExitGames.Client.Photon.Chat;

public class InviteMemberContent : MonoBehaviour
{
    //슬롯의 루트
    public Transform slotRoot;

    [SerializeField]
    private Text numOfinviteMember;     //초대 가능 멤버 수

    //슬롯 리스트
    private List<GameObject> slots;

    private FriendSystemManager friendsystemManager;

    public void Awake()
    {
        slots = new List<GameObject>();   //리스트 생성

        friendsystemManager = FriendSystemManager.Instance;
    }

    public void OnDisable()
    {
        //UI가 헤제될때 마다
        //모든 슬롯을 삭제한다
        DeleteSlotAll();
    }

    public void RenewalInviteMemberList()
    {
        DeleteSlotAll();

        int order = 1;

        foreach (KeyValuePair<string, FriendItem> fID in friendsystemManager.friendItemDictionary)
        {
            if (fID.Value.relationStatus == FriendItem.FriendRelationStatus.FRIEND)
            {
                //오프라인 , 게임중 상태의 친구를 제외한 목록을 표시
                if (fID.Value.userStatus != ChatUserStatus.Offline
                    && fID.Value.userStatus != ChatUserStatus.Playing)
                {
                    AddInviteMemberContent(order, fID.Value.nickName, fID.Key);
                    order++;
                }
            }
        }

        numOfinviteMember.text = (order - 1).ToString();
    }

    public void AddInviteMemberContent(int order, string nickname, string userid)
    {
        Vector3 originScale = slotRoot.localScale;

        GameObject prefab = Resources.Load("Prefab/UI/InviteMemberContent") as GameObject;
        GameObject invitememberContent = MonoBehaviour.Instantiate(prefab) as GameObject;

        invitememberContent.name = "invitememberslot";
        invitememberContent.transform.SetParent(slotRoot);
        invitememberContent.transform.localScale = originScale;
        invitememberContent.GetComponent<InviteMemberSlot>().SetSlot(order, nickname, userid);

        slots.Add(invitememberContent);
    }
    /// <summary>
    /// ////////////////////모든 슬롯 삭제 함수/////////////////////////
    /// </summary>
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
