using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Communication;

public class TeamRequestConstant : MonoSingleton<TeamRequestConstant>
{
    //슬롯루트
    public Transform slotRoot;

    //슬롯리스트
    private List<GameObject> slots;

    private TeamSystemManager teamsystemManager;
    private FriendSystemManager friendsystemManager;

    [SerializeField]
    private Text numofTReq;
    private int numofReq;

    public void Awake()
    {
        teamsystemManager = TeamSystemManager.Instance;
        friendsystemManager = FriendSystemManager.Instance;

        slots = new List<GameObject>();        //리스트 생성

        numofReq = 0;
    }

    public void OnDisable()
    {
        DeleteSlotAll();
    }

    public void RenewalTeamRequestList()
    {
        DeleteSlotAll();

        numofReq = 0;
        for (int iCnt = 0; iCnt < teamsystemManager.teamMatchingManageList.Count; iCnt++)
        {
            AddTeamRequestContent(teamsystemManager.teamMatchingManageList[iCnt],
                friendsystemManager.friendItemDictionary[teamsystemManager.teamMatchingManageList[iCnt]].nickName);

            numofReq++;
        }

        numofTReq.text = numofReq.ToString();
    }

    public void AddTeamRequestContent(string userid, string nickname)
    {
        Vector3 originScale = slotRoot.localScale;

        GameObject prefab = Resources.Load("Prefab/UI/TeamRequestContent") as GameObject;
        GameObject teamrequestSlot = MonoBehaviour.Instantiate(prefab) as GameObject;

        teamrequestSlot.name = "teamrequestSlot";
        teamrequestSlot.transform.SetParent(slotRoot);
        teamrequestSlot.transform.localScale = originScale;
        teamrequestSlot.GetComponent<TeamRequestSlot>().SetSlot(userid, nickname);

        slots.Add(teamrequestSlot);
    }

    public void DeleteTeamRequestSlot(string userid)
    {
        for (int iCnt = 0; iCnt < slots.Count; iCnt++)
        {
            if (slots[iCnt].GetComponent<TeamRequestSlot>().GetuserID().CompareTo(userid) == 0)
            {
                Destroy(slots[iCnt]);
                slots.RemoveAt(iCnt);
                break;
            }
        }

        numofReq--;
        numofTReq.text = numofReq.ToString();
    }

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
