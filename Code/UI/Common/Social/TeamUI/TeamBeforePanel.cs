using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamBeforePanel : MonoBehaviour
{
    //팀생성 알림
    public delegate void createTeamBtnHandler();
    public static event createTeamBtnHandler createTeamInform;

    public void ActivateTeamBeforePanel()
    {
        this.gameObject.SetActive(true);
    }

    public void DeActivateTeamBeforePanel()
    {
        this.gameObject.SetActive(false);
    }

    /////////////////////버튼이벤트////////////////////////
    public void OnClickCreateTeamBtn()                  //팀생성 버튼
    {
        //팀생성 버튼이 눌려졌음을 알림
        createTeamInform();

        DeActivateTeamBeforePanel();
    }
}
