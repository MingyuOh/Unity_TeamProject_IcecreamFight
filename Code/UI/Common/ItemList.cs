using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemList : MonoSingleton<ItemList> {

    //public
    public Image Skin_Image;                //자식슬롯을 검색하기위한 상위 이미지        
    public Image Acc_Image;                 //

    public List<Button> AllSlot_Skin;       //스킨창의 슬롯
    public List<Button> AllSlot_Acc;        //악세사리창의 슬롯


    //public List<string> ItemStringList;
    //private

    private int SlotCount;                  //슬롯의 갯수
    
	void Start () {

        //자식 슬롯들을 슬롯리스트에 넣어준다.
        foreach (Button Slot in Skin_Image.transform.GetComponentsInChildren<Button>())
        {
            AllSlot_Skin.Add(Slot);
        }
        foreach (Button Slot in Acc_Image.transform.GetComponentsInChildren<Button>())
        {
            AllSlot_Acc.Add(Slot);
        }

        //슬롯의 갯수 설정(스킨과 악세사리가 동일)
        SlotCount = AllSlot_Skin.Count;
	}

    public void AddItem_Skin(string SkinName)
    {
        foreach (Button Cur_Slot in AllSlot_Skin)
        {
            Slot slot = Cur_Slot.GetComponent<Slot>();

            //슬롯이 비어있으면 통과
            //if (slot.isEmpty)
            //{
                
            //    string Path = "Char/Skin/Face02";//"Assets/04.Images/Char/Skin/" + SkinName;     //Assets/04.Images/Char/Skin/Face01.png
            //    Sprite texture = Resources.Load(Path, typeof(Sprite)) as Sprite;// (Path, typeof(Sprite)) as Sprite;//(Path, typeof(Sprite)) as Sprite;
            //    slot.AddItem(texture);

            //    return;
            //}
        }
    }

}


