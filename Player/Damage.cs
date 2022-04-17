using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damage : MonoBehaviour {

    //public
    public float CurrHP;                    //현재 체력

    //private
    private float InitHP = 100.0f;          //처음 체력

    void Start()
    {
        CurrHP = InitHP;    
    }

    //캐릭터 피격 체크함수
    void OnTriggerEnter(Collider coll)
    {
        //원거리 피격 or 근거리 피격시 체력 감소

        //플레이어 현재 체력 0 이하시
        //사망
        if (CurrHP <= 0.0f)
        {
            PlayerDie();
        }
    }

    //Player 사망 루틴
    void PlayerDie()
    {

    }
}
