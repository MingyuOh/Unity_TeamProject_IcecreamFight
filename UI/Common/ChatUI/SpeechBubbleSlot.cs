using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UI;

public class SpeechBubbleSlot : MonoBehaviour
{
    [SerializeField]
    private Text SpeechText;

    public Text userName;


    //처음 텍스트를 셋팅하는 함수
    public void SetText(string str, string username)
    {
        userName.text = username;
        SpeechText.text = str;
    }

    //텍스트를 추가하는 함수
    public void AddText(string str)
    {
        SpeechText.text += '\n';
        SpeechText.text += str;
    }


}
