using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UITextBlink : MonoBehaviour {

    public float FadeTime = 1f;                 //Fade 애니메이션 재생 시간

    public UnityEngine.UI.Text fadeText;     //Image 컴포넌트 참조변수

    private float start;               //Mathf.Lerp 메소드의 첫번째 값
    private float end;                 //Mathf.Lerp 메소드의 두번째 값
    private float time = 0;                //Mathf.Lerp 메소드의 시간 값

    bool fadeTime = false;              //반복적인 재생을 위해
    bool FirstTime = true;              //첫타임에 타임값을 0으로
    Color color;                        //색상값

    void Awake()
    {
        color = fadeText.color;
    }
    void Update()
    {
        //Fade 애니메이션 재생
        PlayFade();
    }

    void PlayFade()
    {
        if (!fadeTime)
        {
            fadeTime = !fadeTime;
            if (FirstTime)
            {
                time = 0;
            }
        }
        // 경과 시간 게산
        // animTime 동안 재생될 수 있도록
        time += Time.deltaTime / FadeTime;

        if (color.a == 1f && fadeTime == true)
        {
            start = 1f;
            end = 0f;
            if (!FirstTime)
                time = 0;
            fadeTime = false;
            FirstTime = !FirstTime;
        }
        else if (color.a == 0f && fadeTime == true)
        {
            start = 0f;
            end = 1f;
            if (!FirstTime)
                time = 0;
            fadeTime = false;
            FirstTime = !FirstTime;
        }

        //알파 값 계산.
        color.a = Mathf.Lerp(start, end, time);
        fadeText.color = color;
    }
}
