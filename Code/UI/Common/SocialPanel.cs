using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SocialPanel : MonoSingleton<SocialPanel>
{
    //소셜패널의 종속패널
    [SerializeField]
    private FriendPanel friendPanel;
    [SerializeField]
    private TeamPanel teamPanel;
    [SerializeField]
    private RequestPanel requestPanel;

    [SerializeField]
    private RectTransform inviteWaitPanel;              //초대대기 패널

    [SerializeField]
    private InputField nicknameInputfield;              //닉네임 인풋필드

    [SerializeField]
    private Image requestImg;
    [SerializeField]
    private Text requestText;
    private int requestNum;
    
    
    FriendSystemManager freindsystemManager;
    //소셜패널의 종속UI

    public void Awake()
    {
        freindsystemManager = FriendSystemManager.Instance;
    }

    public void ActivateSocialPanel()
    {
        this.gameObject.SetActive(true);
        //초기상태
        friendPanel.ActivateFriendPanel();
        teamPanel.DeActivateTeamPanel();
        requestPanel.DeActivateRequestPanel();

        inviteWaitPanel.gameObject.SetActive(false);
        nicknameInputfield.text = null;

        if (RequestImg.Instance.GetrequestNum() != 0)
        {
            SetRequestNumText(RequestImg.Instance.GetrequestNum());
            requestImg.gameObject.SetActive(true);
        }
        else
        {
            DeActivateRequestImg();
        }
    }

    public void DeActivateSocialPanel()
    {
        this.gameObject.SetActive(false);
    }

    public void ActiveInviteWaitPanel()
    {
        //초대 대기 패널 활성화
        inviteWaitPanel.gameObject.SetActive(true);

        //상대방이 초대를 수락하거나
        //확인을 누를시 패널 비활성화
    }
    public void DeActiveInviteWaitPanel()
    {
        //초대 대기 패널 비활성화
        inviteWaitPanel.gameObject.SetActive(false);
    }

    public void DeActivateRequestImg()
    {
        requestImg.gameObject.SetActive(false);
    }

    public void SetRequestNumText(int num)
    {
        requestNum = num;
        requestText.text = requestNum.ToString();
    }

    public int GetRequestNum()
    {
        return requestNum;
    }
    ///////////////////////버튼 클릭 이벤트////////////////////////
    public void OnClickFriendBtn()
    {
        friendPanel.ActivateFriendPanel();
        teamPanel.DeActivateTeamPanel();
        requestPanel.DeActivateRequestPanel();
    }
    public void OnClickTeamBtn()
    {
        friendPanel.DeActivateFriendPanel();
        teamPanel.ActivateTeamPanel();
        requestPanel.DeActivateRequestPanel();
    }

    public void OnClickRequestBtn()
    {
        friendPanel.DeActivateFriendPanel();
        teamPanel.DeActivateTeamPanel();
        requestPanel.ActivateRequestPanel();
    }

    public void OnClickXbutton()
    {
        DeActivateSocialPanel();
    }

    public void OnClickSearchUserBtn()
    {
        StartCoroutine(GetFoundPlayerInfo());
    }

    IEnumerator GetFoundPlayerInfo()
    {
        IEnumerator coroutine = freindsystemManager.FindFriendCoroutine(nicknameInputfield.text);
        yield return StartCoroutine(coroutine);

        if (coroutine.Current != null)
        {
            //찾고자 하는 플레이어가 존재할때
            PlayerInfo playerInfo = (PlayerInfo)coroutine.Current;
			
			UserInfoPanel.Instance.ActivateOnUserInfoPanel(playerInfo);
			DeActivateSocialPanel();
        }
        else
        {
            //찾고자 하는 플레이어가 존재하지 않을때
            Debug.Log("플레이어가 존재하지 않습니다");
			StopCoroutine(coroutine);
        }
    }

    //서브패널 버튼 이벤트
    public void OnClickInviteNoticePanelOkButtn()
    {
        DeActiveInviteWaitPanel();
    }
}
