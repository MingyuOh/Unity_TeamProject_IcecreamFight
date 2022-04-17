using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour {

    //적캐릭터의 상태 정의
    public enum State
    {
        PATROL,
        TRACE,
        ATTACK,
        DIE
    }

    //public
    public State state = State.PATROL;      //상태를 저장할 변수

    public float attackDist;                //공격 사정거리
    public float traceDist;                 //추적 사정거리

    public bool isDie;                      //사망 여부 판단

    //private
    private Transform playerTr;             //Player의 위치를 저장할 변수
    private Transform enemyTr;              //Enemy의 위치를 저장할 변수

    private WaitForSeconds ws;              //코루틴에서 사용할 지연시간 변수

    void Awake()
    {
        var player = GameObject.FindGameObjectWithTag("PLAYER");        //Player 게임오브젝트 추출

        //Player의 Transform 컴포넌트 추출
        if (player != null)
        {
            playerTr = player.GetComponent<Transform>();
        }

        //Enemy의 Transform 컴포넌트 추출
        enemyTr = GetComponent<Transform>();

        ws = new WaitForSeconds(0.3f);
    }

    void OnEnable()
    {
        //코루틴 시작
        StartCoroutine(CheckState());
    }

    //Enemy의 상태를 검사하는 코루틴 함수
    IEnumerator CheckState()
    {
        //죽지않았을때
        while (!isDie)
        {
            //상태가 사망이면 코루틴함수 종료
            if (state == State.DIE) yield break;

            //주인공과 적 캐릭터 간의 거리 계산
            float dist = Vector3.Distance(playerTr.position, enemyTr.position);

            //사정거리 이내일 경우
            if (dist <= attackDist)
            {
                state = State.ATTACK;
            }
            //사정거리 밖일 경우
            else if (dist <= traceDist)
            {
                state = State.TRACE;
            }
            else
            {
                state = State.PATROL;
            }
            //ws동안 대기하는 동안 제어권을 양보

            yield return ws;
        }
    }
}
