using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Communication;

public class RequestContent : MonoSingleton<RequestContent>
{
    //슬롯루트
    public Transform slotRoot;
    
    //슬롯리스트
    private List<GameObject> slots;

    private FriendSystemManager friendsystemManager;


    [SerializeField]
    private Text numofFReq;
    private int numofReq;

    public void Awake()
    {
        friendsystemManager = FriendSystemManager.Instance;
        slots = new List<GameObject>();        //리스트 생성
    }

    public void OnDisable()
    {
        DeleteSlotAll();
    }

    public void RenewalfriendRequestList()
    {
        numofReq = 0;

        DeleteSlotAll();

        foreach (KeyValuePair<string, FriendItem> fID in friendsystemManager.friendItemDictionary)
        {
            if (fID.Value.relationStatus == FriendItem.FriendRelationStatus.REQUESTED)
            {
                AddfriendRequestContent(fID.Key, fID.Value.nickName);
                numofReq++;
            }
        }

        numofFReq.text = numofReq.ToString();
    }

    public void AddfriendRequestContent(string userid, string nickname)
    {
        Vector3 originScale = slotRoot.localScale;

        GameObject prefab = Resources.Load("Prefab/UI/RequestContent") as GameObject;
        GameObject requestSlot = MonoBehaviour.Instantiate(prefab) as GameObject;

        requestSlot.name = "requestSlot";
        requestSlot.transform.SetParent(slotRoot);
        requestSlot.transform.localScale = originScale;
        requestSlot.GetComponent<RequestSlot>().SetSlot(userid, nickname);

        slots.Add(requestSlot);
    }

    public void DeletefriendRequestSlot(string userid)
    {
        for (int iCnt = 0; iCnt < slots.Count; iCnt++)
        {
            if (slots[iCnt].GetComponent<RequestSlot>().GetuserID().CompareTo(userid) == 0)
            {
                Destroy(slots[iCnt]);
                slots.RemoveAt(iCnt);

                break;
            }
        }

        numofReq--;
        numofFReq.text = numofReq.ToString();
    }

    public void DeleteSlotAll()
    {
        if (slots.Count == 0)
            return;
        for (int iCnt = slots.Count-1; iCnt >= 0; iCnt--)
        {
            Destroy(slots[iCnt]);
            slots.RemoveAt(iCnt);
        }
    }
}
