using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using NCMB;

namespace Communication
{
	[RequireComponent(typeof(DataStoreCoroutine))]
	public class GameSettingManager : MonoSingleton<GameSettingManager>
	{
		private DataStoreCoroutine dataStoreCoroutine;

		private GameSettingInfo currentGameSettingInfo;

		public bool IsLoaded
		{
			get
			{
				return currentGameSettingInfo != null;
			}
		}

		protected override void Awake()
		{
			base.Awake();

			dataStoreCoroutine = GetComponent<DataStoreCoroutine>();

			//currentGameSettingInfo = new GameSettingInfo();

			DontDestroyOnLoad(this.gameObject);
		}

		public IEnumerator FetchGameSetting()
		{
			NCMBQuery<NCMBObject> query = new NCMBQuery<NCMBObject>(DataStoreClass.GAMESETTING);

			// 이 파일은 하나만 존재
			query.Limit = 1;

			IEnumerator coroutine = dataStoreCoroutine.FindAsyncCoroutine(query, Main.Instance.ForceToTitle);
			 yield return StartCoroutine(coroutine);

			if (coroutine.Current != null)
			{
				List<NCMBObject> ncmbObjectList = (List<NCMBObject>)coroutine.Current;

				// 오브젝트 리스트에 정보가 하나도 없다면
				if (!ncmbObjectList.Any())
				{
					// 데이터 스토어에 없는 경우는 GameSettingInfo 클래스의 초기값으로 작동한다 //
					currentGameSettingInfo = new GameSettingInfo();
					yield break;
				}
				else
				{
					currentGameSettingInfo = new GameSettingInfo(ncmbObjectList.First());
				}

				// 단말기에 갱신 이력이 있는지 없는지
				if (PlayerPrefs.HasKey(PlayerPrefsKey.GAMESETTING_UPDATEDATE))
				{
					// 날짜가 바뀌었는지
					string dateTimeString
						= PlayerPrefs.GetString(PlayerPrefsKey.GAMESETTING_UPDATEDATE);

					DateTime lastUpdateDateTime
						= DateTime.FromBinary(Convert.ToInt64(dateTimeString));

					// 날짜 변화가 잇는 경우에 한해 배너 이미지를 다시 취득
					if (lastUpdateDateTime != currentGameSettingInfo.updateDate)
					{
						yield return UpdateResourceFiles();
					}
				}
				else
				{
					yield return UpdateResourceFiles();
				}

				// 단말기에 갱신 파일의 작성 날짜를 저장
				PlayerPrefs.SetString(PlayerPrefsKey.GAMESETTING_UPDATEDATE,
					currentGameSettingInfo.updateDate.ToBinary().ToString());

				PlayerPrefs.Save();
			}
		}

		private IEnumerator UpdateResourceFiles()
		{
			// 배너 파일을 다시 취득
			if(string.IsNullOrEmpty(currentGameSettingInfo.bannerFileName))
			{
				// 새로운 배너파일 존재 시 기존 배너파일 삭제
				//fileStoreMananger.DeleteBannerCacheFile();
				yield break;
			}

			//yield return fileStoreManager.FetchBannerFileCoroutine(currentGameSettingInfo.bannerFileName, Main.Instance.ForceToTitle);
			// 일단 리턴
			yield return null;
		}

		public string GetUpdateDateString()
		{
			if (currentGameSettingInfo.updateDate == new DateTime())
			{
				return string.Empty;
			}

			return currentGameSettingInfo.updateDate.ToString("yyyy-MM-dd HH:mm:ss");
		}

		public string GetTermsOfUse()
		{
			return currentGameSettingInfo.termsOfUse;
		}

		public bool IsServiceEnable()
		{
			return currentGameSettingInfo.isServiceEnable;
		}
	}
}