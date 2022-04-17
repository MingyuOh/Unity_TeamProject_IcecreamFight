using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInfoData : MonoSingleton<GameInfoData>
{
    public List<string> Face;

    void Awake()
    {
        DataLoad("Assets/Resources/Char/Skin/");
    }


    private void DataLoad(string DataPath)
    {
        System.IO.DirectoryInfo Di = new System.IO.DirectoryInfo(DataPath);

        foreach (System.IO.FileInfo File in Di.GetFiles())
        {
            if (File.Extension.ToLower().CompareTo(".png") == 0)
            {
                string FileName = File.Name.Substring(0, File.Name.Length);
                Face.Add(FileName);
            }
        }
    }
}
