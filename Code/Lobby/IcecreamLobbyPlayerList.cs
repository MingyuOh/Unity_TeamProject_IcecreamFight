using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace IcecreamLobby
{
	public class IcecreamLobbyPlayerList : MonoBehaviour
	{

		public static IcecreamLobbyPlayerList instance = null;


		//작성했다가 취소 public RectTransform RedTeam_PlayerListContentTransfrom;    //레드팀 플레이어 리스트 패널

		public GameObject warningDirectPlayServer;

		protected VerticalLayoutGroup _layout_Red;      //Red팀 플레이어 리스트 레이아웃
		protected List<IcecreamLobbyPlayer> playerList = new List<IcecreamLobbyPlayer>();
		public int playerListCount
		{
			get { return playerList.Count; }
		}

		public void OnEnable()
		{
			instance = this;
			//작성했다가 취소 _layout_Red = RedTeam_PlayerListContentTransfrom.GetComponent<VerticalLayoutGroup>();
		}

		// 서버 오브젝트 경고 팝업 함수
		public void DisplayDirectServerWarning(bool enabled)
		{
			if (warningDirectPlayServer != null)
				warningDirectPlayServer.SetActive(enabled);
		}


		void Update()
		{
			//
		}

		// 플레이어 추가 함수
		public void AddPlayer(IcecreamLobbyPlayer player)
		{
			// 플레이어 리스트에 전달된 플레이어가 존재하면 추가하지 않음
			if (playerList.Contains(player))
				return;

			// 플레이어 리스트에 플레이어 추가
			playerList.Add(player);

			///////////////////추가했다가 취소
			//부모 설정
			//player.transform.SetParent(RedTeam_PlayerListContentTransfrom, transform);
			//////////////////////////////////

			// 플레이어 리스트 수정
			PlayerListModified();
		}

		// 플레이어 제거 함수
		public void RemovePlayer(IcecreamLobbyPlayer player)
		{
			// 플레이어 제거
			playerList.Remove(player);

			// 플레이어 리스트 수정
			PlayerListModified();
		}

		// 플레이어 리스트 수정 함수
		public void PlayerListModified()
		{
			// 플레이어 한명이 나가면 디스플레이 되는 사용자들 위치 변경
		}
	}
}
