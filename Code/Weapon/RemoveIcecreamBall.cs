using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoveIcecreamBall : MonoBehaviour {

	// 충돌이 시작할 때 발생하는 이벤트
	private void OnCollisionEnter(Collision collision)
	{
		// 충돌한 게임오브젝트의 태그값 비교
		if(collision.collider.tag == "ICECREAM_BALL")
		{
			// 충돌한 게임오브젝트 삭제
			Destroy(collision.gameObject);
		}
	}
}
