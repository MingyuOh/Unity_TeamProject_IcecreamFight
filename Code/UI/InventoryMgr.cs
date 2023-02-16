using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class InventoryMgr : MonoSingleton<InventoryMgr> {

    public Button IL_Button_Skin;
    public Button IL_Button_Acc;

    private Transform Skin_ParentObj;
    private Transform[] Skin_Childeren;

    private Transform Acc_ParentObj;
    private Transform[] Acc_Childeren;

    void Start()
    {
        Skin_ParentObj = IL_Button_Skin.transform;
        Skin_Childeren = Skin_ParentObj.gameObject.GetComponentsInChildren<Transform>();

        Acc_ParentObj = IL_Button_Acc.transform;
        Acc_Childeren = Acc_ParentObj.gameObject.GetComponentsInChildren<Transform>();
    }

    public void OnClickItemListButton_Skin()
    {

        foreach (var i in Skin_Childeren)
        {
            i.gameObject.SetActive(true);
        }

        foreach (var i in Acc_Childeren)
        {
            i.gameObject.SetActive(false);
        }
        Acc_Childeren[0].gameObject.SetActive(true);
        Acc_Childeren[1].gameObject.SetActive(true);
    }

    public void OnClickItemListButton_Acc()
    {

        foreach (var i in Acc_Childeren)
        {
            i.gameObject.SetActive(true);
        }

        foreach (var i in Skin_Childeren)
        {
            i.gameObject.SetActive(false);
        }
        Skin_Childeren[0].gameObject.SetActive(true);
        Skin_Childeren[1].gameObject.SetActive(true);
    }
}
