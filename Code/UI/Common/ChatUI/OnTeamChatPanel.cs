using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Communication;

public class OnTeamChatPanel : MonoBehaviour
{
    [SerializeField]
    private SpeechBubbleContent speechbubbleContent;

    [SerializeField]
    private InputField inputField;                    //인풋필드에 쓰여진 텍스트

    private string selectedChannel로;

    
    public void ActivateOnTeamChatPanel()
    {
        this.gameObject.SetActive(true);
    }

    public void DeActivateOnTeamChatPanel()
    {
        this.gameObject.SetActive(false);
    }

    //////////////////////////버튼이벤트////////////////////////
    public void OnClickSpeechBtn()
    {
        if (speechbubbleContent.ReturnLastBubbleType() == 1)
        {
            //마지막 말풍선이 상대방의 말일때
            //새로운 말풍선 추가

            speechbubbleContent.AddRightBubble(inputField.text, true);
        }
        else
        {
            bool isOver = speechbubbleContent.IsOverLastBubbleConstantSize();
            speechbubbleContent.AddRightBubble(inputField.text, isOver);
            //마지막 말풍선이 내 말일때
            //말풍선의 세로크기가 맥스치가 아닐때 - 텍스트추가
            //말풍선의 세로크기가 맥스치일때 - 새로운 말풍선 추가
        }
        NetworkSocialManager.Instance.SendChatMessage(inputField.text);
        inputField.text = null;
    }
}
