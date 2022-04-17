using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Communication;
using Photon;

public class PlayerPhysics : PunBehaviour, IActorPhysics
{
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
	// 아이스크림 던질 좌표
	public Transform throwPos;
	// 던지기 지연 시간
	private float attackDelayTime;
	// 던지기 가능 여부
	private bool enableAttack;
	// 애니메이터 컨트롤러에 파라미터의 해시값 추출
	private readonly int hasThrow = Animator.StringToHash("Throw");
	private readonly int hasThrowAndAction = Animator.StringToHash("ThrowAndAction");

	// 공격 애니메이션 시간
	private float attackTime;
	// 던지기 애니메이션 가능 여부
	private bool enableAttackAnim = false;

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

	// Use this for initialization
	void Awake () {
		playerTransform = GetComponent<Transform>();
		playerRigidbody = GetComponent<Rigidbody>();
		playerAnim = GetComponent<Animator>();
	}

	private void Start()
	{
		jumpAnimTime = AnimationTimeController.Instance[(int)AnimationTimeController.AnimType.JUMP];
		dashAnimTime = AnimationTimeController.Instance[(int)AnimationTimeController.AnimType.DASH];
		attackTime = AnimationTimeController.Instance[(int)AnimationTimeController.AnimType.THROW];
		DieAnimTime = AnimationTimeController.Instance[(int)AnimationTimeController.AnimType.DIE];
        DieSecAnimTime = AnimationTimeController.Instance[(int)AnimationTimeController.AnimType.DIE2];
    }

	//////////////////////////////////////////////////////////////////////
	// 수정 필요 IActorPhysics 정의하여 Jump와 Dash안에 이 작업을 다 넣어야함
	// 플레이어 핸들러(인풋) 클래스 필요
	//////////////////////////////////////////////////////////////////////
	private void Update()
	{
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

		// 대쉬 체크
		if (Input.GetKeyDown(KeyCode.F))
		{
			// 대쉬 스킬 쿨타임(5초)과 
			if (dashDelayTime >= dashCoolTime)
			{
				isDash = true;
				enableDashAnim = true;
				dashDelayTime = 0.0f;
			}
		}

		// 공격
		Throw();

		////////////////////////////////죽음 애니메이션 테스트용//////////////////////////////
		//추후 캐릭터 체력과 관련해서 수정
		if (Input.GetKeyDown(KeyCode.F1))
        {
            isDie = true;
        }

        if (Input.GetKeyDown(KeyCode.F2))
        {
            isDieSec = true;
        }
        //////////////////////////////////////////////////////////////////////////////////////


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
		if(photonView.isMine == false)
		{
			return;
		}

		// 이동
		Move();
		// 점프
		Jump();
		// 대쉬
		Dash();
        // 죽음
        Die();
        Die_Sec();
    }

	// 현재 오브젝트가 자신의 것인지 찾아주는 헬퍼 메소드
	public PhotonView GetView()
	{
		return this.photonView;
	}

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

			// 방향벡터 정규화
			movement = movement.normalized;
            // 회전
            Turning();

			// 이동
			Running();
		}

        /////////////////////////////////조이스틱 테스트/////////////////////////////////
        //if (JoyStick.Instance.DragFlag)
        //{
        //    //조이스틱클래스로 부터 넘겨받은 오일러각을
        //    //방향벡터로 전환
        //    Vector3 dir = new Vector3(0, 0, 1);
        //    dir = Quaternion.Euler(JoyStick.Instance.Player_eulerAngles) * dir;

        //    //정규화화여 movement에 대입

        //    movement = dir.normalized;
        //    ///////////////////////////////////////////

        //    // 회전
        //    Turning();

        //    // 이동
        //    Running();

        //    //isMove 변수 사용하여 애니메이션까지 할수있게..

        //}
        ///////////////////////////////////////////////////////////////////////////////////

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
	private void MoveAnim()
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
		if(jumptickTime > jumpAnimTime * 0.2f)
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
		if (isDash == false)
		{
			dashDelayTime += Time.deltaTime;
			return;
		}

		float h = Input.GetAxis("Horizontal");

		// 대쉬 애니메이션 시간이 대쉬 누적시간보다 작을 때 까지 대쉬 프로세스 실행
		if(dashAnimTime <= dashTickTime)
		{
			dashTickTime = 0.0f;
			isDash = false;
		}
		else
		{
			// 대쉬 프로세스 진행 중 애니메이션 한번만 실행
			if(enableDashAnim == true)
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
		// 공격 가능 여부
		if (Input.GetMouseButtonDown(0))
		{
			// 마우스 버튼 클릭시 공격(Dash 행동 시 공격 불가능)
			if (enableAttack == true && PlayerPhysics.isDash == false)
			{
				//현재 클라이언트 위치 및 캐릭터 회전을 함께 전송하여 샷 위치 동기화
				//또한 추가 대역폭을 절약하기 위해 짧은 배열(x,z - Skip y만 해당)으로 전송
				short[] pos = new short[] { (short)throwPos.position.x, (short)throwPos.position.z };
				this.photonView.RPC("CmdThrow", PhotonTargets.AllViaServer, pos, playerTransform.rotation);
				// Icecream Ball 프리팹을 동적으로 생성
				// 오브젝트 풀 사용해야 함
				Instantiate(icecreamBall, throwPos.position, throwPos.rotation);

				enableAttackAnim = true; // 애니메이션 가능
				enableAttack = false;    // 공격 불가능
			}
		}
		else
		{
			// 공격 대기시간
			attackDelayTime += Time.deltaTime;
			if (attackTime <= attackDelayTime)
			{
				enableAttack = true;     // 공격 가능
				attackDelayTime = 0.0f;  // 공격 대기시간 초기화
			}
		}
	}

	// 먼저 서버에서 호출되지만 모든 클라이언트로 전달
	[PunRPC]
	protected void CmdThrow(short[] position, short angle)
	{
		int currentIcecreamBall = GetView().GetIcecreamBall();

		// 전송된 샷 위치와 현재 서버 위치 사이의 중심 계산(요인 0.6f = 40% 클라이언트, 60% 서버)
		// 이 작업은 네트워크 지연을 보상하고 클라이언트/서버 위치 간에 원활하게 수행하기 위함
		Vector3 shotCenter = Vector3.Lerp(throwPos.position, new Vector3(position[0], throwPos.position.y, position[1]), 0.6f);
		Quaternion syncedRot = playerTransform.rotation = Quaternion.Euler(0.0f, angle, 0.0f);

		// 오브젝트 풀로 생성해야하지만 아직 미구현이므로 instance
		
	}

	private void Aim()
	{
		Ray ray = Camera.main.ScreenPointToRay(throwPos.position);
		RaycastHit rayHit;
		//float rayLength = 500.0f;
		int floorMask = LayerMask.GetMask("STATIC_OBJECT");

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
		bool isAction = (PlayerPhysics.isJumping == true || PlayerPhysics.isMove == true) && PlayerPhysics.isDash == false;
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
}
