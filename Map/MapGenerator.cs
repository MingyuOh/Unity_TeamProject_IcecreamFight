using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour {

	// 저장된 맵 배열
	public Map[] maps;
	// 현재 지정하고 있는 맵의 인덱스
	public int mapIndex;

	public Transform tilePrefab;
	public Transform[] obstaclePrefab;
	public Transform navMeshMaskPrefab;
	public Transform navMeshFloor;
	public Vector2 maxMapSize;

	[Range(0, 1)]
	public float outlinePercent;

	// 타일 크기
	public float tileSize;

	// 모든 타일 좌표에 대한 리스트
	private List<Coordinate> allTileCoords;
	// 셔플된 좌표를 저장할 변수
	private Queue<Coordinate> shuffledTileCoords;
	// 스폰 위치(최대 6개)
	private List<Coordinate> spawnCoords;

	// 현재 맵을 저장 할 변수
	private Map currentMap;

	// Use this for initialization
	void Start () {
		GenerateMap();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void GenerateMap()
	{
		currentMap = maps[mapIndex];
		System.Random radomObstacle = new System.Random(currentMap.seed);

		///////////////////////////////////////////////////////////////////////////
		// 좌표 생성
		///////////////////////////////////////////////////////////////////////////
		allTileCoords = new List<Coordinate>();
		for (int x = 0; x < currentMap.mapSize.x; x++)
		{
			for (int y = 0; y < currentMap.mapSize.y; y++)
			{
				allTileCoords.Add(new Coordinate(x, y));
			}
		}
		// 셔플된 좌표를 저장할 변수 할당
		shuffledTileCoords = new Queue<Coordinate>(Utility.ShuffleArray(allTileCoords.ToArray(), currentMap.seed));

		////////////////////////////////////////////////////////////////////////////
		// 맵 홀더 오브젝트 생성
		///////////////////////////////////////////////////////////////////////////
		// 타일들을 담을 부모 오브젝트를 생성
		// 이미 타일이 존재하면 지우고 새로 만든다.
		string holderName = "Generated Map";
		if (transform.Find(holderName))
		{
			// DestroyImmediate()는 게임 오브젝트만 파괴 가능(에디터에서 호출하므로 Destroy함수 호출이 아닌 DestroyImmediate 함수 호출)
			DestroyImmediate(transform.Find(holderName).gameObject);
		}
	
		Transform mapHolder = new GameObject(holderName).transform;
		// 현재 오브젝트의 아래에 위치시킨다.
		mapHolder.parent = transform;


		////////////////////////////////////////////////////////////////////////////
		// 타일 스폰
		///////////////////////////////////////////////////////////////////////////
		for (int x = 0; x < currentMap.mapSize.x; x++)
		{
			for(int y = 0; y < currentMap.mapSize.y; y++)
			{
				// 맨 왼쪽 위 좌표(0 , 0)이므로 -mapsize / 2.0f
				// 중심이 (0, 0)에 가므로 + 0.5f
				Vector3 tilePos = CoordToPosition(x, y);
				// 새로운 타일을 타일 위치에 놓고 x축으로 90도 회전시킨다.
				Transform newTile = Instantiate(tilePrefab, tilePos, Quaternion.Euler(Vector3.right * 90.0f)) as Transform;
				// Vector3.one * (1 - outlinePercent) 테두리 영역만큼 타일의 크기를 줄여 할당
				newTile.localScale = Vector3.one * (1 - outlinePercent) * tileSize;
				// 새 타일 생성할 때마다, 새 타일의 부모를 맵 홀더로 해준다.
				newTile.parent = mapHolder;
			}
		}

		////////////////////////////////////////////////////////////////////////////
		// 장애물 오브젝트 스폰
		///////////////////////////////////////////////////////////////////////////
		// 맵 전체에 접근이 가능한지 여부
		bool[,] obstacleMap = new bool[(int)currentMap.mapSize.x, (int)currentMap.mapSize.y];

		// 모든 타일 중 장애물로 지정될 타일에 대한 퍼센테이지
		int obstacleCount = (int)(currentMap.mapSize.x * currentMap.mapSize.y * currentMap.obstaclePercent);
		int currentObstacleCount = 0;
		for (int iCnt = 0; iCnt < obstacleCount; iCnt++)
		{
			Coordinate randomCoord = GetRandomCoord();
			// obstacleMap 갱신
			obstacleMap[randomCoord.x, randomCoord.y] = true;
			currentObstacleCount++;

			// 스폰위치와 비교해야한다. 지금은 맵 정중앙이 센터
			if (randomCoord != currentMap.mapCenter && MapIsFullyAccessible(obstacleMap, currentObstacleCount))
			{
				int obstacleIndex = radomObstacle.Next(0, obstaclePrefab.Length);
				// 얻은 좌표를 게임 월드의 Vector3로 변환
				Vector3 obstacleposition = CoordToPosition(randomCoord.x, randomCoord.y);
				Transform newObstacle = Instantiate(obstaclePrefab[obstacleIndex], obstacleposition, obstaclePrefab[obstacleIndex].rotation) as Transform;
				// 타일 크기에 맞춰서 장애물의 크기도 변경하려면 아래 주석 풀어준다.
				//newObstacle.localScale = Vector3.one * (1 - outlinePercent) * tileSize;
				newObstacle.parent = mapHolder;
			}
			else
			{
				// 생성 실패하였을 경우
				obstacleMap[randomCoord.x, randomCoord.y] = false;
				currentObstacleCount--;
			}
		}

		////////////////////////////////////////////////////////////////////////////
		// NavMesh Mask 생성
		///////////////////////////////////////////////////////////////////////////
		// NavMesh Mask 왼쪽
		// 최대 맵사이즈와 실제 맵사이즈 사이의 중앙 점 계산
		Transform maskLeft = Instantiate(navMeshMaskPrefab, Vector3.left * (currentMap.mapSize.x + maxMapSize.x) / 4 * tileSize, Quaternion.identity) as Transform;
		maskLeft.parent = mapHolder;
		// 실제 맵의 가장자리와 최대 맵 사이즈의 가장자리 사이의 거리
		maskLeft.localScale = new Vector3((maxMapSize.x - currentMap.mapSize.x)/ 2, 1.0f, currentMap.mapSize.y) * tileSize;

		// NavMesh Mask 오른쪽
		// 최대 맵사이즈와 실제 맵사이즈 사이의 중앙 점 계산
		Transform maskRight = Instantiate(navMeshMaskPrefab, Vector3.right * (currentMap.mapSize.x + maxMapSize.x) / 4 * tileSize, Quaternion.identity) as Transform;
		maskRight.parent = mapHolder;
		// 실제 맵의 가장자리와 최대 맵 사이즈의 가장자리 사이의 거리
		maskRight.localScale = new Vector3((maxMapSize.x - currentMap.mapSize.x) / 2, 1.0f, currentMap.mapSize.y) * tileSize;

		// NavMesh Mask 위
		// 최대 맵사이즈와 실제 맵사이즈 사이의 중앙 점 계산
		Transform maskTop = Instantiate(navMeshMaskPrefab, Vector3.forward * (currentMap.mapSize.y + maxMapSize.y) / 4 * tileSize, Quaternion.identity) as Transform;
		maskTop.parent = mapHolder;
		// 실제 맵의 가장자리와 최대 맵 사이즈의 가장자리 사이의 거리
		maskTop.localScale = new Vector3(maxMapSize.x, 1.0f, (maxMapSize.y - currentMap.mapSize.y) / 2) * tileSize;

		// NavMesh Mask 아래
		// 최대 맵사이즈와 실제 맵사이즈 사이의 중앙 점 계산
		Transform maskBottom = Instantiate(navMeshMaskPrefab, Vector3.back * (currentMap.mapSize.y + maxMapSize.y) / 4 * tileSize, Quaternion.identity) as Transform;
		maskBottom.parent = mapHolder;
		// 실제 맵의 가장자리와 최대 맵 사이즈의 가장자리 사이의 거리
		maskBottom.localScale = new Vector3(maxMapSize.x, 1.0f, (maxMapSize.y - currentMap.mapSize.y) / 2) * tileSize;

		navMeshFloor.localScale = new Vector3(maxMapSize.x, maxMapSize.y) * tileSize;
	}

	// Flood Fill 알고리즘을 통해 새 장애물이 추가됬어도 맵이 전체가 접근 가능한지 체크
	// 접근 가능하면 그대로 진행하여 새 장애물을 게임 월드에 인스턴스화 하고
	// 접근 불가능하면 되돌아가서 새 좌표를 찾는다.
	// 중앙에 장애물이 없으니 먼저 obstacleMap의 중앙에서부터 시작해서 밖으로 퍼져나가며 타일들을 검색
	// 그 다음, 전체 타일수가 얼마나 되는지 알고 있는 상태에서 currentObstacleCount를 이용하여
	// 장애물이 아닌 타일이 얼만큼 반드시 존재해야 하는지 알 수 있다.
	// Flood Fill 알고리즘으로 얻은 값이 반드시 존재해야 하는 비장애물 타일 개수와 다르다면
	// 맵에 있는 모든 타일에 닿지 못하였다는 것이다.(장애물에의해 막혔다는 뜻)
	// 이 경우 맵 전체가 접근 가능한 것이 아니라는 것이고 false를 리턴한다.
	bool MapIsFullyAccessible(bool[,] obstacleMap, int currentObstacleCount)
	{
		bool[,] mapFlags = new bool[obstacleMap.GetLength(0), obstacleMap.GetLength(1)];
		Queue<Coordinate> queue = new Queue<Coordinate>();
		// 중앙부터 타일을 넣는다.
		queue.Enqueue(currentMap.mapCenter);
		mapFlags[currentMap.mapCenter.x, currentMap.mapCenter.y] = true;

		// 접근가능했던 타일 수
		int accessibleTileCount = 1;

		// 실제 Flood Fill
		// 큐 안에 좌표가 있는 동안(처음엔 mapCenter 하나뿐)
		while(queue.Count > 0)
		{
			// queue는 큐의 첫번째 아이템을 가져올 뿐만아니라 그것을 큐에서 제거한다.
			Coordinate tile = queue.Dequeue();
			
			// 큐에서 꺼내온 좌표에 근접한 네개의 이웃 타일들을 루프
			for(int x = -1; x <= 1; x++)
			{
				// 큐에서 꺼내온 좌표에 근접한 네개의 이웃 타일들을 루프
				for (int y = -1; y <= 1; y++)
				{
					int neighbourX = tile.x + x;
					int neighbourY = tile.y + y;
					// 맵 전체의 모서리와 테두리부분 제어를 잘 해야한다.
					// 대각선 방향은 체크하지 않는다.
					if(x == 0 || y == 0)
					{
						// 맵 내부인지 체크
						if(neighbourX >= 0 && neighbourX < obstacleMap.GetLength(0) 
							&& neighbourY >= 0 && neighbourY < obstacleMap.GetLength(1))
						{
							// 타일을 이전에 체크하지 않았다면 그리고 장애물인지 아닌지 체크
							if(mapFlags[neighbourX, neighbourY] == false &&
								obstacleMap[neighbourX, neighbourY] == false)
							{
								// 타일 체크
								mapFlags[neighbourX, neighbourY] = true;
								queue.Enqueue(new Coordinate(neighbourX, neighbourY));
								accessibleTileCount++;
							}
						}
					}
				}
			}
		}
		// 본래 장애물이 아닌 타일이 얼마나 존재하였는지 알아야한다.
		int targetAccessibleTileCount = (int)(currentMap.mapSize.x * currentMap.mapSize.y - currentObstacleCount);
		return targetAccessibleTileCount == accessibleTileCount;
	}

	Vector3 CoordToPosition(int x, int y)
	{ 
		return new Vector3((-currentMap.mapSize.x / 2) + 0.5f + x, 0.0f, (-currentMap.mapSize.y / 2) + 0.5f + y) * tileSize;
	}

	// 큐로 부터 다음 아이템을 얻어 랜덤 좌표를 반환하는 함수
	public Coordinate GetRandomCoord()
	{
		// 셔플된 타일 좌표 큐의 첫 아이템을 가지도록 Dequeue호출하여 할당
		Coordinate randomCoord = shuffledTileCoords.Dequeue();
		// 큐의 마지막으로 다시 되돌려 놓는다.
		shuffledTileCoords.Enqueue(randomCoord);
		return randomCoord;
	}

	// 맵의 모든 타일에 대한 모든 좌표를 생성
	[System.Serializable]
	public struct Coordinate
	{
		public int x;
		public int y;

		public Coordinate(int _x, int _y)
		{
			x = _x;
			y = _y;
		}

		public static bool operator==(Coordinate coord, Coordinate otherCoord)
		{
			return coord.x == otherCoord.x && coord.y == otherCoord.y;
		}

		public static bool operator !=(Coordinate coord, Coordinate otherCoord)
		{
			return !(coord == otherCoord);
		}
	}

	[System.Serializable]
	public class Map
	{
		public Coordinate mapSize;
		[Range(0, 1)]
		public float obstaclePercent;       // 타일 생성이후 생성할 장애물들 수를 정한다
		public int seed;

		// 플레이어 스폰 위치(중앙) - 변경되어야 한다.
		public Coordinate mapCenter
		{
			get
			{
				return new Coordinate(mapSize.x / 2, mapSize.y / 2);
			}
		}
	}
}
