using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 레이를 가지고 컬링 처리하는 매니저 클래스
public class ObjectCullingManager : MonoBehaviour {
	
	// 현재 레이와 충돌된 오브젝트 리스트
	private List<GameObject> currentHitObjects = new List<GameObject>();
	// 이전 프레임 레이와 충돌된 오브젝트 리스트
	private List<GameObject> prevHitObjects = new List<GameObject>(); 

	// RayHit List
	private List<RaycastHit> rayHitList = new List<RaycastHit>();

	// 대상 위치
	public Transform tr;

	// 타겟 위치
	public Transform target;

	// 레이 개수
	public int rayCount;

	// 타겟과 레이를 쏘는 대상 사이의 높이
	public float height;

	// 타겟과 레이를 쏘는 대상 사이의 거리
	public float distance;

	// 레이 각도
	public float angle;

	// 레이 길이 변수
	private float rayDistance;
	
	private void Start()
	{
		// 레이 길이 정의
		rayDistance = Vector3.Distance(Vector3.up * height, Vector3.forward * distance);
	}

	private void Update()
	{
		CheckObject();
	}
	
	// 함수 분리화 필요
	public void CheckObject()
	{
		// 카메라용
		Vector3 rayPos = target.position + (Vector3.up * height - Vector3.forward * distance);
		Vector3 dir = (target.position - rayPos).normalized;
		Vector3 Dir = Quaternion.Euler(0.0f, angle, 0.0f) * dir;

		// 레이와 충돌된 정보들을 레이히트리스트에 추가
		for (int iRayCount = 0; iRayCount < rayCount; iRayCount++)
		{
			RaycastHit[] hitListInfo = null;
			// 레이와 충돌된 오브젝트들을 추출
			// 이건 레이 3개 사용하는 카메라용(태그를 이용하여 사용해야한다.)
			// 변경 필요
			if (iRayCount == 0)
			{
				hitListInfo = Physics.RaycastAll(rayPos, dir, rayDistance); // 가운데
				Debug.DrawRay(rayPos, dir * rayDistance, Color.green);
			}
			else if(iRayCount == 1)
			{
				hitListInfo = Physics.RaycastAll(rayPos, Dir, rayDistance); // 오른쪽
				Debug.DrawRay(rayPos, Dir * rayDistance, Color.blue);
			}
			else
			{
				Dir = Quaternion.Euler(0.0f, -angle, 0.0f) * dir;
				hitListInfo = Physics.RaycastAll(rayPos, Dir, rayDistance); // 왼쪽
				Debug.DrawRay(rayPos, Dir * rayDistance, Color.red);
			}

			for (int iCnt = 0; iCnt < hitListInfo.Length; iCnt++)
			{
				// 스태틱오브젝트만 검출하여 CurrentHitObjects리스트에 추가
				if (hitListInfo[iCnt].collider.gameObject.layer.Equals("STATIC_OBJECT"))
				{
					MeshRenderer renderer;
					renderer = hitListInfo[iCnt].collider.GetComponent<MeshRenderer>();
					Color color = renderer.material.color;
					color.a = 0.5f;
					renderer.material.color = color;
					currentHitObjects.Add(hitListInfo[iCnt].collider.gameObject);
				}
			}
		}
		// 이전 오브젝트가 현재 오브젝트에 존재하지 않으면 이전 오브젝트 복원
		// 코루틴으로 돌린다면 코루틴용 리스트를 하나 더 만들고 그 리스트를 제어하여 Fade-in-out을 구현한다.
		for (int iPrevObj = 0; iPrevObj < prevHitObjects.Count; iPrevObj++)
		{
			// 이전히트오브젝트 체크
			bool IsRestore = false;

			// 현재 히트된 오브젝트가 없으면 이전 히트된 오브젝트 원상 복귀
			if (currentHitObjects.Count == 0)
				IsRestore = true;

			for (int iCurObj = 0; iCurObj < currentHitObjects.Count; iCurObj++)
			{
				// 오브젝트 비교
				if (System.Object.ReferenceEquals(prevHitObjects[iPrevObj], currentHitObjects[iCurObj]) == true)
				{
					IsRestore = false;
					break;
				}
				else
				{
					if (IsRestore == false)
						IsRestore = true;
				}
			}

			// 복원해야하는 오브젝트 복원
			if(IsRestore == true)
			{
				MeshRenderer renderer;
				renderer = prevHitObjects[iPrevObj].GetComponent<MeshRenderer>();
				Color color = renderer.material.color;
				color.a = 1.0f;
				renderer.material.color = color;
			}
		}
		// PrevHitObjects리스트 초기화
		prevHitObjects.Clear();
		prevHitObjects.AddRange(currentHitObjects);
		// 현재 저장된 오브젝트 리스트 비움
		currentHitObjects.Clear();
	}
}
