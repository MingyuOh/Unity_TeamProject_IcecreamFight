using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using System.IO;

public class Item
{
    public int ID;
    public string Name;
    public string ImageRoute;

    public Item(int id, string name, string imageroute)
    {
        ID = id;
        Name = name;
        ImageRoute = imageroute;
    }
}

public class JsonManager : MonoBehaviour
{
    public List<Item> ItemList = new List<Item>();

    void Start()
    {
        
    }

    public void SaveFunc()
    {
        //리스트의 제이슨파일화
        JsonData ItemJson = JsonMapper.ToJson(ItemList);

        //제이슨파일로 저장
        File.WriteAllText(Application.dataPath + "/Resource/ItemData.json", ItemJson.ToString());

    }
}
