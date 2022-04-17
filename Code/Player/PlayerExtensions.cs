using Hashtable = ExitGames.Client.Photon.Hashtable;

public static class PlayerExtensions
{
	//사용자 지정 속성에서 값에 액세스하고 액세스하기위한 키 해시 테이블
	public const string team = "Team";
	public const string health = "Health";
	public const string icecreamBall = "IcecreamBall";

	// 유저 이름 반환 메소드(싱글플레이일 경우 AI 이름)
	public static string GetName(this PhotonView player)
	{
		if(PhotonNetwork.offlineMode == true)
		{
			PlayerBot bot = player.GetComponent<PlayerBot>();
			if (bot != null)
			{
				return bot.myName;
			}
		}

		return player.owner.NickName;
	}

	// 팀 반환 함수(싱글플레이일 경우 팀 인덱스 반환)
	public static int GetTeam(this PhotonView player)
	{
		if (PhotonNetwork.offlineMode == true)
		{
			PlayerBot bot = player.GetComponent<PlayerBot>();
			if (bot != null)
			{
				return bot.teamIndex;
			}
		}

		return player.owner.GetTeam(true);
	}

	// Online : 플레이어의 네트워크 팀 번호를 속성에서 반환
	public static int GetTeam(this PhotonPlayer player, bool custom)
	{
		return System.Convert.ToInt32(player.CustomProperties[team]);
	}

	// Online : 플레이어 봇의 팀 번호를 로컬로 동기화
	// 방장이 온라인 모드로 또는 오프라인 모드가 해제 된 경우 폴백
	public static void SetTeam(this PhotonView player, int teamIndex)
	{
		if (PhotonNetwork.offlineMode == true)
		{
			PlayerBot bot = player.GetComponent<PlayerBot>();
			if (bot != null)
			{
				bot.teamIndex = teamIndex;
				return;
			}
		}

		player.owner.SetTeam(teamIndex);
	}

	// Online : 플레이어를 통해 모든 플레이어의 팀 번호를 등록 정보를 통해 동기화
	public static void SetTeam(this PhotonPlayer player, int teamIndex)
	{
		player.SetCustomProperties(new Hashtable() { { team, (byte)teamIndex } });
	}

	// Offline: PlayerBot에 저장된 봇의 상태 값을 반환
	// 방장의 경우 또는 오프라인 모드가 꺼진 경우 온라인 모드로 변경
	public static int GetHealth(this PhotonView player)
	{
		if (PhotonNetwork.offlineMode == true)
		{
			PlayerBot bot = player.GetComponent<PlayerBot>();
			if (bot != null)
			{
				return bot.health;
			}
		}

		return player.owner.GetHealth();
	}

	// Online : 속성에서 플레이어의 네트워크 상태 값을 반환
	public static int GetHealth(this PhotonPlayer player)
	{
		return System.Convert.ToInt32(player.CustomProperties[health]);
	}

	// Offline : PlayerBot의 상태 값을 로컬에서 동기화
	// 방장이 온라인 모드로 또는 오프라인 모드가 해제 된 경우 폴백.
	public static void SetHealth(this PhotonView player, int value)
	{
		if (PhotonNetwork.offlineMode == true)
		{
			PlayerBot bot = player.GetComponent<PlayerBot>();
			if (bot != null)
			{
				bot.health = value;
				return;
			}
		}

		player.owner.SetHealth(value);
	}

	// Online : 속성을 통해 모든 플레이어에 대해 플레이어의 상태 값을 동기화
	public static void SetHealth(this PhotonPlayer player, int value)
	{
		player.SetCustomProperties(new Hashtable() { { health, (byte)value } });
	}

	// Offline : PlayerBot에 저장된 봇의 총알 색인을 반환
	// 방장이 온라인 모드로 또는 오프라인 모드가 해제 된 경우 폴백
	public static int GetIcecreamBall(this PhotonView player)
	{
		if (PhotonNetwork.offlineMode == true)
		{
			PlayerBot bot = player.GetComponent<PlayerBot>();
			if (bot != null)
			{
				return bot.currentIcecreamBall;
			}
		}

		return player.owner.GetIcecreamBall();
	}

	// Online : 플레이어의 네트워크 아이스크림 타입을 속성에서 반환
	public static int GetIcecreamBall(this PhotonPlayer player)
	{
		return System.Convert.ToInt32(player.CustomProperties[icecreamBall]);
	}

	// Offline : 현재 선택된 PlayerBot 글 머리표를 로컬에서 동기화
	// 방장이 온라인 모드로 또는 오프라인 모드가 해제 된 경우 폴백
	public static void SetIcecreamBall(this PhotonView player, int value)
	{
		if (PhotonNetwork.offlineMode == true)
		{
			PlayerBot bot = player.GetComponent<PlayerBot>();
			if (bot != null)
			{
				bot.currentIcecreamBall = value;
				return;
			}
		}

		player.owner.SetIcecreamBall(value);
	}

	// Online : 현재 선택된 아이스크림을 모든 플레이어에 대해 속성을 통해 동기화
	public static void SetIcecreamBall(this PhotonPlayer player, int value)
	{
		player.SetCustomProperties(new Hashtable() { { icecreamBall, (byte)value } });
	}

	// Offline : PlayerBot의 모든 속성을 로컬에서 제거
	// 방장이 온라인 모드로 또는 오프라인 모드가 해제 된 경우 폴백
	public static void Clear(this PhotonView player)
	{
		if (PhotonNetwork.offlineMode == true)
		{
			PlayerBot bot = player.GetComponent<PlayerBot>();
			if (bot != null)
			{
				bot.currentIcecreamBall = 0;
				bot.currentCustomization = 0;
				bot.health = 0;
				return;
			}
		}

		player.owner.Clear();
	}

	// Online : 한 명령의 속성을 통해 플레이어의 모든 네트워크 변수를 제거
	public static void Clear(this PhotonPlayer player)
	{
		player.SetCustomProperties(new Hashtable() {
			{ PlayerExtensions.icecreamBall, (byte)0 },
			{ PlayerExtensions.health, (byte)0 } });
	}
}
