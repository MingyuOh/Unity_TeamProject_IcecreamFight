using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ExitGames.Client.Photon.Chat;
using UI;

public class SocialFriendSlot : MonoBehaviour
{
    [SerializeField]
    private Text Order;
    [SerializeField]
    private Image CharImg;
    [SerializeField]
    private Text Name;
    [SerializeField]
    private Image RankImg;
    [SerializeField]
    private Text Score;

    [SerializeField]
    private Text statusText;

    //////////////////////////OnOff이미지///////////////////////
    [SerializeField]
    private Image OnImg;           //활성화 이미지
    [SerializeField]
    private Image OffImg;        //비활성화 이미지

    public void SetSlot(int order, string name, string tier, int userstatus)
    {
        //슬롯 셋팅함수
        //일단은 string만 이미지셋팅은 일단은 임시 이미지로..
        Order.text = order.ToString();
        Name.text = name;
        Score.text = tier;

        if (userstatus == ChatUserStatus.Offline)
        {
            //유저가 오프라인일때 off이미지 활성화
            SetOnOffImgCondition(false);
            statusText.text = UserStatus.OFFLINE;

        }
        else
        {
            SetOnOffImgCondition(true);
            switch (userstatus)
            {
                case ChatUserStatus.Invisible:
                    statusText.text = UserStatus.ONLINE;
                    break;
                case ChatUserStatus.Online:
                    statusText.text = UserStatus.ONLINE;
                    break;
                case ChatUserStatus.Away:
                    statusText.text = UserStatus.ONLINE;
                    break;
                case ChatUserStatus.DND:
                    statusText.text = UserStatus.ONLINE;
                    break;
                case ChatUserStatus.LFG:
                    statusText.text = UserStatus.ONLINE;
                    break;
                case ChatUserStatus.Playing:
                    statusText.text = UserStatus.PLAYING;
                    break;
            }
        }
    }
    
    //버튼의 상태 변경 함수
    private void SetOnOffImgCondition(bool isActive)
    {
        if (isActive)      
        {
            //활성화 상태일때
            OnImg.enabled = true;
            OffImg.enabled = false;
        }
        else
        {
            OnImg.enabled = false;
            OffImg.enabled = true;
        }
    }

}
