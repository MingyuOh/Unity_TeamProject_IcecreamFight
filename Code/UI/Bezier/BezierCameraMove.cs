using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BezierCameraMove : MonoSingleton<BezierCameraMove>
{
    //카메라 오브젝트
    public Camera MainCamera;

    //곡선 경로
    public CsBezierCurve path;

    //멤버 변수
    public bool isMovingEnd = false;

    [Range(0f,1f)]
    public float u;

    //ZoomIn
    private float Z_start;                   //Mathf.Lerp 메소드의 첫번째 값
    private float Z_end;                   //Mathf.Lerp 메소드의 두번째 값
    private float Z_In_time;                 //Mathf.Lerp 메소드의 시간 값
    private float Z_Out_time;                 //Mathf.Lerp 메소드의 시간 값

    //Moving
    private float M_start = 0f;                   //Mathf.Lerp 메소드의 첫번째 값
    private float M_end = 1f;                   //Mathf.Lerp 메소드의 두번째 값
    private float M_time;                 //Mathf.Lerp 메소드의 시간 값

    public bool PlayingZoomIn = false;
    public bool PlayingZoomOut = false;

    [Range(5f,0f)]
    public float CameraZoomSize = 5f;

    //MovingTime
    public float MovingTime;
    public float ZoomingTime;

    void Awake()
    {
        Screen.SetResolution(1080, 1920, true);    
    }
    public void CameraMoveMaintoCamera()
    {
        StartCoroutine("CameraMove");
    }

    public void ResetMemberVariables()
    {
        u = 0f;
        Z_In_time = 0f;
        Z_Out_time = 0f;
        M_time = 0f;

        PlayingZoomIn = false;
        PlayingZoomOut = false;
        isMovingEnd = false;
        path = null;
    }

    IEnumerator CameraMove()
    {
        while (u < 1f)
        {

            if (!PlayingZoomOut)
            {
                Z_start = 5f;
                Z_end = 3f;
                StartCoroutine("CameraZoomOut");
            }

            if (u > 0.5f)
            {
                if (!PlayingZoomIn)
                {
                    Z_start = 3f;
                    Z_end = 5.0f;
                    StartCoroutine("CameraZoomIn");
                }
            }
            M_time += Time.deltaTime / MovingTime;
            u = Mathf.Lerp(M_start, M_end, M_time);

            //곡선 이동
            MainCamera.transform.position = path.GetPointInBezierCurve(u);
            yield return null;
        }
        isMovingEnd = true;
        //MainCamera.orthographic = true;
    }

    //bigger
    IEnumerator CameraZoomIn()
    {
        PlayingZoomIn = true;
        while (CameraZoomSize < 5.0f)
        {
            Z_In_time += Time.deltaTime / (ZoomingTime / 2);
            CameraZoomSize = Mathf.Lerp(Z_start, Z_end, Z_In_time);
            MainCamera.orthographicSize = CameraZoomSize;
            yield return null;
        }
    }
    
    //smaller
    IEnumerator CameraZoomOut()
    {
        PlayingZoomOut = true;
        while (CameraZoomSize > 3f)
        {
            Z_Out_time += Time.deltaTime / (ZoomingTime / 2);
            CameraZoomSize = Mathf.Lerp(Z_start, Z_end, Z_Out_time);
            MainCamera.orthographicSize = CameraZoomSize;
            yield return null;
        }
    }
}
