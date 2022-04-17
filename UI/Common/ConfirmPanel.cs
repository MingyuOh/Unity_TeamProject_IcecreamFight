using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfirmPanel : MonoBehaviour
{
    //확정 이벤트
    public delegate void InputConfirmHandler();
    public static event InputConfirmHandler InformConfirm;

    public void OnClickYesButton()
    {
        InformConfirm();
        Main.Instance.HideConfirmPanel();
    }

    public void OnClickNoButton()
    {
        Main.Instance.HideConfirmPanel();
    }
}
