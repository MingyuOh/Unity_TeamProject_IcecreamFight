using NCMB;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Communication;

public class MenuPlayerPrefab : MenuPrefab
{
    //Player의 경우
    //UserAuth에서 정보를 가져온다
    private UserAuth userAuth;

	public void Awake()
	{
		// 유저 정보
		userAuth =  UserAuth.Instance;
	}

    public void Start()
	{
		chracterTypeName = userAuth.characterType;
		skinTypeName = userAuth.skinType;

		string path = "Menu/" + chracterTypeName;
		//playerPrefab = PhotonNetwork.Instantiate(Resources.Load(path), MenuPlayerPosition, Quaternion.identity, 0);
		playerPrefab = Instantiate(Resources.Load(path), MenuPlayerPosition) as GameObject;

		// 스킨 변경
		if (playerPrefab != null)
		{
			Utility.ChangeSkin(playerPrefab, chracterTypeName, skinTypeName);
		}
	}
}
