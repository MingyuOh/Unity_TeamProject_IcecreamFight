using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Loading : MonoSingleton<Loading>
{
    [SerializeField]
    private float rotSpeed;
    
    [SerializeField]
    private Text LoadingText;    //로딩시 뜨는 텍스트
    
    [SerializeField]
    private Transform LoadingImageWheel;  //로딩 이미지

    private Vector3 WheelEular;  //로딩 이미지의 오일러 각

	protected override void Awake()
	{
		base.Awake();
	}

	public void LoadingTextSetting(string str)
    {
        LoadingText.text = str;
    }

    private void Update()
    {
        WheelEular = LoadingImageWheel.transform.rotation.eulerAngles;
        WheelEular.z -= rotSpeed * Time.deltaTime;
        LoadingImageWheel.rotation = Quaternion.Euler(WheelEular);
    }  
}
