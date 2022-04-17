using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InviteNoticePanel : MonoSingleton<InviteNoticePanel>
{
    [SerializeField]
    private Text idText;            //초대한 유저아이디

    public void Start()
    {
        DeActiveInviteNoticePanel();
    }
    public void ActiveInviteNoticePanel(string idStr)
    {
        if (idStr == null)
        {
            Debug.Log("초대 오류입니다");
            return;
        }

        this.gameObject.SetActive(true);
        this.idText.text = idStr;
    }

    public void DeActiveInviteNoticePanel()
    {
        this.idText.text = null;
        this.gameObject.SetActive(false);
    }

    //////////////////버튼 이벤트////////////////////
    public void OnClickAcceptButton()
    {
        //초대 수락
        //생성된 파티에 추가 되는 로직

        //초대자에게 수락완료 메시지 보냄

        //패널 종료
        DeActiveInviteNoticePanel();
    }

    public void OnClickRefuseButton()
    {
        //초대 거절
        //초대자에게 거절 메시지 보냄

        //패널 종료
        DeActiveInviteNoticePanel();
    }
}
