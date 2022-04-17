using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Communication;

public class TeamPanel : MonoBehaviour
{
    [SerializeField]
    private TeamBeforePanel teambeforePanel;            //팀생성 전 패널
    [SerializeField]
    private CreatedTeamPanel createdteamPanel;          //팀생성 후 패널

    private TeamSystemManager teamsystemManager;

    public void Awake()
    {
        teamsystemManager = TeamSystemManager.Instance;
    }
    public void ActivateTeamPanel()
    {
        this.gameObject.SetActive(true);

        //초기상태
        //생성된 팀이 존재할 경우
        if (teamsystemManager.isTeam)
        {
            teambeforePanel.DeActivateTeamBeforePanel();
            createdteamPanel.ActivateCreatedTeamPanel();
        }
        else {
            teambeforePanel.ActivateTeamBeforePanel();
            createdteamPanel.DeActivateCreatedTeamPanel();
        }

        CreatedTeamPanel.deleteTeamInform += DeleteTeam;
        TeamBeforePanel.createTeamInform += CreateTeam;
    }

    public void DeActivateTeamPanel()
    {
        this.gameObject.SetActive(false);

        TeamBeforePanel.createTeamInform -= CreateTeam;
        CreatedTeamPanel.deleteTeamInform -= DeleteTeam;
    }

    //팀생성 함수
    public void CreateTeam()
    {
        //팀생성
        teamsystemManager.OnCreateTeam();

        //팀생성패널 활성화
        createdteamPanel.ActivateCreatedTeamPanel();
    }

    //팀삭제 함수
    public void DeleteTeam()
    {
        //팀 탈퇴 버튼
        teamsystemManager.OnWithdrawalFromATeam();

        teambeforePanel.ActivateTeamBeforePanel();
    }
}
