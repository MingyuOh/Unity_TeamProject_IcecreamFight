using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Communication;

public class RankIndividualContent : MonoBehaviour
{
    public Transform slotRoot;      //슬롯의 루트

    //슬롯 리스트
    private List<RankIndividualSlot> slots;

    //MyRank슬롯
    [SerializeField]
    private RankIndividualSlot myRankslot;

    //DB의 유저정보들에 접근할 필요..
    //100명의 User를 Score가 높은순으로 추가
    public void Start()
    {
        slots = new List<RankIndividualSlot>(); //리스트 생성
    }

    //점수 변동이 있을때 마다 돌려주어야하는 리뉴얼 함수
    IEnumerator RenewalRankIndividualList()
    {
        //DB의 유저정보들에 접근할 필요..
        //100명의 User를 Score가 높은순으로 추가

        Vector3 originScale = slotRoot.localScale;

        //100만큼 슬롯 추가
        for (int RCnt = 0; RCnt < 100; RCnt++)
        {
            
        }

        //내 정보를 받아와 내 랭크슬롯을 셋팅
        //myRankslot.SetSlot();

        yield return null;
    }
}
