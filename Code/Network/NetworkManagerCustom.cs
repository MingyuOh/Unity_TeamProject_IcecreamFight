using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Communication;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class NetworkManagerCustom : Photon.PunBehaviour
{

	#region Public Variables
	
	public static readonly string appVersion = "v1.0";      // 게임 버전
	public GameObject[] playerPrefabs;						// 플레이어 프리팹
	public static int currentRoomCount = 0;                 // 현재 방의 개수
	public int gameSceneIndex = 2;							// 게임 씬 인덱스
	public byte maxPlayers = 6;                             // 최대 플레이어 수
	public static event Action connectionFailedEvent;       // 매치메이킹 연결에 실패하였을 때 발생하는 이벤트
	public bool isConnected = false;						// 포톤 초기화 여부
	
	#endregion

	#region Private Variables

	private static NetworkManagerCustom instance;            // 로비매니저 싱글톤
	private FriendSystemManager friendSystemManager;         // 친구 시스템 매니저
	private TeamSystemManager teamSystemManager;             // 팀 시스템 매니저 
	
	private bool isReady = false;							 // 게임 준비 플래그
	private bool rejoin = false;                             // 게임중 서버 연결상태 체크 플래그

	#endregion

	[HideInInspector]
	public bool _isMatchmaking = false;                     // 매칭 플래그(매칭을 종료 할 때 클라이언트를 올바르게 연결 해제하는 데 사용)

	// NetworkManager의 클라이언트 numPlayers는 항상 0이므로, 
	// LobbyPlayer의 연결 / 제거를 통해 숫자를 계산합니다.
	// 플레이어의 수를 의미하므로 클라이언트가 얼마나 많은 플레이어인지 알 수 있습니다.
	[HideInInspector]
	public int playerNumber = 0;

	//protected bool disconnectServer = false;               // 서버 연결여부 플래그

	//protected ulong currentMatchID;                        // 매칭 아이디


	/// <summary>
	/// 테스트용 타이머
	/// </summary>
	float testTime = 0.0f;

	// 네트워크 매니저 인스턴스 리턴 함수
	public static NetworkManagerCustom GetInstance()
	{
		return instance;
	}

	#region 유니티 함수

	private void Awake()
	{
		// 싱글톤 객체 생성
		if (instance == null)
		{
			instance = this;
			DontDestroyOnLoad(gameObject);
		}
		else if (instance != this)
		{
			Destroy(gameObject);
			return;
		}
	}

	private void Start()
	{
		// 싱글톤 객체 할당
		friendSystemManager = FriendSystemManager.Instance;
		teamSystemManager = TeamSystemManager.Instance;
	}
	
	void Update()
	{
		// Debug용 서버 연결상태 확인
		if(testTime > 5.0f)
		{
			Debug.LogFormat("<color=blue>포톤 서버</color> 연결 상태: <color=red>{0}</color>", PhotonNetwork.connectionState);
			testTime = 0.0f;
		}
		else
		{
			testTime += Time.deltaTime;
		}
	}

	private void OnEnable()
	{
		PhotonNetwork.OnEventCall += OnPhotonReJoinPlayerEvent;
	}

	private void OnDisable()
	{
		PhotonNetwork.OnEventCall -= OnPhotonReJoinPlayerEvent;
	}

	#endregion

	///////////////////////////////////////////////////////////////////////////
	// 로비 서버
	///////////////////////////////////////////////////////////////////////////

	#region 로비 Server

	// 	포톤 네트워크에 접속
	public void AwakePhotonConnect()
	{
		// PhotonLogLevel.Full은 디버깅용에만 사용
		// 배포시 변경 필요
		PhotonNetwork.logLevel = PhotonLogLevel.Full;

		PhotonNetwork.autoJoinLobby = false;

		// 마스터 클라이언트와 동일한 레벨로 룸 안의 모든 클라이언트들이 로드 해야하는지를 정의 합니다
		// 로드된 레벨을 동기화 해야 하면 마스터 클라이언트는 
		// PhotonNetwork.LoadLevel 을 사용해야 합니다. 
		// 모든 클라이언트는 갱신을 받거나 참여했을 때 새로운 씬을 로드하게 될 것 입니다.
		// 내부적으로 로드된 씬에 커스텀 룸 프로퍼티가 설정 됩니다. 
		// 클라이언트가 이 프로퍼티를 읽을 때 아직 동일한 씬에 있지 않은 경우에 즉시 메시지큐를 잠시 멈추고(PhotonNetwork.isMessageQueueRunning = false) 로드 합니다. 
		// 씬 로드가 완료 될 때, PUN 은 자동적으로 메시지 큐를 사용하도록 설정할 것 입니다.
	    PhotonNetwork.automaticallySyncScene = true;

		// 플레이명 셋팅 후 게임 참가
		PhotonNetwork.playerName = UserAuth.Instance.GetNickName();

		// 데이터베이스와 Photon 서버 UserID 연동
		PhotonNetwork.AuthValues = new AuthenticationValues();
		PhotonNetwork.AuthValues.UserId = NCMB.NCMBUser.CurrentUser.UserName;

		// 고유 한 viewID로이 gameobject에 뷰를 추가
		// 이것은 씬에서 같은 ID를 가지지 않기 위함
		PhotonView view = gameObject.AddComponent<PhotonView>();
		view.viewID = 999;

		// 포톤 서버 접속
		PhotonNetwork.PhotonServerSettings.HostType = ServerSettings.HostingOption.BestRegion;
		PhotonNetwork.ConnectToBestCloudServer(appVersion);
	}

	// 로비 방 생성 함수
	public static void StartMatch(NetworkMode mode)
	{
		switch (mode)
		{
			// Photon 서버에서 사용할 수있는 클라우드 게임에 연결
			case NetworkMode.Online:
				// 포톤 서버 접속
				PhotonNetwork.PhotonServerSettings.HostType = ServerSettings.HostingOption.BestRegion;
				PhotonNetwork.ConnectToBestCloudServer(appVersion);
				break;
			case NetworkMode.Offline:
				PhotonNetwork.offlineMode = true;
				break;
		}
	}

	/// <summary>
	/// 연결이 설정되기 전에 
	/// Photon 서버에 대한 연결 호출이 실패한 경우 호출(처음 접속할 때)
	/// </summary>
	public override void OnFailedToConnectToPhoton(DisconnectCause cause)
	{
		if (connectionFailedEvent != null)
			connectionFailedEvent();
	}

	/// <summary>
	/// 무언가가 연결을 실패하게 만들 때 호출되는 콜백 (설정된 후)
	/// </summary>
	public override void OnConnectionFail(DisconnectCause cause)
	{
		if(PhotonNetwork.Server == ServerConnection.GameServer)
		{
			switch(cause)
			{
				case DisconnectCause.DisconnectByClientTimeout:
					rejoin = true;
					break;
				default:
					rejoin = false;
					break;
			}
		}

		if (connectionFailedEvent != null)
			connectionFailedEvent();
	}

	/// <summary>
	/// 포톤서버로부터 연결이 끊어진 후 호출되는 콜백 함수
	/// </summary>
	public override void OnDisconnectedFromPhoton()
	{
		// 이 처리를 해주어야함 방이 닫혀있다면 다시 들어갈 필요 없음
		// 게임 오버 스크린이 이미 표시될 때 씬을 자동 전환 하지 않음
		//if (GameManager.GetInstance() && GameManager.GetInstance().ui.gameOverMenu.activeInHierarchy)
		//	return;

		// 게임 중에 연결이 끊겼을 때 게임에 재접속
		if (rejoin == true && PhotonNetwork.ReconnectAndRejoin() == false)
		{
			Debug.LogError("서버에 다시 연결하고 게임에 재접속 하는 중 오류가 발생하였습니다.");
		}
		else
		{
			PhotonNetwork.Reconnect();
		}
	}

	/// <summary>
	/// 마스터와의 연결이 설정된 후에 호출 함수
	/// </summary>
	public override void OnConnectedToMaster()
	{		
		PhotonNetwork.JoinLobby();

		isConnected = true;
	}

	/// <summary>
	/// 랜덤방에 참가하지 못하였을 때 호출 함수
	/// </summary>
	public override void OnPhotonRandomJoinFailed(object[] codeAndMsg)
	{
		Debug.Log("우리가 연결되어있는 마스터 클라이언트에서 Photon에 일치하는 것을 찾지 못했습니다.");
		Debug.Log("참여할 수 있는 방이 존재하지 않으므로 자동 방 만들기");

		CreateRoom();
	}

	/// <summary>
	/// 방 생성에 실패하였을 때 호출
	/// </summary>
	public override void OnPhotonCreateRoomFailed(object[] codeAndMsg)
	{
		if (connectionFailedEvent != null)
			connectionFailedEvent();
	}

	/// <summary>
	/// 현재 마스터 서버에서 새로운 마스터 서버로 전환된 후 호출
	/// 클라이언트가 룸에 들어갈 때는 호출 되지 않습니다. 
	/// 이 메소드가 호출 될 때 이전의 마스터클라이언트는 여전히 플레이어 리스트에 있습니다.
	/// </summary>
	public override void OnMasterClientSwitched(PhotonPlayer newMasterClient)
	{
		// 마스터 클라이언트가 자동으로 변경됨
		// newMasterClient가 마스터 클라이언트임
	}

	/// <summary>
	/// 룸에 플레이어의 액티브 상태가 변경되었을 때 호출되는 콜백 함수
	/// (RoomOptions.PlayerTTL != -1 일 때 호출)
	/// </summary>
	/// <param name="otherPlayer"></param>
	public override void OnPhotonPlayerActivityChanged(PhotonPlayer otherPlayer)
	{
		/// 마스터 클라이언트가 otherPlayer일 경우는 테스트를 통해 로직을 구현해야함
		if (otherPlayer.IsInactive == false)
		{
			// 이벤트 옵션 설정
			RaiseEventOptions options = new RaiseEventOptions()
			{
				CachingOption = EventCaching.DoNotCache,
				Receivers = ReceiverGroup.All
			};

			/// 부하가 많이 걸릴 것으로 예상되므로
			/// 추후 모바일 빌드 테스트 후 로직 결정
			foreach (PhotonView view in FindObjectsOfType<PhotonView>())
			{
				// 이탈했던 플레이어의 view 정보를 가지고 옴
				if (view.owner == otherPlayer)
				{
					// 플레이어 상태로 복귀
					if (view.gameObject.tag.Equals("Player") == true)
						ChangeTheElementsOfAPlayer(view.gameObject, false);

					// 오브젝트 데이터 저장
					object[] datas = new object[]
					{
						view.viewID,
						view.transform.position,
						view.transform.rotation
					};

					// RaiseEvent 호출
					PhotonNetwork.RaiseEvent((byte)PhotonCustomEventCode.PlayerInfo, datas, false, options);
				}
			}
		}
		else
		{
			/// 플레이어가 나갔을 경우 AI로 대체
			foreach (PhotonView view in FindObjectsOfType<PhotonView>())
			{
				// 이탈 플레이어의 view 정보를 가지고 옴
				if (view.owner == otherPlayer)
				{
					// 플레이어일 경우 AI로 대체
					if (view.gameObject.tag.Equals("Player") == true)
					{
						ChangeTheElementsOfAPlayer(view.gameObject, true);
						break;
					}
				}
			}
		}
	}

	/// <summary>
	/// 플레이어가 나가거나 재접속 했을때 캐릭터의 
	/// 각종 컴포넌트를 변경하기위한 함수
	/// </summary>
	/// <param name="view"></param>
	/// <param name="isInactive"></param>
	private void ChangeTheElementsOfAPlayer(GameObject playerGameObject, bool isInactive)
	{
		// 플레이어가 게임 중 방 이탈
		if(isInactive == true)
		{
			/// AI 대체를 위한 
			StartCoroutine(LeavePlayerElemenentSettingCoroutine(playerGameObject));
		}
		else // 이탈한 플레이어가 방 재참가
		{
			StartCoroutine(ReJoinPlayerElemenentSettingCoroutine(playerGameObject));
		}
	}

	/// <summary>
	/// 플레이어가 방을 떠났을때 컴포넌트 교체 코루틴
	/// </summary>
	/// <param name="playerGameObject"></param>
	/// <returns></returns>
	private IEnumerator LeavePlayerElemenentSettingCoroutine(GameObject playerGameObject)
	{
		/// AI 대체를 위한 
		// 컴포넌트 제거
		playerGameObject.GetComponent<PhotonView>().ObservedComponents.Remove(playerGameObject.GetComponent<Player>());
		Destroy(playerGameObject.GetComponent<Player>());

	    // PhotonTransFormView 비활성화
		playerGameObject.GetComponent<PhotonTransformView>().enabled = false;

		// Destroy를 위해 한 프레임 정지
		yield return null;
		
		// 컴포넌트 추가
		playerGameObject.AddComponent<UnityEngine.AI.NavMeshAgent>();

		playerGameObject.AddComponent<PlayerBot>();
		PlayerBot playerBot = playerGameObject.GetComponent<PlayerBot>();
		playerGameObject.GetComponent<PhotonView>().ObservedComponents.Add(playerBot);

	}

	/// <summary>
	/// 플레이어가 게임에 재접속 시 컴포넌트 교체 코루틴
	/// </summary>
	/// <param name="playerGameObject"></param>
	/// <returns></returns>
	private IEnumerator ReJoinPlayerElemenentSettingCoroutine(GameObject playerGameObject)
	{
		// 컴포넌트 제거
		playerGameObject.GetComponent<PhotonView>().ObservedComponents.Remove(playerGameObject.GetComponent<PlayerBot>());
		Destroy(playerGameObject.GetComponent<PlayerBot>());
		Destroy(playerGameObject.GetComponent<UnityEngine.AI.NavMeshAgent>());

		// PhotonTransFormView 활성화
		playerGameObject.GetComponent<PhotonTransformView>().enabled = true;

		// Destroy를 위해 한 프레임 정지
		yield return null;

		// 컴포넌트 추가
		playerGameObject.AddComponent<Player>();
		Player player = playerGameObject.GetComponent<Player>();
		playerGameObject.GetComponent<PhotonView>().ObservedComponents.Add(player);
	}

	/// Remote 플레이어가 방에서 나갔을 때 호출
	/// PhotonNetwork.leaveRoom을 호출하면 PUN은 나머지 클라이언트에서이 메서드를 호출
	/// 원격 클라이언트가 연결을 끊거나 닫히면이 콜백이 실행 시간 초과 후 몇초
	/// 해당 플레이어(otherPlayer)가 RoomOptions.EmptyRoomTtl 시간이 경과되었을 때 호출
	public override void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer)
	{
	}

	#endregion 로비 Server



	///////////////////////////////////////////////////////////////////////////
	// 로비 클라이언트
	///////////////////////////////////////////////////////////////////////////
	#region 로비 Client

	/// <summary>
	/// 게임 준비 코루틴
	/// </summary>
	public void Play()
	{
		// 매치메이킹 준비 완료
		isReady = true;

		// 팀이 존재한 상태
		if(teamSystemManager.isTeam == true)
		{
			// 팀 멤버에게 메세지 전송
			NetworkSocialManager.Instance.chatClient.PublishMessage(teamSystemManager.currentTeamChannelName, SocialMessage.TEAM_READY);

			StartCoroutine(teamSystemManager.CheckingTeamMemberAllReady(checking => 
			{
				if(checking == true)
				{
					// 팀원 모두가 레디했을 경우 방에 참가
					OnJoinRandomRoom();
				}
			}));
		}
		else
		{
			OnJoinRandomRoom();
		}
	}

	// 클라이언트가 방을 만들고 들어 갈 때 호출
	public void CreateRoom()
	{
		// 방을 만들었으므로 초기 방 속성을 팀 채우기 및 점수 배열 채우기와 비슷하게 설정
		// 랜덤 룸에 합류하는 대신 사이즈와 스코어 별 매치 메이킹 선택을 정의하는 데 사용합니다
		Hashtable roomProps = new Hashtable();
		roomProps.Add(RoomExtensions.size, new int[RoomExtensions.initialArrayLength]);
		roomProps.Add(RoomExtensions.score, new int[RoomExtensions.initialArrayLength]);

		RoomOptions roomOptions = new RoomOptions();
		roomOptions.MaxPlayers = maxPlayers;				// 최대 플레이어 설정
		roomOptions.IsOpen = true;                          // 방 오픈
		roomOptions.PublishUserId = true;                   // 유저 정보를 가지고올 수 있음
		roomOptions.CustomRoomProperties = roomProps;       // 커스텀 룸 옵션
		roomOptions.PlayerTtl = 180000;						// 룸 재참여(int.MaxValue : 게임이 끝날때까지 재참여 가능)
		roomOptions.CleanupCacheOnLeave = false;
		/// 몇 초로 할지 정하고 주석 풀어야함
		//roomOptions.EmptyRoomTtl = 30000;				// 60초 = 60000 30초 또는 1분으로 설정해야함

		if (teamSystemManager.isTeam == true)
		{
			// 팀이 존재하는경우 팀과 같이 게임을 진행 할 방 생성
			List<string> teamMemberDictionaryConvertToList = new List<string>(teamSystemManager.teamMemberDictionary.Keys);
			PhotonNetwork.CreateRoom(null, roomOptions, TypedLobby.Default, teamMemberDictionaryConvertToList.ToArray());
		}
		else
		{
			// 랜덤방에 참가 불가능 할 경우 내가 방을 만듦
			PhotonNetwork.CreateRoom(null, roomOptions, null);
		}
	}

	// 방에 들어갈 때 호출(만들거나 참가함으로써)
	public void OnJoinRandomRoom()
	{
		// 초기 방 속성을 팀 채우기 및 점수 배열 채우기와 비슷하게 설정
		Hashtable roomProps = new Hashtable();
		roomProps.Add(RoomExtensions.size, new int[RoomExtensions.initialArrayLength]);
		roomProps.Add(RoomExtensions.score, new int[RoomExtensions.initialArrayLength]);

		if (teamSystemManager.isTeam == true)
		{
			// 팀이 존재하는경우 팀과 같이 게임을 진행 할 방 생성
			List<string> teamMemberDictionaryConvertToList = new List<string>(teamSystemManager.teamMemberDictionary.Keys);
			// 팀이 존재하는경우 팀과 같이 게임을 진행
			// 매개변수: 룸 옵션, 기대 최대 플레이어 수, 매치메이킹 모드, 로비타입, sql, 기대 플레이어 유저
			PhotonNetwork.JoinRandomRoom(roomProps, maxPlayers, MatchmakingMode.FillRoom,
				TypedLobby.Default, null, teamMemberDictionaryConvertToList.ToArray());
		}
		else
		{
			// 랜덤방에 참가 불가능 할 경우 내가 방을 만듦
			PhotonNetwork.JoinRandomRoom();
		}
	}

	/// <summary>
	/// 방에 들어갈 때 (룸에 참가 또는 생성하여) 호출
	/// 룸에 입장 했을 때 호출 됩니다 (룸 생성 또는 룸 참여에 의해). 
	/// 모든 클라이언트들에서 호출 됩니다 (마스터 클라이언트도 포함)
	/// 이 메소드는 일반적으로 플레이어 캐릭터들을 생성할 때 사용 됩니다.
	/// 경기가 "활발하게" 시작되어야 한다면 사용자가 버튼을 누르거나 타이머에 의해서 트리거되는 PunRPC 를 호출 할 수 있습니다.
    /// 이 메소드가 호출 되었을 때는 일반적으로 PhotonNetwork.playerList 를 이용해 룸안에 있는 플레이어들의 목록에 접근 할 수 있습니다.
	/// 또한 모든 커스텀 프로퍼티들은 Room.customProperties 로 사용 할 수 있어야 합니다.
	/// Room.playerCount 를 체크하여 룸에 게임을 시작할 수 있는 충분한 플레이어들이 있는지 체크 합니다.
	/// </summary>
	public override void OnJoinedRoom()
	{
		/// 디버그용 팀과 함께 방에 입장할 때 사용
		/// (팀으로서 조인할때 체크해야함)
		Debug.LogFormat("player actorNr={0} userId={1} nickname={2} joined room {3}", PhotonNetwork.player.ID, PhotonNetwork.player.UserId, PhotonNetwork.player.NickName, PhotonNetwork.room.Name);

		if (teamSystemManager.isTeam && teamSystemManager.teamMemberDictionary.Count > 0)
		{
			Debug.LogFormat("number of expectedPlayers inside room {0}", PhotonNetwork.room.ExpectedUsers.Length);
			for (int i = 0; i < PhotonNetwork.room.ExpectedUsers.Length; i++)
			{
				Debug.LogFormat("expectedPlayer {0} userId={1}", i, PhotonNetwork.room.ExpectedUsers[i]);
			}
		}
		Debug.LogFormat("number of players inside the room: {0}", PhotonNetwork.room.PlayerCount);
		for (int i = 0; i < PhotonNetwork.playerList.Length; i++)
		{
			Debug.LogFormat("player {0} actorNr={1} userId={2} nickname={3}", i, PhotonNetwork.playerList[i].ID, PhotonNetwork.playerList[i].UserId, PhotonNetwork.playerList[i].NickName);
		}

		Debug.LogFormat("<color=red>방 이름: {0}</color>", PhotonNetwork.room.Name);
		
		// 재접속 플레이어라면 실행시키지 않음
		if (rejoin == true)
		{
			StartCoroutine(OnPhotonPlayerReJoinRoomSettingGameDataCoroutine());
			return;
		}

		// 여기서 게임 데이터를 생성하고
		// 매칭 중에 "매칭 취소" 버튼을 클릭했을 때
		// Delete함수를 실행해야한다.
		SaveGameRoomInfoInLocal();

		// 게임을 로딩해야함
		StartCoroutine(WaitForCompleteMatchMaking(PhotonNetwork.player));
	}

	/// <summary>
	/// remote 플레이어가 방에 접속했을 때 호출
	/// 원격 플레이어가 룸에 들어왔을 때 호출 됩니다. 이 PhotonPlayer 가 playerlist 에 이미 추가 된 시점 입니다.
	/// 특정 수의 플레이어로 게임이 시작 해야 한다면 이 콜백에서 Room.playerCount 를 체크하여 게임을 시작할 수 있는지 체크 할 때 매우 유용 합니다.
	/// </summary>
	/// <param name="player">리모트 플레이어</param>
	public override void OnPhotonPlayerConnected(PhotonPlayer player)
	{

	}

	/// <summary>
	/// 게임을 시작했을 때 이전에 게임 플레이를 하던 도중
	/// 앱 종료 이력이 있는지 확인하는 함수
	/// </summary>
	public IEnumerator CheckIsGamePlayingBeforeAppTerminatedCoroutine()
	{
		#if UNITY_EDITOR
			string path = Application.dataPath + "/Resources/Text/";
		#elif UNITY_STANDARDONE
			string path = Application.dataPath;
		#else
			string path = Application.persistentDataPath;
		#endif
		PlayingData data = new PlayingData();
		bool isLoaded = FileContorller<PlayingData>.LoadData(ref data, path, "PlayingData.dat");
		
		if(isLoaded == true)
		{
			// Processing
			// 포톤 상태 체크
			while (isConnected == false)
			{
				yield return null;
			}

			// 룸 정보 가져오기
			/// 게임에 들어갔을 때 IsVisible = false로 해놔서 roomList.Count = 0 이므로
			/// 이걸 생략할지 아니면 IsVisible을 true로 할지 결정해야한다.
			/// (방이 엄청 많으면 오래걸리므로 삭제하는편이 낫다고 지금은 판단됨)
			/// 반문: EmtpyTTL로 인해 방문이 닫힌 순간의 경우를 생각해야함
			List<RoomInfo> roomList = PhotonNetwork.GetRoomList().ToList();
			RoomInfo room = roomList.FirstOrDefault(r => r.Name == data.roomName);
			bool exists = (room != null);
			if (exists == false)
			{
				yield break;
			}

			// 현재 로그인 시간과 게임 시작 시간 차이 계산
			TimeSpan calculateTime = DateTime.Now - data.gameStartTime;

			// 아직 게임이 진행중일 경우
			// 3.0은 게임시간(3분) - 추후 변경해야함
			if (calculateTime.TotalMinutes < 3.0f && calculateTime.TotalSeconds >= 0.0f)
			{
				// 플레이어 인스턴스 생성을 막기 위해 플래그 설정
				rejoin = true;

				// 룸에 재 참여
				PhotonNetwork.ReJoinRoom(data.roomName);
				yield return true;
			}	
		}
		else
		{
			yield break;
		}
	}

	#endregion 로비 Client

	///////////////////////////////////////////////////////////////////////////
	// 로비 서버 / 클라이언트 공통 함수
	///////////////////////////////////////////////////////////////////////////
	#region 로비 Server / Client 공통 
		
	/// <summary>
	/// 포톤네트워크의 친구 상태 업데이트 함수
	/// </summary>
	public override void OnUpdatedFriendList()
	{
		if (PhotonNetwork.Friends != null)
		{
			for (int count = 0; count < PhotonNetwork.Friends.Count; count++)
			{
				FriendInfo friendInfo = new FriendInfo();
				Debug.LogFormat("[ {0} ] 친구정보: {1},", count, friendInfo);
			}
		}
	}

	/// <summary>
	/// 어플리케이션을 종료하였을 때 호출하는 함수
	/// 편집기가 응답하지 않게하려면 OnApplicationQuit에서 모든 Photon 연결 해제
	/// </summary>
	public void OnApplicationQuit()
	{
		if (PhotonNetwork.connectionState == ConnectionState.Connected && PhotonNetwork.inRoom)
		{
			if (PhotonNetwork.isMasterClient == true)
			{
				// Game is in Background | pause | quit
				ChangePhotonMasterClient();
				PhotonNetwork.SendOutgoingCommands();
			}
		}

		PhotonNetwork.Disconnect();
	}

	/// <summary>
	/// 어플리케이션을 일시정지 했을 때 호출되는 함수
	/// </summary>
	/// <param name="pause"></param>
	public void OnApplicationPause(bool pause)
	{
		if (pause)
		{
			if (PhotonNetwork.connectionState == ConnectionState.Connected && PhotonNetwork.inRoom && PhotonNetwork.isMasterClient)
			{
				// Game is in Background | pause | quit

				ChangePhotonMasterClient();
				PhotonNetwork.SendOutgoingCommands();
			}
		}
		else
		{
			if (PhotonNetwork.connectionState == ConnectionState.Connected && PhotonNetwork.inRoom && PhotonNetwork.isMasterClient)
			{
				// 일시 정지했을 때, 원하는 것을 여기서 처리
				// 당신이 살 수있는 시간에 도달하지 않으면 당신은 이미 연결되어 있고 inroom 입니다.
			}

			// 게임 종료 또는 방이 파괴되었거나 시간 초과되었는지 확인
		}
	}


	/// <summary>
	/// 마스터 클라이언트를 변경 가능한지 체크하고
	/// 가능하다면 변경하는 함수
	/// </summary>
	public void ChangePhotonMasterClient()
	{
		if (PhotonNetwork.isMasterClient == false)
			return;
		if (PhotonNetwork.room.PlayerCount <= 1)
			return;

		PhotonNetwork.SetMasterClient(PhotonNetwork.masterClient.GetNext());
	}

	#endregion

	///////////////////////////////////////////////////////////////////////////
	// 게임 함수
	///////////////////////////////////////////////////////////////////////////
	#region 게임 함수

	/// <summary>
	/// 게임 플레이를 시작하였을 때 룸 내의 플레이어 수가 MaxPlayer에
	/// 도달 할때까지 대기하는 함수
	/// </summary>
	/// <param name="player"></param>
	/// <returns></returns>
	private IEnumerator WaitForCompleteMatchMaking(PhotonPlayer player)
	{
		Debug.Log("<color=red>WaitForCompleteMatchMaking() 함수 호출</color>");
		while (PhotonNetwork.playerList.Length < maxPlayers)
		{
			/// ~~~~~~~~~~~UI~~~~~~~~~~~~~~
			/// 매칭 UI 코루틴 시작!
			/// "매칭중...N초"를 계속 뿌려주어야함
			yield return null;
		}

		yield return StartCoroutine(LoadGameData(player));
	}

	/// <summary>
	/// 매치메이킹이 완료되었을 때 게임 데이터 로드하는 코루틴 함수
	/// (이후 작업은 게임 매니저에서 실행)
	/// </summary>
	/// <returns></returns>
	private IEnumerator LoadGameData(PhotonPlayer player)
	{
		Debug.Log("<color=red>LoadGameDataCoroutine() 함수 호출</color>");
		if(PhotonNetwork.isMasterClient == true)
		{
			yield return StartCoroutine(MasterClientLoadedGameDataCoroutine(player));
		}
		else
		{
			yield return StartCoroutine(NonMasterClientLoadedGameDataCoroutine(player));
		}
	}

	/// <summary>
	/// 마스터 클라이언트일 경우 게임 데이터를 로드하는 코루틴 함수
	/// </summary>
	/// <param name="player"></param>
	/// <returns></returns>
	private IEnumerator MasterClientLoadedGameDataCoroutine(PhotonPlayer player)
	{
		Debug.Log("<color=red>MasterClientLoadedGameDataCoroutine() 함수 호출</color>");
		/// 방을 닫음
		PhotonNetwork.room.IsOpen = false;

		/// 게임씬 로드
		/// 자동으로 다른 플레이어들도 씬 레벨 맞춤
		PhotonNetwork.LoadLevel(gameSceneIndex);

		/// 게임데이터 및 유저정보를 로드하고 
		/// 게임 셋팅을 시작
		while (UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex != gameSceneIndex
				&& GameManager.GetInstance() == null)
		{
			yield return null;
		}

		/// 플레이어 생성 및 팀 할당
		CreateGameData(player);
	}

	/// <summary>
	/// 마스터 클라이언트가 아닐 경우 게임 데이터를 로드하는 코루틴 함수
	/// </summary>
	/// <param name="player"></param>
	/// <returns></returns>
	private IEnumerator NonMasterClientLoadedGameDataCoroutine(PhotonPlayer player)
	{
		Debug.Log("<color=red>NonMasterClientLoadedGameDataCoroutine() 함수 호출</color>");
		/// 게임데이터 및 유저정보를 로드하고 
		/// 게임 셋팅을 시작
		while (UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex != gameSceneIndex
				&& GameManager.GetInstance() == null)
		{
			yield return null;
		}

		/// 플레이어 생성 및 팀 할당
		CreateGameData(player);

		/// 여기서 게임 시작 전 이벤트(카메라, 이펙트 등)코루틴을 실행
	}

	/// <summary>
	/// 게임 매니저 데이터가 로드되었을 때 각 인스턴스 및 게임 데이터를 생성하는 함수
	/// </summary>
	/// <param name="player"></param>
	private void CreateGameData(PhotonPlayer player)
	{
		Debug.Log("<color=red>CreateGameData() 함수 호출</color>");

		/// 게임데이터셋팅을 위해 플레이어 전달
		this.photonView.RPC("RPC_GameDataSetting", PhotonTargets.MasterClient, player);

		/// 캐릭터 생성(자신에게 호출)
		this.photonView.RPC("AddPlayer", player);
	}

	/// <summary>
	/// 참여한 방의 정보를 직렬화로 저장하는 함수
	/// </summary>
	private void SaveGameRoomInfoInLocal()
	{
		// 현재 게임 방 정보 Resources/Text 폴더에
		// 직렬화하여 저장
		PlayingData playingData = new PlayingData();
		playingData.roomName = PhotonNetwork.room.Name;
		playingData.gameStartTime = DateTime.Now;

		#if UNITY_EDITOR
			string path = Application.dataPath + "/Resources/Text/";
		#elif UNITY_STANDARDONE
			string path = Application.dataPath;
		#else
			string path = Application.persistentDataPath;
		#endif

		FileContorller<PlayingData>.SaveData(playingData, path, "PlayingData.dat");
	}

	/// <summary>
	/// 재접속한 포톤플레이어를 위한 게임데이터 셋팅 코루틴 함수
	/// </summary>
	private IEnumerator OnPhotonPlayerReJoinRoomSettingGameDataCoroutine()
	{
		// 재접속 플래그 설정
		rejoin = false;

		// 게임 씬에 도달할때까지 대기
		if (GameManager.GetInstance() == null)
			yield return null;

		// 게임 UI 셋팅
		GameManager.GetInstance().ui.SetGameUICustomRoomProperties();
	}

	#endregion

	#region RPC 함수

	/// <summary>
	/// 플레이어 캐릭터 생성 함수
	/// </summary>
	/// <param name="characterType"></param> 캐릭터 타입
	/// <param name="characterSkin"></param> 캐릭터 스킨
	[PunRPC]
	private void AddPlayer()
	{
		// InstantiateDatas 생성
		object[] instantiateDatas = new object[3] { PhotonNetwork.playerName, UserAuth.Instance.characterType, UserAuth.Instance.skinType };

		// 캐릭터 타입에 따른 인덱스 설정
		int index = Utility.FetchIndexAccordingToCharacterType(UserAuth.Instance.characterType);

		// 스타트 포지션 생성
		Transform startPos = GameManager.GetInstance().teams[(int)PhotonNetwork.player.GetTeam()].spawn;

		// 네트워크 인스턴스 생성
		if (startPos != null)
		{
			PhotonNetwork.Instantiate(playerPrefabs[index].name, startPos.position, startPos.rotation, 0, instantiateDatas);
		}
		else
		{
			PhotonNetwork.Instantiate(playerPrefabs[index].name, Vector3.zero, Quaternion.identity, 0, instantiateDatas);
		}
	}

	/// <summary>
	/// 게임 데이터 셋팅 RPC 함수
	/// </summary>
	/// <param name="player"></param>
	[PunRPC]
	private void RPC_GameDataSetting(PhotonPlayer player)
	{
		// 플레이어가 속할 다음 팀의 인덱스를 얻음
		// 플레이어 속성들을 할당하고 속성들을 업데이트
		/// 팀일 경우 고려하여 로직 구현해야함
		int teamIndex = GameManager.GetInstance().GetTeamFill();
		PhotonNetwork.room.AddSize(teamIndex, +1);
		player.SetTeam(teamIndex);

		//연결 해제 및 연결 시 플레이어 속성도 지워지지 않음
		//자동으로, 모든 기존 속성을 null로 설정
		//이러한 기본값은 올바른 데이터로 인해 곧 재정의
		player.Clear();
	}
	
	#endregion

	#region RaiseEvent함수

	/// <summary>
	/// 재접속 플레이어에 대한 RaiseEvent 함수
	/// </summary>
	/// <param name="eventCode"></param>
	/// <param name="content"></param> 
	/// <param name="senderId"></param>
	private void OnPhotonReJoinPlayerEvent(byte eventCode, object content, int senderId)
	{
		PhotonCustomEventCode code = (PhotonCustomEventCode)eventCode;

		if (code == PhotonCustomEventCode.PlayerInfo)
		{
			object[] datas = content as object[];
			if (datas.Length == 3)
			{
				foreach (PhotonView view in FindObjectsOfType<PhotonView>())
				{
					if (view.viewID == (int)datas[0])
					{
						view.gameObject.transform.position = (Vector3)datas[1];
						view.gameObject.transform.rotation = (Quaternion)datas[2];
					}
				}
			}
		}
	}

	#endregion
}

#region Enum

/// <summary>
/// Network Mode selection for preferred network type.
/// </summary>
public enum NetworkMode
{
	Online,
	Offline
}

public enum PhotonCustomEventCode
{
	PlayerInfo = 0,
}

#endregion

public static class RoomExtensions
{
	// 새로운 방을 만드는 서버 게임의 초기 팀 크기.
	// GameManager를 통해 설정할 수 없음
	public const short initialArrayLength = 2;

	// 방 속성 중 팀별 팀 채우기 액세스에 필요한 키
	public const string size = "size";

	// 방 속성 중 팀당 플레이어 점수에 액세스하는 데 필요한 키
	public const string score = "score";

	// 모든 팀에서 네트워크로 팀 채우기를 반환
	public static int[] GetSize(this Room room)
	{
		return (int[])room.CustomProperties[size];
	}

	// 새 플레이어가 게임에 참여할 때 한 팀의 팀 채우기를 한 씩 증가
	// 이것은 음수 값을 사용하여 플레이어 분리시에도 사용
	public static int[] AddSize(this Room room, int teamIndex, int value)
	{
		int[] sizes = room.GetSize();
		sizes[teamIndex] += value;

		room.SetCustomProperties(new Hashtable() { { size, sizes } });
		return sizes;
	}

	// 팀 점수를 네트워크에 연결된 모든 팀들에게 반환
	public static int[] GetScore(this Room room)
	{
		return (int[])room.CustomProperties[score];
	}

	// 새로운 선수가 팀에 점수를 매겼을 때 팀 점수를 1 증가
	public static int[] AddScore(this Room room, int teamIndex)
	{
		int[] scores = room.GetScore();
		scores[teamIndex] += 1;

		room.SetCustomProperties(new Hashtable() { { score, scores } });
		return scores;
	}
}