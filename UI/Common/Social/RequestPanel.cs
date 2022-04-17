using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RequestPanel : MonoBehaviour
{
    //버튼
    [SerializeField]
    private ReqBtn friendReqBtn;
    [SerializeField]
    private ReqBtn teamReqBtn;

    //패널
    [SerializeField]
    private FriendReqPanel friendreqPanel;
    [SerializeField]
    private TeamReqPanel teamreqPanel;

    public void ActivateRequestPanel()
    {
        this.gameObject.SetActive(true);
        OnClickfriendReqBtn();
    }

    public void DeActivateRequestPanel()
    {
        this.gameObject.SetActive(false);
    }

    /// <summary>
    /// ///////////////////////////버튼 이벤트///////////////////////////////
    /// </summary>
    public void OnClickfriendReqBtn()
    {
        friendReqBtn.ActiviateBackGroundImg();
        teamReqBtn.DeActiviateBackGroundImg();

        //패널 활성화
        friendreqPanel.ActivateFriendReqPanel();
        teamreqPanel.DeActivateTeamReqPanel();
    }

    public void OnClickteamReqBtn()
    {
        friendReqBtn.DeActiviateBackGroundImg();
        teamReqBtn.ActiviateBackGroundImg();

        //패널 활성화
        friendreqPanel.DeActivateFriendReqPanel();
        teamreqPanel.ActivateTeamReqPanel();
    }
}
