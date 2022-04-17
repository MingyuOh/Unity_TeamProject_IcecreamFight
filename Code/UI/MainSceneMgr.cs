using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainSceneMgr : MonoSingleton<MainSceneMgr> {

    //메인화면의 캔버스그룹
    public Canvas Main;
    public Canvas Shop;
    public Canvas Inven;

    public Canvas CurrentCanvas;

    public GameObject GameModePanel;

    //private CanvasToCanvasMove C_Script;
    private ArrayList C_Script = new ArrayList();

   // private CanvasToCanvasMove[] C_Script;
    void Start()
    {
        Screen.SetResolution(Screen.width, Screen.width * 16 / 9, false);

        //처음 FadeIn
        //ImageFadeIn.Instance.StartFadeAnim();

        //메인 캔버스의 사이즈를 스태틱 캔버스사이즈 변수에 담는다

        //TestCanvasRectSize.CanvasSizeX = transform.GetComponent<RectTransform>().sizeDelta.x;
        //TestCanvasRectSize.CanvasSizeY = transform.GetComponent<RectTransform>().sizeDelta.y;

        //모든 캔버스를 다 돌며 CanvasToCanvasMove스크립트를 스크립트 배열에 추가
        CanvasToCanvasMove[] CTVM_Script = Main.GetComponents<CanvasToCanvasMove>();    //메인
        foreach (var i in CTVM_Script)
            C_Script.Add(i);

        CTVM_Script = null;

        CTVM_Script = Shop.GetComponents<CanvasToCanvasMove>(); //상점
        foreach (var i in CTVM_Script)
            C_Script.Add(i);
        CTVM_Script = null;


        CTVM_Script = Inven.GetComponents<CanvasToCanvasMove>(); //창고
        foreach (var i in CTVM_Script)
            C_Script.Add(i);
        CTVM_Script = null;
        //

        CurrentCanvas = Main;
    }


    //버튼 이벤트 test용
    //나중에 따로 스크립트로 빼자

    //메인캔버스 버튼
    public void OnClickStartBtn()
    {
        GameModePanel.SetActive(true);
    }

    public void OnClickShopBtn()
    {
        CanvasToCanvasMove Cur_Script = (CanvasToCanvasMove)C_Script[0];
        Cur_Script.OnMovingCanvasToCanvas();
    }

    public void OnClickInvenBtn()
    {
        CanvasToCanvasMove Cur_Script = (CanvasToCanvasMove)C_Script[1];
        Cur_Script.OnMovingCanvasToCanvas();
    }

    //상점캔버스 버튼
    public void OnClickShopToMainBackBtn()
    {
        CanvasToCanvasMove Cur_Script = (CanvasToCanvasMove)C_Script[2];
        Cur_Script.OnMovingCanvasToCanvas();
    }

    //창고캔버스 버튼
    public void OnClickInvenToMainBackBtn()
    {
        CanvasToCanvasMove Cur_Script = (CanvasToCanvasMove)C_Script[3];
        Cur_Script.OnMovingCanvasToCanvas();
    }


}
