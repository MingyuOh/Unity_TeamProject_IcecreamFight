using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerThrowController : MonoBehaviour {

	// 아이스크림 볼 프리팹
	public GameObject icecreamBall;
	// 아이스크림 던질 좌표
	public Transform throwPos;

	// 던지기 지연 시간
	private float attackDelayTime;
	// 던지기 가능 여부
	private bool enableAttack;

	////////////////////////////////////////////////////////////////////////////////////
	// 애니메이션
	////////////////////////////////////////////////////////////////////////////////////
	private Animator anim;
	// 애니메이터 컨트롤러에 파라미터의 해시값 추출
	private readonly int hasThrow = Animator.StringToHash("Throw");
	private readonly int hasThrowAndAction = Animator.StringToHash("ThrowAndAction");

	// 공격 애니메이션 시간
	private float attackTime;
	// 던지기 애니메이션 가능 여부
	private bool enableAttackAnim = false;
	

	private void Awake()
	{
		anim = GetComponent<Animator>();
	}

	// Use this for initialization
	void Start ()
	{
		attackTime = AnimationTimeController.Instance[(int)AnimationTimeController.AnimType.THROW];
		
	}
	
	// Update is called once per frame
	void Update ()
	{
		// 레이케스트
		Aim();

		//////////////////////////////////////////////////////////////////////
		// 수정 필요 IActorAction 정의하여 Throw안에 이 작업을 다 넣어야함
		//////////////////////////////////////////////////////////////////////
		// 공격 가능 여부
		if (Input.GetMouseButtonDown(0))
		{
			// 마우스 버튼 클릭시 공격(Dash 행동 시 공격 불가능)
			if (enableAttack == true && PlayerPhysics.isDash == false)
			{
				// Icecream Ball 프리팹을 동적으로 생성
				// 오브젝트 풀 사용해야 함
				Instantiate(icecreamBall, throwPos.position, throwPos.rotation);

				enableAttackAnim = true; // 애니메이션 가능
				enableAttack = false;	 // 공격 불가능
			}
		}
		else
		{
			// 공격 대기시간
			attackDelayTime += Time.deltaTime;
			if (attackTime <= attackDelayTime)
			{
				enableAttack = true;	 // 공격 가능
				attackDelayTime = 0.0f;  // 공격 대기시간 초기화
			}
		}
	}

	private void LateUpdate()
	{
		// 애니메이션 한번만 실행
		if (enableAttackAnim == true)
		{
			ThrowAnim();
			enableAttackAnim = false;
		}
	}

	// 아이스크림 볼 생성 함수
	private void Throw()
	{
		// Icecream Ball 프리팹을 동적으로 생성
		// 오브젝트 풀 사용해야 함
		Instantiate(icecreamBall, throwPos.position, throwPos.rotation);
	}

	private void Aim()
	{
		Ray ray = Camera.main.ScreenPointToRay(throwPos.position);
		RaycastHit rayHit;
		//float rayLength = 500.0f;
		int floorMask = LayerMask.GetMask("STATIC_OBJECT");

		if(Physics.Raycast(throwPos.position, throwPos.TransformDirection(Vector3.forward), out rayHit, Mathf.Infinity, floorMask))
		{
			Debug.DrawRay(throwPos.position, throwPos.TransformDirection(Vector3.forward) * rayHit.distance, Color.magenta);
		}
		else
		{
			Debug.DrawRay(throwPos.position, throwPos.TransformDirection(Vector3.forward) * 1000.0f, Color.yellow);
		}
	}

	private void ThrowAnim()
	{
		bool isAction = (PlayerPhysics.isJumping == true || PlayerPhysics.isMove == true) && PlayerPhysics.isDash == false;
		if (isAction == true )
		{
			anim.SetTrigger(hasThrowAndAction);
		}
		else
		{
			anim.SetTrigger(hasThrow);
		}
	}
}
