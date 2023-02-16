using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 이 클래스는 네트워크 개체 풀링 기능을 제공하고 모든 풀 참조를 저장
// Spawning 및 despawning은 해당 방법을 호출하여 처리하지만, 또한
// 런타임에 풀을 생성하거나 풀의 모든 gameobject를 완전히 삭제하는 방법
public class PoolManager : MonoBehaviour
{
	//Prefab을 풀 컨테이너에 매핑하여 모든 인스턴스를 관리
	private static Dictionary<GameObject, Pool> Pools = new Dictionary<GameObject, Pool>();

	// 각 풀 자체에서 호출하면 사전에 추가
	public static void Add(Pool pool)
	{
		// 풀에 프리팹이 포함되어있지 않은지 확인
		if(pool.prefab == null)
		{
			Debug.LogError("풀의 프리팹: " + pool.gameObject.name + "Pools Dictionary에 이미 추가 되었습니다.");
			return;
		}

		// 사전에 추가
		Pools.Add(pool.prefab, pool);
	}

	// 런타임에 새 풀을 생성
	// 아직 연결되지 않은 프리팹들에 대해 호출
	// 에디터에서 씬의 풀로 연결되지만 그럼에도 불구하고 Spawn()을 통해 호출
	public static void CreatePool(GameObject prefab, int preLoad, bool limit, int maxCount)
	{
		// 이미 풀이 추가된경우 디버그 에러 발생
		if (Pools.ContainsKey(prefab))
		{
			Debug.LogError("이미 풀 매니저에 프리팹 풀이 포함: " + prefab.gameObject.name);
			return;
		}

		// 새로운 풀 컴포넌트를 고정할 새로운 gameobject 생성
		GameObject newPoolGO = new GameObject("Pool " + prefab.name);
		// 새로운 게임오브젝트를 씬안에 풀 컴포넌트를 추가
		Pool newPool = newPoolGO.AddComponent<Pool>();
		// 기본 매개변수 할당
		newPool.prefab = prefab;
		newPool.preLoad = preLoad;
		newPool.limit = limit;
		newPool.maxCount = maxCount;
		// 변수를 할당한 후 초기화
		newPool.Awake();
	}

	// 원하는 위치에서 전달된 프리팹에 대한 사전 인스턴스를 활성화
	public static GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
	{
		// 풀에서 prefab을 찾을 수 없는 경우 로그 항목 제거
		// 실행 시, 새로운 풀을 생성하기 때문에 이는 중요하지 않음
		if(Pools.ContainsKey(prefab) == false)
		{
			Debug.Log("기존 풀에서 프리팹을 찾을 수 없음: " + prefab.name + " 새 풀이 생성되었습니다.");
			CreatePool(prefab, 0, false, 0);
		}

		// 해당 풀에서 인스턴스 생성
		return Pools[prefab].Spawn(position, rotation);
	}

	// 나중에 사용하기 위해 이전에 생성 된 인스턴스를 비활성화
	// despawn 루틴을 지연시키기 위해 선택적으로 시간 값을 사용
	public static void Despawn(GameObject instance, float time = 0.0f)
	{
		if (time > 0)
			GetPool(instance).Despawn(instance, time);
		else
			GetPool(instance).Despawn(instance);
	}

	// 풀링된 객체를 빠르게 조회할 수 있는 편리한 방법
	// 인스턴스가 발견된 풀 구성 요소를 반환
	public static Pool GetPool(GameObject instance)
	{
		// 풀로 이동하여 인스턴스 찾기
		foreach(GameObject prefab in Pools.Keys)
		{
			if (Pools[prefab].active.Contains(instance))
				return Pools[prefab];
		}

		// 풀에서 인스턴스를 찾을 수 없습니다.
		Debug.LogError("PoolManager 풀에서 인스턴스를 찾을 수 없음: " + instance.name);
		return null;
	}

	// 풀의 모든 인스턴스를 삭제하여 나중에 사용할 수 있도록 함
	public static void DeactivatePool(GameObject prefab)
	{
		// 풀을 이전에 추가하지 않은 경우 삭제 오류
		if(Pools.ContainsKey(prefab) == false)
		{
			Debug.LogError("PoolManager가 prefab을 비활성화할 풀을 찾을 수 없음: " + prefab.name);
			return;
		}

		// 활성화 캐시 수
		int count = Pools[prefab].active.Count;
		// 각 활성 인스턴스를 순환
		for(int iCnt = count - 1; iCnt > 0; iCnt--)
		{
			Pools[prefab].Despawn(Pools[prefab].active[iCnt]);
		}
	}

	// 모든 풀에서 폐기된 모든 인스턴스를 폐기하여 메모리를 확보
	// 'limitToPreLoad' 매개 변수는 사전 로드 위의 인스턴스만 결정
	// 비활성화된 인스턴스를 최소한으로 유지하려면 값을 제거
	public static void DestroyAllInactive(bool limitToPreLoad)
	{
		foreach (GameObject prefab in Pools.Keys)
			Pools[prefab].DestroyUnused(limitToPreLoad);
	}

	// 특정 Prefab에 대한 풀을 삭제
	// 호출 후, 활성 또는 비활성 인스턴스를 더 이상 사용할 수 없음
	public static void DestroyPool(GameObject prefab)
	{
		// 풀을 이전에 추가하지 않은 경우 삭제 오류
		if (!Pools.ContainsKey(prefab))
		{
			Debug.LogError("PoolManager가 prefab을 제거할 풀을 찾을 수 없음: " + prefab.name);
			return;
		}

		// 모든 자식들을 포함하여 gameobject를 삭제
		// 우리 게임 로직은 인스턴스를 수정하지 않음
		// 하지만 그럴 경우 활성 및 비활성 인스턴스를 수동으로 순환하여 폐기해야함
		Destroy(Pools[prefab].gameObject);
		// 사전에서 키 - 값 쌍을 제거
		Pools.Remove(prefab);
	}

	// 관리자 사전에 저장된 모든 풀을 소멸시킴
	// 활성 또는 비활성 인스턴스를 호출 한 후 더 이상 사용할 수 없음
	public static void DestroyAllPools()
	{
		// 풀 사전에 루프를 돌리고 모든 풀을 파괴 게임 객체
		// 추가 코멘트를 위해 DestroyPool 메소드를 참조
		foreach (GameObject prefab in Pools.Keys)
			DestroyPool(Pools[prefab].gameObject);
	}

	// 정적 변수는 항상 씬 변경에 대한 값을 유지
	// 게임이 끝나거나 씬을 전환 할 때 재설정해야함
	private void OnDestroy()
	{
		Pools.Clear();
	}
}
