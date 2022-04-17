using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Communication;

public class CreatedTeamPanel : MonoBehaviour
{

    //버튼
    [SerializeField]
    private ReqBtn infoBtn;     //팀정보버튼
    [SerializeField]
    private ReqBtn inviteBtn;   //초대목록버튼

    //패널
    [SerializeField]
    private TeamInfoPanel teaminfoPanel;        //팀정보패널
    [SerializeField]
    private TeamInvitePanel teaminvitePanel;        //팀초대패널

    //팀삭제 알림
    public delegate void deleteTeamBtnHandler();
    public static event deleteTeamBtnHandler deleteTeamInform;

    public void ActivateCreatedTeamPanel()
    {
        this.gameObject.SetActive(true);

        //초기상태
        OnClickInfoBtn();
    }

    public void DeActivateCreatedTeamPanel()
    {
        this.gameObject.SetActive(false);
    }

    ////////////////////////버튼이벤트///////////////////////////
    public void OnClickInfoBtn()
    {
        infoBtn.ActiviateBackGroundImg();
        inviteBtn.DeActiviateBackGroundImg();

        teaminfoPanel.ActiveTeamInfoPanel();
        teaminvitePanel.DeActiveTeamInvitePanel();
    }

    public void OnClickInviteBtn()
    {
        infoBtn.DeActiviateBackGroundImg();
        inviteBtn.ActiviateBackGroundImg();

        teaminfoPanel.DeActiveTeamInfoPanel();
        teaminvitePanel.ActiveTeamInvitePanel();
    }
    
    //팀 나가기 버튼
    public void OnClickWithDrawalBtn()
    {
        deleteTeamInform();

        DeActivateCreatedTeamPanel();
        //팀탈퇴후 팀정보 삭제
    }
}
