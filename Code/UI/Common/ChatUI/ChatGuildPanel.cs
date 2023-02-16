using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChatGuildPanel : MonoBehaviour
{
    [SerializeField]
    private Button guildchatinBtn;

    [SerializeField]
    private Text noguildText;

    //길드채팅 오픈버튼이 눌러졌음을 알림
    //public delegate void teamchatinBtnHandler();
    //public static event teamchatinBtnHandler teamchatinInform;

    public void ActivateChatSubPanelPanel()
    {
        this.gameObject.SetActive(true);

        //텍스트 활성화
        noguildText.gameObject.SetActive(true);
        //버튼 제거
        guildchatinBtn.gameObject.SetActive(false);

        //if (TeamSystemManager.Instance.isTeam)
        //{
        //    //팀이 있을경우
        //    //텍스트 제거
        //    noteamText.gameObject.SetActive(false);
        //    //버튼 활성화
        //    teamchatinBtn.gameObject.SetActive(true);
        //}
        //else
        //{
        //    //팀이 없을경우
        //    //텍스트 활성화
        //    noteamText.gameObject.SetActive(true);
        //    //버튼 제거
        //    teamchatinBtn.gameObject.SetActive(false);
        //}
    }

    public void DeActivateChatSubPanelPanel()
    {
        this.gameObject.SetActive(false);
    }

    /// <summary>
    /// ///////////////////////버튼이벤트/////////////////////////
    /// </summary>
    public void OnClickGuildChatInBtn()
    {

    }
}
