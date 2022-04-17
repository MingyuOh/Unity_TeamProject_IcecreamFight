using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Communication;
using Photon;

// 인증방식으로 접근 방식으로 서버 및 클라이언트 로직을 모두 포함
// 플레이어 행동
public class Player : PunBehaviour, IActorPhysics
{
	#region 변수

	#region 플레이어 정보 변수
	public Text label;              // 플레이어 이름
	public Slider healthSlider;     // 플레이어 체력 바

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
	public AudioClip dieClip;		// 플레이어 죽는 사운드
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

		// 호스트만 초기화 가능
		if (PhotonNetwork.isMasterClient == false)
			return;

		// 게임에 참가했을 때 캐릭터 현재 체력 저장
		GetView().SetHealth(maxHealth);
	}

	// 모든 클라이언트에서 동기화 된 값을 초기화
	// 이 로컬 클라이언트에 대한 카메라 및 입력을 초기화
   void Start ()
	{
		// 애니메이션 시간 계산
		jumpAnimTime = AnimationTimeController.Instance[(int)AnimationTimeController.AnimType.JUMP];
		dashAnimTime = AnimationTimeController.Instance[(int)AnimationTimeController.AnimType.DASH];
		attackTime = AnimationTimeController.Instance[(int)AnimationTimeController.AnimType.THROW];
		DieAnimTime = AnimationTimeController.Instance[(int)AnimationTimeController.AnimType.DIE];
		DieSecAnimTime = AnimationTimeController.Instance[(int)AnimationTimeController.AnimType.DIE2];

		// 해당 팀을 구하고 팀 색상으로 특정 객체(이름or팀 아이콘) 렌더러를 채색
		Team team = GameManager.GetInstance().teams[GetView().GetTeam()];

		// 이름 라벨에 저장
		//label.text = UserAuth.Instance.GetNickName();

		// 업데이트를 위해 수동으로 Hook을 호출
		OnHealthChange(GetView().GetHealth());

		// 자신의 클라이언트에서만 호출
		if (photonView.isMine == false)
			return;

		// 로컬 플레이어에 대한 전역 참조를 설정
		GameManager.GetInstance().localPlayer = this;

		// 컴포넌트를 얻고 카메라 타겟 설정
		camFollow = Camera.main.GetComponent<CameraController>();
		camFollow.target = transform;

		//모바일 장치의 입력 컨트롤 초기화
		//[0]=동작용(왼쪽 조이스틱), [1]=공격용(오른쪽 조이스틱)
#if !UNITY_STANDALONE && !UNITY_WEBGL
            GameManager.GetInstance().ui.controls[0].onDrag += Move;
            GameManager.GetInstance().ui.controls[0].onDragEnd += MoveEnd;

            GameManager.GetInstance().ui.controls[1].onDragBegin += ShootBegin;
            GameManager.GetInstance().ui.controls[1].onDrag += RotateTurret;
            GameManager.GetInstance().ui.controls[1].onDrag += Shoot;
#endif
	}

	private void Update()
	{ 
		if(PhotonNetwork.isMasterClient)
		{
			if(NetworkSplatterEvn.instance != null)
			{
				mySplatterState = NetworkSplatterEvn.instance.GetSplatterState(0, transform.position);
			}
		}
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
		// 디버깅 마치면 이 주석 해제
		if (photonView.isMine == false)
			return;

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

	#region 네트워크 관련

	//이 메서드는 플레이어 속성이 네트워크에서 변경 될 때마다 호출
	public override void OnPhotonPlayerPropertiesChanged(object[] playerAndUpdatedProps)
	{
		//이 플레이어의 속성 변경에만 반응
		PhotonPlayer player = playerAndUpdatedProps[0] as PhotonPlayer;
		if (player != photonView.owner)
			return;
	
		// 시각화를 위해 언제든지 변경 될 수있는 값을 업데이트
		OnHealthChange(GetView().GetHealth());
	}

	// 네트워크 지연 갱신을 위한 함수
	// Rigidbody를 사용하므로 여기서 위치 동기화를 해주어야함
	// Photon가이드에 지연 보상 가이드
	// Rigidbody의 위치, 회전 및 속도를 전송
	// 아직 미구현이므로 반드시 가이드 숙지
	//이 메서드는 초당 여러 번 호출(적어도 10 회 이상)
	void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if(stream.isWriting)
		{
			// 여기서 캐릭터 회전 각도를 다른 클라이언트에 전송
			stream.SendNext(playerRigidbody.rotation);
		}
		else
		{
			// 여기에서는 다른 사람들로부터 캐릭터 회전 각을 받고 적용
			Quaternion rot = (Quaternion)stream.ReceiveNext();
			OnPlayerRotation(rot.y);
		}
	}

	#endregion

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
		if(Time.time > nextFire)
		{
			nextFire = Time.time + 0.75f;

			//현재 클라이언트 위치 및 캐릭터 회전을 함께 전송하여 샷 위치 동기화
			//또한 추가 대역폭을 절약하기 위해 짧은 배열(x,z - Skip y만 해당)으로 전송
			short[] pos = new short[] { (short)throwPos.position.x, (short)throwPos.position.z };
			this.photonView.RPC("CmdThrow", PhotonTargets.AllViaServer, pos, playerRotation);

			enableAttackAnim = true; // 애니메이션 가능
		}
		else
		{
			enableAttackAnim = false;
		}
	}

	// 먼저 서버에서 호출되지만 모든 클라이언트로 전달
	[PunRPC]
	protected void CmdThrow(short[] position, short angle)
	{
		// 전송된 샷 위치와 현재 서버 위치 사이의 중심 계산(요인 0.6f = 40% 클라이언트, 60% 서버)
		// 이 작업은 네트워크 지연을 보상하고 클라이언트/서버 위치 간에 원활하게 수행하기 위함
		Vector3 throwCenter = Vector3.Lerp(throwPos.position, new Vector3(position[0], throwPos.position.y, position[1]), 0.6f);
		Quaternion syncedRot = playerTransform.rotation;// = Quaternion.Euler(0.0f, angle, 0.0f);

		// 풀링을 사용하여 아이스크림 볼 스폰
		GameObject obj = PoolManager.Spawn(icecreamBall, throwCenter, transform.rotation);
		obj.GetComponent<IcecreamBall>().owner = gameObject;
		
		// 모든 클라이언트에게 이펙트를 스폰하여 전달
		if (throwFX || throwClip)
			RpcOnThrow();
	}

	// 아이스크림 생성 후 모든 클라이언트에 호출
	// 설정된 경우 효과 또는 소리를 로컬로 재생
	protected void RpcOnThrow()
	{
		//if (throwFX)
		//	PoolManager.Spawn(throwFX, throwPos.position, Quaternion.identity);
		//if (throwClip)
		//	AudioManager.Play3D(throwClip, throwPos.position, 0.1f);
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

	// 현재 오브젝트가 자신의 것인지 찾아주는 헬퍼 메소드
	public PhotonView GetView()
	{
		return this.photonView;
	}

	// 로컬에서 상태를 업데이트하기위한 Hook
	// (플레이어 속성을 통해 실제 값이 업데이트 됨)
	protected void OnHealthChange(int value)
	{
		//healthSlider.value = (float)value / maxHealth;
	}

	void OnPlayerRotation(float rot)
	{
		playerTransform.rotation = Quaternion.Euler(0, rot, 0);
	}

	// 서버만 실행: 플레이어에서 취해야 할 손상 계산
	// 점수 증가 및 사망 시 리스폰 워크플로우를 트리거
	public void TakeDamage(IcecreamBall icecreamBall)
	{
		// 일시적으로 변수들을 네트워크에 저장
		int health = GetView().GetHealth();

		// 체력에서 데미지를 뺌
		// 현재는 로컬에서만, 나중에 업데이트를 한 번만 수행
		health -= icecreamBall.damage;

		// 아이스크림 볼로 플레이어를 죽임
		if(health <= 0)
		{
			// 게임이 이미 끝났으니 아무것도 하지 마라
			if (GameManager.GetInstance().IsGameOver())
				return;

			// 그 팀의 점수를 올리고 킬러를 얻음
			Player other = icecreamBall.owner.GetComponent<Player>();
			int otherTeam = other.GetView().GetTeam();
			PhotonNetwork.room.AddScore(otherTeam);

			// 최고점에 도달하였다면
			if(GameManager.GetInstance().IsGameOver())
			{
				// 플레이어들이 참여하지 못하게 방을 닫는다
				//PhotonNetwork.room.IsOpen = false;
				// 모든 클라이언트들에게 우승팀을 전송
				this.photonView.RPC("RpcGameOver", PhotonTargets.All, (byte)otherTeam);
				return;
			}

			// 게임이 아직 끝나지 않음
			// 런타임 값을 재설정 또 모든 클라이언트에게 이 플레이어를 폐기하라고 전달
			GetView().SetHealth(maxHealth);
			GetView().SetIcecreamBall(0);
			this.photonView.RPC("RpcRespawn", PhotonTargets.All);
		}
		else
		{
			// 죽지않음, 새로운 체력값을 설정
			GetView().SetHealth(health);
		}
	}

	// 플레이어 사망 과 리스폰 둘다 모든 클라이언트에게 전달해야함
	// 리스폰 시에만 클라이언트가 요청을 보낸다는 점만 다름
	[PunRPC]
	protected virtual void RpcRespawn()
	{
		// 플레이어 게임 오브젝트에 대한 가시성 설정/해제
		gameObject.SetActive(!gameObject.activeInHierarchy);
		bool isActive = gameObject.activeInHierarchy;

		// 플레이어가 죽었을 때
		if(isActive == false)
		{
			// 현재 사용자가 Kill을 담당했는지 여부를 감지
			// 예, 그건 내 Kill입니다 : 로컬 킬 카운터를 늘리고 EveryPlay를 통해 미리보기를 가져옴
			if(killedBy == GameManager.GetInstance().localPlayer.gameObject)
			{
				GameManager.GetInstance().ui.killCounter[0].text = (int.Parse(GameManager.GetInstance().ui.killCounter[0].text) + 1).ToString();
				//GameManager.GetInstance().ui.killCounter[0].GetComponent<Animator>().Play("Animation");
				//UnityEveryplayManager.TakeThumbnail();
			}

			// 이 부분 커스텀하여 적용 필요
			//if (explosionFX)
			//{
			//	//spawn death particles locally using pooling and colorize them in the player's team color
			//	GameObject particle = PoolManager.Spawn(explosionFX, transform.position, transform.rotation);
			//	ParticleColor pColor = particle.GetComponent<ParticleColor>();

			//	if (pColor)
			//		pColor.SetColor(GameManager.GetInstance().teams[GetView().GetTeam()].material.color);
			//}

			////play sound clip on player death
			//if (explosionClip) 
			//  AudioManager.Play3D(explosionClip, transform.position);
		}

		// 더 많은 변경 사항은 로컬 클라이언트에만 적용
		if (photonView.isMine == false)
		{
			return;
		}

		if(isActive == true)
		{
			ResetPosition();
		}
		else
		{
			// 로컬 플레이어가 살해되어 킬러를 추적하도록 카메라를 설정
			camFollow.target = killedBy.transform;
			// 입력 컨트롤 및 기타 HUD 요소 숨기기
			camFollow.HideMask(true);
			// 리스폰 창을 표시(로컬 플레이어 전용)
			GameManager.GetInstance().DisplayDeath();
		}
	}

	// 서버와 다른 모든 사용자에게 클라이언트가 다시 시작할 준비가 되었다고 명령
	// 리스폰 지연이 종료되거나 비디오 광고가 지켜진 경우
	public void CmdRespawn()
	{
		this.photonView.RPC("RpcRespawn", PhotonTargets.AllViaServer);
	}

	// 팀 영역의 위치를 변경하고 카메라 및 입력 변수를 재설정
	// 로컬 플레이어에 대해서만 호출
	public void ResetPosition()
	{
		// 다시 로컬플레이를 따라가도록 적용
		camFollow.target = transform;
		camFollow.HideMask(false);

		// 팀 영역을 가져와 다시 배치
		transform.position = GameManager.GetInstance().GetSpawnPosition(GetView().GetTeam());

		// 입력으로 수정된 힘 재설정
		playerRigidbody.velocity = Vector3.zero;
		playerRigidbody.angularVelocity = Vector3.zero;
		playerTransform.rotation = Quaternion.identity;

		// 입력 재설정
		GameManager.GetInstance().ui.control.OnEndDrag(null);
	}

	// 게임 끝난 후 이긴 팀에 대해 모든 클라이언트에게 호출
	[PunRPC]
	protected void RpcGameOver(byte teamIndex)
	{
		// 윈도우에 "게임 오버" 디스플레이
		GameManager.GetInstance().DisplayGameOver(teamIndex);
	}

	/// <summary>
	/// PhotonNetwork Instantiate가 호출되었을 때 실행되는 콜백 함수
	/// (이 함수가 실행되려면 네트워크 인스턴스화 된 오브젝트 안에서 호출)
	/// </summary>
	/// <param name="info"></param>
	public override void OnPhotonInstantiate(PhotonMessageInfo info)
	{
		object[] instantiateDatas = this.photonView.instantiationData;

		if (instantiateDatas.Length == 3)
		{
			// 이름 변경
			this.gameObject.name = instantiateDatas[0].ToString();
			// 캐릭터 이름
			_characterTypeName = instantiateDatas[1].ToString();
			// 스킨 이름
			_characterSkinName = instantiateDatas[2].ToString();
		}

		Renderer playerRenderer;                     // 플레이어 렌더러
		Material playerMaterial;                     // 플레이어 머테리얼
		// GameObject playerHeadAccessory;           // 플레이어 머리 악세사리
		// GameObject playerChestAccessory;          // 플레이어 가슴 악세사리

		// 렌더러 등록
		Transform meshObject = this.photonView.gameObject.transform.Find("Mesh").GetComponent<Transform>();
		playerRenderer = meshObject.Find(characterTypeName).GetComponent<Renderer>();

		//// 플레이어 머리 악세사리 오브젝트
		//playerHeadAccessory = playerPrefab.transform.Find("Head_Accessories_locator").GetComponent<GameObject>();

		//// 플레이어 가슴 악세사리 오브젝트
		//playerHeadAccessory = playerPrefab.transform.Find("Accessories_locator").GetComponent<GameObject>();

		playerMaterial = Resources.Load<Material>("Materials/" + _characterTypeName + "/" + _characterSkinName);

		playerRenderer.material = playerMaterial;

	}
}
