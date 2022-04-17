using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NCMB;
using Communication;
using ExitGames.Client.Photon.Chat;

public class NetworkSocialManager : MonoSingleton<NetworkSocialManager>, IChatClientListener
{
	#region Private variable

	private UserAuth userAuth;                      // 유저 정보

	#endregion

	#region Public variable

	public ChatClient chatClient;               // 포톤 채팅 클라이언트
	public string[] channelList = { "asia" };   // 채널 리스트

	#endregion

	/// <summary>
	/// 테스트용 타이머
	/// </summary>
	float testTime = 0.0f;

	#region MonoBehavior method

	private void Start()
	{
		// 유저 정보
		userAuth = UserAuth.Instance;
	}

	private void Update()
	{
		if (this.chatClient != null)
		{
			// 이것을 정기적으로 호출 
			// 노력을 내부적으로 제한하기 때문에 자주 호출을 해야함
			this.chatClient.Service();

			// Debug용 서버 연결상태 확인
			if (testTime > 5.0f)
			{
				Debug.LogFormat("<color=yellow>포톤 채팅 서버</color> 연결 상태: <color=red>{0}</color>", chatClient.State);
				Debug.LogFormat("<color=magenta>채팅 가능 여부: </color> <color=red>{0}</color>", chatClient.CanChat);
				testTime = 0.0f;
			}
			else
			{
				testTime += Time.deltaTime;
			}
		}
	}
	#endregion

	#region 채팅 서버
	/// <summary>
	/// 채팅 네트워크 연결
	/// </summary>
	public void Connect()
	{
		chatClient = new ChatClient(this);
		chatClient.ChatRegion = "ASIA";
		chatClient.UseBackgroundWorkerForSending = true;
		chatClient.Connect(PhotonNetwork.PhotonServerSettings.ChatAppID,
			NetworkManagerCustom.appVersion,
			new ExitGames.Client.Photon.Chat.AuthenticationValues(PhotonNetwork.AuthValues.UserId));
	}

	/// <summary>
	/// 채팅 네트워크 연결
	/// </summary>
	public void OnConnected()
	{
		if(channelList != null && channelList.Length > 0)
		{
			// 아시아 채널 구독
			chatClient.Subscribe(channelList);
		}

		// 팀이 존재할 경우 팀 채널에 다시 접속해야함
		/// 추후 해결해야할 문제 추가 되어야 함
		if(TeamSystemManager.Instance.isTeam == true)
		{
			TeamSystemManager.Instance.SubscribeTeamChannel();
		}

		// 현재 채팅 클라이언트 상태 저장
		chatClient.SetOnlineStatus(ChatUserStatus.Online);
	}

	/// <summary>
	/// 채팅 네트워크 "연결 해제" 함수
	/// </summary>
	public void OnDisconnected()
	{
		// 채팅 클라이언트 상태 변경
		chatClient.SetOnlineStatus(ChatUserStatus.Offline);

		// 공개채널 구독 해지(이 부분은 고민 해봐야함)
		if(TeamSystemManager.Instance.isTeam == false)
		{
			TeamSystemManager.Instance.UnsubscribeTeamChannel(null);
		}

		if (channelList != null && channelList.Length > 0)
		{
			// 아시아 채널 구독
			chatClient.Unsubscribe(channelList);
		}

		// 현재 채팅 클라이언트 상태 저장
		Reconnect();
	}

	/// <summary>
	/// 포톤 채팅서버에 재연결하는 함수
	/// </summary>
	public void Reconnect()
	{
		if (chatClient != null)
		{
			if (chatClient.Connect(chatClient.AppId, chatClient.AppVersion, chatClient.AuthValues) == false)
			{
				// 에러를 나타내야함
				Debug.Log("채팅서버에 연결을 실패하였습니다.");
			}
		}
		else
		{
			// chatClient에 값이 존재하지않으면 연결
			Connect();
		}
	}

	/// <summary>
	/// 채팅 네트워크 상태 변경 함수
	/// </summary>
	public void OnChatStateChange(ChatState state)
	{
		switch(state)
		{
			case ChatState.Disconnecting:
				Reconnect();
				break;
			case ChatState.Disconnected:
				Reconnect();
				break;
			case ChatState.DisconnectingFromFrontEnd:
				///상대방이 게임을 하다가 룸/ 게임에서 나간 이후
				///(게임서버에서 마스터 서버로 전이 - ChatState = DisconnectiongFroemFrontEnd)
				///게임서버일 때 친구요청 또는 수락 메세지를 받지 못했다면
				///이 상태 메세지를 처리하는 조건문에서 메세지를 한번 더 전송해야함
				Reconnect();
				break;
		
		}
		// OnConnected () 및 OnDisconnected ()를 사용합니다.
		// 이 메소드는 앞으로 더 복잡한 상태가 사용될 때 더욱 유용 할 수 있음

	}

	/// <summary>
	/// 다른 사용자의 새로운 상태 (친구 목록에 있는 사용자의 상태 업데이트를 받습니다).
	/// </summary>
	public void OnStatusUpdate(string user, int status, bool gotMessage, object message)
	{
		Debug.LogWarning("status: " + string.Format("{0} is {1}. Msg:{2}", user, status, message));

		/// FriendItem Class
		/// 친구 상태 및 메시지를 나타내는 데 사용되는 친구 UI 항목. 
		/// 예를 들어 여러분과 다른 방에서 노는 친구의 Health을 공유하는 방법을 보여주는 것이 목적이다.
		/// 하지만 물론 메시지는 어떤 것이든 될 수 있고 훨씬 더 복잡할 수 있다.

	}

	/// <summary>
	/// 서버에서 새로운 메시지를 수신한 클라이언트 앱 알림 전송자의 수가 'messages' 내의 메시지의 수와 동일
	/// 0 의 숫자를 가진 전송은 메시지 번호 0 에 상응하며 1을 가지고 있는 전송자는 메시지 번호 1 과 상응
	/// </summary>
	public void OnGetMessages(string channelName, string[] senders, object[] messages)
	{
		// 내 메세지도 받음(senders가 어떻게 넘어오는지 확인해야함 
		// - 닉네임인지 userID인지(예제에서는 닉네임으로 넘어옴))
		// 테스트 필요
		for(int msgCount = 0; msgCount < senders.Length; msgCount++)
		{
			string msg = messages[msgCount].ToString();
			bool equalToUserID = (senders[msgCount] != NCMBUser.CurrentUser.UserName) ? true : false;

            // 어떤 플레이어가 탈퇴하거나 추방되었을때
            // 어떤 플레이어가 내가 아니라면 팀 멤버에서 제거
            if (msg == SocialMessage.TEAM_WITHDRAWAL && equalToUserID)
            {
                TeamSystemManager.Instance.SomePlayersLeaveTheTeam(senders[msgCount]);
                // 여기 아래서 메세지 생성하여 대화창에 뿌려주어야함
                // "~~님이 팀에서 나갔습니다."
                SpeechBubbleContent.Instance.AddCenterBubble(2, senders[msgCount]);
            }
            else if (msg == SocialMessage.TEAM_BANISH && equalToUserID)
            {
                TeamSystemManager.Instance.SomePlayersLeaveTheTeam(senders[msgCount]);
                // 여기 아래서 메세지 생성하여 대화창에 뿌려주어야함
                // "~~님이 팀에서 강퇴 되었습니다."
                SpeechBubbleContent.Instance.AddCenterBubble(1, senders[msgCount]);
            }
            else if (msg == SocialMessage.TEAM_JOINED)
            {
                TeamSystemManager.Instance.IntegrateTeamInformation(senders[msgCount]);
                // 여기 아래서 메세지 생성하여 대화창에 뿌려주어야함
                SpeechBubbleContent.Instance.AddCenterBubble(3, senders[msgCount]);
            }
			else if( msg == SocialMessage.TEAM_READY)
			{
				// 레디버튼을 누른 유저 Ready 처리
				TeamSystemManager.Instance.TeamMemberReadyMessage(senders[msgCount]);
			}
            else if (channelName.Equals(TeamSystemManager.Instance.currentTeamChannelName)
                && equalToUserID)
            {
                //UI의 팀 채팅창 업데이트
                //팀원의 메세지는 항상 왼쪽 말풍선으로 추가
                bool isover = SpeechBubbleContent.Instance.IsOverLastBubbleConstantSize();
                SpeechBubbleContent.Instance.AddLeftBubble(msg, isover, senders[msgCount]);
            }
            ///////////////////////////////////////////////////////////////////////////
            /// UI에 표시될 채팅이나 알람을 표시하기위해 여기서 실행해야함
            ///////////////////////////////////////////////////////////////////////////
        }
	}

	/// <summary>
	/// 응답 메세지를 클라이언트에게 전송
	/// </summary>
	public void OnPrivateMessage(string sender, object message, string channelName)
	{
		// 자신의 메세지는 무시
		if (sender == chatClient.AuthValues.UserId)
		{
			return;
		}

		string msg = message as string;
		if (msg != null)
		{
			if (msg == SocialMessage.FRIEND_REQUEST) // 친구 요청 메세지
			{
				// 요청 메세지를 받았으니 sender의 정보를 토대로
				// 요청 목록 UI에 해당 유저 정보를 뿌려주어야 함
				// (요청자가 온라인이고 피요청자가 오프라인에서 온라인 상태는 요청자가
				// 메세지를 보내옮으로 friendItemDictionary에 추가적으로 들어가지 않게 중복작업 필요)
				// 서로 온라인이면 이 메세지를 전달 받음
				StartCoroutine(FriendSystemManager.Instance.RequestAddFriendMessage(sender));
			}
			else if (msg == SocialMessage.FRIEND_ACCEPT)
			{
				// 서로 온라인일 경우
				// accept 메세지를 받게되면 friendItemDictionary에 존재하므로
				// 이 메세지를 받은 요청자의 friendItemDictionary Value의 RelationStatus 값을 변경하고
				// 내 AcceptList에서 삭제하던지 다시 킬때 삭제하던지 해야함
				// friendList에 추가하고 Photon과 PhotonChat에도 추가해야 함
				// playerInfo도 받아와야함
				StartCoroutine(FriendSystemManager.Instance.AcceptAddFriendMessage(sender));
			}
			else if (msg == SocialMessage.FRIEND_REFUSE)
			{
				// 상대방이 온라인일 경우만 생각하면 됨
				// 이 메세지를 받게되면
				// 요청자(내 자신)의 friendIdDictionary에서 제거
				FriendSystemManager.Instance.RefuseAddFriendMessage(sender);
			}
			else if (msg == SocialMessage.TEAM_REQUEST) // 팀 요청 메세지
			{
				// 팀 시스템의 요청 메세지 처리 함수 호출
				TeamSystemManager.Instance.TeamMasterRequestMessage(sender);
			}
			else if (msg == SocialMessage.TEAM_ACCEPT) // 팀 수락 메세지
			{
				// 팀 시스템의 수락 메세지 처리 함수 호출
				TeamSystemManager.Instance.ReceiverAcceptTeamMessage(sender);
			}
			else if(msg == SocialMessage.TEAM_JOINED) // 팀 참가 메세지
			{
				TeamSystemManager.Instance.IntegrateTeamInformation(sender);
			}
			else if(msg == SocialMessage.TEAM_MASTER_WITHDRAWAL) // 팀장이 팀에서 탈퇴 메세지
			{
				TeamSystemManager.Instance.ChangeTeamMaster(sender);
			}
			else if(msg == SocialMessage.TEAM_REBUILDING)
			{
				TeamSystemManager.Instance.RebuildingATeam(sender);
			}
			// 팀에 참가할 수 없을 경우(팀이 가득찼을 경우, 거절했을 경우, 이미 팀이 존재할 경우)
			else if (msg == SocialMessage.TEAM_MAX || 
				msg == SocialMessage.ALREADY_TEAM_REQUEST ||
				msg == SocialMessage.TEAM_REFUSE)
			{
				// 팀 시스템의 "TEAM_MAX" 메세지 처리 함수 호출
				// 팀 시스템의 "ALREADY_TEAM_REQUEST" 메세지 처리 함수 호출
				// 팀 시스템의 "TEAM_REFUSE" 메세지 처리 함수 호출
				TeamSystemManager.Instance.CanNotJoinTeamMessage(sender, msg);
			}
			else // 팀 채널 명 전달
			{
				// 팀원이 초대메세지를 보냈을 경우
				// msg에 채널명이 넘어옴
				TeamSystemManager.Instance.TeamMemberRequestMessage(sender, msg);
			}
		}
	}

	/// <summary>
	/// Subscribe 오퍼레이션의 결과. 모든 요청된 채널명에 대한 구독 결과를 리턴
	/// 설명: 만약 여러 채널에 Subscribe 오퍼레이션이 전송되면, 
	///		  OnSubscribed 이 여러번 호출될 것이고, 
	///		  각 호출은 전송 배열의 부분 또는 "channels" 파라미터내의 단일 채널로 호출됩니다. 
	///		  "channels" 파라미터내의 호출 순서와 채널의 순서는 Subscribe 오퍼레이션의 "channels" 파라미터의 채널 순서와 다를 수 있습니다.
	/// </summary>
	public void OnSubscribed(string[] channels, bool[] results)
	{
		
	}

	/// <summary>
	/// Unsubscribe 오퍼레이션의 결과. 채널이 이제 구독되지 않으면 채널명을 리턴합니다.
	/// 설명: Unsubscribe 오퍼레이션으로 여러개의 채널이 전송되었다면, 
	///		  OnUnsubscribed 이 여러번 호출될 것이고, 
	///		  각 호출은 "channels" 파라미터내의 배열 또는 단일 채널로 호출됩니다. 
	///		  "channels" 파라미터내의 호출 순서와 채널의 순서는 Unsubscribe 오퍼레이션의 "channels" 파라미터의 채널 순서와 다를 수 있습니다.
	/// </summary>
	public void OnUnsubscribed(string[] channels)
	{

	}

	/// <summary>
	/// 채팅 네트워크 디버그 리턴 함수 (UI로 제공해주는것이 좋음)
	/// </summary>
	public void DebugReturn(ExitGames.Client.Photon.DebugLevel level, string message)
	{
		//if (level == ExitGames.Client.Photon.DebugLevel.ERROR)
		//{
		//	UnityEngine.Debug.LogError(message);
		//}
		//else if (level == ExitGames.Client.Photon.DebugLevel.WARNING)
		//{
		//	UnityEngine.Debug.LogWarning(message);
		//}
		//else
		//{
		//	UnityEngine.Debug.Log(message);
		//}
	}

	/// <summary>
	/// 편집기가 응답하지 않게하려면 OnDestroy에서 모든 Photon 연결 해제
	/// </summary>
	public void OnDestroy()
	{
		if(chatClient != null)
		{
			chatClient.Disconnect();
		}
	}

	/// <summary>
	/// 편집기가 응답하지 않게하려면 OnApplicationQuit에서 모든 Photon 연결 해제
	/// </summary>
	public void OnApplicationQuit()
	{
		if(chatClient != null)
		{
			chatClient.Disconnect();
		}
	}

	/// <summary>
	/// 어플리케이션이 일시정지 상태일 때 호출되는 함수
	/// </summary>
	/// <param name="paused"></param>
	public void OnApplicationPause(bool paused)
	{
		if (paused)
		{
			chatClient.Disconnect();
		}
	}

	#endregion

	#region 채팅 클라이언트

	/// <summary>
	/// 팀 또는 길드에 메세지 전달 함수(공개 메세지만)
	/// </summary>
	public void SendChatMessage(string inputMessage)
	{
        if (string.IsNullOrEmpty(inputMessage))
        {
            //인풋메세지가 비어있음으로
            //메세지 입력이 되지 않았음을
            //UI를 통해 알려주어야한다
            return;
        }
        else
        {
            //길드에게 보내는건지
            //팀에게 보내는건지 구분필요..
            //일단은 채널을 팀으로
            this.chatClient.PublishMessage(TeamSystemManager.Instance.currentTeamChannelName, inputMessage);
        }
	}

	/// <summary>
	/// 상대방 상태 업데이트
	/// </summary>
	void IChatClientListener.OnStatusUpdate(string user, int status, bool gotMessage, object message)
	{
		// 친구 상태 업데이트
		// 현재 친구는 아니지만 친구 요청을 하고난 후 상대방 데이터 또한 저장되어 있음
		// 피요청자의 상태가 변했을 때(Online => Offline, Offline => Online....)
		if (FriendSystemManager.Instance.friendItemDictionary.ContainsKey(user) == true)
		{
			FriendItem friendItem = FriendSystemManager.Instance.friendItemDictionary[user];
			if (friendItem != null)
			{
				// 상대방 상태 변경
				friendItem.OnFriendStatusUpdate(status, gotMessage, message);

				// 상대방의 상태가 메세지를 받을 수 있는 상황의 경우
				bool checkStatus = (friendItem.userStatus == ChatUserStatus.Online) ? true : false;

				// 현재 채팅을 할 수 없는 경우 대응 해야함
				//if(chatClient.CanChat == false)
				//{
				//	OnChatStateChange(chatClient.State);
				//}

				// 친구 요청을 한 상태
				if (friendItem.relationStatus == FriendItem.FriendRelationStatus.REQUESTING && checkStatus == true)
				{
					// 친추추가 메세지 전송
					chatClient.SendPrivateMessage(user, SocialMessage.FRIEND_REQUEST);
				}
				else if (friendItem.relationStatus == FriendItem.FriendRelationStatus.ACCEPT && checkStatus == true)
				{
					// 친추 수락 메세지 전송
					chatClient.SendPrivateMessage(user, SocialMessage.FRIEND_ACCEPT);

					// 관계상태 변경
					FriendSystemManager.Instance.friendItemDictionary[user].relationStatus = FriendItem.FriendRelationStatus.FRIEND;
				}
			}
		}
	}

	#endregion
}
