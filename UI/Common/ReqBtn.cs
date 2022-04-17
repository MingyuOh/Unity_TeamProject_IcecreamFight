using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReqBtn : MonoBehaviour
{
    [SerializeField]
    private Image backgroundImg;

    public void Awake()
    {
        //배경이미지 렌더링 X
        DeActiviateBackGroundImg();
    }

    //버튼이 클릭되었을때 이 함수가 불려져
    //백그라운드이미지가 활성화 되어야함
    public void ActiviateBackGroundImg()
    {
        backgroundImg.enabled = true;
    }

    //버튼 해제시
    //백그라운드이미지 비활성화
    public void DeActiviateBackGroundImg()
    {
        backgroundImg.enabled = false;
    }

}
