using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamInvitePanel : MonoBehaviour
{
    [SerializeField]
    private InviteMemberContent invitememberContent;         //친구요청목록컨텐트

    public void ActiveTeamInvitePanel()
    {
        invitememberContent.RenewalInviteMemberList();
        this.gameObject.SetActive(true);
    }

    public void DeActiveTeamInvitePanel()
    {
        this.gameObject.SetActive(false);
    }
}
