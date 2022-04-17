using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuTeamMemberPrefab : MenuPrefab
{
    private PlayerInfo _playerInfo;
    public PlayerInfo playerInfo
    {
        get { return _playerInfo; }
        set { _playerInfo = playerInfo; }
    }

    //현재 프리팹에 캐릭터가 들어있는지
    //들어있지 않는지 판단하기위한 bool 변수
    public bool isFull;

    private void Awake()
    {
        isFull = false;
    }

    //팀멤버프리팹을 세팅하는 함수
    public void SettingTeamMemberPrefab(PlayerInfo playerInfo)
    {
        isFull = true;

        this.playerInfo = playerInfo;
        chracterTypeName = playerInfo.characterType;
        skinTypeName = playerInfo.characterSkin;

        string path = "Menu/" + chracterTypeName;
        playerPrefab = Instantiate(Resources.Load(path), MenuPlayerPosition) as GameObject;

        // 스킨 변경
        if (playerPrefab != null)
        {
            Utility.ChangeSkin(playerPrefab, chracterTypeName, skinTypeName);
        }
    }

    //팀원이나가거나 추방당했을경우
    //팀멤버프리팹을 초기화하는 함수(프리팹만 삭제)
    public void ResetTeamMemberPrefab()
    {
        this.playerInfo = null;
        chracterTypeName = null;
        skinTypeName = null;

        isFull = false;

        Destroy(playerPrefab);
    }
}
