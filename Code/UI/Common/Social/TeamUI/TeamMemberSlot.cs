using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Communication;

public class TeamMemberSlot : MonoBehaviour
{
    [SerializeField]
    private Text order;
    [SerializeField]
    private Text name;
    [SerializeField]
    private Text score;

    private string userID;

    /// <summary>
    /// ////////////마스터에게 추방권한을 부여하기 위한/////////////////////
    /// ////////////추방버튼////////////////////////////////////////////////
    /// </summary>
    [SerializeField]
    private Button exileBtn;

    public void SetSlot(int order, string name, string score, string userid,  bool isMaster)
    {
        this.order.text = order.ToString();
        this.name.text = name;
        this.score.text = score;

        this.userID = userid;

        if (isMaster)
            exileBtn.gameObject.SetActive(true);
        else
            exileBtn.gameObject.SetActive(false);
    }

    ///////////////////////////버튼이벤트///////////////////////////////
    public void OnClickExileBtn()
    {
        //자신이 방장일때만 가능
        TeamSystemManager.Instance.OnBanishFromATeam(userID);
    }
}
