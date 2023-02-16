using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleToMain : MonoBehaviour {

    void Start()
    {
        //처음 FadeIn
        //ImageFadeIn.Instance.StartFadeAnim();
    }
    void Update()
    {
        //마우스클릭시 -> 추후 아무곳터치로 변경
        //타이틀화면은 처음에만 보여지고 후에는 필요없음
        //따라서 LoadSceneMode.Single로 씬전환시 삭제
        
        //if (Input.GetMouseButton(0))
        //{
        //    //FadeOut
        //    ImageFadeOut.Instance.StartFadeAnim();
        //}
    }
}
