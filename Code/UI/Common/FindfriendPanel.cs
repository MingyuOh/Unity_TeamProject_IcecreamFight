using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FindfriendPanel : MonoBehaviour
{
    [SerializeField]
    private RectTransform noticePanel;

    [SerializeField]
    private Text nickname;          //찾고자 하는 유저의 닉네임

    [SerializeField]
    private Text noticeText;        //알림 메시지 스트링

    FriendSystemManager friendsystemManager;
    public void Start()
    {
        noticePanel.gameObject.SetActive(false);

        friendsystemManager = FriendSystemManager.Instance;
    }


    public void OnClickSearchPlayer()
    {
        StartCoroutine(GetFoundPlayerInfo());

    }

    public void OnClickAddNewFriend()
    {
        //StartCoroutine(freindsystemManager.FriendRequestCoroutine())
        StartCoroutine(ShowNoticePanel("친구추가 메세지를 보냈습니다"));
    }

    //찾은 친구 정보를 가져오는 코루틴
    IEnumerator GetFoundPlayerInfo()
    {
        IEnumerator coroutine = friendsystemManager.FindFriendCoroutine(nickname.text);
        yield return StartCoroutine(coroutine);

        if (coroutine.Current != null)
        {
            PlayerInfo playerinfo = (PlayerInfo)coroutine.Current;
        }
    }

    public void OnClickXButton()
    {
        this.gameObject.SetActive(false);
    }

    IEnumerator ShowNoticePanel(string noticestr)
    {
        noticePanel.gameObject.SetActive(true);
        noticeText.text = noticestr;
        //2초 대기
        yield return new WaitForSeconds(2f);

        noticeText.text = null;
        noticePanel.gameObject.SetActive(false);

    }
}
