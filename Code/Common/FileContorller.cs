using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FileContorller<T>
{ 
	/// <summary>
	/// 데이터 파일을 직렬화로 저장하는 함수
	/// </summary>
	public static void SaveData(T data, string path, string fileName)
	{
		// 경로와 파일 이름 결합
		string combinePath = Path.Combine(path, fileName);

		try
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			// 파일 생성
			FileStream fileStream = File.Create(combinePath);
			
			binaryFormatter.Serialize(fileStream, data);
			fileStream.Close();
		}
		catch (Exception ex)
		{
			Debug.LogError("<color=red>파일을 저장하는데 예외가 발생하였습니다.</color>");
			Debug.LogErrorFormat("<color=red>예외 메세지: {0}</color>", ex.Message);
		}
	}

	/// <summary>
	/// 데이터 파일을 로드하고 역직렬하여 데이터를 로드하는 함수
	/// </summary>
	/// <returns></returns>
	public static bool LoadData(ref T data, string path, string fileName)
	{
		// 경로와 파일 이름 결합
		string combinePath = Path.Combine(path, fileName);

		// 파일이 존재하지 않으면 리턴
		if (File.Exists(combinePath) == false)
			return false;

		// 도구 생성
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		FileStream fileStream = File.Open(combinePath, FileMode.Open);

		if (fileStream != null && fileStream.Length > 0)
		{
			// 역 직렬화로 데이터 가지고 옮
			data = (T)binaryFormatter.Deserialize(fileStream);

			// 파일 닫기
			fileStream.Close();

			// 읽어오면 파일 삭제
			DeleteData(path);

			return true;
		}
		else
		{
			// 파일 닫기
			fileStream.Close();

			return false;
		}
	}

	/// <summary>
	/// 게임 데이터 파일 삭제 함수
	/// </summary>
	/// <returns></returns>
	public static bool DeleteData(string path)
	{
		//string path = "Text/PlayingData.bytes";
		if (File.Exists(path) == true)
		{
			try
			{
				File.Delete(path);
			}
			catch (Exception ex)
			{
				Debug.LogError("파일 삭제중 예외 발생");
				Debug.LogErrorFormat("예외 메세지: {0}", ex.Message);
			}
		}
		return false;
	}

}

[Serializable]
public class PlayingData
{
	public string roomName;
	public DateTime gameStartTime;
}