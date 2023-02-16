using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackButtonEvent : MonoBehaviour
{
    //백버튼이벤트
    public delegate void BackButtonEventHandler();
    public static event BackButtonEventHandler InformOnClickBackButton;

    public void OnClickBacButton()
    {
        InformOnClickBackButton();
    }
}
