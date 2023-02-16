using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;


/// <summary>
/// 오프라인 모드에서 AI 봇을 생성하고 온라인이면 비활성화
/// </ summary>
public class BotSpawner :PunBehaviour
{
	/// <summary>
	/// 모든 팀에 스폰시킬 봇의 양
	/// </ summary>
	public int maxBots;

	/// <summary>
	/// 선택할 수있는 bot 프리팹을 선택
	/// </ summary>
	public GameObject[] prefabs;

	private void Awake()
	{
		// 오프라인 모드가 아닌 경우 비활성화 됨
		if(PhotonNetwork.offlineMode == false)
		{
			this.enabled = false;
		}
	}

	// Use this for initialization
	IEnumerator Start ()
	{
		// 모든 스크립트가 초기화 될 때까지 기다림
		yield return new WaitForSeconds(1);

		for(int iCnt = 0; iCnt < maxBots; iCnt++)
		{
			// 무작위로 봇 프리팹 배열에서 봇을 선택
			// 시뮬레이트 된 사설 네트워크에서 봇을 생성
			int randIndex = Random.Range(0, prefabs.Length);
			GameObject obj = PhotonNetwork.Instantiate(prefabs[randIndex].name,
				Vector3.zero, Quaternion.identity, 0);

			// 로컬 호스트가 팀 할당을 결정하도록 함
			Player player = obj.GetComponent<Player>();
			player.GetView().SetTeam(GameManager.GetInstance().GetTeamFill());


			// 해당 팀의 크기를 증가 시킴
			PhotonNetwork.room.AddSize(player.GetView().GetTeam(), +1);

			yield return new WaitForSeconds(0.25f);
		}
	}

}
