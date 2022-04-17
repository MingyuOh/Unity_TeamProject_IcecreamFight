using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


/// <summary>
/// Unity UI 이벤트를 사용하여 플레이어 이동 및 동작을 제어하는 ​​조이스틱 구성 요소
/// 화면에 여러 개의 조이스틱이 동시에 있을 수 있으며, 서로 다른 콜백을 구현
/// </ summary>
public class UIJoystick : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
	/// <summary>
	/// 사용자 입력에 의해 조이스틱이 움직이기 시작할 때 콜백이 트리거
	/// </ summary>
	public event Action onDragBegin;

	/// <summary>
	/// 조이스틱이 움직이거나 홀드될 때 콜백이 트리거
	/// </ summary>
	public event Action<Vector2> onDrag;

	/// <summary>
	/// 조이스틱 입력이 해제되면 콜백이 트리거
	/// </ summary>
	public event Action onDragEnd;

	/// <summary>
	/// 대상 객체, 즉 사용자가 드래그 한 조이스틱 엄지 손가락.
	/// </ summary>
	public Transform target;

	/// <summary>
	/// 중심으로부터 거리에 따라 이동할 대상 오브젝트의 최대 반경.
	/// </ summary>
	public float radius = 50.0f;

	/// <summary>
	/// 2D 공간에서 x 축과 y 축에 대한 대상 객체의 현재 위치
	/// 왼쪽 / 오른쪽 아래 / 위로 번역 된 [-1, 1]의 범위에서 값이 계산
	/// </ summary>
	public Vector2 position;

	// 현재 드래그 상태를 추적
	private bool isDragging = false;

	// 드래그 한 엄지 손가락에 대한 참조
	private RectTransform thumb;

	private void Start()
	{
		thumb = target.GetComponent<RectTransform>();

		// 편집기에서 조이스틱 그래픽이 수신 한 입력을 비활성화
		// 조이스틱을 볼 수 있지만 모든 입력을 수신하거나 차단하지 않음
		#if UNITY_EDITOR
			Graphic[] graphics = GetComponentsInChildren<Graphic>();
			for (int iCnt = 0; iCnt < graphics.Length; iCnt++)
				graphics[iCnt].raycastTarget = false;
		#endif
	}

	/// <summary>
	/// 드래그 시작시 UI Eventsystem에 의해 시작된 이벤트
	/// </ summary>
	public void OnBeginDrag(PointerEventData data)
	{
		isDragging = true;
		if (onDragBegin != null)
			onDragBegin();
	}

	/// <summary>
	/// 드래그에 UI Eventsystem에 의해 시작된 이벤트
	/// </ summary>
	public void OnDrag(PointerEventData data)
	{
		// 관련된 구성 요소의 RectTransforms를 가져옴
		RectTransform draggingPlane = transform as RectTransform;
		Vector3 mousePos;

		// 드래그 된 위치가 드래깅 rect 안에 있는지 확인하고,
		// 전역 마우스 위치를 설정하고 조이스틱을 엄지 손가락에 지정
		if(RectTransformUtility.ScreenPointToWorldPointInRectangle(draggingPlane, data.position, data.pressEventCamera, out mousePos))
		{
			thumb.position = mousePos;
		}

		// 터치 벡터의 길이 (크기)
		// 조이스틱 엄지의 상대적 위치로부터 계산
		float length = target.localPosition.magnitude;


		// 엄지 손가락이 조이스틱의 경계를 벗어나면 최대 반경까지 클램프
		if(length > radius)
		{
			target.localPosition = Vector3.ClampMagnitude(target.localPosition, radius);
		}

		// 실제 스프라이트 위치를 기반으로 Vector2 thumb 위치를 설정
		position = target.localPosition;

		// 이전 위치를 기반으로 Vector2 엄지 손가락 위치를 부드럽게 지정
		position = position / radius * Mathf.InverseLerp(radius, 2, 1);
	}

	// 조이스틱 엄지 위치를 설정하여 각 프레임 위치로 드래그
	private void Update()
	{

		// 편집기에서 조이스틱 위치가 움직이지 않으면 시뮬레이트해야 함
		// 플레이어 입력을 조이스틱 위치로 미러링하고 그 위치에서 엄지 손가락 위치를 계산
		#if UNITY_EDITOR
			target.localPosition = position * radius;
			target.localPosition = Vector3.ClampMagnitude(target.localPosition, radius);
		#endif

		// 실제 드래그 상태와 콜백을 확인합니다. Update ()에서 이 작업을 수행
		// OnDrag가 아니다. OnDrag는 조이스틱이 움직일 때만 호출되기 때문
		// 하지만 실제로 jostick이 눌러져 있어도 플레이어를 계속 움직이기를 원함
		if (isDragging && onDrag != null)
			onDrag(position);
	}

	/// <summary>
	/// 드래그가 끝나면 UI Eventsystem에 의해 시작된 이벤트
	/// </ summary>
	public void OnEndDrag(PointerEventData data)
	{
		// 더이상 드래그를 할 수 없으며 위치를 기본값으로 재설정
		position = Vector2.zero;
		target.position = transform.position;

		// 드래그 플래그를 false로 설정하고 onDragEnd() 콜백함수 호출
		isDragging = false;
		if (onDragEnd != null)
			onDragEnd();
	}
}
