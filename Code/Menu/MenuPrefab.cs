using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Communication;

public class MenuPrefab : MonoBehaviour
{
    //상속되는 파생클래스에서 접근하기 위해
    //Protected로 선언되는 변수들
    protected GameObject playerPrefab;                    // 플레이어 프리팹
    protected Renderer playerRenderer;                    // 플레이어 렌더러
    protected Material playerMaterial;                    // 플레이어 머테리얼
    protected GameObject playerHeadAccessory;             // 플레이어 머리 악세사리
    protected GameObject playerChestAccessory;            // 플레이어 가슴 악세사리

    protected string chracterTypeName;                   // 플레이어 캐릭터 타입 이름
    protected string skinTypeName;						// 플레이어 스킨 타입 이름

    //Public으로 선언되는 변수
    public Transform MenuPlayerPosition;				// 메뉴에서 플레이어 Show 위치

    private void Awake()
    {
        //playerPrefab = this.gameObject;
    }
}
