using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Communication;

public class ChatPanel : MonoSingleton<ChatPanel>
{

    [SerializeField]
    private ChoosingPanel choosingPanel;
    [SerializeField]
    private OnTeamChatPanel onteamchatPanel;
    
    public void ActivateChatPanel()
    {
        this.gameObject.SetActive(true);
        ChatTeamPanel.teamchatinInform += TeamChatIn;
        //초기상태
        choosingPanel.ActivateChoosingPanel();
        onteamchatPanel.DeActivateOnTeamChatPanel();
    }

    public void DeActivateChatPanel()
    {
        ChatTeamPanel.teamchatinInform -= TeamChatIn;
        this.gameObject.SetActive(false);
    }

    //팀채팅인 함수
    public void TeamChatIn()
    {
        BackButtonEvent.InformOnClickBackButton += OutTeamChatPanel;
        choosingPanel.DeActivateChoosingPanel();
        onteamchatPanel.ActivateOnTeamChatPanel();
    }

    public void OutTeamChatPanel()
    {
        BackButtonEvent.InformOnClickBackButton -= OutTeamChatPanel;
        onteamchatPanel.DeActivateOnTeamChatPanel();
        choosingPanel.ActivateChoosingPanel();
    }

    //////////////////////////버튼이벤트////////////////////////
    public void OnClickXBtn()
    {
        DeActivateChatPanel();
    }

    
}
