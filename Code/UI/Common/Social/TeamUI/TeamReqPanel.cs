using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamReqPanel : MonoBehaviour
{
    [SerializeField]
    private TeamRequestConstant teamrequestConstant;         //팀요청목록컨텐트

    public void ActivateTeamReqPanel()
    {
        this.gameObject.SetActive(true);
        teamrequestConstant.RenewalTeamRequestList();
    }

    public void DeActivateTeamReqPanel()
    {
        this.gameObject.SetActive(false);
    }
}
