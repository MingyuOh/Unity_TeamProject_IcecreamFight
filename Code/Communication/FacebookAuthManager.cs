using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Facebook.Unity;
using Communication;

namespace NCMB
{
	public class FacebookAuthManager : MonoSingleton<FacebookAuthManager>
	{
		private UserAuth userAuth;

		bool isFBConnecting = true;

		protected override void Awake()
		{
			// 현재 씬에서만 사용하는 싱글톤 객체
			recycleObj = false;

			// 싱글톤 생성
			base.Awake();

			if (FB.IsInitialized == false)
			{
				FB.Init(InitCallback, OnHideUnity);
			}
			else
			{
				// 만약 로그인이 되어있다면 페이스북 버튼 hide
				FB.ActivateApp();
			}
		}

		private void Start()
		{
			// 임시 방편 로그아웃
			StartCoroutine(LogOutCoroutine());

			userAuth = UserAuth.Instance;
		}

		/////////////////////////////////////////////////////////////////////////////////////
		// 페이스북 회원가입 및 자동 로그인 코루틴
		/////////////////////////////////////////////////////////////////////////////////////
		public IEnumerator FacebookAutoSignUpAndLoginCoroutine()
		{
			if (FB.IsInitialized)
			{
				var list = new List<string>() { "public_profile", "email", "user_friends" };

				FB.LogInWithReadPermissions(list, SignUpAndLoginThroughFacebook);

				while (isFBConnecting == true)
				{
					yield return null;
				}
				Debug.Log("페이스북 로그인 완료!");
			}
			else
			{
				Debug.Log("Facebook SDK 초기화에 실패하였습니다.");
				yield break;
			}
		}

		/////////////////////////////////////////////////////////////////////////////////////
		// 회원가입 함수
		/////////////////////////////////////////////////////////////////////////////////////
		private void SignUpAndLoginThroughFacebook(ILoginResult result)
		{
			if (FB.IsLoggedIn == true)
			{
				// AccessToken 클래스는 세션 세부 정보를 갖는다.
				var aToken = Facebook.Unity.AccessToken.CurrentAccessToken;
				var userId = aToken.UserId;
				var toke = aToken.TokenString;
				var extime = aToken.ExpirationTime;
				
				// NCMB 인증 매개 변수 만들기
				NCMBFacebookParameters parameters = new NCMBFacebookParameters(
					userId, toke, extime);

				// 유저 생성
				NCMBUser user = new NCMBUser();
				user.AuthData = parameters.param;

				// 유저 데이터 저장
				user.LogInWithAuthDataAsync((NCMBException e) =>
				{
					if (e != null)
					{
						Main.Instance.ForceToTitle(e);
					}

					// 처음 로그인 사용자인지 체크
					if (userAuth.IsFirstUser(user) == true)
					{
						StartCoroutine(UserAuth.Instance.SignUpCoroutine(user, Main.Instance.ForceToTitle));
					}

					Debug.Log("로그인 성공");
					isFBConnecting = false;
				});
			}
			else
			{
				Debug.Log("사용자가 로그인을 취소했습니다.");
			}
		}

		/////////////////////////////////////////////////////////////////////////////////////
		// 페이스북 초기화 콜백 함수
		/////////////////////////////////////////////////////////////////////////////////////
		private void InitCallback()
		{
			if (FB.IsInitialized)
			{
				FB.ActivateApp();
			}
			else
			{
				Debug.Log("Facebook SDK 초기화에 실패하였습니다.");
			}
		}

		/////////////////////////////////////////////////////////////////////////////////////
		// 유니티에서 숨기는 콜백 함수(API라서 무슨의미인지.. 나중에 API 분석)
		/////////////////////////////////////////////////////////////////////////////////////
		private void OnHideUnity(bool isGameShown)
		{
			if (isGameShown == false)
			{
				Time.timeScale = 0;
			}
			else
			{
				Time.timeScale = 1;
			}
		}

		/////////////////////////////////////////////////////////////////////////////////////
		// 로그아웃 코루틴
		/////////////////////////////////////////////////////////////////////////////////////
		public IEnumerator LogOutCoroutine()
		{
			bool isConnecting = true;

			NCMBUser.LogOutAsync((NCMBException e) =>
			{
				if (e != null)
				{
					// 로그아웃 될때까지 재귀처리하여 처리
					StartCoroutine(LogOutCoroutine());
				}

				isConnecting = false;
			});

			while (isConnecting == true)
			{
				yield return null;
			}
		}
	}
}
