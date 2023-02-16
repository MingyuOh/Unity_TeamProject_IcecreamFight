using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageFadeIn : MonoSingleton<ImageFadeIn>
{
    public float animTime = 0.5f;                 //Fade 애니메이션 재생 시간

    [SerializeField]
    private Image fadeInImage;                     //Image 컴포넌트 참조변수

    private float start = 1f;                   //Mathf.Lerp 메소드의 첫번째 값
    private float end = 0f;                     //Mathf.Lerp 메소드의 두번째 값
    private float time = 0f;                    //Mathf.Lerp 메소드의 시간 값
    
    private bool isPlaying = false;             //Fade 애니메이션의 중복 재생을 방지

    private void Awake()
    {
        fadeInImage = this.GetComponent<Image>();
    }

    public void StartFadeAnim()
    {
        if (isPlaying == true)
            return;

        //Fade 애니메이션 재생.
        ResetToFirstState();

    }
    //Fade 애니메이션 메소드.
    public IEnumerator PlayFadeIn()
    {
        if (isPlaying == true)
            yield break;

        ////Fade 애니메이션 재생.
        ResetToFirstState();

        //애니메이션 재생중.
        isPlaying = true;

        //색상 값
        Color color = fadeInImage.color;
        time = 0f;
        color.a = Mathf.Lerp(start, end, time);

        while (color.a > 0f)
        {
            //경과 시간 계산.
            //animTime동안 재생될 수 있도록
            time += Time.deltaTime / animTime;

            //알파 값 계산.
            color.a = Mathf.Lerp(start, end, time);
            //계산한 알파 값 다시 설정.
            this.GetComponent<Image>().color = color;

            yield return null;
        }

        //애니메이션 재생 완료.
        isPlaying = false;

        //페이드인 애니메이션이 끝났음을 알림
        //EndOfFadeIn();
    }

    private void ResetToFirstState()
    {
        start = 1f;
        end = 0f;
        time = 0f;

        Color color = fadeInImage.color;
        color.a = 1f;
        fadeInImage.color = color;
    }
}