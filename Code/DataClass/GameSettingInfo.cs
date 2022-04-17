using System;
using UnityEngine;
using Communication;
using NCMB;

public class GameSettingInfo
{
	public readonly DateTime updateDate;
	public readonly bool isServiceEnable = true;
	public readonly string bannerFileName = string.Empty;
	public readonly string termsOfUse = "규약 데이터가 없습니다.";
	
	public GameSettingInfo(NCMBObject ncmbObject)
	{
		updateDate = Utility.UtcToLocal((DateTime)ncmbObject.UpdateDate);

		if(ncmbObject.ContainsKey(GameSettingInfoKey.IS_SERVICE_ENABLE))
		{
			isServiceEnable = (bool)ncmbObject[GameSettingInfoKey.IS_SERVICE_ENABLE];
		}

		if(ncmbObject.ContainsKey(GameSettingInfoKey.BANNERFILE_NAME))
		{
			bannerFileName = ncmbObject[GameSettingInfoKey.BANNERFILE_NAME] as string;
		}
	}

	public GameSettingInfo()
	{
		updateDate = new DateTime(0);
	}
}
