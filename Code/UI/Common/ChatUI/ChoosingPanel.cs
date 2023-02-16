using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Communication;

public class ChoosingPanel : MonoBehaviour
{
    //내부 패널
    [SerializeField]
    private ChatTeamPanel chatteamPanel;
    [SerializeField]
    private ChatGuildPanel chatguildPanel;


    public void ActivateChoosingPanel()
    {
        this.gameObject.SetActive(true);
        //초기상태
        chatteamPanel.ActivateChatSubPanelPanel();
        chatguildPanel.DeActivateChatSubPanelPanel();
    }

    public void DeActivateChoosingPanel()
    {
        this.gameObject.SetActive(false);
    }

    //////////////////////////버튼이벤트////////////////////////
    public void OnClickChatTeamBtn()
    {
        chatteamPanel.gameObject.SetActive(true);
        chatguildPanel.gameObject.SetActive(false);
    }

    public void OnClickChatGuildBtn()
    {
        chatteamPanel.gameObject.SetActive(false);
        chatguildPanel.gameObject.SetActive(true);
    }
}
