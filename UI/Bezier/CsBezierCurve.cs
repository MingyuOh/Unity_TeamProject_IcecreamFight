using UnityEngine;
using System.Collections;

[System.Serializable]
public class BezierSegment
{
	public Vector3	Position;
	public Vector3	startTangent;
	public Vector3	endTangent;
}

public class CsBezierCurve : MonoBehaviour {

	[HideInInspector]
	public bool				autoTangent = true;

	[HideInInspector]
	public bool				controlSegment = true;

	[HideInInspector]
	public bool				controlTangent = true;

	public BezierSegment[]	bezierCurve = new BezierSegment[2];

	//==================================================//

	public Vector3 GetPointInBezierCurve (float _u) {
		_u = _u > 0f ? _u : 0f;
		_u = _u < 1f ? _u : 1f;

		if (_u == 1f) {
			return bezierCurve [bezierCurve.Length-1].Position;
		}


		float uv = _u * (bezierCurve.Length - 1);
		int bezierIndex = (int)uv;

		return GetPointInRecursiveBezierCurve (bezierIndex, bezierIndex + 1, uv - bezierIndex);
	}

	private Vector3 GetPointInRecursiveBezierCurve (int _from, int _to, float _u) {
		Vector3 midPoint01 = Vector3.Lerp (bezierCurve [_from].Position, bezierCurve [_from].endTangent, _u);
		Vector3 midPoint02 = Vector3.Lerp (bezierCurve [_from].endTangent, bezierCurve [_to].startTangent, _u);
		Vector3 midPoint03 = Vector3.Lerp (bezierCurve [_to].startTangent, bezierCurve [_to].Position, _u);

		midPoint01 = Vector3.Lerp(midPoint01, midPoint02, _u);
		midPoint02 = Vector3.Lerp(midPoint02, midPoint03, _u);

		midPoint01 = Vector3.Lerp (midPoint01, midPoint02, _u);

		return midPoint01;
	}
}