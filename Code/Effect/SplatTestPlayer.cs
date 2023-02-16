using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplatTestPlayer : MonoBehaviour, IActorPhysics
{
	#region 변수

	#region 플레이어 정보 변수

	private int maxHealth = 100;    // 플레이어 최대 체력
	public GameObject killedBy;    // 플레이어를 죽인 적 오브젝트

	[HideInInspector]
	public CameraController camFollow;  // 플레이어 전용 카메라

	public SplatterState mySplatterState { get; private set; }

	private string _characterTypeName = null;
	public string characterTypeName { get { return _characterTypeName; } }
	private string _characterSkinName = null;
	public string characterSkinName { get { return _characterSkinName; } }

	//현재 캐릭터 회전 및 사격 방향
	[HideInInspector]
	public short playerRotation;

	#endregion

	#region 이펙트 변수

	public GameObject throwFX;      // 플레이어 공격 이펙트
	public GameObject splatterFX;   // 플레이어 무기 오브젝트 충돌 시 이펙트

	#endregion

	#region 사운드 변수

	public AudioClip throwClip;     // 플레이어 공격 사운드
	public AudioClip damageClip;    // 플레이어 데미지 사운드
	public AudioClip dieClip;       // 플레이어 죽는 사운드
	public AudioClip splatterClip;  // 오브젝트에 튀거나 묻는 사운드

	#endregion

	#region 플레이어 물리 및 애니메이션

	////////////////////////////////////////////////////////////////////////////////////
	// 이동
	////////////////////////////////////////////////////////////////////////////////////
	// 명령에 따른 Transform 변화 시 리턴 할 변수
	public Transform playerTransform;
	public Rigidbody playerRigidbody;
	// 이동 방향 변수
	private Vector3 movement;
	// 이동 속도 변수
	public float moveSpeed = 5.0f;
	// 회전 속도 변수  
	public float rotSpeed = 5.0f;
	// 이동 여부 변수
	public static bool isMove = false;

	////////////////////////////////////////////////////////////////////////////////////
	// 점프
	////////////////////////////////////////////////////////////////////////////////////
	// 점프 여부 변수
	public static bool isJumping = false;
	// 점프 속도 변수
	public float jumpSpeed = 7.0f;
	// 점프 누적 시간 
	private float jumptickTime;
	private bool enableJumpAnim;
	private float jumpDelayTime;
	// 중력 값
	//private float gravity = 20.0f;

	////////////////////////////////////////////////////////////////////////////////////
	// 대쉬
	////////////////////////////////////////////////////////////////////////////////////
	// 대쉬 속도
	public float dashSpeed = 7.0f;
	// 대쉬 가능 여부
	public static bool isDash = false;
	// 대쉬 누적 시간
	private float dashTickTime;
	// 대쉬 딜레이 시간
	private float dashDelayTime = 5.0f;
	// 대쉬 쿨타임
	private const float dashCoolTime = 5.0f;

	////////////////////////////////////////////////////////////////////////////////////
	// 공격
	////////////////////////////////////////////////////////////////////////////////////
	// 아이스크림 볼 프리팹
	public GameObject icecreamBall;
	// 아이스크림 볼 인덱스
	public int icecreamIndex;
	// 아이스크림 던질 좌표
	public Transform throwPos;
	// 다음 공격이 일어날 타임 스탬프
	private float nextFire;
	// 애니메이터 컨트롤러에 파라미터의 해시값 추출
	private readonly int hasThrow = Animator.StringToHash("Throw");
	private readonly int hasThrowAndAction = Animator.StringToHash("ThrowAndAction");

	// 공격 애니메이션 시간
	private float attackTime;
	// 공격 애니메이션 가능 여부
	[HideInInspector]
	public bool enableAttackAnim = false;

	////////////////////////////////////////////////////////////////////////////////////
	// 죽음
	////////////////////////////////////////////////////////////////////////////////////
	// 죽음 여부 변수
	public static bool isDie = false;
	public static bool isDieSec = false;

	////////////////////////////////////////////////////////////////////////////////////
	// 애니메이션
	////////////////////////////////////////////////////////////////////////////////////
	// 플레이어 애니메이터
	private Animator playerAnim;
	// 플레이어 애니메이터 컨트롤러에서 파라미터의 해시값 추출
	private readonly int hasMove = Animator.StringToHash("IsMove");
	private readonly int hasJump = Animator.StringToHash("Jump");
	private readonly int hasDash = Animator.StringToHash("Dash");
	private readonly int hasDie = Animator.StringToHash("isDie");
	private readonly int hasDieSec = Animator.StringToHash("isDie_Sec");
	// 점프 애니메이션 시간
	private float jumpAnimTime;
	// 대쉬 애니메이션 시간
	private float dashAnimTime;
	// 대쉬 애니메이션 가능 여부
	private bool enableDashAnim = true;

	// 죽음 애니메이션 시간
	private float DieAnimTime;
	private float DieSecAnimTime;

	#endregion

	#endregion

	private void Awake()
	{
		playerTransform = GetComponent<Transform>();
		playerRigidbody = GetComponent<Rigidbody>();
		playerAnim = GetComponent<Animator>();

		icecreamBall = (GameObject)Resources.Load("IcecreamBall");

		throwPos = transform.Find("ThrowPos").transform;
	}

	// 모든 클라이언트에서 동기화 된 값을 초기화
	// 이 로컬 클라이언트에 대한 카메라 및 입력을 초기화
	void Start()
	{
		// 애니메이션 시간 계산
		jumpAnimTime = AnimationTimeController.Instance[(int)AnimationTimeController.AnimType.JUMP];
		dashAnimTime = AnimationTimeController.Instance[(int)AnimationTimeController.AnimType.DASH];
		attackTime = AnimationTimeController.Instance[(int)AnimationTimeController.AnimType.THROW];
		DieAnimTime = AnimationTimeController.Instance[(int)AnimationTimeController.AnimType.DIE];
		DieSecAnimTime = AnimationTimeController.Instance[(int)AnimationTimeController.AnimType.DIE2];

		// 컴포넌트를 얻고 카메라 타겟 설정
		camFollow = Camera.main.GetComponent<CameraController>();
		camFollow.target = transform;

		Texture icecreamTexture = icecreamBall.GetComponentInChildren<Renderer>().material.mainTexture;
		icecreamIndex = SplatterManager.Instance.icecreamTextureList.FindIndex(tex => tex == icecreamTexture);
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

	private void FixedUpdate()
	{
		// 이동
		Move();

		// 공격
		if (Input.GetButton("Fire1"))
			Throw();

		// 점프 (재구현 필요)
		// 점프 체크
		if (Input.GetButtonDown("Jump"))
		{
			if (jumpAnimTime <= jumpDelayTime)
			{
				isJumping = true;     // 공격 가능
				enableJumpAnim = true;
				jumpDelayTime = 0.0f;  // 공격 대기시간 초기화
			}
		}
		else
		{
			jumpDelayTime += Time.deltaTime;
		}

		Jump();

		// 대쉬
		if (Input.GetKeyDown(KeyCode.F))
			Dash();

		// 죽음
		Die();
		Die_Sec();
	}

	#region 플레이어 물리 및 애니메이션 함수

	////////////////////////////////////////////////////////////////////////////////////
	// 이동 관련 함수
	////////////////////////////////////////////////////////////////////////////////////
	public void Move()
	{
		// 이동 체크
		float h = Input.GetAxis("Horizontal");
		float v = Input.GetAxis("Vertical");

		isMove = h != 0.0f || v != 0.0f;

		if (isMove == true)
		{
			// 움직일 축 설정
			movement.Set(h, 0.0f, v);

			// 일러스트레이션을 위해 입력을 모바일 컨트롤에 복제
			//#if UNITY_EDITOR
			//	GameManager.GetInstance().ui.control.position = new Vector3(h, v, 0);
			//#endif

			// 방향벡터 정규화
			movement = movement.normalized;

			// 회전
			Turning();

			// 이동
			Running();
		}

		// 이동 애니메이션
		MoveAnim();

	}

	// 이동 함수
	private void Running()
	{
		// 이동
		// 진행방향으로 프레임 시간 * 속도 크기로 진행 
		movement = movement * moveSpeed * Time.fixedDeltaTime;

		// 물리를 통한 이동
		playerRigidbody.MovePosition(playerTransform.position + movement);
	}

	// 회전 함수
	private void Turning()
	{
		// 회전
		Quaternion Rot = Quaternion.LookRotation(movement);
		playerRigidbody.rotation = Quaternion.Slerp(playerTransform.rotation, Rot, rotSpeed * Time.fixedDeltaTime);
	}

	// 이동 애니메이션 함수
	public void MoveAnim()
	{
		playerAnim.SetBool(hasMove, isMove);
	}

	////////////////////////////////////////////////////////////////////////////////////
	// 점프 함수
	////////////////////////////////////////////////////////////////////////////////////
	public void Jump()
	{
		if (isJumping == false)
			return;

		// 시간이 안맞으면 애니메이션 프레임으로 처리해야함
		if (jumptickTime > jumpAnimTime * 0.2f)
		{
			playerRigidbody.AddForce(Vector3.up * jumpSpeed, ForceMode.Impulse);
			jumptickTime = 0.0f;
			isJumping = false;
		}
		else
		{
			jumptickTime += Time.deltaTime;
		}

		// 점프 애니메이션
		JumpAnim();
	}

	private bool IsGrounded()
	{
		float distToGround = 0.0f;
		return Physics.Raycast(playerTransform.position, -Vector3.up, distToGround + 0.1f);
	}

	// 점프애니메이션 함수
	private void JumpAnim()
	{
		if (enableJumpAnim == true)
		{
			playerAnim.SetTrigger(hasJump);
			enableJumpAnim = false;
		}
	}

	////////////////////////////////////////////////////////////////////////////////////
	// 대쉬 함수
	////////////////////////////////////////////////////////////////////////////////////
	public void Dash()
	{
		// 대쉬 스킬 쿨타임(5초)과 
		if (dashDelayTime >= dashCoolTime)
		{
			isDash = true;
			enableDashAnim = true;
			dashDelayTime = 0.0f;
		}

		if (isDash == false)
		{
			dashDelayTime += Time.deltaTime;
			return;
		}

		float h = Input.GetAxis("Horizontal");

		// 대쉬 애니메이션 시간이 대쉬 누적시간보다 작을 때 까지 대쉬 프로세스 실행
		if (dashAnimTime <= dashTickTime)
		{
			dashTickTime = 0.0f;
			isDash = false;
		}
		else
		{
			// 대쉬 프로세스 진행 중 애니메이션 한번만 실행
			if (enableDashAnim == true)
			{
				// 대쉬 애니메이션 실행
				DashAnim();
				enableDashAnim = false;
			}
			// 대쉬 애니메이션 실행하는 동안 계속 힘을 가한다.
			playerRigidbody.AddForce(Vector3.right * h * dashSpeed, ForceMode.Impulse);
			// 대쉬 시간 누적
			dashTickTime += Time.deltaTime;
		}
	}

	private void DashAnim()
	{
		playerAnim.SetTrigger(hasDash);
	}

	////////////////////////////////////////////////////////////////////////////////////
	// 공격 함수
	////////////////////////////////////////////////////////////////////////////////////
	public void Throw()
	{
		if (Time.time > nextFire)
		{
			nextFire = Time.time + 0.75f;

			//현재 클라이언트 위치 및 캐릭터 회전을 함께 전송하여 샷 위치 동기화
			//또한 추가 대역폭을 절약하기 위해 짧은 배열(x,z - Skip y만 해당)으로 전송
			short[] pos = new short[] { (short)throwPos.position.x, (short)throwPos.position.z };

			Vector3 throwCenter = Vector3.Lerp(throwPos.position, new Vector3(pos[0], throwPos.position.y, pos[1]), 0.6f);
			Quaternion syncedRot = playerTransform.rotation;

			// 풀링을 사용하여 아이스크림 볼 스폰
			GameObject obj = PoolManager.Spawn(icecreamBall, throwCenter, transform.rotation);
			obj.GetComponent<IcecreamBall>().owner = gameObject;

			enableAttackAnim = true; // 애니메이션 가능
		}
		else
		{
			enableAttackAnim = false;
		}
	}

	private void Aim()
	{
		Ray ray = Camera.main.ScreenPointToRay(throwPos.position);
		RaycastHit rayHit;
		//float rayLength = 500.0f;
		int floorMask = LayerMask.GetMask("Wall");

		if (Physics.Raycast(throwPos.position, throwPos.TransformDirection(Vector3.forward), out rayHit, Mathf.Infinity, floorMask))
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
		bool isAction = (isJumping == true || isMove == true) && isDash == false;
		if (isAction == true)
		{
			playerAnim.SetTrigger(hasThrowAndAction);
		}
		else
		{
			playerAnim.SetTrigger(hasThrow);
		}
	}

	////////////////////////////////////////////////////////////////////////////////////
	// 죽음 함수
	////////////////////////////////////////////////////////////////////////////////////
	public void Die()
	{
		//캐릭터의 체력이 0 일때로 수정필요!!

		if (isDie == false)
			return;

		DieAnim();
	}

	private void DieAnim()
	{
		playerAnim.SetTrigger(hasDie);
	}

	public void Die_Sec()
	{
		if (isDieSec == false)
			return;

		DieSecAnim();
	}

	private void DieSecAnim()
	{
		playerAnim.SetTrigger(hasDieSec);
	}

	#endregion
}
