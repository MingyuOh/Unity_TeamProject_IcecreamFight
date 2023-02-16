using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// 충돌 / 피해 로직을 플레이어 던지기의 발사체 스크립트
public class IcecreamBall : Photon.MonoBehaviour
{
	private readonly Vector3[] ray = new Vector3[14]{
		new Vector3(0, 0, 1), new Vector3(0, 1, 0), new Vector3(1, 0, 0),
		new Vector3(0, 0, -1), new Vector3(0, -1, 0), new Vector3(-1, 0, 0),
		new Vector3(1, 1, 1), new Vector3(-1, 1, 1), new Vector3(1, -1, 1),
		new Vector3(1, 1, -1), new Vector3(-1, -1, 1), new Vector3(-1, 1, -1),
		new Vector3(1, -1, -1), new Vector3(-1, -1, -1) };

	// 발사체의 이동 속도
	public float speed = 100;

	// 플레이어가 상대방을 맞췄을 때의 피해량
	public int damage = 10;

	// 아무 것도 부딪히지 않을 때 자동적으로 해제 될 때까지 지연
	public float despawnDelay = 1.0f;

	// 플레이어가 다른 플레이어에게 맞았을 때 사운드
	public AudioClip hitClip;

	// 플레이어가 맞았을 때 발생되는 이펙트
	public GameObject hitFX;

	// 발사체가 퍼졌을 때 실행 될 사운드
	public AudioClip splatterClip;

	// 발사체가 사라지면 생성되는 이펙트
	public GameObject splatterFX;

	// 참조할 리지드 바디 컴포넌트
	private Rigidbody rb;
	// 참조할 구 바운딩박스 컴포넌트
	private SphereCollider sphereCol;

	// 발사체를 발생시킨 플레이어
	[HideInInspector]
	public GameObject owner;

	private void Awake()
	{
		rb = GetComponent<Rigidbody>();
		sphereCol = GetComponent<SphereCollider>();
	}

	// 초기 이동 속도 설정
	// 호스트에서 자동적으로 despawn coroutine 추가
	void OnSpawn()
	{
		rb.velocity = speed * transform.forward;
		PoolManager.Despawn(gameObject, despawnDelay);
	}

	// 충돌시 무엇이 맞았는지 확인
	private void OnTriggerEnter(Collider col)
	{
		// 충돌된 게임오브젝트를 캐시
		GameObject obj = col.gameObject;

		// 충돌 한 게임 개체에서 플레이어 컴포넌트 요소를 가져옴
		Player player = obj.GetComponent<Player>();

		// 우리는 실제로 플레이어를 때림
		// 추가 검사 수행
		if(player != null)
		{
			// "내 자신"과 "같은 팀" 무시
			if (player.gameObject == owner || player.gameObject == null)
				return;
			else if (player.GetView().GetTeam() == owner.GetComponent<Player>().GetView().GetTeam())
				return;

			//// 맞췄을 시, 사운드와 이펙트 생성
			//if (hitFX)
			//	PoolManager.Spawn(hitFX, transform.position, Quaternion.identity);
			//if (hitClip)
			//	AudioManager.Play3D(hitClip, transform.position);

			// 맞은 플레이어에서, 이 아이스크림 볼의 주인을 살인자 플레이로 설정
			// 이 소유자가 실제로 플레이어를 죽였을 수도 있지만 이 검사는 플레이어 스크립트에서 수행
			player.killedBy = owner;
		}

		//var layer = col.gameObject.layer;
		//if (layer == LayerMask.NameToLayer("Wall"))
		//{
		//	OnHitWall();
		//}

		var layer = col.gameObject.layer;
		if (layer == LayerMask.NameToLayer("WALL"))
		{
			RaycastHit raycastHit;
			if (Physics.Raycast(transform.position, owner.transform.forward, out raycastHit))
			{
				SplatterReceiver receiver = raycastHit.collider.gameObject.GetComponent<SplatterReceiver>();
				if (receiver != null)
				{
					// SplatTestPlayer를 Player로 변경해야함
					// IcecreamIndex가 Player에는 현재 존재하지 않음
					SplatTestPlayer ownerPlayer = owner.GetComponent<SplatTestPlayer>();
					receiver.OnDrawSplatter(raycastHit.textureCoord2, ownerPlayer.icecreamIndex, SplatterManager.Instance.GetSplatTextureInfos());
				}
			}
		}

		// 게임오브젝트 despawn
		PoolManager.Despawn(gameObject);

		// 앞의 코드는 클라이언트에 전혀 동기화되지 않습니다. 
		// 클라이언트에 필요한 것은 모두 아이스크림 볼의 초기 위치와 방향은 끝에서 똑같은 동작을 계산
		// 이 시점에서 중요한 게임 요소는 서버에서만 계속됨
		if (PhotonNetwork.isMasterClient == false)
			return;
		
		// 충돌한 플레이어에게 데미지
		if (player)
			player.TakeDamage(this);
	}

	// despawn 효과 설정 및 변수 재설정
	void OnDespawn()
	{
		//// despawn에 파티클과 사운드를 생성
		//if (splatterFX)
		//	PoolManager.Spawn(splatterFX, transform.position, transform.rotation);
		//if (splatterClip)
		//	AudioManager.Play3D(splatterClip, transform.position);

		// 수정된 변수를 초기 상태로 재설정
		rb.velocity = Vector3.zero;
		rb.angularVelocity = Vector3.zero;
	}

	private void OnHitWall()
	{
		int teamIndex = owner.GetPhotonView().GetTeam();
		NetworkSplatterEvn.instance.Splatter(transform.position, teamIndex, 0.5f);
	}
}
