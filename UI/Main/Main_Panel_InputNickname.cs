using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Main_Panel_InputNickname : MonoBehaviour {

    //닉네임 인풋필드
    InputField Input_NickName;
    
    //닉네임 텍스트
    public Text NickName;

    //하위패널
    public RectTransform Panel_Success;
    public RectTransform Panel_Fail;

    void Start()
    {
        Input_NickName = transform.GetComponentInChildren<InputField>();
        Panel_Success.gameObject.SetActive(false);
        Panel_Fail.gameObject.SetActive(false);
    }

    public void NickName_Changed(string newText)
    {
        //닉네임 설정
        NickName.text = newText;
        //DB 닉네임데이터 변경
    }

    public void OK_button()
    {
        //예외처리
        //두글자 이상 입력
        if (Input_NickName.text.Length < 2)
        {
            StartCoroutine("Show_Panel_Fail");
        }
        else
        {
            //확정 패널
            //Debug.Log("성공");
            Panel_Success.gameObject.SetActive(true);
        }
        //패널창 종료
    }

    public void Cancel_button()
    {
        gameObject.SetActive(false);
        //패널창 종료
    }

    IEnumerator Show_Panel_Fail()
    {
        Panel_Fail.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.3f);
        Panel_Fail.gameObject.SetActive(false);
    }

    public void Confirmation_InputNickName()
    {
        //닉네임 확정

        //닉네임 변경
        NickName_Changed(Input_NickName.text);

        //DB 데이터 변경
        //추가

        //패널창 종료
        Panel_Success.gameObject.SetActive(false);
        gameObject.SetActive(false);
    }

    public void Confirmation_Cancel()
    {
        //닉네임 확정 취소
        Panel_Success.gameObject.SetActive(false);
    }
}
