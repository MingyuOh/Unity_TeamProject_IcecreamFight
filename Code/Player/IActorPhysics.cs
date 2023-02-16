using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IActorPhysics
{
	// 이동 함수
	void Move();
	// 점프 함수
	void Jump();
	// 대쉬 함수
	void Dash();
	// 공격 함수
	void Throw();
    // 죽음 함수
    void Die();
    // 죽음2 함수
    void Die_Sec();
}
