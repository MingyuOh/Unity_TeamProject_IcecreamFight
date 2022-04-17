using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

namespace IcecreamLobby
{
	// 모든 LobbyHook은 플레이어 마다 캐릭터 상태를 가져와 게임 플레이어 프리팹에 값을 전달 함 
	public class IcecreamLobbyPlayer : NetworkLobbyPlayer
	{
		// 플레이어의 스킨을 가져와야함
		private Material playerSkin;

		// 플레이어 캐릭터

		// 플레이어 이름
		private string playerName;

		// 로컬플레이어 저장
		private void SetupLocalPlayer()
		{
			// 로컬플레이어 일때 캐릭터 이름 등등 저장

			// OnClientEnterLobby가 호출 될 때, 아직 PlayerController가 생성되지 않았으므로 여기에서 다시 실행 시켜서 비활성화해야함
			// maxLocalPlayer에 도달하면 추가 버튼. OnClientEnter 로비에서 이미 계산되었으므로 0을 전달합니다.

		}

		// 다른 플레이어 저장
		private void SetupOtherPlayer()
		{
			// 로컬플레이어가 아닐 때 캐릭터 이름 등등 저장
		}

		// 플레이어 퇴장 시 방 내 플레이어 수 감소
		public void OnDestroy()
		{
			IcecreamLobbyPlayerList.instance.RemovePlayer(this);
		}
	}
}
