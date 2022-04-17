using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Communication;

public class InviteMemberSlot : MonoBehaviour
{
    [SerializeField]
    private Text order;
    [SerializeField]
    private Text name;

    private string userID;

    /// <summary>
    /// //////////////////팀 초대 기능을 위한 UI///////////////////
    /// </summary>
    [SerializeField]
    private Button inviteBtn;       //초대버튼
    [SerializeField]
    private Image activeImg;        //초대활성화이미지
    [SerializeField]
    private Image deactiveImg;      //초대비활성화이미지

    private TeamSystemManager teamsystemManager;

    public void Awake()
    {
        teamsystemManager = TeamSystemManager.Instance;
    }
    public void SetSlot(int order, string name, string userid)
    {
        this.order.text = order.ToString();
        this.name.text = name;
        this.userID = userid;

        ActiveInviteBtn();
    }

    //초대버튼 활성화 함수
    public void ActiveInviteBtn()
    {
        inviteBtn.interactable = true;
        activeImg.enabled = true;
        deactiveImg.enabled = false;
    }

    //초대버튼 비활성화 함수
    public void DeActiveInviteBtn()
    {
        inviteBtn.interactable = false;
        activeImg.enabled = false;
        deactiveImg.enabled = true;
    }

    //////////////////////버튼이벤트//////////////////////////
    public void OnClickInviteBtn()
    {
        //초대 함수
        teamsystemManager.InviteAtTeamMessage(userID);

        //초대완료 패널 활성화
        SocialPanel.Instance.ActiveInviteWaitPanel();

        //상대방이 수락/거절 하기전까지 초대버튼 비활성화
        DeActiveInviteBtn();
    }
}
