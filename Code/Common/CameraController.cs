using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;                // 추적할 대상
    public float moveDamping = 15.0f;       // 이동 속도 계수
    public float distance = 3.0f;          // 추적 대상과의 거리
    public float height = 5.0f;            // 추적 대상과의 높이
    Vector3 camRealpos;

	/// <summary>
	/// Reference to the Camera component.
	/// </summary>
	[HideInInspector]
	public Camera cam;

	/// <summary>
	/// Layers to hide after calling HideMask().
	/// </summary>
	public LayerMask respawnMask;

	// CameraRig의 Transform 컴포넌트
	private Transform tr;

    // Use this for initialization
    void Start()
    {
		cam = GetComponent<Camera>();
        // CameraRig의 Transform 컴포넌트 추출
        tr = GetComponent<Transform>();
		if (Static.Instance.TeamColor)
			tr.transform.Rotate(Vector3.up, 180);

		// 이 장면의 AudioListener는이 카메라에 직접 첨부되지 않지만,
		// 카메라를 부모로하는 별도의 게임 객체로 전달됩니다. 이것은
		// 일반적으로 카메라는 플레이어 위에 위치하지만 AudioListener
		// 3D 공간에서 플레이어의 위치에서 오디오 클립을 고려해야합니다.
		// 여기에서 AudioListener 자식 객체를 대상 위치에 배치합니다.
		// 주의 : Player에 대한 AudioListener의 부모 역할은 작동하지 않습니다.
		// 죽음에 따라 사용 불가 상태가되어 사운드 재생이 완전히 멈 춥니 다.
		Transform listener = GetComponentInChildren<AudioListener>().transform;
		listener.position = transform.position + transform.forward * distance;
	}

	// 주인공 캐릭터의 이동 로직이 완료된 후 처리하기 위해 FixedUpdate에서 구현
	private void FixedUpdate()
    {
		if (!target)
			return;
		//convert the camera's transform angle into a rotation
		Quaternion currentRotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);

		//set the position of the camera on the x-z plane to:
		//distance units behind the target, height units above the target
		Vector3 pos = target.position;
		pos -= currentRotation * Vector3.forward * Mathf.Abs(distance);
		pos.y = target.position.y + Mathf.Abs(height);
		transform.position = pos;

		//look at the target
		transform.LookAt(target);

		//clamp distance
		transform.position = target.position - (transform.forward * Mathf.Abs(distance));
		
		
		//if (Static.Instance.TeamColor)
		//{
		//	camRealpos = target.position + (Vector3.up * height - (-Vector3.forward) * distance);
		//	// 이동할 때의 속도 계수를 적용
		//	tr.position = Vector3.Slerp(tr.position, camRealpos, Time.deltaTime * moveDamping);
		//}
		//else
		//{
		//	camRealpos = target.position + (Vector3.up * height - Vector3.forward * distance);
		//	// 이동할 때의 속도 계수를 적용
		//	tr.position = Vector3.Slerp(tr.position, camRealpos, Time.deltaTime * moveDamping);
		//}
	}

	// 카메라로 지정된 'respawnMask' 레이어를 삭제
	public void HideMask(bool shouldHide)
	{
		if (shouldHide)
			cam.cullingMask &= ~respawnMask;
		else
			cam.cullingMask |= respawnMask;
	}
}
