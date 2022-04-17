using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FriendReqPanel : MonoBehaviour
{
    [SerializeField]
    private RequestContent requestContent;         //친구요청목록컨텐트

    public void ActivateFriendReqPanel()
    {
        requestContent.RenewalfriendRequestList();
        this.gameObject.SetActive(true);
    }

    public void DeActivateFriendReqPanel()
    {
        this.gameObject.SetActive(false);
    }

}
