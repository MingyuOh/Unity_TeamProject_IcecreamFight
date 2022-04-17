using NCMB;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Communication
{
	public class UserAuth : MonoSingleton<UserAuth>
	{
		private string _userID = null;
		public string userID
		{
			get { return _userID; }
		}
		private string _nickName = null;
		public string nickName
		{
			get { return _nickName; }
		}
		[HideInInspector]
		public string characterType = null;
		[HideInInspector]
		public string skinType = null;

		/// <summary>
		///  테스트
		/// </summary>
		/// 

		public IEnumerator TestSignUpCoroutine(
		string userName,
		string password,
		UnityAction<NCMBException> errorCallback)
		{
			NCMBUser user = new NCMBUser();
			user.UserName = userName;
			user.Password = password;

			bool isConnecting = true;

			user.SignUpAsync((NCMBException e) =>
			{
				if (e != null)
				{
					errorCallback(e);
				}

				// 처음 로그인 사용자인지 체크
				if (IsFirstUser(user) == true)
				{
					StartCoroutine(SignUpCoroutine(user, Main.Instance.ForceToTitle));
				}

				isConnecting = false;
			});

			while (isConnecting)
			{
				yield return null;
			}

		}

		public IEnumerator LogInCoroutine(string userName, string password, UnityAction<NCMBException> errorCallback)
		{
			bool isConnecting = true;

			NCMBUser.LogInAsync(userName, password, (NCMBException e) =>
			{
				if (e != null)
				{
					errorCallback(e);
				}

				isConnecting = false;
			});

			while (isConnecting) { yield return null; }
		}

		#region 회원 정보 함수
		/////////////////////////////////////////////////////////////////////////////////////
		// 회원가입 코루틴
		/////////////////////////////////////////////////////////////////////////////////////
		public IEnumerator SignUpCoroutine(NCMBUser user, UnityAction<NCMBException> errorCallback)
		{
			bool isConnecting = true;

			// 기본 데이터 생성
			SaveFirstUserData(user);

			// 공개 데이터 생성
			yield return PlayerInfoManager.Instance.CreatePlayerInfoDataCoroutine(errorCallback);

			// 친구 정보 데이터 생성
			yield return FriendSystemManager.Instance.CreateOwnFriendInfoDataCoroutine(errorCallback);

			// 회원가입
			user.SignUpAsync((NCMBException e) =>
			{

				if (e != null)
				{
					errorCallback(e);
				}

				isConnecting = false;
				Debug.Log("회원가입 성공!");
			});

			while(isConnecting == true)
			{
				yield return null;
			}
		}

		/////////////////////////////////////////////////////////////////////////////////////
		// 처음 등록 시 데이터베이스에 등록하는 함수
		/////////////////////////////////////////////////////////////////////////////////////
		public void SaveFirstUserData(NCMBUser user)
		{
			string nickName = string.Empty;

			// 스킨 테이블
			Dictionary<string, List<string>> SkinTable = new Dictionary<string, List<string>>();
			SkinTable.Add(CharacterTypeClass.BEAR, new List<string> { "Bear00" });	
			SkinTable.Add(CharacterTypeClass.BUNNY, new List<string> { "Bunny00" });
			SkinTable.Add(CharacterTypeClass.CAT, new List<string> { "Cat00" });

			// 친구 리스트
			List<PlayerInfo> friendList = new List<PlayerInfo>();

			user.Add(UserKey.NICKNAME, nickName);									// 닉네임
			user.Add(UserKey.CHARACTERTYPE, CharacterTypeClass.BEAR);				// 캐릭터 타입
			user.Add(UserKey.SKINTYPE, "Bear00");									// 스킨 타입
			user.Add(UserKey.HEAD_ACCESSORY_TYPE, string.Empty);					// 머리 악세사리
			user.Add(UserKey.CHEST_ACCESSORY_TYPE, string.Empty);					// 가슴 악세사리
			user.Add(UserKey.COINCOUNT, 0);											// 골드(가상화폐)
			user.Add(UserKey.DIAMONDCOUNT, 0);										// 다이아(가상화폐)
			user.Add(UserKey.SKINTABLE, SkinTable);									// 스킨 테이블
			user.Add(UserKey.HEAD_ACCESSORY_LIST, new List<string>());				// 머리 악세사리 리스트
			user.Add(UserKey.CHEST_ACCESSORY_LIST, new List<string>());				// 가슴 악세사리 리스트
			user.Add(UserKey.TIER, "Bronze");										// 티어
			user.Add(UserKey.POINT, 1000);											// 점수
			user.Add(UserKey.FRIENDID_LIST, friendList);                            // 친구리스트
			user.Add(UserKey.DAILYBONUSFLAG, true);                                 // 데일리 보너스
		}

		///////////////////////////////////////////////////  //////////////////////////////////
		// 현재 유저 정보 저장 코루틴
		/////////////////////////////////////////////////////////////////////////////////////
		public IEnumerator SaveAsyncCurrentUserCoroutine(UnityAction<NCMBException> errorCallback)
		{
			bool isConnecting = true;

			NCMBUser.CurrentUser.SaveAsync((NCMBException e) =>
			{
				if(e != null)
				{
					errorCallback(e);
				}

				isConnecting = false;
			});

			while (isConnecting == true)
			{
				yield return null;
			}
		}

		/////////////////////////////////////////////////////////////////////////////////////
		// 로그인 날짜 체크 함수(메인 씬 시작과 동시 코루틴에서 사용되야함)
		/////////////////////////////////////////////////////////////////////////////////////
		public IEnumerator FetchLoginDateSpan(UnityAction<NCMBException> errorCallback)
		{
			// 마지막 세이브 시각과 단말기의 날짜가 하루를 지났는지 확인
			if((DateTime.Now.Date - GetUpdateTime().Date).Days == 0)
			{
				yield return 0;
				yield break;
			}

			// Now.Date는 신뢰할 수 없으므로 NCMB의 날짜를 갱신하여 엄격히 체크
			// 마지막 update 날짜를 취득
			DateTime lastUpdateDate = GetUpdateTime().Date;

			// NCMBUser는 저장을 할 필요가 없는 경우 통신을 수행하지 않으므로
			// DailyBonus 플래그를 변경한다.
			bool value = (bool)NCMBUser.CurrentUser[UserKey.DAILYBONUSFLAG];
			NCMBUser.CurrentUser[UserKey.DAILYBONUSFLAG] = !value;

			// 날짜 갱신을 위해 세이브 실행
			bool isConnecting = true;
			NCMBUser.CurrentUser.SaveAsync((NCMBException e) =>
			{
				if(e != null)
				{
					errorCallback(e);
				}

				isConnecting = false;
			});

			while(isConnecting == true)
			{
				yield return null;
			}

			// 갱신 후의 update 날짜와 비교하여 날 수를 반환한다.
			yield return (GetUpdateTime().Date - lastUpdateDate).Days;
		}

		/////////////////////////////////////////////////////////////////////////////////////
		// SNS에 연결되어있는지 체크 함수
		/////////////////////////////////////////////////////////////////////////////////////
		public bool CheckConnectwithSNS(string snsName)
		{
			// sns에 연결되어있으면 true 반환
			if (NCMBUser.CurrentUser.IsLinkWith(snsName) == true)
			{
				return true;
			}
			return false;
		}

		/////////////////////////////////////////////////////////////////////////////////////
		// 처음 사용자인지 여부 체크 함수
		/////////////////////////////////////////////////////////////////////////////////////
		public bool IsFirstUser(NCMBUser user)
		{
			if(user.CreateDate == user.UpdateDate)
			{
				return true; 
			}
			return false;
		}

		#endregion

		#region 유저 정보 추가 함수(비공개데이터)

		/////////////////////////////////////////////////////////////////////////////////////
		// 데이터베이스 리스트 요소 추가 함수 
		// (친구 / 머리 / 가슴 악세사리 리스트에 공통으로 사용되지만 
		//  key의 모호성때문에 각각의 함수 만들어 놓음)
		/////////////////////////////////////////////////////////////////////////////////////
		public void AddObjectInList(string key, string objectId)
		{
			List<string> keyList = GetStringListFromUserField(key);

			keyList.Add(objectId);

			NCMBUser.CurrentUser[key] = new ArrayList(keyList);

			NCMBUser.CurrentUser.SaveAsync();
		}

		public void SaveNickName(string nickName)
		{
			if (NCMBUser.CurrentUser != null && NCMBUser.CurrentUser.ContainsKey(UserKey.NICKNAME))
			{
				NCMBUser.CurrentUser[UserKey.NICKNAME] = nickName;

				NCMBUser.CurrentUser.SaveAsync();
			}
		}

		/////////////////////////////////////////////////////////////////////////////////////
		// 코인 수 추가 함수
		/////////////////////////////////////////////////////////////////////////////////////
		public void AddCoinCount(int num)
		{
			if(NCMBUser.CurrentUser != null && NCMBUser.CurrentUser.ContainsKey(UserKey.COINCOUNT))
			{
				NCMBUser.CurrentUser[UserKey.COINCOUNT] = GetCoinCount() + num;

				NCMBUser.CurrentUser.SaveAsync();
			}
		}

		/////////////////////////////////////////////////////////////////////////////////////
		// 다이아 수 추가 함수
		/////////////////////////////////////////////////////////////////////////////////////
		public void AddDiamondCount(int num)
		{
			if (NCMBUser.CurrentUser != null && NCMBUser.CurrentUser.ContainsKey(UserKey.DIAMONDCOUNT))
			{
				NCMBUser.CurrentUser[UserKey.DIAMONDCOUNT] = GetDiamondCount() + num;

				NCMBUser.CurrentUser.SaveAsync();
			}
		}

		/////////////////////////////////////////////////////////////////////////////////////
		// 점수 추가 함수
		/////////////////////////////////////////////////////////////////////////////////////
		public void AddPointCount(int num)
		{
			if(NCMBUser.CurrentUser != null && NCMBUser.CurrentUser.ContainsKey(UserKey.POINT))
			{
				NCMBUser.CurrentUser[UserKey.POINT] = GetPointCount() + num;

				NCMBUser.CurrentUser.SaveAsync();
			}
		}

		/////////////////////////////////////////////////////////////////////////////////////
		// 친구 추가 함수
		/////////////////////////////////////////////////////////////////////////////////////
		public void AddFriend(string userId)
		{
			List<string> friendList = GetStringListFromUserField(UserKey.FRIENDID_LIST);

			if(friendList.Contains(userId) == true)
			{
				// 이미 존재하는 친구 입니다.
				return;
			}

			friendList.Add(userId);

			NCMBUser.CurrentUser[UserKey.FRIENDID_LIST] = new ArrayList(friendList);

			NCMBUser.CurrentUser.SaveAsync();
		}

		/////////////////////////////////////////////////////////////////////////////////////
		// 친구 제거 함수
		/////////////////////////////////////////////////////////////////////////////////////
		public void RemoveFriend(string userId)
		{
			List<string> friendList = GetStringListFromUserField(UserKey.FRIENDID_LIST);

			if (friendList.Contains(userId) == false)
			{
				// 친구목록에 해당 플레이어 존재하지 않음
				return;
			}

			friendList.Remove(userId);

			NCMBUser.CurrentUser[UserKey.FRIENDID_LIST] = new ArrayList(friendList);

			NCMBUser.CurrentUser.SaveAsync();
		}

		#endregion

		#region 반환 함수

		/////////////////////////////////////////////////////////////////////////////////////
		// 로그인 시간 반환 함수
		/////////////////////////////////////////////////////////////////////////////////////
		public DateTime GetUpdateTime()
		{
			DateTime time = (DateTime)NCMBUser.CurrentUser.UpdateDate;
			return Utility.UtcToLocal(time);
		}

		/////////////////////////////////////////////////////////////////////////////////////
		// 유저 닉네임 반환 함수
		/////////////////////////////////////////////////////////////////////////////////////
		public string GetNickName()
		{
			if(NCMBUser.CurrentUser[UserKey.NICKNAME] as string == string.Empty)
			{
				Main.Instance.ForceToTitle("닉네임이 존재하지 않습니다.");
				return string.Empty;
			}

			if (NCMBUser.CurrentUser != null)
			{
				return NCMBUser.CurrentUser[UserKey.NICKNAME] as string;
			}

			return "Unknown";
		}

		/////////////////////////////////////////////////////////////////////////////////////
		// 유저 캐릭터 타입 반환 함수
		/////////////////////////////////////////////////////////////////////////////////////
		public string GetCharacterType()
		{
			if (NCMBUser.CurrentUser != null && NCMBUser.CurrentUser.ContainsKey(UserKey.CHARACTERTYPE))
			{
				return NCMBUser.CurrentUser[UserKey.CHARACTERTYPE] as string;
			}

			return "Unknown";
		}

		/////////////////////////////////////////////////////////////////////////////////////
		// 유저 캐릭터 스킨 반환 함수
		/////////////////////////////////////////////////////////////////////////////////////
		public string GetSkinType()
		{
			if (NCMBUser.CurrentUser != null && NCMBUser.CurrentUser.ContainsKey(UserKey.SKINTYPE))
			{
				return NCMBUser.CurrentUser[UserKey.SKINTYPE] as string;
			}

			return "Unknown";
		}

		/////////////////////////////////////////////////////////////////////////////////////
		// 유저 코인 수 반환 함수
		/////////////////////////////////////////////////////////////////////////////////////
		public int GetCoinCount()
		{
			if (NCMBUser.CurrentUser != null && NCMBUser.CurrentUser.ContainsKey(UserKey.COINCOUNT))
			{
				return Convert.ToInt32(NCMBUser.CurrentUser[UserKey.COINCOUNT]);
			}

			return 0;
		}

		/////////////////////////////////////////////////////////////////////////////////////
		// 유저 다이아 수 반환 함수
		/////////////////////////////////////////////////////////////////////////////////////
		public int GetDiamondCount()
		{
			if (NCMBUser.CurrentUser != null && NCMBUser.CurrentUser.ContainsKey(UserKey.DIAMONDCOUNT))
			{
				return Convert.ToInt32(NCMBUser.CurrentUser[UserKey.DIAMONDCOUNT]);
			}

			return 0;
		}

		/////////////////////////////////////////////////////////////////////////////////////
		// 유저 점수 반환 함수
		/////////////////////////////////////////////////////////////////////////////////////
		public int GetPointCount()
		{
			if (NCMBUser.CurrentUser != null && NCMBUser.CurrentUser.ContainsKey(UserKey.POINT))
			{
				return Convert.ToInt32(NCMBUser.CurrentUser[UserKey.POINT]);
			}

			return 0;
		}

		/////////////////////////////////////////////////////////////////////////////////////
		// 유저 티어 반환 함수
		/////////////////////////////////////////////////////////////////////////////////////
		public string GetTier()
		{
			return NCMBUser.CurrentUser[UserKey.TIER] as string;
		}

		/////////////////////////////////////////////////////////////////////////////////////
		// 친구 리스트 반환 함수
		/////////////////////////////////////////////////////////////////////////////////////
		public List<string> GetFriendIdList()
		{
			return GetStringListFromUserField(UserKey.FRIENDID_LIST);
		}

		/////////////////////////////////////////////////////////////////////////////////////
		// 머리 악세사리 리스트 반환 함수
		/////////////////////////////////////////////////////////////////////////////////////
		public List<string> GetHeadAccessoryList()
		{
			return GetStringListFromUserField(UserKey.HEAD_ACCESSORY_LIST);
		}

		/////////////////////////////////////////////////////////////////////////////////////
		// 가슴 악세사리 리스트 반환 함수
		/////////////////////////////////////////////////////////////////////////////////////
		public List<string> GetChestAccessoryList()
		{
			return GetStringListFromUserField(UserKey.CHEST_ACCESSORY_LIST);
		}

		/////////////////////////////////////////////////////////////////////////////////////
		// 친구 수 반환 함수
		/////////////////////////////////////////////////////////////////////////////////////
		public int GetFriendCount()
		{
			return 0;
		}

		/////////////////////////////////////////////////////////////////////////////////////
		// 데이터베이스에 존재하는 key의 이름을 갖는 리스트 반환 함수
		/////////////////////////////////////////////////////////////////////////////////////
		public List<string> GetKeyList(string key)
		{
			return GetStringListFromUserField(key);
		}

		/////////////////////////////////////////////////////////////////////////////////////
		// 스킨 테이블 반환 함수                       (테스트 필요 String을 잘 가져오는지 모름)
		/////////////////////////////////////////////////////////////////////////////////////
		public Dictionary<string, List<string>> GetSkinTable()
		{
			if (NCMBUser.CurrentUser != null && NCMBUser.CurrentUser.ContainsKey(UserKey.SKINTABLE))
			{
				return GetSKinDictionaryFromNCMBUserField(UserKey.SKINTABLE);
			}
			return new Dictionary<string, List<string>>();
		}

		/////////////////////////////////////////////////////////////////////////////////////
		// NCMB 데이터 리스트 문자열로 변환하여 반환하는 함수
		/////////////////////////////////////////////////////////////////////////////////////
		public List<string> GetStringListFromUserField(string key)
		{
			if(NCMBUser.CurrentUser.ContainsKey(key))
			{
				ArrayList arrayList = NCMBUser.CurrentUser[key] as ArrayList;

				if(arrayList != null)
				{
					return arrayList.ToList<string>();
				}
			}

			return new List<string>();
		}

		/////////////////////////////////////////////////////////////////////////////////////
		// NCMB 데이터 Dictionary value값을 문자열로 변환하여 반환하는 함수(이거 테스트 필요)
		/////////////////////////////////////////////////////////////////////////////////////
		public Dictionary<string, List<string>> GetSKinDictionaryFromNCMBUserField(string key)
		{
			if (NCMBUser.CurrentUser.ContainsKey(key))
			{
				Dictionary<string, List<string>> table = NCMBUser.CurrentUser[key] as Dictionary<string, List<string>>;

				if (table != null)
				{
					return table;
				}
			}

			return new Dictionary<string, List<string>>();
		}

		/// <summary>
		/// 나중 수정 
		/// </summary>
		public void LoadUserInfo()
		{
			//if(_nickName == null)
			{
				_nickName = GetNickName();
			}

			//if(characterType == null)
			{
				characterType = GetCharacterType();
			}

			//if(skinType == null)
			{
				skinType = GetSkinType();
			}
		}

		#endregion
	}
}