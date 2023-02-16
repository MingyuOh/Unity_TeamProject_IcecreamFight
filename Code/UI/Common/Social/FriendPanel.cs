using System.Collections;
using System.Collections.Generic;
using Communication;
using UnityEngine;
using UnityEngine.UI;

public class FriendPanel : MonoBehaviour
{
    [SerializeField]
    private SocialFriendContent socialfriendCotnet;         //친구목록컨텐트

    public void ActivateFriendPanel()
    {
        this.gameObject.SetActive(true);

        //친구목록 리뉴얼
        socialfriendCotnet.RenewalFriendList();
    }

    public void DeActivateFriendPanel()
    {
        this.gameObject.SetActive(false);
    }
}
