using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Communication
{
    public class InputNickName : MonoBehaviour
    {
        private UserAuth userAuth;

        public InputField NickNameInputField;   //인풋필드

        //닉네임 입력완료 알림
        public delegate void InformCompleteInputHandler();
        public static event InformCompleteInputHandler InformComplete;

        public void Awake()
        {
            userAuth = UserAuth.Instance;
        }
        //닉네임 확정 함수
        public void ConfirmOfNickName()
        {
            ConfirmPanel.InformConfirm -= ConfirmOfNickName;
            userAuth.SaveNickName(NickNameInputField.text);
            InformComplete();   //완료됨을 알림
        }

        public void OnClickOKButton()
        {
            //닉네임은 2~10자
            if (NickNameInputField.text.Length > 2 && NickNameInputField.text.Length < 10)
            {
                ConfirmPanel.InformConfirm += ConfirmOfNickName;
                Main.Instance.ShowConfirmPanel();
            }
            else
            {
                /////////////////////////임시/////////////////////////
                Debug.Log("입력실패");
            }
        }
        
        public void OnClickXbutton()
        {
            Main.Instance.HideInputNickNamePanel();
        }
    }
}
