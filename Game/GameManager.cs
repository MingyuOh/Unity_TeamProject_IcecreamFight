using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;
#if UNITY_ADS
using UnityEngine.Advertisements;
#endif

//////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////GameManger///////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////////////
// 게임 흐름을 관리하고 게임 중에 네트워크 논리에 대한 높은 수준의 액세스를 제공
// 팀 채우기, 점수 및 게임 종료와 같은 기능을 관리하지만 비디오 광고 결과도 포함
//////////////////////////////////////////////////////////////////////////////////////////

public class GameManager : PunBehaviour
{
	// 게임매니저 싱글톤
	private static GameManager instance;

	// 게임 통계를 표시하는 UI 스크립트에 대한 참조
	public UIGame ui;
		
	//클라이언트에 의해 생성 된 로컬 플레이어 인스턴스
	[HideInInspector]
	public Player localPlayer;

	//추가 속성을 가진 팀 플레이의 정의
	public Team[] teams;

	// 게임을 끝내기 위해 도해야 할 킬 수
	public int maxScore = 30;

	// 플레이어가 사망 한 후 다시 리스폰하기까지의 지연 시간(초).
	public int respawnTime = 3;

	// 변수 초기화
	private void Awake()
	{
		instance = this;
		// 유니티 광고가 사용 설정된 경우 결과 콜백을 연결
		#if UNITY_ADS
                UnityAdsManager.adResultEvent += HandleAdResult;
		#endif
	}

	// 게임매니저 스크립트의 인스턴스를 참조하여 반환
	public static GameManager GetInstance()
	{
		return instance;
	}

	// 이 클라이언트가 방장인지 여부를 확인
	public static bool isMaster()
	{
		return PhotonNetwork.isMasterClient;
	}

	// 플레이어가 배정되어야 할 다음 팀 인덱스를 반환
	// 이 함수 로비로 가야 함
	public int GetTeamFill()
	{
		// 변수 초기화
		int[] size = PhotonNetwork.room.GetSize();
		int teamNo = 0;
		
		int min = size[0];
		// 가장 적은 인원수를 보유한 팀에 채우기위해 찾기를 반복
		for(int iCnt = 0; iCnt < teams.Length; iCnt++)
		{
			// 채우기가 이전 값보다 낮은 경우
			// 다음 반복을 위해 새로운 채우기 및 팀 저장
			if(size[iCnt] < min)
			{
				min = size[iCnt];
				teamNo = iCnt;
			}
		}

		// 가장 적은 팀의 인덱스를 반환
		return teamNo;
	}

	// 팀의 스폰 지역 내 임의의 스폰 위치를 반환
	public Vector3 GetSpawnPosition(int teamIndex)
	{
		// 변수 초기화
		Vector3 pos = teams[teamIndex].spawn.position;
		BoxCollider col = teams[teamIndex].spawn.GetComponent<BoxCollider>();

		if (col != null)
		{
			// 박스 콜라이더 범위 안의 위치를 찾고, 처음 고정 y 위치 설정
			// 카운터 범위를 벗어난 경우 새 위치를 계산하는 빈도를 결정
			pos.y = col.transform.position.y;
			int counter = 10;

			// 충돌 범위 내에서 임의의 위치를 얻고 범위 내에 있지 않으면 다른 반복을 수행
			do
			{
				pos.x = UnityEngine.Random.Range(col.bounds.min.x, col.bounds.max.x);
				pos.z = UnityEngine.Random.Range(col.bounds.min.z, col.bounds.max.z);
				counter--;
			} while (col.bounds.Contains(pos) == false && counter > 0);
		}
		return pos;
	}

	// 광고보기가 완료 될 때 수행 할 작업 구현
#if UNITY_ADS
    void HandleAdResult(ShowResult result)
    {
        switch (result)
        {
            //in case the player successfully watched an ad,
            //it sends a request for it be respawned
            case ShowResult.Finished:
            case ShowResult.Skipped:
                localPlayer.CmdRespawn();
                break;
                
            //in case the ad can't be shown, just handle it
            //like we wouldn't have tried showing a video ad
            //with the regular death countdown (force ad skip)
            case ShowResult.Failed:
                DisplayDeath(true);
                break;
        }
    }
#endif

	// 팀이 최대 게임 점수에 도달했는지 여부를 반환
	public bool IsGameOver()
	{
		bool isOver = false;
		int[] score = PhotonNetwork.room.GetScore();

		// 최고 점수를 가진 팀을 찾음
		for (int iCnt = 0; iCnt < teams.Length; iCnt++)
		{
			// max Score와 같거나 더 큰 점수이면 게임 종료
			if (score[iCnt] >= maxScore)
			{
				isOver = true;
				break;
			}
		}

		// 결과 리턴
		return isOver;
	}

	// 이 플레이어에 대해서만 : 살인자가 죽임을 당했다는 사망 텍스트를 설정
	// Unity Ads를 사용하는 경우 재발송 지연 중에 광고를 표시하려고 시도
	// 'skipAd'매개 변수를 사용하면 광고를 건너 뛸 수 있음
	public void DisplayDeath(bool skipAd = false)
	{
		// 나를 죽인 플레이어의 컴포넌트를 가지고온다.
		Player other = localPlayer.killedBy.GetComponent<Player>();
		// 게임 방에 킬 수를 증가 시킨다.
		// 유아이 부분
		ui.killCounter[1].text = (int.Parse(ui.killCounter[1].text) + 1).ToString();
		//ui.killCounter[1].GetComponent<Animator>().Play("Animation");


		// 사망 텍스트를 설정 및 즉시 부활 지연 대기 시작
		ui.SetDeathText(other.GetView().GetName(), teams[other.GetView().GetTeam()]);
		StartCoroutine(SpawnCoroutine());
	}


	// 리스폰 딜레이 후 플레이어 스폰 코루틴
	IEnumerator SpawnCoroutine()
	{
		// 리스폰 시간 계산
		float TargetTime = Time.time + respawnTime;

		// respawn이 끝나기를 기다리고, 기다리는 동안 respawn 카운트 다운을 업데이트
		while(TargetTime - Time.time > 0)
		{
			// 유아이 시간도 줄임
			yield return null;
		}

		// 서버에 리스폰 요청을 보냄
		ui.DisableDeath();
		localPlayer.CmdRespawn();
	}

	// 플레이어에게 승리팀을 설명하는 텍스트에 대한 게임을 설정
	// 네트워크를 통해 업데이트가 전송되지 않도록 플레이어의 움직임을 비활성화
	public void DisplayGameOver(int teamIndex)
	{
		localPlayer.enabled = false;
		localPlayer.camFollow.HideMask(true);
		ui.SetGameOverText(teams[teamIndex]);

		// 게임오버 창 활성화 시작
		StartCoroutine(DisplayGameOver());
	}

	// 짤은 지연 후에 게임오버 윈도우를 표시
	IEnumerator DisplayGameOver()
	{
		// 어떤 팀이 이겼는지 읽을 시간을 제공
		// 그 후 게임오버 스크린 활성화
		yield return new WaitForSeconds(3);

		// 게임오버 스크린 띄워주고 네트워크 연결을 끊음
		ui.ShowGameOver();
		// PhotonNetwork.Disconnect(); ??
	}


	//clean up callbacks on scene switches
	void OnDestroy()
	{
		#if UNITY_ADS
                UnityAdsManager.adResultEvent -= HandleAdResult;
		#endif
	}
}

// 팀 속성 정의 클래스
[System.Serializable]
public class Team
{
	// 승리팀의 이름을 보여주기위한 변수
	public string name;

	// UI 및 플레이어 프리 팹 팀의 색상
	public Material material;

	// 씬에 있는 팀의 스폰 지점
    // 컴포넌트에 BoxCollider가 존재하는 경우, 충돌 경계 안에 있는 점을 사용
	public Transform spawn;
}
