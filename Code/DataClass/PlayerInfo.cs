using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NCMB;
using Communication;

public class PlayerInfo
{
	public readonly string userId = string.Empty;
	public readonly string nickName = string.Empty;
	public readonly string characterType = string.Empty;
	public readonly string characterSkin = string.Empty;
	public readonly string headAccessory = string.Empty;
	public readonly string chestAccessory = string.Empty;
	public readonly string tier = string.Empty;


	public PlayerInfo(NCMBObject obj)
	{
		userId = obj[PlayerInfoKey.USERID] as string;
		nickName = obj[PlayerInfoKey.NICKNAME] as string;
		characterType = obj[PlayerInfoKey.CHARACTERTYPE] as string;
		characterSkin = obj[PlayerInfoKey.SKINTYPE] as string;
		headAccessory = obj[PlayerInfoKey.HEAD_ACCESSORY_TYPE] as string;
		chestAccessory = obj[PlayerInfoKey.CHEST_ACCESSORY_TYPE] as string;
		tier = obj[PlayerInfoKey.TIER] as string;
	}

	public static NCMBObject CreateObject()
	{
		NCMBObject obj = new NCMBObject(DataStoreClass.PLAYERINFO_LIST);

		if (NCMBUser.CurrentUser != null)
		{
			obj[PlayerInfoKey.USERID] = NCMBUser.CurrentUser.UserName as string;
			obj[PlayerInfoKey.NICKNAME] = NCMBUser.CurrentUser[UserKey.NICKNAME] as string;
			obj[PlayerInfoKey.CHARACTERTYPE] = NCMBUser.CurrentUser[UserKey.CHARACTERTYPE] as string;
			obj[PlayerInfoKey.SKINTYPE] = NCMBUser.CurrentUser[UserKey.SKINTYPE] as string;
			obj[PlayerInfoKey.HEAD_ACCESSORY_TYPE] = NCMBUser.CurrentUser[UserKey.HEAD_ACCESSORY_TYPE] as string;
			obj[PlayerInfoKey.CHEST_ACCESSORY_TYPE] = NCMBUser.CurrentUser[UserKey.CHEST_ACCESSORY_TYPE] as string;
			obj[PlayerInfoKey.TIER] = NCMBUser.CurrentUser[UserKey.TIER] as string;

#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
        obj[NCMBDataStoreKey.INSTALLSTION_OBJECTID]
           = NCMBInstallation.getCurrentInstallation().ObjectId;
#endif
		}
		else
		{
			Debug.Log("저장된 유저 데이터가 존재하지 않습니다.");
		}

		return obj;
	}
}
