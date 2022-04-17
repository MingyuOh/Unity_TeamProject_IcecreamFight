using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Communication;

public class TeamMemberContent : MonoBehaviour
{
    //슬롯의 루트
    public Transform slotRoot;

    [SerializeField]
    private Text numOfteamMember;

    //슬롯 리스트
    private List<GameObject> slots;

    private TeamSystemManager teamsystemManager;
    private FriendSystemManager friendsystemManager;
    private UserAuth userAuth;

    public void Awake()
    {
        slots = new List<GameObject>();   //리스트 생성

        teamsystemManager = TeamSystemManager.Instance;
        friendsystemManager = FriendSystemManager.Instance;
        userAuth = UserAuth.Instance;
    }

    public void OnDisable()
    {
        //UI가 헤제될때 마다
        //모든 슬롯을 삭제한다
        DeleteSlotAll();
    }

    public void RenewalTeamMemberList()
    {
        DeleteSlotAll();
        int order = 1;
        //자신은 방장이 아닌경우로 등록하여
        //초대버튼이 활성화 되지 않도록 한다
        AddTeamMebmerContent(order++, userAuth.GetNickName(), userAuth.GetTier(), null, false);

        //팀멤버의 정보를 담고있는 리스트를 가져온다
        List<PlayerInfo> teamMember = teamsystemManager.FetchTeamMemberPlayerInfos();

        //팀원의 카운트만큼 슬롯 추가
        for (int TCnt = 0; TCnt < teamMember.Count; TCnt++)
        {
            if(teamsystemManager.isTeamMaster)
                AddTeamMebmerContent(order++, teamMember[TCnt].nickName, teamMember[TCnt].tier, teamMember[TCnt].userId, true);     //방장인경우
            else
                AddTeamMebmerContent(order++, teamMember[TCnt].nickName, teamMember[TCnt].tier, teamMember[TCnt].userId, false);    //방장이 아닌경우

        }

        numOfteamMember.text = (teamMember.Count + 1).ToString();
    }

    public void AddTeamMebmerContent(int order, string nickname, string tier, string userid, bool isMaster)
    {
        Vector3 originScale = slotRoot.localScale;

        GameObject prefab = Resources.Load("Prefab/UI/TeamMemberContent") as GameObject;
        GameObject teammemberContent = MonoBehaviour.Instantiate(prefab) as GameObject;

        teammemberContent.name = "teammemberslot";
        teammemberContent.transform.SetParent(slotRoot);
        teammemberContent.transform.localScale = originScale;
        teammemberContent.GetComponent<TeamMemberSlot>().SetSlot(order, nickname, tier, userid, isMaster);

        slots.Add(teammemberContent);
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
