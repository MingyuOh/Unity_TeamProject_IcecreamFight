using UnityEngine;
using UnityEditor;
using System.Collections;

#if UNITY_EDITOR
[CustomEditor(typeof(CsBezierCurve))]
public class CsBezierCurveEditor : Editor {

	private CsBezierCurve S;

	void OnEnable(){
		S = (CsBezierCurve)target;
	}

	void OnSceneGUI () {
		if (S.bezierCurve.Length <= 1)
			return;

		Handles.matrix = S.transform.localToWorldMatrix;

		DrawCustomHandles ();

		DrawCustomGUIs ();
	}


	void DrawCustomHandles () {
		DrawCustomPositionHandle ();
		DrawCustomBezierCurve ();
	}

	void DrawCustomPositionHandle () {
		Vector3 gap = Vector3.zero;


		//S.bezierCurve [0].Position = 0;
		S.bezierCurve [0].startTangent = Vector3.zero;
		if (S.controlSegment) {
			Handles.SphereCap (0, S.bezierCurve [0].Position, Quaternion.identity, 0.2f);
		}
		if (S.controlTangent) {
			S.bezierCurve [0].endTangent = Handles.PositionHandle (S.bezierCurve [0].endTangent, Quaternion.identity);
			Handles.color = Color.black;
			Handles.DrawLine (S.bezierCurve [0].Position, S.bezierCurve [0].endTangent);
			Handles.CubeCap (0, S.bezierCurve [0].endTangent, Quaternion.identity, 0.2f);
		}




		int numOfSegment = S.bezierCurve.Length;
		for (int i = 1; i < numOfSegment; i++) {
			Handles.color = Color.white;
			if (S.controlSegment) {
				gap = Handles.PositionHandle (S.bezierCurve [i].Position, Quaternion.identity) - S.bezierCurve [i].Position;
				Handles.SphereCap (0, S.bezierCurve [i].Position, Quaternion.identity, 0.2f);
				S.bezierCurve [i].Position += gap;
			}

			if (S.controlTangent) {
				S.bezierCurve [i].startTangent = Handles.PositionHandle (
					S.bezierCurve [i].startTangent,
					Quaternion.identity
				) + gap;
			} else {
				S.bezierCurve [i].startTangent += gap;
			}

	

			if (S.autoTangent) {
				S.bezierCurve [i].endTangent = S.bezierCurve [i].Position * 2f - S.bezierCurve [i].startTangent;
			} else {
				if (S.controlTangent) {
					S.bezierCurve [i].endTangent = Handles.PositionHandle (S.bezierCurve [i].endTangent, Quaternion.identity) + gap;
					Handles.color = Color.black;
					Handles.CubeCap (0, S.bezierCurve [i].endTangent, Quaternion.identity, 0.2f);
				} else {
					S.bezierCurve [i].endTangent += gap;
				}
			}

			if (S.controlTangent) {
				Handles.color = Color.black;
				Handles.DrawLine (S.bezierCurve [i].Position, S.bezierCurve [i].startTangent);
				Handles.DrawLine (S.bezierCurve [i].Position, S.bezierCurve [i].endTangent);
				Handles.CubeCap (0, S.bezierCurve [i].startTangent, Quaternion.identity, 0.2f);
			}
		}
	}

	void DrawCustomBezierCurve () {
		int numOfSegment = S.bezierCurve.Length - 1;
		for (int i = 0; i < numOfSegment; i++) {
			Handles.DrawBezier (
				S.bezierCurve [i].Position,
				S.bezierCurve [i+1].Position,
				S.bezierCurve [i].endTangent,
				S.bezierCurve [i+1].startTangent,
				Color.white,
				null,
				2f
			);
		}
	}

	void DrawCustomGUIs () {
		Handles.BeginGUI ();
		GUILayout.BeginArea (new Rect(50, 200, 150, 500));

		if (GUILayout.Button ("AutoTangent", GUILayout.MinHeight(50f))) {
			S.autoTangent = !S.autoTangent;
		}
		if (GUILayout.Button ("ShowPathOnly", GUILayout.MinHeight(25f))) {
			S.controlSegment = false;
			S.controlTangent = false;
		}

		if (S.controlTangent) {
			if (GUILayout.Button ("ControlSegmentOnly", GUILayout.MinHeight (50f))) {
				S.controlSegment = true;
				S.controlTangent = false;
			}
		} else {
			if (GUILayout.Button ("ControlTangentOnly", GUILayout.MinHeight(50f))) {
				S.controlSegment = false;
				S.controlTangent = true;
			}
		}

		if (GUILayout.Button ("ControlBoth", GUILayout.MinHeight(25f))) {
			S.controlSegment = true;
			S.controlTangent = true;
		}


		GUILayout.EndArea ();
		Handles.EndGUI ();
	}
}
#endif