using System;
using Communication;
using NCMB;

public class ShopInfo
{
	public readonly int generalGachaPrice = 1000;	// 코인
	public readonly int specialGachaPrice = 100;	// 다이아몬드

	public ShopInfo(NCMBObject ncmbObject)
	{
		if(ncmbObject.ContainsKey(ShopInfoKey.GENERAL_GACHAPRICE))
		{
			generalGachaPrice = Convert.ToInt32(ncmbObject[ShopInfoKey.GENERAL_GACHAPRICE]);
		}

		if (ncmbObject.ContainsKey(ShopInfoKey.SPECIAL_GACHAPRICE))
		{
			generalGachaPrice = Convert.ToInt32(ncmbObject[ShopInfoKey.SPECIAL_GACHAPRICE]);
		}
	}

	public ShopInfo()
	{

	}
}
