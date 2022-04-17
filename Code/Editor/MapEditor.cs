using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// 에디터 스크립트가 어떤 클래스 혹은 스크립트를 다루는지 명시
// CustomEditor 키워드로 이 에디터 스크립트가 다룰것이라 선언한
// 오브젝트는 target으로 접근할 수 있게 자동으로 설정
#if UNITY_EDITOR
[CustomEditor(typeof(MapGenerator))]
public class MapEditor : Editor 
{
	public override void OnInspectorGUI()
	{
		// 맵 에디터가 다루는 대상 오브젝트를 가리키는 target 할당
		MapGenerator map = target as MapGenerator;

		// 인스펙터에서의 값이 갱신됬을 때만 true를 반환한다.
		if (DrawDefaultInspector())
		{
			// GUI가 그려지는 매 프레임 조건이 맞았을 때 메소드를 호출해서 맵을 그림
			map.GenerateMap();
		}

		if (GUILayout.Button("Generate Map"))
		{
			// 인스펙터창에 있는 버튼을 누르면 맵을 다시 갱신
			map.GenerateMap();
		}
	}
}
#endif
