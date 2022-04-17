using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Communication;
using NCMB;
using UI;

[RequireComponent(typeof(DataStoreCoroutine))]
public class FriendSystemManager : MonoSingleton<FriendSystemManager>
{
	#region Private variable

	private NetworkSocialManager networkSocialManager;						// 네트워크 소셜 매니저
	private UserAuth userAuth;												// 유저 권한
	private DataStoreCoroutine dataStoreCoroutine;                          // 데이터 코루틴 함수

	#endregion

	#region Public variable

	public List<string> friendList;											// 친구 리스트
	public List<PlayerInfo> friendPlayerInfoList = new List<PlayerInfo>();  // 친구 정보 리스트

	public Dictionary<string, FriendItem> friendItemDictionary
		= new Dictionary<string, FriendItem>();

	#endregion

	#region Unity Method

	protected override void Awake()
	{
		// 싱글톤 생성
		base.Awake();

		dataStoreCoroutine = GetComponent<DataStoreCoroutine>();
	}

	private void Start()
	{
		networkSocialManager = NetworkSocialManager.Instance;
		userAuth = UserAuth.Instance;
	}

	#endregion

	/////////////////////////////////////////////////////////////////////////////////////////////////////
	///////////////////////////////////////// 추가 요소 작업 ////////////////////////////////////////////
	/////////////////////////////////////////////////////////////////////////////////////////////////////
	/// 친구 상태를 지속적으로 갱신해주어야 함(친구목록 버튼을 클릭하였을때)
	/// 오프라인이었던 친구가 온라인 상태로 바뀌면								 
	/// friendStateDic의 정보를 갱신하고 friendPlayerInfoList에서 유저 정보를 가지고 와 UI를 갱신
	////////////////////////////////////////////////////////////////////////////////////////////////////

	/// <summary>
	/// 친구 시스템 데이터 초기화 함수
	/// </summary>
	public IEnumerator InitFriendSystemDataCoroutine()
	{
		// 현재 포톤 채팅 연결 여부 확인
		while(networkSocialManager.chatClient.CanChat == false)
		{
			yield return null;
		}

		// 친구추가 정보 데이터 로드
		int count = 0;
		IEnumerator coroutine = FetchFriendInfoDataStoreCoroutine(NCMBUser.CurrentUser.UserName, Main.Instance.ForceToTitle);
		yield return StartCoroutine(coroutine);

		if (coroutine.Current != null)
		{
			// 친구 정보 데이터스토어
			List<Communication.FriendInfo> friendInfo = (List<Communication.FriendInfo>)coroutine.Current;
			for (count = 0; count < friendInfo.Count; count++)
			{
				// 친구 요청리스트 체크
				if(friendInfo[count].requestIdNicknameDictionary.Count > 0 && friendInfo[count].requestIdNicknameDictionary != null)
				{
					// ( + ) UI와 요청목록에 표시하기 위함
					foreach (KeyValuePair<string, string> items in friendInfo[count].requestIdNicknameDictionary)
					{
						// 요청정보 응용프로그램에 저장
						// 요청자 FriendItem 등록
						FriendItem friendItem = new FriendItem(items.Value, FriendItem.FriendRelationStatus.REQUESTING);
						friendItemDictionary.Add(items.Key, friendItem);
					}
				}

				// 친구 수락리스트 체크
				if (friendInfo[count].acceptIdList.Count > 0 && friendInfo[count].acceptIdList != null)
				{
					for (int acceptCnt = 0; acceptCnt < friendInfo[count].acceptIdList.Count; acceptCnt++)
					{
						// DB에 친구 등록
						userAuth.AddFriend(friendInfo[count].acceptIdList[acceptCnt]);
					}

					// 변경된 AcceptIDList 적용
					yield return DeleteAcceptIDListToOwnData(friendInfo[count].acceptIdList, Main.Instance.ForceToTitle);
				}
			}

			// 친구 목록 로드
			friendList = userAuth.GetFriendIdList();

			// 친구의 공개 데이터 로드
			if (friendList.Count > 0)
			{
				// 친구의 공개데이터 로드
				coroutine = PlayerInfoManager.Instance.FetchFriendsPlayerInfoCoroutine(friendList, Main.Instance.ForceToTitle);
				yield return StartCoroutine(coroutine);

				if (coroutine.Current != null)
				{
					// 얘를 UI 부분에 넘겨주어 처리
					List<PlayerInfo> playerInfo = (List<PlayerInfo>)coroutine.Current;
					for (count = 0; count < playerInfo.Count; count++)
					{
						// 친구 공개 정보 추가
						friendPlayerInfoList.Add(playerInfo[count]);

						// FriendItem 등록(내 친구들 등록)
						FriendItem friendItem = new FriendItem(playerInfo[count].nickName, FriendItem.FriendRelationStatus.FRIEND);
						friendItemDictionary.Add(playerInfo[count].userId, friendItem);
					}
				}

				// 유저아이템 사전 값을 토대로 오름차순으로 정렬
				//var items = from pair in friendItemDictionary orderby pair.Value ascending select pair;
				friendItemDictionary.OrderBy(i => i.Value);

				// 포톤 채팅 친구리스트에 등록
				networkSocialManager.chatClient.AddFriends(friendList.ToArray());

				// 포톤 친구리스트에 등록
				UpdateAFriendListToAPhoton();
			}
		}
	}

	/// <summary>
	/// 친구 찾기 함수
	/// </summary>
	public IEnumerator FindFriendCoroutine(string nickName)
	{
		IEnumerator coroutine = PlayerInfoManager.Instance.SearchByNickNameCoroutine(nickName, friendList, Main.Instance.ForceToTitle);
		yield return StartCoroutine(coroutine);

		if (coroutine.Current != null)
		{
			List<PlayerInfo> playerInfoList = (List<PlayerInfo>)coroutine.Current;
			if(playerInfoList.Count == 1)
			{
				PlayerInfo playerInfo = playerInfoList[0];
				yield return playerInfo;
			}
			else
			{
				yield return null;
			}
		}
	}
	
	/// <summary>
	/// 친구 요청 함수(친구 수와 내 자신 제한 걸어야 함 - 지금은 때려박기 식 OOP화 필요)
	/// </summary>
	public IEnumerator FriendRequestCoroutine(PlayerInfo playerInfo)
	{
		int maxFriendNum = 20;

		if (friendList.Count >= maxFriendNum)
		{
			// 친구목록이 가득 찼습니다.
			yield break;
		}
		else if (playerInfo.userId == NCMBUser.CurrentUser.UserName as string)
		{
			// 내 자신을 추가할 수 없습니다.
			yield break;
		}

		// 중복 체크
		if (friendItemDictionary.ContainsKey(playerInfo.userId) == true)
		{
			FriendItem item = friendItemDictionary[playerInfo.userId];
			if (item.relationStatus == FriendItem.FriendRelationStatus.FRIEND)
			{
				Debug.LogFormat("{0} 님이 친구목록에 존재합니다.", playerInfo.nickName);
			}
			else if (item.relationStatus == FriendItem.FriendRelationStatus.REQUESTING)
			{
				// 이미 친구 추가요청을 전송하였습니다.
				Debug.LogFormat("{0}님에게 이미 친구 요청을 하였습니다.", playerInfo.nickName);
			}

		}

		// 현재 응용프로그램에서 친구추가 대상을 등록(상대방이 온라인에 대비하기 위함)
		friendItemDictionary.Add(playerInfo.userId, new FriendItem(playerInfo.nickName, FriendItem.FriendRelationStatus.REQUESTING));

		// 친구 + 친구요청 유저들의 Key(userID)값을 리스트로 변환
		List<string> requestFriendList = new List<string>(friendItemDictionary.Keys);

		// Photon Chat FriendList에 친구 추가
		networkSocialManager.chatClient.AddFriends(requestFriendList.ToArray());

		// 상대방 DB에 내 정보 추가
		yield return AddFriendInRequestList(playerInfo.userId, userAuth.GetNickName(), Main.Instance.ForceToTitle);
	}

	/// <summary>
	/// 친구추가 요청 메세지 함수(피요청자 Call)
	/// </summary>
	public IEnumerator RequestAddFriendMessage(string sender)
	{
		// 요청 메세지를 받았으니 sender의 정보를 토대로
		// 요청 목록 UI에 해당 유저 정보를 뿌려주어야 함
		// (요청자가 온라인이고 피요청자가 오프라인에서 온라인 상태는 요청자가
		// 메세지를 보내옮으로 friendItemDictionary에 추가적으로 들어가지 않게 중복작업 필요)
		// 서로 온라인이면 이 메세지를 전달 받음
		
		if(friendItemDictionary.ContainsKey(sender) == true)
			yield break;

		// 플레이어 정보 로드
		IEnumerator coroutine = PlayerInfoManager.Instance.SearchByUserIDCoroutine(sender, friendList, Main.Instance.ForceToTitle);
		yield return StartCoroutine(coroutine);

		if (coroutine.Current != null)
		{
			List<PlayerInfo> playerInfo = (List<PlayerInfo>)coroutine.Current;
			for (int count = 0; count < playerInfo.Count; count++)
			{
				// 친구아이템 리스트 요소를 변경 또는 추가
				if (friendItemDictionary.ContainsKey(sender) == true)
				{
					// friendItemDictionary에 존재한다면 관계상태 변경
					friendItemDictionary[sender].relationStatus = FriendItem.FriendRelationStatus.REQUESTED;
				}  
				else
				{
					// 아이템 추가
					FriendItem friendItem = new FriendItem(playerInfo[count].nickName, FriendItem.FriendRelationStatus.REQUESTED);
					friendItemDictionary.Add(sender, friendItem);
				}

                // 클라이언트 단에서 UI를 표시해주어야 함
                RequestImg.Instance.ActivateRequestImg();
			}
		}
	}

	/// <summary>
	/// 친구추가 수락 함수(요청자 UserID)
	/// </summary>
	public IEnumerator AcceptAddFriend(string target, UnityAction<NCMBException> errorCallback)
	{
		// 카운트
		int count = 0;

		// DB에 요청자 친구 등록
		userAuth.AddFriend(target);

		// 상대방 AcceptIdList에 추가(온/오프라인 상관없이 추가)
		// (나중에 상태에 따라서 처리하는 로직으로 변경)
		IEnumerator coroutine = FetchFriendInfoObjectCoroutine(target, errorCallback);
		yield return StartCoroutine(coroutine);

		if(coroutine.Current != null)
		{
			List<NCMBObject> objList = (List<NCMBObject>)coroutine.Current;
			for(count = 0; count < objList.Count; count++)
			{
				objList[count].AddToList(FriendInfoKey.ACCEPTID_LIST, NCMBUser.CurrentUser.UserName);
				yield return dataStoreCoroutine.SaveAsyncCoroutine(objList[count], errorCallback);
			}
		}

		// FriendInfo 데이터 스토어에 존재하는 
		// 자신의 requestInfoList에서 target 데이터를 제거
		coroutine = FetchFriendInfoObjectCoroutine(NCMBUser.CurrentUser.UserName, errorCallback);
		yield return StartCoroutine(coroutine);

		if (coroutine.Current != null)
		{
			List<NCMBObject> objList = (List<NCMBObject>)coroutine.Current;
			for (count = 0; count < objList.Count; count++)
			{
				Dictionary<string, string> requestDictionary = Utility.ChangedTheObjectToADictionary<string, string>(objList[count][FriendInfoKey.REQUESTID_NICKNAME_DICTIONARY]);
				if(requestDictionary.ContainsKey(target) == true)
				{
					requestDictionary.Remove(target);
				}
				objList[count][FriendInfoKey.REQUESTID_NICKNAME_DICTIONARY] = requestDictionary;
				yield return dataStoreCoroutine.SaveAsyncCoroutine(objList[count], errorCallback);
			}
		}

		// 플레이어 정보 로드
		coroutine = PlayerInfoManager.Instance.SearchByUserIDCoroutine(target, friendList ,errorCallback);
		yield return StartCoroutine(coroutine);

		if(coroutine.Current != null)
		{
			List<PlayerInfo> playerInfo = (List<PlayerInfo>)coroutine.Current;
			for (count = 0; count < playerInfo.Count; count++)
			{
				// 플레이어 정보 추가
				friendPlayerInfoList.Add(playerInfo[count]);

				// 친구아이템 리스트 요소를 변경 또는 추가
				if (friendItemDictionary.ContainsKey(target) == true)
				{
					// friendItemDictionary에 존재한다면 관계상태 변경
					// 요청자가 온라인이라면 OnStatusUpdate에서 메세지를 보낸 후
					// 관계상태를 FriendItem.FriendRelationStatus.FRIEND로 변경
					friendItemDictionary[target].relationStatus = FriendItem.FriendRelationStatus.ACCEPT;
				}
				else
				{
					// 아이템 추가
					FriendItem friendItem = new FriendItem(playerInfo[count].nickName, FriendItem.FriendRelationStatus.ACCEPT);
					friendItemDictionary.Add(target, friendItem);
				}
			}
		}

		// 친구 리스트에 추가
		friendList.Add(target);

		// 친구 + 친구요청 유저들의 Key(userID)값을 리스트로 변환
		List<string> requestFriendList = new List<string>(friendItemDictionary.Keys);

		// 포톤채팅 친구리스트에 추가
		networkSocialManager.chatClient.AddFriends(requestFriendList.ToArray());

		// Photon 친구리스트 업데이트
		UpdateAFriendListToAPhoton();

		// 수락 버튼이 눌렸을 때 이 함수가 호출된다면
		// 함수 로직 종료 후 UI 제거
		RequestContent.Instance.DeletefriendRequestSlot(target);
        SocialPanel.Instance.SetRequestNumText(SocialPanel.Instance.GetRequestNum() - 1);
        RequestImg.Instance.SetrequestNum(RequestImg.Instance.GetrequestNum() - 1);
        if (SocialPanel.Instance.GetRequestNum() == 0)
        {
            SocialPanel.Instance.DeActivateRequestImg();
		}
    }

	/// <summary>
	/// 친구추가 수락 메세지 함수(요청자 Call)
	/// </summary>
	public IEnumerator AcceptAddFriendMessage(string sender)
	{
		if (friendItemDictionary.ContainsKey(sender) == true)
		{
			// 관계상태 변경
			friendItemDictionary[sender].relationStatus = FriendItem.FriendRelationStatus.FRIEND;
		}
		else
		{
			// 아이템 추가
			FriendItem friendItem = new FriendItem(sender, FriendItem.FriendRelationStatus.FRIEND);
			friendItemDictionary.Add(sender, friendItem);
		}

		// 플레이어 정보 탐색
		// 플레이어 정보가 없으면 DB에서 탐색
		// 리스트에 존재할 때 해당 플레이어의 정보가 필요하면
		// int senderPlayerInfoIndex = FindPlayerAtFriendInfoList(sender);
		// FindPlayerAtFriendInfoList[senderPlayerInfoIndex]로 찾으면 됨
		if (FindPlayerAtFriendPlayerInfoList(sender) == -1)
		{
			IEnumerator coroutine = PlayerInfoManager.Instance.SearchByUserIDCoroutine(sender, friendList, Main.Instance.ForceToTitle);
			yield return StartCoroutine(coroutine);

			if (coroutine.Current != null)
			{
				List<PlayerInfo> playerInfo = (List<PlayerInfo>)coroutine.Current;
				for (int count = 0; count < playerInfo.Count; count++)
				{
					friendPlayerInfoList.Add(playerInfo[count]);
				}
			}
		}
		
		// 친구 추가
		userAuth.AddFriend(sender);

		// AcceptIDList에서 삭제
		yield return DeleteObjectAtAcceptIDListToOwnData(sender, Main.Instance.ForceToTitle);

		// 친구 리스트에 추가
		friendList.Add(sender);
			
		// 친구 + 친구요청 유저들의 Key(userID)값을 리스트로 변환
		List<string> requestFriendList = new List<string>(friendItemDictionary.Keys);

		// 포톤채팅 친구리스트에 추가
		networkSocialManager.chatClient.AddFriends(requestFriendList.ToArray());

		// Photon 친구리스트 업데이트
		UpdateAFriendListToAPhoton();
		
	}
	/// <summary>
	/// 친구추가 거절 함수
	/// </summary>
	public IEnumerator RefuseAddFriend(string target)
	{
		if(friendItemDictionary.ContainsKey(target) == true)
		{
			// DB에서 요청자 제거
			IEnumerator coroutine = FetchFriendInfoObjectCoroutine(NCMBUser.CurrentUser.UserName, Main.Instance.ForceToTitle);
			yield return StartCoroutine(coroutine);

			if(coroutine.Current != null)
			{
				List<NCMBObject> objList = (List<NCMBObject>)coroutine.Current;
				List<string> targetList = new List<string>();

				for (int count = 0; count < objList.Count; count++)
				{
					// 타겟 추가
					targetList.Add(target);

					// 요정자 DB에서 제거
					Dictionary<string, string> requestDictionary = Utility.ChangedTheObjectToADictionary<string, string>(objList[count][FriendInfoKey.REQUESTID_NICKNAME_DICTIONARY]);
					if (requestDictionary.ContainsKey(target) == true)
					{
						requestDictionary.Remove(target);
					}
					objList[count][FriendInfoKey.REQUESTID_NICKNAME_DICTIONARY] = requestDictionary;
					yield return dataStoreCoroutine.SaveAsyncCoroutine(objList[count], Main.Instance.ForceToTitle);
				}

				// 거절 메세지 전송
				networkSocialManager.chatClient.SendPrivateMessage(target, SocialMessage.FRIEND_REFUSE);

				// 해당 플레이어 딕셔너리에서 삭제
				friendItemDictionary.Remove(target);
			}
            // UI에서 해당 플레이어 삭제
            RequestContent.Instance.DeletefriendRequestSlot(target);
            SocialPanel.Instance.SetRequestNumText(SocialPanel.Instance.GetRequestNum() - 1);
            RequestImg.Instance.SetrequestNum(RequestImg.Instance.GetrequestNum() - 1);
            if (SocialPanel.Instance.GetRequestNum() == 0)
            {
                SocialPanel.Instance.DeActivateRequestImg();
            }
        }
		else
		{
			// 해당 유저 존재하지 않음(이건 말이 안됨 UI에서 다 가져가서 보여주기 때문)
			// 어딘가에서 삭제되지 않아서 생기는 문제(UI 추가 후 문제 발생시 이 부분 참조)
		}
	}

	/// <summary>
	/// 친구추가 거절 메세지 함수(요청자 Call)
	/// </summary>
	public void RefuseAddFriendMessage(string sender)
	{
		if(friendItemDictionary.ContainsKey(sender) == true)
		{
			// 친구 + 친구요청 유저들의 Key(userID)값을 리스트로 변환
			List<string> requestFriendList = new List<string>(friendItemDictionary.Keys);

			// friendItemDictionary에서 삭제
			friendItemDictionary.Remove(sender);

			// 딕셔너리의 키를 리스트로 변환하였을 때 sender삭제
			requestFriendList.Remove(sender);

			// 포톤채팅 친구리스트에 추가
			networkSocialManager.chatClient.AddFriends(requestFriendList.ToArray());
		}

		// 플레이어 정보 삭제
		int indexTargetInFriendPlayerInfoList = FindPlayerAtFriendPlayerInfoList(sender);
		if(indexTargetInFriendPlayerInfoList != -1)
		{
			friendPlayerInfoList.RemoveAt(indexTargetInFriendPlayerInfoList);
		}

	}

	/// <summary>
	/// 친구리스트에서 해당 유저 탐색
	/// </summary>
	public int FindPlayerAFriendtList(string target)
	{
		for (int count = 0; count < friendList.Count; count++)
		{
			if (target == friendList[count])
			{
				return count;
			}
		}

		return -1;
	}

	/// <summary>
	/// 친구 정보 리스트에서 해당 유저 탐색
	/// </summary>
	public int FindPlayerAtFriendPlayerInfoList(string target)
	{
		for (int count = 0; count < friendPlayerInfoList.Count; count++)
		{
			if (target == friendPlayerInfoList[count].userId)
			{
				return count;
			}
		}

		return -1;
	}

	/// <summary>
	/// FriendItemDictionary에서 해당 유저 탐색
	/// </summary>
	public FriendItem FindPlayerAtFriendItemDictionary(string userID)
	{
		if(friendItemDictionary.ContainsKey(userID) == true)
		{
			return friendItemDictionary[userID];
		}

		return null;
	}

	/// <summary>
	/// FriendPlayerInfoList에서 target 유저의 정보 반환 함수
	/// </summary>
	public PlayerInfo FetchPlayerInfoAtFriendPlayerList(string userID)
	{
		int targetNum = FindPlayerAtFriendPlayerInfoList(userID);
		if(targetNum != -1)
		{
			return friendPlayerInfoList[targetNum];
		}

		return null;
	}

	/// <summary>
	/// 포톤에 친구리스트 적용 함수
	/// </summary>
	public void UpdateAFriendListToAPhoton()
	{
		if (PhotonNetwork.connected == false || PhotonNetwork.Server != ServerConnection.MasterServer)
		{
			// 네트워크 연결 문제 UI 띄워주어야함
			return;
		}

		// PhotonNetwork에 저장
		if (friendList.Count > 0)
		{
			PhotonNetwork.FindFriends(friendList.ToArray());
		}
	}


	#region 데이터 전용 함수

	/// <summary>
	/// FriendInfo 데이터 생성 함수
	/// </summary>
	public IEnumerator CreateOwnFriendInfoDataCoroutine(UnityAction<NCMBException> errorCallback)
	{
		// 오브젝트 생성
		NCMBObject obj = Communication.FriendInfo.CreateObject();

		IEnumerator coroutine = dataStoreCoroutine.SaveAsyncCoroutine(obj, errorCallback);
		yield return StartCoroutine(coroutine);

		NCMBUser.CurrentUser[UserKey.FRIENDINFO] = (NCMBObject)coroutine.Current;
	}

	/// <summary>
	/// FriendInfo 데이터 로드 함수
	/// </summary>
	public IEnumerator FetchOwnFriendInfoDataCoroutine(UnityAction<NCMBException> errorCallback)
	{
		if (NCMBUser.CurrentUser == null)
		{
			errorCallback(new NCMBException());
			yield break;
		}

		yield return dataStoreCoroutine.FetchAsyncCoroutine((NCMBObject)NCMBUser.CurrentUser[UserKey.FRIENDINFO], errorCallback);
	}

	/// <summary>
	/// FriendInfo 데이터 삭제 함수
	/// </summary>
	public IEnumerator DeleteOwnFriendInfoDataCoroutine(UnityAction<NCMBException> errorCallback)
	{
		if (NCMBUser.CurrentUser != null)
		{
			NCMBObject friendInfo = (NCMBObject)NCMBUser.CurrentUser[UserKey.FRIENDINFO];

			if (friendInfo != null)
			{
				yield return dataStoreCoroutine.DeleteAsyncCoroutine(friendInfo, errorCallback);
			}
		}
	}

	/// <summary>
	/// FriendInfo 데이터의 어트리뷰트인 RequestList에 user 추가하는 함수
	/// </summary>
	public IEnumerator AddFriendInRequestList(string userId, string nickName, UnityAction<NCMBException> errorCallback)
	{
		string myUserID = NCMBUser.CurrentUser.UserName;

		IEnumerator coroutine = FetchFriendInfoObjectCoroutine(userId, errorCallback);
		yield return StartCoroutine(coroutine);

		if (coroutine.Current != null)
		{
			List<NCMBObject> objList = (List<NCMBObject>)coroutine.Current;
			// count가 1이상이면 문제있음
			for (int count = 0; count < objList.Count; count++)
			{
				Dictionary<string, string> requestDictionary = objList[count][FriendInfoKey.REQUESTID_NICKNAME_DICTIONARY] as Dictionary<string, string>;
				if (requestDictionary != null)
				{
					for (int requestIndex = 0; requestIndex < requestDictionary.Count; requestIndex++)
					{
						if (requestDictionary.ContainsKey(myUserID) == false)
						{
							// 내정보 추가
							requestDictionary.Add(myUserID, nickName);
						}
					}
				}
				else
				{
					requestDictionary = new Dictionary<string, string>();
					requestDictionary.Add(myUserID, nickName);
				}
				objList[count][FriendInfoKey.REQUESTID_NICKNAME_DICTIONARY] = requestDictionary;
				yield return dataStoreCoroutine.SaveAsyncCoroutine(objList[count], errorCallback);
			}
		}
	}

	/// <summary>
	/// 플레이어가 입력한 userID로부터 FriendInfo 데이터 가져오는 함수
	/// </summary>
	public IEnumerator FetchFriendInfoDataStoreCoroutine(string userID, UnityAction<NCMBException> errorCallback)
	{
		NCMBQuery<NCMBObject> query = new NCMBQuery<NCMBObject>(DataStoreClass.FRIEND_INFO);

		if (NCMBUser.CurrentUser != null)
		{
			// UserID와 일치하는
			query.WhereEqualTo(FriendInfoKey.USERID, userID);
		}

		IEnumerator coroutine = dataStoreCoroutine.FindAsyncCoroutine(query, errorCallback);
		yield return StartCoroutine(coroutine);

		if (coroutine.Current != null)
		{
			List<NCMBObject> myRequestList = (List<NCMBObject>)coroutine.Current;
			yield return myRequestList.ConvertAll(friend => new Communication.FriendInfo(friend)).ToList();
		}
	}

	/// <summary>
	/// 플레이어가 입력한 nickName으로부터 FriendInfo 데이터 가져오는 함수
	/// </summary>
	public IEnumerator FetchNicknameListCoroutine(string nickName, UnityAction<NCMBException> errorCallback)
	{
		NCMBQuery<NCMBObject> query = new NCMBQuery<NCMBObject>(DataStoreClass.FRIEND_INFO);

		if (NCMBUser.CurrentUser != null)
		{
			// UserID와 일치하는
			query.WhereEqualTo(FriendInfoKey.NICKNAME, nickName);
		}

		IEnumerator coroutine = dataStoreCoroutine.FindAsyncCoroutine(query, errorCallback);

		yield return StartCoroutine(coroutine);

		if (coroutine.Current != null)
		{
			List<NCMBObject> myRequestList = (List<NCMBObject>)coroutine.Current;
			yield return myRequestList.ConvertAll(friend => new Communication.FriendInfo(friend)).ToList();
		}
	}

	/// <summary>
	/// 플레이어가 입력한 userID로부터 FriendInfo의 Object를 가져오는 함수
	/// </summary>
	public IEnumerator FetchFriendInfoObjectCoroutine(string userID, UnityAction<NCMBException> errorCallback)
	{
		NCMBQuery<NCMBObject> query = new NCMBQuery<NCMBObject>(DataStoreClass.FRIEND_INFO);

		if (NCMBUser.CurrentUser != null)
		{
			// UserID와 일치하는
			query.WhereEqualTo(FriendInfoKey.USERID, userID);
		}

		IEnumerator coroutine = dataStoreCoroutine.FindAsyncCoroutine(query, errorCallback);
		yield return StartCoroutine(coroutine);

		if (coroutine.Current != null)
		{
			List<NCMBObject> myRequestList = (List<NCMBObject>)coroutine.Current;
			yield return myRequestList;
		}
	}

	/// <summary>
	/// 플레이어가 입력한 DB에 저장된 UserID로 사용자를 검색하는 함수
	/// </summary>
	public IEnumerator SearchByUserIDCoroutine(string userID, UnityAction<NCMBException> errorCallback)
	{
		NCMBQuery<NCMBObject> query = new NCMBQuery<NCMBObject>(DataStoreClass.FRIEND_INFO);

		// 자신과 친구리스트 탐색에서 제외
		NCMBObject me = (NCMBObject)NCMBUser.CurrentUser[UserKey.FRIENDINFO];

		// 임시 저장장소 생성
		List<string> tempFriendList = new List<string>();
		tempFriendList.CopyTo(friendList.ToArray());

		// 자신을 추가
		tempFriendList.Add(me[FriendInfoKey.USERID] as string);

		// 리스트와 일치하지 않는
		query.WhereNotContainedIn(FriendInfoKey.USERID, tempFriendList);
			
		// 닉네임과 일치하는
		query.WhereEqualTo(FriendInfoKey.USERID, userID);

		IEnumerator coroutine = dataStoreCoroutine.FindAsyncCoroutine(query, errorCallback);
		yield return StartCoroutine(coroutine);

		if(coroutine.Current != null)
		{
			List<NCMBObject> objList = (List<NCMBObject>)coroutine.Current;
			yield return objList.ConvertAll(obj => new Communication.FriendInfo(obj)).ToList();
		}
	}

	/// <summary>
	/// 닉네임 저장 함수
	/// </summary>
	public IEnumerator SaveNicknameToOwnData(string nickName, UnityAction<NCMBException> errorCallback)
	{
		NCMBObject ownFriendInfo = (NCMBObject)NCMBUser.CurrentUser[UserKey.FRIENDINFO];
		ownFriendInfo[FriendInfoKey.NICKNAME] = nickName;
		yield return dataStoreCoroutine.SaveAsyncCoroutine(ownFriendInfo, errorCallback);
	}

	/// <summary>
	/// AcceptIDList 삭제 함수
	/// </summary>
	public IEnumerator DeleteAcceptIDListToOwnData(List<string> acceptIDList, UnityAction<NCMBException> errorCallback)
	{
		NCMBObject ownFriendInfo = NCMBUser.CurrentUser[UserKey.FRIENDINFO] as NCMBObject;
		yield return dataStoreCoroutine.FetchAsyncCoroutine(ownFriendInfo, errorCallback);

		ownFriendInfo.RemoveRangeFromList(FriendInfoKey.ACCEPTID_LIST, acceptIDList);
		yield return dataStoreCoroutine.SaveAsyncCoroutine(ownFriendInfo, errorCallback);
	}

	/// <summary>
	/// AcceptIDList에서 한가지 데이터 삭제 함수
	/// </summary>
	public IEnumerator DeleteObjectAtAcceptIDListToOwnData(string acceptUserID, UnityAction<NCMBException> errorCallback)
	{
		NCMBObject ownFriendInfo = NCMBUser.CurrentUser[UserKey.FRIENDINFO] as NCMBObject;
		yield return dataStoreCoroutine.FetchAsyncCoroutine(ownFriendInfo, errorCallback);
		List<string> acceptIDList = Utility.GetStringListFromFriendInfoField(ownFriendInfo, FriendInfoKey.ACCEPTID_LIST);

		if (acceptIDList.Contains(acceptUserID) == true)
		{
			acceptIDList.Remove(acceptUserID);
			ownFriendInfo[FriendInfoKey.ACCEPTID_LIST] = acceptIDList;
			yield return dataStoreCoroutine.SaveAsyncCoroutine(ownFriendInfo, errorCallback);
		}
	}

	/// <summary>
	/// AcceptIDList 로드 함수
	/// </summary>
	public IEnumerator FetchObjectFriendInfo(UnityAction<NCMBException> errorCallback)
	{
		NCMBObject ownFriendInfo = (NCMBObject)NCMBUser.CurrentUser[UserKey.FRIENDINFO];
		yield return dataStoreCoroutine.FetchAsyncCoroutine(ownFriendInfo, errorCallback);
	}

	#endregion
}

namespace Communication
{
	public class FriendInfo
	{
		public readonly string userId = string.Empty;       // 자신의 userID
		public readonly string nickName = string.Empty;		// 자신의 닉네임
		public readonly Dictionary<string, string> requestIdNicknameDictionary = null;
		public readonly List<string> acceptIdList = null;

		public FriendInfo(NCMBObject obj)
		{
			// 유저 아이디
			userId = obj[FriendInfoKey.USERID] as string;

			// 닉네임
			nickName = obj[FriendInfoKey.NICKNAME] as string;

			// 요청리스트
			requestIdNicknameDictionary = Utility.GetStringDictionaryFromFriendInfoField(obj, FriendInfoKey.REQUESTID_NICKNAME_DICTIONARY);

			// 수락리스트
			acceptIdList = Utility.GetStringListFromFriendInfoField(obj, FriendInfoKey.ACCEPTID_LIST);
		}

		public static NCMBObject CreateObject()
		{
			NCMBObject obj = new NCMBObject(DataStoreClass.FRIEND_INFO);
			
			obj[FriendInfoKey.USERID] = NCMBUser.CurrentUser.UserName as string;
			obj[FriendInfoKey.NICKNAME] = NCMBUser.CurrentUser[UserKey.NICKNAME] as string;
			obj[FriendInfoKey.REQUESTID_NICKNAME_DICTIONARY] = new Dictionary<string, string>();
			obj[FriendInfoKey.ACCEPTID_LIST] = new List<string>();

			return obj;
		}
	}
}

namespace Communication
{
	using ExitGames.Client.Photon.Chat;
	public class FriendItem
	{
		public string nickName;							// 닉네임
		public int userStatus;							// 유저 상태
		private FriendRelationStatus _relationStatus;	// 유저 관계 
		public FriendRelationStatus relationStatus
		{
			get { return _relationStatus; }
			set { _relationStatus = value; }
		}

		/// <summary>
		/// 친구 상태 변경 함수
		/// </summary>
		public void OnFriendStatusUpdate(int status, bool gotMessage, object message)
		{
			switch (status)
			{
				case 1:
					userStatus = ChatUserStatus.Invisible;
					break;
				case 2:
					userStatus = ChatUserStatus.Online;
					break;
				case 3:
					userStatus = ChatUserStatus.Away;
					break;
				case 4:
					userStatus = ChatUserStatus.DND;
					break;
				case 5:
					userStatus = ChatUserStatus.LFG; // Look for group
					break;
				case 6:
					userStatus = ChatUserStatus.Playing;
					break;
				default:
					userStatus = ChatUserStatus.Offline;
					break;
			}
		}

		#region 생성자

		public FriendItem()
		{
			nickName = null;
			userStatus = ChatUserStatus.Offline;
			_relationStatus = FriendRelationStatus.NONE;
		}

		public FriendItem(string nick, FriendRelationStatus status)
		{
			nickName = nick;
			userStatus = ChatUserStatus.Offline;
			_relationStatus = status;
		}

		#endregion

		/// <summary>
		/// 친구상태 열거
		/// </summary>
		public enum FriendRelationStatus
		{
			NONE = 0,
			FRIEND,
			REQUESTING,
			REQUESTED,
			ACCEPT,
			REFUSE
		}
	}
}