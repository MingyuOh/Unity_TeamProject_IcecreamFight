using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Communication;
using UI;

public class TeamRequestSlot : MonoBehaviour
{
    private string userID;

    [SerializeField]
    private Text nickName;

    public void SetSlot(string userid, string nickname)
    {
        this.userID = userid;
        nickName.text = nickname;
    }

    public string GetuserID()
    {
        return userID;
    }

    /// <summary>
    /// /////////////////////////////버튼이벤트///////////////////////////
    /// </summary>
    public void OnClickAcceptBtn()
    {
        TeamSystemManager.Instance.OnAcceptInviteAtTeam(userID);
        //수락완료 UI패널
        //StartCoroutine(NoticePanel.Instance.ActiveNoticePanel(TeamStr.NOTICECOMPELTE_TEAMPARTIN, 2.0f));

        SocialPanel.Instance.SetRequestNumText(SocialPanel.Instance.GetRequestNum() - 1);
        RequestImg.Instance.SetrequestNum(RequestImg.Instance.GetrequestNum() - 1);
        if (SocialPanel.Instance.GetRequestNum() == 0)
        {
            SocialPanel.Instance.DeActivateRequestImg();
        }
        //프리팹 삭제
        TeamRequestConstant.Instance.DeleteTeamRequestSlot(userID);
    }

    public void OnClickRefuseBtn()
    {
        TeamSystemManager.Instance.RefuseInviteAtTeam(userID);

        SocialPanel.Instance.SetRequestNumText(SocialPanel.Instance.GetRequestNum() - 1);
        RequestImg.Instance.SetrequestNum(RequestImg.Instance.GetrequestNum() - 1);
        if (SocialPanel.Instance.GetRequestNum() == 0)
        {
            SocialPanel.Instance.DeActivateRequestImg();
        }
        //프리팹 삭제
        TeamRequestConstant.Instance.DeleteTeamRequestSlot(userID);
    }
}
