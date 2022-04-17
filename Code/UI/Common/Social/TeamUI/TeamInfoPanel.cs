using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamInfoPanel : MonoBehaviour
{
    [SerializeField]
    private TeamMemberContent temmemberContent;         //친구요청목록컨텐트

    public void ActiveTeamInfoPanel()
    {
        temmemberContent.RenewalTeamMemberList();
        this.gameObject.SetActive(true);
    }

    public void DeActiveTeamInfoPanel()
    {
        this.gameObject.SetActive(false);
    }
}
