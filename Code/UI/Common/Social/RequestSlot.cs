using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UI;

public class RequestSlot : MonoBehaviour
{
    private string userID;

    [SerializeField]
    private Text nickName;

    public void SetSlot(string userid , string nickname)
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
        //수락버튼
        StartCoroutine(FriendSystemManager.Instance.AcceptAddFriend(userID, Main.Instance.ForceToTitle));

        //수락완료 UI패널
        //StartCoroutine(NoticePanel.Instance.ActiveNoticePanel(nickName.text + SocialStr.NOTICECOMPLETE_ACCEPTFRIEND, 2.0f));
    }

    public void OnClickRefuseBtn()
    {
        //거절버튼
        StartCoroutine(FriendSystemManager.Instance.RefuseAddFriend(userID));
    }
}
