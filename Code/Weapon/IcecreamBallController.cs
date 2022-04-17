using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IcecreamBallController : MonoBehaviour {

	// 아이스크림 볼 파괴력
	public float damage = 20.0f;

	// 아이스크림 볼 속도
	public float speed = 1000.0f;

	// Use this for initialization
	void Start () {
		GetComponent<Rigidbody>().AddForce(transform.forward * speed);
	}
}
