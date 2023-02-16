using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NCMB;

namespace Communication
{
	/// <summary>
	/// 팀 시스템 - 친구끼리만 팀 가능
	/// </summary>
	public class TeamSystemManager : MonoSingleton<TeamSystemManager>
	{
		// 최대 팀 인원
		private const int maxTeamOfMembers = 2;

		// 팀 유무 변수
		private bool _isTeam = false;
		public bool isTeam
		{
			get { return _isTeam; }
		}

		private bool isAllReady = false;

		// 방장 유무 변수
		private bool _isTeamMaster = false;
		public bool isTeamMaster
		{
			get { return _isTeamMaster; }
		}

		private NetworkSocialManager networkSocialManager;                  // 네트워크 소셜 매니저
		private FriendSystemManager friendSystemManager;                    // 친구시스템 매니저
		private string _currentTeamChannelName = null;                       // 현재 속한 팀 채널 이름
        public string currentTeamChannelName
        {
            get { return _currentTeamChannelName; }
        }

        private Dictionary<string, string> teamChannelNameDictionary		// 채널 이름 딕셔너리
			= new Dictionary<string, string>();                             // (Key = 요청자(userID), value = 팀장userID(채널명))

		public List<string> teamMatchingManageList = new List<string>();    // 팀 초대 멤버 응답 리스트
		public Dictionary<string, bool> teamMemberDictionary                // 팀 구성원 및 Ready Flag
			= new Dictionary<string, bool>();

		private void Start()
		{
			networkSocialManager = NetworkSocialManager.Instance;
			friendSystemManager = FriendSystemManager.Instance;
		}

		/// <summary>
		/// 팀 생성 함수 
		/// </summary>
		public void OnCreateTeam()
		{
			// 팀에 속함 
			_isTeam = true;

			// 방장
			_isTeamMaster = true;

            // 채널 등록
            _currentTeamChannelName = NCMBUser.CurrentUser.UserName;

			// 채널 구독
			SubscribeTeamChannel();
		}

		#region 송신자 함수

		/// <summary>
		/// 팀 초대 메세지 함수
		/// </summary>
		public void InviteAtTeamMessage(string targetUserID)
		{
			if (teamMemberDictionary.Count >= maxTeamOfMembers)
			{
				// 팀 인원이 가득 찼습니다.
				CanNotJoinTeamMessage(null, SocialMessage.TEAM_MAX);
				return;
			}

			// ResponseList에 targetUser가 존재 유무 체크
			int iCnt = FindPlayerAtTeamMatchingManageList(targetUserID);

			// 매칭 리스트에 존재하지 않는다면 리스트에 추가
			if (iCnt == -1)
			{
				teamMatchingManageList.Add(targetUserID);
			}

			// 요청 메세지 전송
			if(NCMBUser.CurrentUser.UserName == currentTeamChannelName)
			{
				networkSocialManager.chatClient.SendPrivateMessage(targetUserID, SocialMessage.TEAM_REQUEST);
			}
			else
			{
				// 방장 이외에 팀원이 초대 메세지를 보내면 팀 채널명을 전송
				networkSocialManager.chatClient.SendPrivateMessage(targetUserID, currentTeamChannelName);
			}
		}

		/// <summary>
		/// 수신자가 팀 초대를 수락했을 때 호출되는 함수
		/// </summary>
		public void ReceiverAcceptTeamMessage(string sender)
		{
			// 팀 멤버가 이미 초과했을 경우
			if(teamMemberDictionary.Count >= maxTeamOfMembers)
			{
				// 상대방에게 팀 인원초과 메세지 전송
				networkSocialManager.chatClient.SendPrivateMessage(sender, SocialMessage.TEAM_MAX);
				return;
			}

			// 팀에 추가
			teamMemberDictionary.Add(sender, false);

			// 매칭 정보 및 채널명 딕셔너리 초기화
			if(teamMemberDictionary.Count >= maxTeamOfMembers)
			{
				teamMatchingManageList.Clear();
				teamChannelNameDictionary.Clear();
			}

            //UI 알림 헤제
            SocialPanel.Instance.DeActiveInviteWaitPanel();

            PlayerInfo senderInfo = friendSystemManager.FetchPlayerInfoAtFriendPlayerList(sender);

            for (int iCnt = 0; iCnt < TeamMemberPrefabManager.Instance.menuTMprefabs.Length; iCnt++)
            {
                if (TeamMemberPrefabManager.Instance.menuTMprefabs[iCnt].isFull == false)
                    TeamMemberPrefabManager.Instance.menuTMprefabs[iCnt].SettingTeamMemberPrefab(senderInfo);
            }
        }

		/// <summary>
		/// 팀 탈퇴(탈퇴하고자 하는 사람이 실행하는 함수 - 버튼용)
		/// </summary>
		public void OnWithdrawalFromATeam()
		{
			if (_isTeamMaster == true)
			{
				foreach (KeyValuePair<string, bool> teamMember in teamMemberDictionary)
				{
					// 팀 멤버 리스트의 첫번째 유저에게 방장 인계
					networkSocialManager.chatClient.SendPrivateMessage(teamMember.Key, SocialMessage.TEAM_MASTER_WITHDRAWAL);
				}
			}

			// 팀 정보 초기화
			InitTeamInfomation();

			// 채널 구독 해지
			UnsubscribeTeamChannel(SocialMessage.TEAM_WITHDRAWAL);

            for (int iCnt = 0; iCnt < TeamMemberPrefabManager.Instance.menuTMprefabs.Length; iCnt++)
            {
                TeamMemberPrefabManager.Instance.menuTMprefabs[iCnt].ResetTeamMemberPrefab();
            }
        }

		/// <summary>
		/// 팀에서 유저 추방
		/// </summary>
		public void OnBanishFromATeam(string teamUserID)
		{
			// 팀장만 추방 가능
			if (_isTeamMaster == true)
			{
				// 팀 멤버에서 삭제
				teamMemberDictionary.Remove(teamUserID);

				// 추방 메세지 전송
				// 팀 채팅에 게시하여 다른 팀원에게 알려야 함
				networkSocialManager.chatClient.PublishMessage(currentTeamChannelName, SocialMessage.TEAM_BANISH);
			}

            for (int iCnt = 0; iCnt < TeamMemberPrefabManager.Instance.menuTMprefabs.Length; iCnt++)
            {
                if (TeamMemberPrefabManager.Instance.menuTMprefabs[iCnt].playerInfo.userId == teamUserID)
                {
                    TeamMemberPrefabManager.Instance.menuTMprefabs[iCnt].ResetTeamMemberPrefab();
                }
            }
        }

		#endregion

		#region 수신자 함수

		/// <summary>
		/// 송신자가 팀을 요청하였을 때 호출되는 함수
		/// </summary>
		public void TeamMasterRequestMessage(string sender)
		{
			// 팀가 존재하는 경우 송신자에게 메세지 전달
			if (_isTeam == true)
			{
				networkSocialManager.chatClient.SendPrivateMessage(sender, SocialMessage.ALREADY_TEAM_REQUEST);
				return;
			}

			// 팀 채널명을 저장
			teamChannelNameDictionary.Add(sender, sender);

			// 송신자 userID 매칭관리리스트에 저장
			teamMatchingManageList.Add(sender);

			//////////////////////////////////////////////////////////////////
			// 여기서부터는 의논하여 협업 작업
			// 송신자 정보(null이면 말이 안됨 어딘가 문제가 있음)
			//////////////////////////////////////////////////////////////////
			PlayerInfo senderInfo = friendSystemManager.FetchPlayerInfoAtFriendPlayerList(sender);

            // 클라이언트에 알람 UI 표시

            RequestImg.Instance.ActivateRequestImg();
        }

        /// <summary>
        /// 송신자가 팀장이 아니고 멤버가 초대하였을 때
        /// </summary>
        public void TeamMemberRequestMessage(string sender, string channelName)
		{
			// 팀가 존재하는 경우 송신자에게 메세지 전달
			if (_isTeam == true)
			{
				networkSocialManager.chatClient.SendPrivateMessage(sender, SocialMessage.ALREADY_TEAM_REQUEST);
				return;
			}

			// 팀 채널명 리스트에 저장
			teamChannelNameDictionary.Add(sender, channelName);

			// 송신자 userID 매칭관리리스트에 저장
			teamMatchingManageList.Add(sender);

			////////////////////////////////////////////////////////////////////////////
			// 이 부분도 PlayerInfo만 가지고 있을지 nickname만 얻어올지 결정해야 함
			// 송신자 정보(null이면 말이 안됨 어딘가 문제가 있음)
			// 수락했을 때 가지고 올지 
			///////////////////////////////////////////////////////////////////////////
			// 플레이어 정보 리스트
			List<PlayerInfo> playerInfos = new List<PlayerInfo>();

			PlayerInfo senderInfo = friendSystemManager.FetchPlayerInfoAtFriendPlayerList(sender);

            // 클라이언트에 알람 UI 표시
            // sender정보 표시
            RequestImg.Instance.ActivateRequestImg();
        }

		/// <summary>
		/// 팀 초대 수락 함수(버튼용)
		/// </summary>
		public void OnAcceptInviteAtTeam(string sender)
		{
			// 팀 초대 리스트에서 요청자 탐색
			int targetNum = FindPlayerAtTeamMatchingManageList(sender);
			if (targetNum != -1)
			{
				// 요청자 정보 삭제
				teamMatchingManageList.RemoveAt(targetNum);
			}
			
			// sender가 이미 팀에 속해있고 팀장이 아닐경우
			// (팀장과 송신자의 정보가 같지 않다면)
			// 팀장의 정보도 MatchingManageList에 존재
			if(teamChannelNameDictionary.ContainsKey(sender) == true)
			{
                // 채널명 취득
                _currentTeamChannelName = teamChannelNameDictionary[sender];
				if(sender != currentTeamChannelName)
				{
					// 팀장 멤버로 추가
					teamMemberDictionary.Add(currentTeamChannelName, false);

					// 팀장에게 내가 참여했다는 메세지 전달
					networkSocialManager.chatClient.SendPrivateMessage(currentTeamChannelName, SocialMessage.TEAM_ACCEPT);
				}

				// 채널명 딕셔너리에서 삭제
				teamChannelNameDictionary.Remove(sender);
			}

			// 팀 멤버에 추가
			teamMemberDictionary.Add(sender, false);

			// 팀 플래그 설정
			_isTeam = true;

			// 팀 채널 구독
			SubscribeTeamChannel();

			// 수락 메세지 전송
			networkSocialManager.chatClient.SendPrivateMessage(sender, SocialMessage.TEAM_ACCEPT);

			// 채널에 구독했으니 공개 메세지로 참여했다는 메세지를 전송
			// ((팀장이 초대했을 경우)현재 팀에 팀장 1명 팀원 1명(teamMemberList.Count == 1)일 때,
			// 이 함수를 호출하는 사람과 팀원은 서로의 정보를 모르기 때문에(서로는 친구가 아닐 수 있다.)
			// 서로 멤버로 추가시켜주어야 한다.
			networkSocialManager.chatClient.PublishMessage(currentTeamChannelName, SocialMessage.TEAM_JOINED);

            PlayerInfo senderInfo = friendSystemManager.FetchPlayerInfoAtFriendPlayerList(sender);

            for (int iCnt = 0; iCnt < TeamMemberPrefabManager.Instance.menuTMprefabs.Length; iCnt++)
            {
                if (TeamMemberPrefabManager.Instance.menuTMprefabs[iCnt].isFull == false)
                    TeamMemberPrefabManager.Instance.menuTMprefabs[iCnt].SettingTeamMemberPrefab(senderInfo);

            }
        }

		/// <summary>
		/// 팀 초대 거절 메세지 함수
		/// </summary>
		public void RefuseInviteAtTeam(string sender)
		{
			// 팀 초대 리스트에서 요청자 탐색
			int targetNum = FindPlayerAtTeamMatchingManageList(sender);
			if (targetNum != -1)
			{
				// 요청자 정보 삭제
				teamMatchingManageList.RemoveAt(targetNum);
			}

			// 거절 메세지 전송
			networkSocialManager.chatClient.SendPrivateMessage(sender, SocialMessage.TEAM_REFUSE);
		}

		/// <summary>
		/// 팀 탈퇴 처리 메세지(밴도 마찬가지)
		/// </summary>
		public void SomePlayersLeaveTheTeam(string sender)
		{
			if (FindPlayerAtTeamMemberDictionary(sender) == true)
			{
				teamMemberDictionary.Remove(sender);
			}
		}

		/// <summary>
		/// 팀장이 변경되었을 때 처리 함수
		/// </summary>
		public void ChangeTeamMaster(string sender)
		{
			// 탈퇴한 팀장 멤버에서 삭제
			if(FindPlayerAtTeamMemberDictionary(sender) == true)
			{
				teamMemberDictionary.Remove(sender);
			}

			// 마스터 권한 부여
			_isTeamMaster = true;

			// 이전 채널 구독 해제
			UnsubscribeTeamChannel(null);

            // 현재 채널 자신의 userID로 교체
            _currentTeamChannelName = NCMBUser.CurrentUser.UserName;

			// 자신의 채널 생성
			SubscribeTeamChannel();

			foreach (KeyValuePair<string, bool> teamMember in teamMemberDictionary)
			{
				// 멤버가 존재할 시 메세지 전송 
				// 다른 유저에게 팀 리빌딩 메세지 전송 
				networkSocialManager.chatClient.SendPrivateMessage(teamMember.Key, SocialMessage.TEAM_REBUILDING);
			}
		}

		/// <summary>
		/// 팀장의 교체로 팀을 리빌딩 함수
		/// </summary>
		public void RebuildingATeam(string sender)
		{
			// 현재 채널 확인
			if(currentTeamChannelName != null && currentTeamChannelName != sender)
			{
				// 이전 팀장 멤버에서 삭제
				if(FindPlayerAtTeamMemberDictionary(_currentTeamChannelName) == true)
				{
					teamMemberDictionary.Remove(_currentTeamChannelName);
				}

				// 이전 채널 구독 해제
				UnsubscribeTeamChannel(null);

                // 새로운 팀장 userID로 채널명 저장
                _currentTeamChannelName = sender;

				// 새로운 팀 채널 구독
				SubscribeTeamChannel();
			}
		}

		#endregion

		#region 공통 함수

		/// <summary>
		/// 팀 생성 시 팀 채널에 참가하기 위한 함수
		/// </summary>
		public void SubscribeTeamChannel()
		{
			if (currentTeamChannelName == null)
				return;
			
			// 팀장 userID로 팀 채널 생성
			networkSocialManager.chatClient.Subscribe(new string[] { currentTeamChannelName });
		}

		/// <summary>
		/// 팀 탈퇴 또는 추방 시 채널 구독 해지 함수
		/// </summary>
		public void UnsubscribeTeamChannel(string message)
		{
			if (message == SocialMessage.TEAM_WITHDRAWAL)
			{
				// 팀장이라면 팀장을 인계해야함
				if (_isTeamMaster == true)
				{
					_isTeamMaster = false;
				}

				// 팀에서 나갔다는 메세지를 팀 채팅에 전송
				networkSocialManager.chatClient.PublishMessage(currentTeamChannelName, message);
			}
			else if(message == SocialMessage.TEAM_BANISH)
			{
				// 팀에서 밴 당했다는 메세지를 팀원 전체에게 전달
				networkSocialManager.chatClient.PublishMessage(currentTeamChannelName, message);
			}

			// 팀 채널 구독 해지
			networkSocialManager.chatClient.Unsubscribe(new string[] { currentTeamChannelName });

            // 팀 채널명 초기화
            _currentTeamChannelName = null;
		}

		/// <summary>
		/// 팀이 최대 인원으로 구성되었을 때 서로의 정보를 통합하기 위한 함수
		/// </summary>
		public void IntegrateTeamInformation(string sender)
		{
			// 팀 멤버에 존재하지 않으면 추가 후 메세지 전송
			if (FindPlayerAtTeamMemberDictionary(sender) == false)
			{
				teamMemberDictionary.Add(sender, false);

				// 해당 유저 정보를 임시로 저장해야함
				// 의논하여 결정

				// 기존 팀 멤버리스트에 존재하지 않았다면 메세지를 전송
				networkSocialManager.chatClient.SendPrivateMessage(sender, SocialMessage.TEAM_JOINED);
			}
		}

		/// <summary>
		/// 팀에 참가할 수 없을 때 호출되는 함수(UI 공통 사용)
		/// </summary>
		public void CanNotJoinTeamMessage(string sender, string socialMessage)
		{
			if (socialMessage == SocialMessage.TEAM_MAX)
			{
				/// <summary>
				/// 팀에 최대 인원으로 구성된 경우 메세지 함수
				/// (송신자가 여러명[ ex) 4명 ]에게 초대를 보냈을 때
				/// 이전에 2명이 수락하여 팀 인원이 Max인 상태에서 
				/// 내가 수락하였을때 받는 메세지 - 수신자 Call)
				/// </summary>

				// 팀 정보 초기화
				InitTeamInfomation();

				// 팀 구독 해지
				UnsubscribeTeamChannel(null);

				// "팀이 가득 찼습니다." 알림 UI를 Show
			}
			else if (socialMessage == SocialMessage.ALREADY_TEAM_REQUEST)
			{
				/// <summary>
				/// 이미 팀에 소속된 플레이어가 팀 초대 메세지에 대해
				/// 응답하는 경우의 함수(송신자 Call)
				/// </summary>
				
				// 팀매칭관리리스트에서 삭제
				int iCnt = FindPlayerAtTeamMatchingManageList(sender);
				if (iCnt != -1)
				{
					// 매칭 리스트에서 삭제
					teamMatchingManageList.RemoveAt(iCnt);
				}

				// "이미 팀이 존재합니다." 알림 UI를 Show
			}
			else if(socialMessage == SocialMessage.TEAM_REFUSE)
			{
				// 팀매칭관리리스트에서 삭제
				int iCnt = FindPlayerAtTeamMatchingManageList(sender);
				if (iCnt != -1)
				{
					// 매칭 리스트에서 삭제
					teamMatchingManageList.RemoveAt(iCnt);
				}

				// "상대방이 초대를 거절하였습니다." 알림 UI를 Show 
			}
		}

		/// <summary>
		/// 메세지를 보낸 유저가 팀일 경우 ready상태로 변경
		/// </summary>
		/// <param name="sender"></param> 메세지를 보낸 팀 유저
		public void TeamMemberReadyMessage(string sender)
		{
			if(FindPlayerAtTeamMemberDictionary(sender) == true)
			{
				// 준비완료
				teamMemberDictionary[sender] = true;
			}

			// 팀원이 모두 준비했는지 체크
			int count = 0;
			foreach (KeyValuePair<string, bool> teamMember in teamMemberDictionary)
			{
				if (teamMember.Value == true)
					count++;
			}

			if(count == teamMemberDictionary.Count)
			{
				isAllReady = true;
			}
		}

		/// <summary>
		/// 팀 정보 초기화(매칭 관련 정보는 유지)
		/// </summary>
		private void InitTeamInfomation()
		{
			// 팀 멤버리스트 초기화
			teamMemberDictionary.Clear();

			// 팀 플래그 설정
			_isTeam = false;

			// 팀장 플래그 설정
			_isTeamMaster = false;
		}

		#endregion

		#region 유틸리티 함수

		/// <summary>
		/// 매칭리스트에 타겟 존재 유무 함수
		/// </summary>
		public int FindPlayerAtTeamMatchingManageList(string target)
		{
			if (target == null)
				return -1;

			for (int iCnt = 0; iCnt < teamMatchingManageList.Count; iCnt++)
			{
				if (teamMatchingManageList[iCnt] == target)
				{
					return iCnt;
				}
			}
			return -1;
		}

		/// <summary>
		/// 팀 리스트에 타겟 존재 유무 함수
		/// </summary>
		public bool FindPlayerAtTeamMemberDictionary(string target)
		{
			if (target == null)
				return false;
			
			if (teamMemberDictionary.ContainsKey(target) == true)
				return true;

			return false;
		}

		/// <summary>
		/// 팀 멤버의 플레이어 정보 가져오는 함수
		/// </summary>
		public List<PlayerInfo> FetchTeamMemberPlayerInfos()
		{
			// 친구정보 리스트가 존재하고 리스트 안의 수가 0보다 클때 실행
			if(friendSystemManager.friendPlayerInfoList != null || friendSystemManager.friendPlayerInfoList.Count > 0)
			{
				List<PlayerInfo> playerInfos = new List<PlayerInfo>();
				foreach(KeyValuePair<string, bool> teamMember in teamMemberDictionary)
				{
					int targetNum = friendSystemManager.FindPlayerAtFriendPlayerInfoList(teamMember.Key);
					if (targetNum != -1)
					{
						// 플레이어 정보 추가
						playerInfos.Add(friendSystemManager.friendPlayerInfoList[targetNum]);
					}
				}
				return playerInfos;
			}
			return null;
		}

		/// <summary>
		/// 팀 멤버가 ready를 했는지 체크하는 함수
		/// </summary>
		public IEnumerator CheckingTeamMemberAllReady(System.Action<bool> checking)
		{
			// 현재 모든 팀 플레이어들이 준비상태가 아니면 yield return null
			// ReadyMessage에서 처리
			while(isAllReady == false)
			{
				yield return null;
			}

			checking(isAllReady);
		}
	}
	#endregion
}
