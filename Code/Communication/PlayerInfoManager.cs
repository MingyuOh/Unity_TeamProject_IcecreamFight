using NCMB;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Communication;

[RequireComponent(typeof(DataStoreCoroutine))]
public class PlayerInfoManager : MonoSingleton<PlayerInfoManager>
{
	private DataStoreCoroutine dataStoreCoroutine;

	protected override void Awake()
	{
		// 싱글톤 생성
		base.Awake();
		dataStoreCoroutine = GetComponent<DataStoreCoroutine>();
	}

	/////////////////////////////////////////////////////////////////////////////////////
	// 플레이어 공개 데이터 생성 함수
	/////////////////////////////////////////////////////////////////////////////////////
	public IEnumerator CreatePlayerInfoDataCoroutine(UnityAction<NCMBException> errorCallback)
	{
		NCMBObject obj = PlayerInfo.CreateObject();

		IEnumerator coroutine = dataStoreCoroutine.SaveAsyncCoroutine(obj, errorCallback);
		yield return StartCoroutine(coroutine);

		NCMBUser.CurrentUser[UserKey.PLAYERINFO] = (NCMBObject)coroutine.Current;
	}

	/////////////////////////////////////////////////////////////////////////////////////
	// 플레이어 공개데이터 가져오는 함수
	/////////////////////////////////////////////////////////////////////////////////////
	public IEnumerator FetchOwnDataCoroutine(UnityAction<NCMBException> errorCallback)
	{
		if (NCMBUser.CurrentUser == null)
		{
			errorCallback(new NCMBException());
			yield break;
		}

		yield return dataStoreCoroutine.FetchAsyncCoroutine((NCMBObject)NCMBUser.CurrentUser[UserKey.PLAYERINFO], errorCallback);
	}

	/////////////////////////////////////////////////////////////////////////////////////
	// 플레이어 공개데이터 삭제 함수
	/////////////////////////////////////////////////////////////////////////////////////
	public IEnumerator DeleteOwnDataCoroutine(UnityAction<NCMBException> errorCallback)
	{
		if (NCMBUser.CurrentUser != null)
		{
			NCMBObject playerInfo = (NCMBObject)NCMBUser.CurrentUser[UserKey.PLAYERINFO];

			if (playerInfo != null)
			{
				yield return dataStoreCoroutine.DeleteAsyncCoroutine(playerInfo, errorCallback);
			}
		}
	}

	/////////////////////////////////////////////////////////////////////////////////////
	// 플레이어가 입력한 DB NickName으로 사용자를 검색하는 함수
	/////////////////////////////////////////////////////////////////////////////////////
	public IEnumerator SearchByNickNameCoroutine(string nickName, List<string> friendList, UnityAction<NCMBException> errorCallback)
	{
		NCMBQuery<NCMBObject> query = new NCMBQuery<NCMBObject>(DataStoreClass.PLAYERINFO_LIST);

		// 자신과 친구리스트 탐색에서 제외
		NCMBObject me = (NCMBObject)NCMBUser.CurrentUser[UserKey.PLAYERINFO];

		List<string> tmpFriendList = friendList.ToList();
		tmpFriendList.Add(me[PlayerInfoKey.USERID] as string);

		// 리스트와 일치하지 않는
		query.WhereNotContainedIn(PlayerInfoKey.USERID, tmpFriendList);
			
		// 닉네임과 일치하는
		query.WhereEqualTo(PlayerInfoKey.NICKNAME, nickName);

		// 최신 접속일 순서로 정렬
		query.OrderByDescending(PlayerInfoKey.UPDATE_DATE);

		IEnumerator coroutine
			= dataStoreCoroutine.FindAsyncCoroutine(query, errorCallback);

		yield return StartCoroutine(coroutine);

		if(coroutine.Current != null)
		{
			List<NCMBObject> objList = (List<NCMBObject>)coroutine.Current;
			yield return objList.ConvertAll(obj => new PlayerInfo(obj)).ToList();
		}
	}

	/////////////////////////////////////////////////////////////////////////////////////
	// 플레이어가 입력한 DB User ID로 사용자를 검색하는 함수
	/////////////////////////////////////////////////////////////////////////////////////
	public IEnumerator SearchByUserIDCoroutine(string userID, List<string> friendList, UnityAction<NCMBException> errorCallback)
	{
		NCMBQuery<NCMBObject> query = new NCMBQuery<NCMBObject>(DataStoreClass.PLAYERINFO_LIST);

		// 자신과 친구리스트 탐색에서 제외
		NCMBObject me = (NCMBObject)NCMBUser.CurrentUser[UserKey.PLAYERINFO];

		List<string> tmpFriendList = new List<string>();
		if(friendList != null)
		{
			tmpFriendList.CopyTo(friendList.ToArray());
		}
		tmpFriendList.Add(me[PlayerInfoKey.USERID] as string);

		// 리스트와 일치하지 않는
		query.WhereNotContainedIn(PlayerInfoKey.USERID, tmpFriendList);

		// 오브젝트 ID와 일치하는
		query.WhereEqualTo(PlayerInfoKey.USERID, userID);

		// 최신 접속일 순서로 정렬
		query.OrderByDescending(PlayerInfoKey.UPDATE_DATE);

		IEnumerator coroutine
			= dataStoreCoroutine.FindAsyncCoroutine(query, errorCallback);

		yield return StartCoroutine(coroutine);

		if (coroutine.Current != null)
		{
			List<NCMBObject> objList = (List<NCMBObject>)coroutine.Current;
			yield return objList.ConvertAll(obj => new PlayerInfo(obj)).ToList();
		}
	}

	/////////////////////////////////////////////////////////////////////////////////////
	// 플레이어 친구의 정보를 가지고오는 함수
	/////////////////////////////////////////////////////////////////////////////////////
	public IEnumerator FetchFriendsPlayerInfoCoroutine(List<string> friendList, UnityAction<NCMBException> errorCallback)
	{
		NCMBQuery<NCMBObject> query = new NCMBQuery<NCMBObject>(DataStoreClass.PLAYERINFO_LIST);

		if (friendList.Count > 0)
		{
			query.WhereContainedIn(PlayerInfoKey.USERID, friendList);
		}
			
		IEnumerator coroutine = dataStoreCoroutine.FindAsyncCoroutine(query, errorCallback);
		yield return StartCoroutine(coroutine);

		if (coroutine.Current != null)
		{
			List<NCMBObject> myfriendList = (List<NCMBObject>)coroutine.Current;
			yield return myfriendList.ConvertAll(friend => new PlayerInfo(friend)).ToList();
		}
	}


	#region 플레이어 공개데이터 Save 함수
	/////////////////////////////////////////////////////////////////////////////////////
	// 닉네임 저장 함수
	/////////////////////////////////////////////////////////////////////////////////////
	public IEnumerator SaveNicknameToOwnData(string nickName, UnityAction<NCMBException> errorCallback)
	{
		NCMBObject ownPlayerInfo = (NCMBObject)NCMBUser.CurrentUser[UserKey.PLAYERINFO];
		ownPlayerInfo[PlayerInfoKey.NICKNAME] = nickName;
		yield return dataStoreCoroutine.SaveAsyncCoroutine(ownPlayerInfo, errorCallback);
	}

	/////////////////////////////////////////////////////////////////////////////////////
	// 티어 저장 함수
	/////////////////////////////////////////////////////////////////////////////////////
	public IEnumerator SaveTierToOwnData(string tierName, UnityAction<NCMBException> errorCallback)
	{
		NCMBObject ownPlayerInfo = (NCMBObject)NCMBUser.CurrentUser[UserKey.PLAYERINFO];
		ownPlayerInfo[PlayerInfoKey.TIER] = tierName;
		yield return dataStoreCoroutine.SaveAsyncCoroutine(ownPlayerInfo, errorCallback);
	}

	/////////////////////////////////////////////////////////////////////////////////////
	// 캐릭터 타입 저장 함수
	/////////////////////////////////////////////////////////////////////////////////////
	public IEnumerator SaveCharacterTypeToOwnData(string characterType, UnityAction<NCMBException> errorCallback)
	{
		NCMBObject ownPlayerInfo = (NCMBObject)NCMBUser.CurrentUser[UserKey.PLAYERINFO];
		ownPlayerInfo[PlayerInfoKey.CHARACTERTYPE] = characterType;
		yield return dataStoreCoroutine.SaveAsyncCoroutine(ownPlayerInfo, errorCallback);
	}

	/////////////////////////////////////////////////////////////////////////////////////
	// 캐릭터 스킨 저장 함수
	/////////////////////////////////////////////////////////////////////////////////////
	public IEnumerator SaveCharacterSkinToOwnData(string characterSkin, UnityAction<NCMBException> errorCallback)
	{
		NCMBObject ownPlayerInfo = (NCMBObject)NCMBUser.CurrentUser[UserKey.PLAYERINFO];
		ownPlayerInfo[PlayerInfoKey.SKINTYPE] = characterSkin;
		yield return dataStoreCoroutine.SaveAsyncCoroutine(ownPlayerInfo, errorCallback);
	}

	/////////////////////////////////////////////////////////////////////////////////////
	// 캐릭터 머리 악세사리 저장 함수
	/////////////////////////////////////////////////////////////////////////////////////
	public IEnumerator SaveHeadAccessoryToOwnData(string headAccessory, UnityAction<NCMBException> errorCallback)
	{
		NCMBObject ownPlayerInfo = (NCMBObject)NCMBUser.CurrentUser[UserKey.PLAYERINFO];
		ownPlayerInfo[PlayerInfoKey.HEAD_ACCESSORY_TYPE] = headAccessory;
		yield return dataStoreCoroutine.SaveAsyncCoroutine(ownPlayerInfo, errorCallback);
	}

	/////////////////////////////////////////////////////////////////////////////////////
	// 캐릭터 가슴 악세사리 저장 함수
	/////////////////////////////////////////////////////////////////////////////////////
	public IEnumerator SaveChestAccessoryToOwnData(string chestAccessory, UnityAction<NCMBException> errorCallback)
	{
		NCMBObject ownPlayerInfo = (NCMBObject)NCMBUser.CurrentUser[UserKey.PLAYERINFO];
		ownPlayerInfo[PlayerInfoKey.SKINTYPE] = chestAccessory;
		yield return dataStoreCoroutine.SaveAsyncCoroutine(ownPlayerInfo, errorCallback);
	}

	#endregion

}