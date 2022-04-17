using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pool : MonoBehaviour {

	// 풀링하기위한 프리팹 생성
	public GameObject prefab;

	// 게임 시작 시 생성할 인스턴스 양
	public int preLoad = 0;

	// 런타임에 새 인스턴스 생성을 제한할지 여부
	public bool limit = false;

	// limit 변수가 활성화된 경우 최대 인스턴스 양
	public int maxCount;

	// 풀에서 활성화된 프리팹 인스턴스 목록
	[HideInInspector]
	public List<GameObject> active = new List<GameObject>();

	// 풀에서 비활성화된 프리팹 인스턴스 목록
	[HideInInspector]
	public List<GameObject> inactive = new List<GameObject>();

	// 런타임 생성 풀에서 풀관리자가 호출하는 초기화
	public void Awake()
	{
		// 프리팹 없이는 초기화 불가능
		if (prefab == null)
			return;

		// 풀 관리자 사전에 이 풀 추가
		PoolManager.Add(this);

		PreLoad();
	}

	// 플레리 시간 전에 지정된 개체 양을 로드
	public void PreLoad()
	{
		if(prefab == null)
		{
			Debug.LogWarning("풀안에 프리팹이 비어 있습니다. 프리로드가 발생하지 않습니다. 참조를 확인하십시오.");
			return;
		}

		// 정의된 프리로드 양을 인스턴스화 하지만 최대 개체 양을 초과하지 않음
		for(int iCnt = totalCount; iCnt < preLoad; iCnt++)
		{
			// 프리팹의 새 인스턴스를 인스턴스화
			GameObject obj = (GameObject)Object.Instantiate(prefab, Vector3.zero, Quaternion.identity);
			// 이 위치의 새 인스턴스를 부모로 지정
			obj.transform.SetParent(transform);

			// 편집자 개요가 용이하도록 고유한 제목으로 변경
			Rename(obj.transform);
			// 자식 개체들을 포함한 개체 비활성화
			obj.SetActive(false);
			// 비활성 인스턴스 리스트에 개체 추가
			inactive.Add(obj);
		}
	}

	// 이 풀의 새 인스턴스를 활성화(또는 인스턴스화)
	public GameObject Spawn(Vector3 position, Quaternion rotation)
	{
		// 변수 초기화
		GameObject obj;
		Transform transform;

		// 활성화할 비활성 개체가 존재
		if(inactive.Count > 0)
		{
			// 리스트에서 첫번째 비활성화 객체를 가져옴
			obj = inactive[0];
			// 가져온 객체를 활성화하고, 비활성화 리스트에서 제거
			inactive.RemoveAt(0);

			// 이 객체의 transform을 가져옴
			transform = obj.transform;
		}
		else
		{
			// 사용할 수 있는 비활성 개체가 없음
			// 새로운 것을 인스턴스화
			// 제한된 카운트가 새 인스턴스화를 허용하는지 확인
			// 아니면 아무것도 반환하지 않음
			if (limit && active.Count >= maxCount)
				return null;

			// 인스턴스화 가능, 프리팹을 새로운 인스턴스로 인스턴스화
			obj = (GameObject)Object.Instantiate(prefab);
			// 인스턴스의 transform을 가져옴
			transform = obj.transform;
			// 편집자 개요를 위해 고유한 제목으로 이름을 변경
			Rename(transform);
		}

		// 위치 및 회전 통과 설정
		transform.position = position;
		transform.rotation = rotation;

		// 부모의 위치에 속하지 않은 경우, 부모 재설정 
		if (transform.parent != transform)
			transform.parent = transform;

		// 활성화 인스턴스들의 리스트에 객체를 추가
		active.Add(obj);
		// 자식오브젝트들을 포함하는 활성화 객체
		obj.SetActive(true);
		// 이 개체의 모든 구성 요소 및 하위 항목에 대해 OnSpawn() 메서드 호출
		obj.BroadcastMessage("OnSpawn", SendMessageOptions.DontRequireReceiver);

		// 인스턴스 반환
		return obj;
	}

	// 나중에 사용할 수 있도록 이 풀의 인스턴스를 비활성화
	public void Despawn(GameObject instance)
	{
		// 이 인스턴스에 대한 활성 인스턴스 검색
		if(active.Contains(instance) == false)
		{
			Debug.LogWarning("인스턴스(아이템)를 사라지게 할 수 없음 - 인스턴스를 찾을 수 없음: " + instance.name);
			return;
		}

		// 런타임 중에 부모 항목이 없는 경우 다시 부모항목 지정
		if (instance.transform.parent != transform)
			instance.transform.parent = transform;

		// 비활성화하고 활성 목록에서 제거
		active.Remove(instance);
		// 비활성화 리스트에 개체 추가
		inactive.Add(instance);
		// 이 객체의 자식과 모든 컴포넌트를 OnDespan() 함수로 호출
		instance.BroadcastMessage("OnDespawn", SendMessageOptions.DontRequireReceiver);
		// 자식객체들을 포함한 이 객체를 비활성화
		instance.SetActive(false);
	}

	// 나중에 사용할 수 있도록 이 풀의 인스턴스 비활성화 시간을 지정
	public void Despawn(GameObject instance, float time)
	{
		// 새로운 PoolTimeObject를 생성하여 인스턴스를 추적
		PoolTimeObject timeObject = new PoolTimeObject();
		// 이 클래스의 시간 및 인스턴스 변수 할당
		timeObject.instance = instance;
		timeObject.time = time;

		// 생성된 속성을 사용하여 시간 비활성화를 사용 안 함
		StartCoroutine(DespawnInTime(timeObject));
	}

	// 인스턴스를 비활성화하기 전에 '시간'을 기다리는 코루틴
	IEnumerator DespawnInTime(PoolTimeObject timeObject)
	{
		// 비활성화 객체 캐시
		GameObject instance = timeObject.instance;

		// 정의된 초를 기다림
		float timer = Time.time + timeObject.time;
		while (instance.activeInHierarchy && Time.time < timer)
			yield return null;

		// 이미 비활성화된 인스턴스
		if (instance.activeInHierarchy == false)
			yield break;

		// 이제 인스턴스(아이템)을 사라지게 함
		Despawn(instance);
	}

	// 이 풀의 모든 비활성 인스턴스를 삭제(가비지 컬랙터 무거움)
	// 매개변수는 사전 로드 값 이상의 인스턴스만 삭제해야 하는지 여부를 결정
	public void DestroyUnused(bool limitToPreLoad)
	{
		// 제한치 이상의 객체만 파괴
		if(limitToPreLoad)
		{
			// 마지막 비활성 인스턴스에서 시작하여 카운트 다운
			// 색인이 한계값에 도달할 때까지 실행
			for(int iCnt = inactive.Count - 1; iCnt >= preLoad; iCnt++)
			{
				// iCnt 위치에 있는 객체를 파괴
				Object.Destroy(inactive[iCnt]);
			}
			// 리스트에 범위 안에 있는 개체들(현재 null 참조)을 제거
			if (inactive.Count > preLoad)
				inactive.RemoveRange(preLoad, inactive.Count - preLoad);
		}
		else
		{
			// limitToPreLoad가 잘못됨
			// 모든 비활성 인스턴스를 삭제
			for (int iCnt = 0; iCnt < inactive.Count; iCnt++)
				Object.Destroy(inactive[iCnt]);

			// 리스트 리셋
			inactive.Clear();
		}
	}

	// 특정 양의 비활성 인스턴스(가비지 컬렉터 무거움)를 제거
	public void DestroyCount(int count)
	{
		//전달된 양이 비활성 인스턴스의 양을 초과
		if(count > inactive.Count)
		{
			Debug.LogWarning("제거 수 값: " + count + "비활성화 개수보다 더 크다: " +
				inactive.Count + ". 사용 가능한 모든 비활성 개체 유형 제거: " +
				prefab.name + ". 같은 목표를 달성하려면 사용되지 않는 삭제(false)를 사용해야 한다.");
			DestroyUnused(false);
			return;
		}

		// 끝부터 시작하여 인덱스를 카운트하고 각 비활성 인스턴스를 삭제(전달된 양을 파괴할 때 까지)
		for (int iCnt = inactive.Count - 1; iCnt >= inactive.Count - count; iCnt--)
			Object.Destroy(inactive[iCnt]);

		// 리스트에 범위 내에 있는 객체들(현재 null 참조)을 제거
		inactive.RemoveRange(inactive.Count - count, count);
	}

	// 인스턴스화 시 각 인스턴스에 대해 고유한 이름 생성
	// 편집기에서 서로 구별
	private void Rename(Transform instance)
	{
		// 총 인스턴스 수 및 다음 사용 가능한 번호 할당
		// 수백의 범위로 변환:
		// 한 번에 수천 개의 인스턴스가 있을 수 없음
		// 예: TestEnem(Clone)001
		instance.name += (totalCount + 1).ToString("#000");
	}

	// 이 풀 옵션의 모든 인스턴스 수
	private int totalCount
	{
		get
		{
			// 카운트 초기화
			int count = 0;
			// 활성화 / 비활성화 리스트 안 객체의 개수 추가
			count += active.Count;
			count += inactive.Count;
			// 최종 수 반환
			return count;
		}
	}

	// 풀을 파괴시켰을 때, 인스턴스들이 속해있는 리스트들을 Clear
	void OnDestroy()
	{
		active.Clear();
		inactive.Clear();
	}

	// 인스턴스의 시간 초과 비활성화에 사용되는 속성을 저장
	[System.Serializable]
	public class PoolTimeObject
	{
		// 비활성화할 인스턴스
		public GameObject instance;

		// 비활성화될 때까지 지연
		public float time;
	}
}
