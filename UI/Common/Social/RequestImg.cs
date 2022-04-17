using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Communication;

public class RequestImg : MonoSingleton<RequestImg>
{
    [SerializeField]
    Text requestNum;        //요청갯수를 받는 텍스트

    private int requestCnt;

    public void Start()
    {
        ActivateRequestImg();
    }

    public void ActivateRequestImg()
    {
        requestCnt = 0;

        //친구 요청수를 계산
        foreach (KeyValuePair<string, FriendItem> fID in FriendSystemManager.Instance.friendItemDictionary)
        {
            if (fID.Value.relationStatus == FriendItem.FriendRelationStatus.REQUESTED)
            {
                requestCnt++;
            }
        }

        //팀원 요청수를 계산
        requestCnt += TeamSystemManager.Instance.teamMatchingManageList.Count;

        if (requestCnt != 0)
        {
            requestNum.text = requestCnt.ToString();
            this.gameObject.SetActive(true);
        }
        else
        {
            DeActivateRequestImg();
        }
    }

    public int GetrequestNum()
    {
        return requestCnt;
    }

    public void SetrequestNum(int num)
    {
        requestNum.text = requestCnt.ToString();
        requestCnt = num;
    }

    public void DeActivateRequestImg()
    {
        this.gameObject.SetActive(false);
    }
}
