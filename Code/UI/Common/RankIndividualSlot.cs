using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RankIndividualSlot : MonoBehaviour
{
    [SerializeField]
    private Text Num;
    [SerializeField]
    private Text Name;
    [SerializeField]
    private Image RankImg;
    [SerializeField]
    private Text Score;

    public void SetSlot(string userID)
    {
        //슬롯 셋팅함수
        //인자로 받은 userID로부터 해당 user정보를 셋팅!
    }
}
