using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

[Serializable]
public class SaveData : ISerializationCallbackReceiver
{
    //싱글 톤을구현하기위한 최초 접속시에 Load
    private static SaveData _instance = null;

    public static SaveData Instance {
        get
        {
            if (_instance == null)
            {
                Load();
            }
            return _instance;
        }
    }

    // SaveData를 Json으로 변환 한 텍스트
    [SerializeField]
    private static string _jsonText = "";

    // ================================================ =================================
    // 저장된 데이터
    // ================================================ =================================

    public int SampleInt = 10;
    public string SampleString = " Sample ";
    public bool SampleBool = false;

    public  List < int > SampleIntList  =  new  List < int > () { 2 , 3 , 5 , 7 , 11 , 13 , 17 , 19 };

    [SerializeField]
    private string _sampleDictJson = " ";
    public Dictionary<string, int> SampleDict = new Dictionary<string, int>() {
    { " Key1 " , 50 },
    { " Key2 " , 150 },
    { " Key3 " , 550 }
    };

    // ================================================ =================================
    // 직렬화, 역 직렬화시의 콜백
    // ================================================ =================================

    /// < summary >
    /// SaveData → Json으로 변환되기 전에 실행된다.
    /// </ summary >
    public void OnBeforeSerialize()
    {
        // Dictionary은 그대로 저장되지 않기 때문에 직렬화하고 텍스트로 저장.
        _sampleDictJson = Serialize(SampleDict);
    }

    /// < summary >
    /// Json → SaveData로 변환 된 후 실행된다.
    /// </ summary >
    public void OnAfterDeserialize()
    {
        // 저장되어있는 텍스트가 있으면 Dictionary에 직렬화한다.
        if (!string.IsNullOrEmpty(_sampleDictJson))
        {
            SampleDict = Deserialize<Dictionary<string, int>>(_sampleDictJson);
        }
    }

    // 인수의 객체를 직렬화하고 반환
    private static string Serialize<T>(T obj)
    {
        BinaryFormatter binaryFormatter = new BinaryFormatter();
        MemoryStream memoryStream = new MemoryStream();
        binaryFormatter.Serialize(memoryStream, obj);
        return Convert.ToBase64String(memoryStream.GetBuffer());
    }

    // 인수의 텍스트를 지정된 클래스에 직렬화하고 반환
    private static T Deserialize<T>(string str)
    {
        BinaryFormatter binaryFormatter = new BinaryFormatter();
        MemoryStream memoryStream = new MemoryStream(Convert.FromBase64String(str));
        return (T)binaryFormatter.Deserialize(memoryStream);
    }

    // ================================================ =================================
    // 취득
    // ================================================ =================================

    /// < summary >
    /// 데이터를 다시로드한다.
    /// </ summary >
    public void Reload()
    {
        JsonUtility.FromJsonOverwrite(GetJson(), this);
    }

    // 데이터를 읽어 들인다.
    private static void Load()
    {
        _instance = JsonUtility.FromJson<SaveData>(GetJson());
    }

    // 저장된 Json을 얻을
    private static string GetJson()
    {
        // 이미 Json을 취득하고있는 경우는 그것을 돌려 준다.
        if (!string.IsNullOrEmpty(_jsonText))
        {
            return _jsonText;
        }

        // Json을 저장하는 경로를 받았다.
        string filePath = GetSaveFilePath();

        // Json이 있는지 조사하고 나서 취득 변환한다. 존재하지 않으면 새로운 클래스를 만들고 그것을 Json으로 변환한다.
        if (File.Exists(filePath))
        {
            _jsonText = File.ReadAllText(filePath);
        }
        else
        {
            _jsonText = JsonUtility.ToJson(new SaveData());
        }

        return _jsonText;
    }

    // ================================================ =================================
    // 저장
    // ================================================ =================================

    /// < summary >
    /// 데이터를 Json에 저장한다.
    /// </ summary >
    public void Save()
    {
        _jsonText = JsonUtility.ToJson(this);
        File.WriteAllText(GetSaveFilePath(), _jsonText);
    }

    // ================================================ =================================
    // 삭제
    // ================================================ =================================

    /// < summary >
    /// 데이터를 모두 삭제하고 초기화한다.
    /// </ summary >
    public void Delete()
    {
        _jsonText = JsonUtility.ToJson(new SaveData());
        Reload();
    }

    // ================================================ =================================
    // 저장 경로
    // ================================================ =================================

    // 저장할 위치의 경로를 받았다.
    private static string GetSaveFilePath()
    {

        string filePath = " SaveData ";

        // 확인하기 쉽도록 편집기에서 Assets와 같은 계층에 저장하고 그 이외에서는 Application.persistentDataPath 다음에 저장하도록.
        #if UNITY_EDITOR
        filePath += " .json ";
        #else
        filePath  =  Application . persistentDataPath  +  " / "  +  filePath ;
        #endif

        return filePath;
    }
}
