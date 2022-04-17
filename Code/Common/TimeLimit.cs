using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeLimit : MonoBehaviour {

    //게임 시간
    public static float PlayTime;

    void Start()
    {
        //게임 시간 60초로 설정
        PlayTime = 60f;             
    }

    void Update()
    {
        if (PlayTime != 0)
        {
            //게임 시간 감소
            PlayTime -= Time.deltaTime;
            if (PlayTime <= 0)
            {
                //게임 시간이 0보다 작아졌을때 0으로 고정
                PlayTime = 0;

                ////////////////////////게임 종료 이벤트//////////////////////////
                /////////////////////////////추가/////////////////////////////////
            }
        }

        int t = Mathf.FloorToInt(PlayTime);
        Text uiText = GetComponent<Text>();
        uiText.text = "Time : " + t.ToString();
    }
}
