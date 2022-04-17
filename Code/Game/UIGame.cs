using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Communication;

/// <summary>
/// 게임 씬의 모든 요소, 팀 이벤트 및 사용자 상호 작용을 위한 UI 스크립트.
/// </ summary>
public class UIGame : Photon.MonoBehaviour
{
	/// <summary>
	/// 플레이어의 움직임과 모바일 장치의 동작을 제어하는 ​​조이스틱 구성 요소.
	/// </ summary>
	public UIJoystick control;

	/// <summary>
	/// 절대 값을 사용하여 각 팀의 팀 채우기를 표시하는 UI 슬라이더
	/// </ summary>
	public Slider[] teamSize;

	/// <summary>
	/// 각 팀의 킬 점수를 표시하는 UI 텍스트
	/// </ summary>
	public Text[] teamScore;

	/// <summary>
	/// 각 팀의 킬 점수를 표시하는 UI 텍스트
	/// </summary>
	public Text[] killCounter;

	/// <summary>
	/// 로컬 플레이어를위한 모바일 십자선 조준 표시기.
	/// </ summary>
	public GameObject aimIndicator;


	/// <summary>
	/// 플레이어 사망을 표시하고 이 플레이어를 죽인 UI 텍스트
	/// </ summary>
	public Text deathText;

	/// <summary>
	/// 플레이어가 다시 생성 할 때까지 남은 시간 (초)을 표시하는 UI 텍스트
	/// </ summary>
	public Text spawnDelayText;

	/// <summary>
	/// 비디오 섬네일을 표시하기 위해 Everyplay에서 사용하는 스프라이트에 대한 참조
	/// 이것은 녹음을 지원하는 장치에서만 사용되며 그렇지 않으면 빈 상태로 유지
	/// </ summary>
	public Image thumbnailImage;

	/// <summary>
	/// 게임 종료를 나타내는 UI 텍스트와 라운드에서 이긴 팀
	/// </ summary>
	public Text gameOverText;

	/// <summary>
	/// 게임 끝에서 활성화 된 UI 창 개체. 공유 및 메뉴 씬으로 돌아가는 버튼을 제공
	/// </ summary>
	public GameObject gameOverMenu;

	IEnumerator Start()
	{

		// 비 휴대 기기에서는 편집기를 제외하고 조이스틱 컨트롤을 숨김
		#if !UNITY_EDITOR && (UNITY_STANDALONE || UNITY_WEBGL)
			ToggleControls(false);
		#endif

		// 모바일 장치에서 추가 조준 표시기 사용 가능
		#if !UNITY_EDITOR && !UNITY_STANDALONE && !UNITY_WEBGL
			Transform indicator = Instantiate(aimIndicator).transform;
			indicator.SetParent(GameManager.GetInstance().localPlayer.throwPos);
			indicator.localPosition = new Vector3(0.0f, 0.0f, 3.0f);	
		#endif
		/////////////////////// 사운드 부분 ///////////////////////
		// 배경음 플레이

		// 지원되지 않는 기기에서 Everyplay 초기화를 계속하지 마십시오.

		// Everyplay에서 사용하는 미리보기 이미지를 이미지 참조로 설정하고 녹화를 시작하고
		// 짧은 지연 후 미리보기 이미지로 표시되도록 즉시 스냅 샷을 생성
		// Unity Everyplay가 제대로 초기화하는 데 시간이 필요
		yield return new WaitForSeconds(0.5f);
	}

	//이 메소드는 네트워크에서 룸 속성이 변경 될 때마다 호출
	private void OnPhotonCustomRoomPropertiesChanged(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
	{
		OnTeamSizeChanged(PhotonNetwork.room.GetSize());
		OnTeamScoreChanged(PhotonNetwork.room.GetScore());
	}

	//이 메소드는 네트워크에서 룸 속성이 변경 될 때마다 호출
	public void SetGameUICustomRoomProperties()
	{
		OnTeamSizeChanged(PhotonNetwork.room.GetSize());
		OnTeamScoreChanged(PhotonNetwork.room.GetScore());
	}


	/// <summary>
	/// 팀 채우기의 변경을 위한 구현
	/// 슬라이더 값 업데이트 (팀 채우기의 UI 표시 업데이트).
	/// </ summary>
	public void OnTeamSizeChanged(int[] size)
	{
		// 슬라이더 값 위로 반복하고 할당
		for (int iCnt = 0; iCnt < size.Length; iCnt++)
			teamSize[iCnt].value = size[iCnt];
	}


	/// <summary>
	/// 팀 스코어 변경을 위한 구현
	/// 텍스트 값 업데이트 (팀 점수의 UI 표시 업데이트)
	/// </ summary>
	public void OnTeamScoreChanged(int[] score)
	{
		// 텍스트를 반복 처리
		for(int iCnt = 0; iCnt < score.Length; iCnt++)
		{
			// 점수가 증가했는지 감지 한 다음 애니메이션 추가
			if (score[iCnt] > int.Parse(teamScore[iCnt].text))
				//teamScore[iCnt].GetComponent<Animator>().Play("Animation");

			// 점수 값을 텍스트에 할당
			teamScore[iCnt].text = score[iCnt].ToString();
		}
	}

	/// <summary>
	/// 조이스틱 컨트롤러를 보여줄지 말지 여부
	/// </summary>
	public void ToggleControls(bool state)
	{
		control.gameObject.SetActive(state);
	}

	/// <summary>
	/// 플레이어를 죽인 사람을 팀 색상으로 표시하는 죽음의 텍스트를 설정
	/// 매개 변수 : 죽인 사람 이름,  상대 팀 색상
	/// </ summary>
	public void SetDeathText(string playerName, Team team)
	{
		// 데스 텍스트를 표시하는 동안 조이스틱 컨트롤 숨기기
		#if UNITY_EDITOR || (!UNITY_STANDALONE && !UNITY_WEBGL)
			ToggleControls(false);
		#endif
		
		// 죽인 사람의 이름을 표시하고 이름을 죽인 사람 팀 색상으로 대체
		// 이건 전체 유저에게 보여주어야 한다.
		// (죽인 사람 이름 (아이스크림 텍스처) 죽은 사람 이름) 이런 형식으로 뿌려주어야 함 
		deathText.text = "KILLED BY\n<color=#" + ColorUtility.ToHtmlStringRGB(team.material.color) + ">" + playerName + "</color>";
	}


	/// <summary>
	/// 수신 된 절대 시간 값으로 표시된 respawn 지연 값을 설정
	/// 남은 시간 값은 GameManager에 의해 동시 루틴에서 계산
	/// </ summary>
	public void SetSpawnDelay(float time)
	{
		spawnDelayText.text = Mathf.Ceil(time) + "";
	}


	/// <summary>
	/// 부활 후 플레이어 죽음과 관련된 UI 구성 요소를 숨김
	/// </ summary>
	public void DisableDeath()
	{
		// 조이스틱 컨트롤러를 보여주고 죽음관련 UI 비활성화
		#if UNITY_EDITOR || (!UNITY_STANDALONE && !UNITY_WEBGL)
			ToggleControls(true);
		#endif

		// 죽음 관련 텍스트 컴포넌트 초기화
		deathText.text = string.Empty;
		spawnDelayText.text = string.Empty;
	}

	/// <summary>
	/// 게임 종료 텍스트를 설정하고 팀을 팀 색상으로 표시
	/// </ summary>
	public void SetGameOverText(Team team)
	{
		// 게임 종료 텍스트를 표시하는 동안 조이스틱 컨트롤 숨기기
		#if UNITY_EDITOR || (!UNITY_STANDALONE && !UNITY_WEBGL)
			ToggleControls(false);
		#endif

		// 게임 종료 후 이긴 팀 텍스트를 화면 맨 위에 표시를 해야함
		// 킬 제일 높은 사람(MVP) 캐릭터를 띄워줘야 함
		// 나머지 사람은 아이디 킬수 텍스트로 표시해야함 
		gameOverText.text = "TEAM <color=#" + ColorUtility.ToHtmlStringRGB(team.material.color) + ">" + team.name + "</color> WINS!";
	}

	/// <summary>
	/// 게임 종료 텍스트를 설정하고 팀을 팀 색상으로 표시
	/// </ summary>
	public void ShowGameOver()
	{
		// UnityEveryplayManager로 사운드 끔

		// 게임 오버 창을 가능하게하고 텍스트를 숨김
		gameOverText.gameObject.SetActive(false);
		gameOverMenu.SetActive(true);


		// 여기에서 광고를 요청
		#if UNITY_ADS
            if(!UnityAdsManager.didShowAd())
                UnityAdsManager.ShowAd(true);
		#endif
	}


	/// <summary>
	/// Everyplay의 공유 대화 상자를 사용하여 기록 된 게임 플레이 비디오를 공유
	/// </ summary>
	public void Share()
	{
		// UnityEveryplayManager.Share();
	}


	/// <summary>
	/// 하드 연결 해제로 추가 네트워크 업데이트 수신을 중지 한 다음 
	/// 시작 장면을 로드
	/// </ summary>
	public void Disconnect()
	{
		if (PhotonNetwork.connected)
			PhotonNetwork.Disconnect();
		Quit();
	}

	/// <summary>
	/// 메뉴 씬을 로드
	/// 연결 해제는 GameOver 화면을 표시 할 때 이미 발생
	/// </ summary>
	public void Quit()
	{
		SceneManager.LoadScene(SceneClass.MENU_SCENE);
	}
}
