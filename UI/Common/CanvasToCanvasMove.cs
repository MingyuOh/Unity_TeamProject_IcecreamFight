using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasToCanvasMove : MonoBehaviour {

    //테스트용
    public Canvas C_Start;  //출발 캔버스
    public Canvas C_End;  //도착 캔버스

    //곡선 경로
    public CsBezierCurve path;

    public bool move_end = false;

    void Update()
    {
        //Debug.Log(C_End.renderMode);
        if (move_end)
        {
            //렌더 모드 변경
            //C_End.worldCamera = BezierCameraMove.Instance.MainCamera;
            Screen.SetResolution(1920, 1080, true);
            C_End.worldCamera = BezierCameraMove.Instance.MainCamera;
            //C_End.renderMode = RenderMode.ScreenSpaceOverlay;
            //C_End.renderMode = RenderMode.ScreenSpaceCamera;
            //C_End.worldCamera = BezierCameraMove.Instance.MainCamera;
            //_scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            //C_End.scaleFactor = 1.0f;
            move_end = false;
            BezierCameraMove.Instance.ResetMemberVariables();

            MainSceneMgr.Instance.CurrentCanvas = C_End;
        }
    }

    //이동클래스
    public void OnMovingCanvasToCanvas()
    {
        //경로 설정
        BezierCameraMove.Instance.path = path;
        //시작캔버스의 rendermode를 worldspace로 변경
        C_Start.renderMode = RenderMode.WorldSpace;

        //Test
        float kkk = 0;
        Debug.Log(kkk);
        //

        //C_Start.worldCamera = null;
        //이동 함수
        BezierCameraMove.Instance.CameraMoveMaintoCamera();

        //이동이끝났는지 체크하는 코루틴 시작
        StartCoroutine("CheckCameraMoveisEnd");
    }

    //카메라 이동이 끝났는지 체크하는 코루틴
    IEnumerator CheckCameraMoveisEnd()
    {

        while (!BezierCameraMove.Instance.isMovingEnd)
        {
            yield return null;
        }
        if (BezierCameraMove.Instance.isMovingEnd)
        {
            move_end = true;
        }
    }
}
