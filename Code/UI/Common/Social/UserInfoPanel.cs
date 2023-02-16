using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UI;

public class UserInfoPanel : MonoSingleton<UserInfoPanel>
{
    private PlayerInfo playerInfo;      //유저정보

    [SerializeField]
    private Button addBtn;              //친구추가 버튼

    /// <summary>
    /// ///////////////유저 정보를 담을 UI//////////////////////
    /// </summary>
    [SerializeField]
    private Text nickName;
    //[SerializeField]
    //private Image rankImg;
    [SerializeField]
    private Text score;
    //캐릭터 로우이미지 추가..

    private FriendSystemManager friendsystemManager;
    public void Start()
    {
        friendsystemManager = FriendSystemManager.Instance;
        DeActivateOnUserInfoPanel();
    }
    public void ActivateOnUserInfoPanel(PlayerInfo playerInfo)
    {
        this.gameObject.SetActive(true);
        this.playerInfo = playerInfo;

        this.nickName.text = playerInfo.nickName;
        this.score.text = playerInfo.tier;
        //티어를 통해 랭크 도출..
        //캐릭터 로우이미지 추가 예정

        //나와 친구사인지 아닌지 판단후
        //추가버튼활성화 비활성화
        if (friendsystemManager.FindPlayerAtFriendPlayerInfoList(playerInfo.userId) == -1)
        {
            ActivateOrDeActivateAddBtn(true);
        }
        else
        {
            ActivateOrDeActivateAddBtn(false);
        }
    }
    public void DeActivateOnUserInfoPanel()
    {
        this.playerInfo = null;
        this.nickName.text = null;
        this.score.text = null;

        //랭크이미지 , 캐릭터로우미지 또한 리셋

        this.gameObject.SetActive(false);
    }
    public void ActivateOrDeActivateAddBtn(bool isFriend)
    {
        if (isFriend)
        {
            addBtn.gameObject.SetActive(true);
        }
        else
        {
            addBtn.gameObject.SetActive(false);
        }
    }

    ////////////////////////버튼 이벤트/////////////////////////////
    public void OnClickXBtn()
    {
        DeActivateOnUserInfoPanel();
    }

    //버튼이 활성화 돼있을경우
    public void OnClickAddFreindBtn()
    {
        //친구 추가 코루틴
        StartCoroutine(friendsystemManager.FriendRequestCoroutine(this.playerInfo));

        //메세지 패널 2초간 활성화
        StartCoroutine(NoticePanel.Instance.ActiveNoticePanel(SocialStr.NOTICECOMPLETE_REQUESTFRIEND, 2.0f));
    }

}
