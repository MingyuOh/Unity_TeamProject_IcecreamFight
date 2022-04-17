using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommonGUI : MonoSingleton<CommonGUI>
{
    [SerializeField]
    private RectTransform CommonGuiPanel;

    [SerializeField]
    private RectTransform Gold;
    [SerializeField]
    private RectTransform Dia;
    [SerializeField]
    private RectTransform Setting;
    [SerializeField]
    private RectTransform Mail;
    [SerializeField]
    private RectTransform Chat;
    [SerializeField]
    private RectTransform BackButton;
    

    private void Awake()
    {
        this.gameObject.SetActive(true);

        HideGold();
        HideDia();
        HideSetting();
        HideMail();
        HideChat();
        HideBackButton();
    }

    //Gold
    public void ShowGold()
    {
        Gold.gameObject.SetActive(true);
    }

    public void HideGold()
    {
        Gold.gameObject.SetActive(false);
    }

    //Dia
    public void ShowDia()
    {
        Dia.gameObject.SetActive(true);
    }

    public void HideDia()
    {
        Dia.gameObject.SetActive(false);
    }

    //Setting
    public void ShowSetting()
    {
        Setting.gameObject.SetActive(true);
    }

    public void HideSetting()
    {
        Setting.gameObject.SetActive(false);
    }

    //Mail
    public void ShowMail()
    {
        Mail.gameObject.SetActive(true);
    }

    public void HideMail()
    {
        Mail.gameObject.SetActive(false);
    }

    //Chat
    public void ShowChat()
    {
        Chat.gameObject.SetActive(true);
    }

    public void HideChat()
    {
        Chat.gameObject.SetActive(false);
    }
    //BackButton
    public void ShowBackButton()
    {
        BackButton.gameObject.SetActive(true);
    }

    public void HideBackButton()
    {
        BackButton.gameObject.SetActive(false);
    }

}
