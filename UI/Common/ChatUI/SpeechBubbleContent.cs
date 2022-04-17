using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Communication;
using UI;
public class SpeechBubbleContent : MonoSingleton<SpeechBubbleContent>
{
    //슬롯의 루트
    public Transform slotRoot;

    //슬롯 리스트
    private List<GameObject> slots;

    public void Awake()
    {
        slots = new List<GameObject>();
    }

    /// <summary>
    /// 슬롯 리스트에 들어있는 마지막 말풍선의 타입을 반환
    /// 왼쪽 말풍선이면 1 , 오른쪽 말풍선이면 -1 반환
    /// </summary>
    /// <returns></returns>
    public int ReturnLastBubbleType()
    {
        int size = slots.Count;

        if (size == 0)          //리스트의 사이즈가 0일때
            return -1;          //테스트용 일단은 -1..

        if (slots[size - 1].name == "leftspeechbubble")
            return 1;
        else if (slots[size - 1].name == "rightspeechbubble")
            return -1;
        else
        {
            Debug.Log("잘못된 말풍선타입입니다.");
            return 0;
        }
    }

    /// <summary>
    /// 마지막 말풍선의 주인(usernickname)을 리턴하는 함수
    /// </summary>
    /// <returns></returns>
    public string ReturnLastBubbleUserName()
    {
        int size = slots.Count;

        if (size == 0)          //리스트의 사이즈가 0일때
            return null;          //테스트용 일단은 -1..
        else
        {
            return slots[size - 1].GetComponent<SpeechBubbleSlot>().userName.text;
        }
    }

    /// <summary>
    /// 마지막 말풍선의 크기가 최대치를 넘어섰는지를
    /// 리턴하는 함수
    /// </summary>
    /// <returns></returns>
    public bool IsOverLastBubbleConstantSize()
    {
        int size = slots.Count;

        if (size == 0)
            return false;

        float ratioOfSize = slots[size - 1].GetComponentInChildren<Image>().GetComponent<RectTransform>().rect.height /
                            slots[size - 1].GetComponentInChildren<Image>().GetComponent<RectTransform>().rect.width;

        //  세로 / 가로 비율이 0.5 이상일경우
        if (ratioOfSize > 0.5)
            return true;
        else
            return false;
    }

    //상대 말풍선 추가

    //내 말을 입력했을때

    //1. 콘텐츠의 마지막이 상대방의 말일때
    //새로운 말풍선 추가

    //2. 콘텐츠의 마지막이 내 말일때
    //말풍선의 세로크기가 맥스치가 아닐때 - 텍스트추가
    //말풍선의 세로크기가 맥스치일때 - 새로운 말풍선 추가

    //팀이 없어졌을때 실행되어야하는 함수

    //왼쪽 말풍선 추가 함수
    public void AddLeftBubble(string str, bool isOver, string userID)
    {
        
        string lastUName = ReturnLastBubbleUserName();
        int size = slots.Count;

        string username = FriendSystemManager.Instance.FetchPlayerInfoAtFriendPlayerList(userID).nickName;


        if (username.CompareTo(lastUName) == 0 && isOver == false && size != 0)
        {
            //보낸사람의 username과 마지막 말풍선의 주인이 같을때
            //마지막 말풍선의 크기가 최대치가 아닐때
            //또한 말풍선컨텐트의 사이즈가 0이 아닐때

            //마지막말풍선에 받은 메세지만을 더한다
            slots[size - 1].GetComponent<SpeechBubbleSlot>().AddText(str);
        }
        else
        {
            //새로운 왼쪽말풍선을 생성한다
            Vector3 originScale = slotRoot.localScale;

            GameObject prefab = Resources.Load("Prefab/UI/LeftSpeechBubble") as GameObject;
            GameObject leftspeechBubble = MonoBehaviour.Instantiate(prefab) as GameObject;

            leftspeechBubble.name = "leftspeechbubble";
            leftspeechBubble.transform.SetParent(slotRoot);
            leftspeechBubble.transform.localScale = originScale;
            leftspeechBubble.GetComponent<SpeechBubbleSlot>().SetText(str, username);

            slots.Add(leftspeechBubble);
        }
    }

    //오른쪽 말풍선 추가 함수
    public void AddRightBubble(string str, bool isOver)
    {
        int size = slots.Count;

        if (isOver || size == 0)
        {
            Vector3 originScale = slotRoot.localScale;

            GameObject prefab = Resources.Load("Prefab/UI/RightSpeechBubble") as GameObject;
            GameObject rightspeechBubble = MonoBehaviour.Instantiate(prefab) as GameObject;

            rightspeechBubble.name = "rightspeechbubble";
            rightspeechBubble.transform.SetParent(slotRoot);
            rightspeechBubble.transform.localScale = originScale;
            rightspeechBubble.GetComponent<SpeechBubbleSlot>().SetText(str, "나");

            slots.Add(rightspeechBubble);
        }
        else
        {
            slots[size - 1].GetComponent<SpeechBubbleSlot>().AddText(str);
        }
    }

    //가운데 말풍선 추가 함수
    //유저가 추방, 참가, 나갔을 경우 생성된다
    public void AddCenterBubble(int num ,string userID)
    {
        //username 셋팅
        string username = FriendSystemManager.Instance.FetchPlayerInfoAtFriendPlayerList(userID).nickName;

        Vector3 originScale = slotRoot.localScale;

        GameObject prefab = Resources.Load("Prefab/UI/CenterSpeechBubble") as GameObject;
        GameObject centerspeechBubble = MonoBehaviour.Instantiate(prefab) as GameObject;

        centerspeechBubble.name = "centerspeechbubble";
        centerspeechBubble.transform.SetParent(slotRoot);
        centerspeechBubble.transform.localScale = originScale;

        //상황에 맞는 메세지 세팅
        switch (num)
        {
            case 1:
                //유저가 추방당했을때
                centerspeechBubble.GetComponent<SpeechBubbleSlot>().SetText(ChatMessage.BANISHUSER, username);
                break;
            case 2:
                //유저가 방을 나갔을때
                centerspeechBubble.GetComponent<SpeechBubbleSlot>().SetText(ChatMessage.OUTUSER, username);
                break;
            case 3:
                //유저가 방을 들어왔을때
                centerspeechBubble.GetComponent<SpeechBubbleSlot>().SetText(ChatMessage.JOINEDUSER, username);
                break;
        }
        slots.Add(centerspeechBubble);
    }
    public void ListClear()
    {
        //클리어
        slots.Clear();
    }
}
