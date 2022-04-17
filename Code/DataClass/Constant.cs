namespace Communication
{
	// PlayerPrefs의 키명 정의
	public static class PlayerPrefsKey
	{
		public static readonly string GAMESETTING_UPDATEDATE = "GameSettingUpdateDate";
	}

	// 회원 관리의 필드명 정의
	public static class UserKey
	{
		public static readonly string NICKNAME = "NickName";
		public static readonly string CHARACTERTYPE = "CharacterType";
		public static readonly string SKINTYPE = "SkinType";
		public static readonly string SKINTABLE = "SkinTable";
		public static readonly string HEAD_ACCESSORY_TYPE = "HeadAccessoryType";
		public static readonly string HEAD_ACCESSORY_LIST = "HeadAccessoryList";
		public static readonly string CHEST_ACCESSORY_TYPE = "ChestAccessoryType";
		public static readonly string CHEST_ACCESSORY_LIST = "ChestAccessoryList";
		public static readonly string COINCOUNT = "CoinCount";
		public static readonly string DIAMONDCOUNT = "DiamondCount";
		public static readonly string TIER = "Tier";
		public static readonly string POINT = "Point";
		public static readonly string PLAYERINFO = "PlayerInfo";
		public static readonly string FRIENDINFO = "FriendInfo";
		public static readonly string FRIENDID_LIST = "FriendIdList";
		public static readonly string INSTALLATION_OBJECTID = "InstallationObjectId";
		public static readonly string DAILYBONUSFLAG = "DailyBonusFlag";
	}

	// 데이터 스토어의 클래스명 정의
	public static class DataStoreClass
	{
		public static readonly string SHOPSETTING = "ShopSetting";
		public static readonly string RANKING = "Ranking";
		public static readonly string PLAYERINFO_LIST = "PlayerInfoList";
		public static readonly string FRIEND_INFO = "FriendInfoList";
		public static readonly string GAMESETTING = "GameSetting";
		public static readonly string ANNOUNCEMENTS = "Announcements";
	}

	// 데이터 스토어의 플레이어정보 요소명 정의
	public static class PlayerInfoKey
	{
		// 기본
		public static readonly string OBJECTID = "ObjectId";
		public static readonly string CREATE_DATE = "CreateDate";
		public static readonly string UPDATE_DATE = "UpdateDate";

		public static readonly string USERID = "UserId";
		public static readonly string NICKNAME = "NickName";
		public static readonly string CHARACTERTYPE = "CharacterType";
		public static readonly string SKINTYPE = "SkinType";
		public static readonly string HEAD_ACCESSORY_TYPE = "HeadAccessoryType";
		public static readonly string CHEST_ACCESSORY_TYPE = "ChestAccessoryType";
		public static readonly string TIER = "Tier";
		public static readonly string INSTALLSTION_OBJECTID = "InstallationObjectId";
	}

	// 데이터 스토어의 친구정보 요소명 정의
	public static class FriendInfoKey
	{
		// 기본
		public static readonly string OBJECTID = "ObjectId";
		public static readonly string CREATE_DATE = "CreateDate";
		public static readonly string UPDATE_DATE = "UpdateDate";

		public static readonly string USERID = "UserId";
		public static readonly string NICKNAME = "NickName";
		public static readonly string REQUESTID_NICKNAME_DICTIONARY = "RequestIDAndNickNameDictionary";
		public static readonly string ACCEPTID_LIST = "AcceptIDList";
	}

	// 데이터 스토어의 상점정보 요소명 정의
	public static class ShopInfoKey
	{
		public static readonly string GENERAL_GACHAPRICE = "GeneralGachaPrice";
		public static readonly string SPECIAL_GACHAPRICE = "SpecialGachaPrice";
	}

	// 데이터 스토어의 알림정보 요소명 정의
	public static class NotificationKey
	{
		// 알림
		public static readonly string TITLE = "Title";
		public static readonly string MAINTEXT = "MainText";
	}

	// 데이터 스토어의 게임셋팅정보 요소명 정의
	public static class GameSettingInfoKey
	{
		// 게임 셋팅
		public static readonly string IS_SERVICE_ENABLE = "IsServiceEnable";
		public static readonly string BANNERFILE_NAME = "BannerFileName";
	}

	// 캐릭터 타입
	public static class CharacterTypeClass
	{
		public static readonly string BEAR = "Bear";
		public static readonly string BUNNY = "Bunny";
		public static readonly string CAT = "Cat";
	}

	// SNS 이름 정의
	public static class SNSClass
	{
		public static readonly string GOOGLE = "google";
		public static readonly string FACEBOOK = "facebook";
		public static readonly string TWITTER = "twitter";
		public static readonly string NAVER = "naver";
	}

	// Scene 이름 정의
	public static class SceneClass
	{
		public static readonly string TITLE_SCENE = "TitleScene";
		public static readonly string MENU_SCENE = "MenuScene";
		public static readonly string LOBBY_SCENE = "LobbyScene";
		public static readonly string ONLINE_GAME_SCENE = "OnlineGameScene";
		public static readonly string OFFLINE_GAME_SCENE = "OfflineGameScene";
	}

	// Social 메세지 정의 
	public static class SocialMessage
	{
		/// <summary>
		/// 팀 메세지
		/// </summary>
		public static readonly string TEAM_REQUEST = "Team request";
		public static readonly string TEAM_ACCEPT = "Team accept";
		public static readonly string TEAM_REFUSE = "Team refuse";
		public static readonly string ALREADY_TEAM_REQUEST = "Already exist team request";
		public static readonly string TEAM_WITHDRAWAL = "Team withdrawal";
		public static readonly string TEAM_BANISH = "Team banish";
		public static readonly string TEAM_MASTER_WITHDRAWAL = "Team master withdrawal";
		public static readonly string TEAM_MAX = "Max player exist in team";
		public static readonly string TEAM_JOINED = "Team joined";
		public static readonly string TEAM_REBUILDING = "Team rebuilding";
		public static readonly string TEAM_READY = "Team Ready";
		
		/// <summary>
		/// 친구 메세지
		/// </summary>
		public static readonly string FRIEND_REQUEST = "Friend request";
		public static readonly string FRIEND_ACCEPT = "Friend accept";
		public static readonly string FRIEND_REFUSE = "Friend refuse";
		public static readonly string FRIEND_DELETE = "Friend delete";
	}
}