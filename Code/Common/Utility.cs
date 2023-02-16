using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Communication;

public static class Utility
{
	// seed : 랜덤값을 만드는데 기준이 되는 초기값
	public static T[] ShuffleArray<T>(T[] array, int seed)
	{
		// 가짜 랜덤 숫자 생성기
		System.Random prng = new System.Random(seed);

		// 카드 섞이에서 마지막꺼는 마지막과 교환하기때문에 생략가능 하다
		// 그러므로 array.Length - 1
		for(int iCnt = 0; iCnt < array.Length - 1; iCnt++)
		{
			// 랜덤인덱스 할당
			int randomIdx = prng.Next(iCnt, array.Length);
			// 카드 섞이에서 카드 교체 작업
			T tempItem = array[randomIdx];
			array[randomIdx] = array[iCnt];
			array[iCnt] = tempItem;
		}

		return array;
	}

	public static string GenerateRandomAlphanumeric(int length = 44, bool removeMistakableChar = true)
	{
		string guid = Guid.NewGuid().ToString("N");

		string str = Convert.ToBase64String(Encoding.UTF8.GetBytes(guid));

		str = str.Substring(0, length);

		if (removeMistakableChar)
		{
			// 대문자로 변환
			str = str.ToUpper();

			// 오해하기 쉬운 숫자를 대체
			StringBuilder sb = new StringBuilder(str);

			sb.Replace("0", "2");
			sb.Replace("O", "Z");
			sb.Replace("8", "4");
			sb.Replace("S", "Y");
			sb.Replace("1", "X");
			sb.Replace("9", "6");

			str = sb.ToString();
		}

		return str;
	}

	public static DateTime UtcToLocal(DateTime utcTime)
	{
		TimeSpan offset = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now);
		return DateTime.SpecifyKind(utcTime + offset, DateTimeKind.Local);
	}

	public static T TryParse<T>(string value)
	{
		try
		{
			return (T)Enum.Parse(typeof(T), value);
		}
		catch
		{
			return default(T);
		}
	}

	public static List<T> ToList<T>(this ArrayList arrayList)
	{
		List<T> list = new List<T>(arrayList.Count);
		foreach (T instance in arrayList)
		{
			list.Add(instance);
		}
		return list;
	}

	public static T GetRandom<T>(this List<T> list)
	{
		return list[UnityEngine.Random.Range(0, list.Count)];
	}

	public static void ChangeSkin(GameObject playerPrefab, string characterTypeName, string skinName)
	{
		 Renderer playerRenderer;                    // 플레이어 렌더러
		 Material playerMaterial;                    // 플레이어 머테리얼
		 //GameObject playerHeadAccessory;           // 플레이어 머리 악세사리
		 //GameObject playerChestAccessory;          // 플레이어 가슴 악세사리
	
		// 렌더러 등록
		Transform meshObject = playerPrefab.transform.Find("Mesh").GetComponent<Transform>();
		playerRenderer = meshObject.Find(characterTypeName).GetComponent<Renderer>();

		//// 플레이어 머리 악세사리 오브젝트
		//playerHeadAccessory = playerPrefab.transform.Find("Head_Accessories_locator").GetComponent<GameObject>();

		//// 플레이어 가슴 악세사리 오브젝트
		//playerHeadAccessory = playerPrefab.transform.Find("Accessories_locator").GetComponent<GameObject>();
		
		playerMaterial = Resources.Load<Material>("Materials/" + characterTypeName + "/" + skinName);

		playerRenderer.material = playerMaterial;
	}

	/////////////////////////////////////////////////////////////////////////////////////
	// 오브젝트 변수를 딕셔너리로 변경하는 함수
	/////////////////////////////////////////////////////////////////////////////////////
	public static Dictionary<TKey, TValue> ChangedTheObjectToADictionary<TKey, TValue>(object obj)
	{
		var stringDictionary = obj as Dictionary<TKey, TValue>;

		if (stringDictionary != null)
		{
			return stringDictionary;
		}
		var baseDictionary = obj as IDictionary;

		if (baseDictionary != null)
		{
			var dictionary = new Dictionary<TKey, TValue>();
			foreach (DictionaryEntry keyValue in baseDictionary)
			{
				if (!(keyValue.Value is TValue))
				{
					// value is not TKey. perhaps throw an exception
					return null;
				}
				if (!(keyValue.Key is TKey))
				{
					// value is not TValue. perhaps throw an exception
					return null;
				}

				dictionary.Add((TKey)keyValue.Key, (TValue)keyValue.Value);
			}
			return dictionary;
		}
		// object is not a dictionary. perhaps throw an exception
		return null;
	}

	/////////////////////////////////////////////////////////////////////////////////////
	// 데이터 리스트 문자열로 변환하여 리스트로 반환하는 함수
	/////////////////////////////////////////////////////////////////////////////////////
	public static List<string> GetStringListFromFriendInfoField(NCMB.NCMBObject obj, string key)
	{
		if (obj[key] != null)
		{
			ArrayList arrayList = obj[key] as ArrayList;

			if (arrayList != null)
			{
				return arrayList.ToList<string>();
			}
		}

		return new List<string>();
	}

	/////////////////////////////////////////////////////////////////////////////////////
	// 데이터 리스트 문자열로 변환하여 딕셔너리로 반환하는 함수
	/////////////////////////////////////////////////////////////////////////////////////
	public static Dictionary<string, string> GetStringDictionaryFromFriendInfoField(NCMB.NCMBObject obj, string key)
	{
		if (obj[key] != null)
		{
			return ChangedTheObjectToADictionary<string, string>(obj[key]);
		}
		return new Dictionary<string, string>();
	}

	/////////////////////////////////////////////////////////////////////////////////////
	// 캐릭터 타입에 따라 인덱스를 반환하는 함수
	/////////////////////////////////////////////////////////////////////////////////////
	public static int FetchIndexAccordingToCharacterType(string characterType)
	{
		if (CharacterTypeClass.BUNNY == characterType)
			return 0;
		else if (CharacterTypeClass.BEAR == characterType)
			return 1;
		else if (CharacterTypeClass.CAT == characterType)
			return 2;
		else
			return -1;
	}
}
