using UnityEngine;
using System.Linq;
using System;
using System.Collections.Generic;

public class SplatterEnv : MonoBehaviour
{
	public Texture2D[] paintTextures;
	public int resolution = 2048;
	public float fieldSize = 50f;
	public Vector4 fieldOffset = new Vector4(0.5f, 0.5f, 0f, 0f);
	public Shader paintShader;
	public Texture2D occupyPaintTexture;
	public Material occupyPaintMaterial;
	public float paintDrawDepth = -100f;
	public bool stopPainting { get; set; }

	private Color[] occupyPaintColors;
	private const float occupyPaintColorStep = 0.2f;
	public RenderTexture paintBuffer;
	private RenderTexture occupationBuffer;
	private Texture2D occupationBufferTexture;
	private const int occupationBufferSize = 128;
	private int paintableOccupationPixelNum = 0;

	private float[] occupationRate;

	private List<PaintData> newDrawPositions = new List<PaintData>();

	private class PaintData
	{
		public Vector3 position { get; private set; }
		public int colorNumber { get; private set; }
		public float size { get; private set; }
		public float rotation;

		internal PaintData(Vector3 pos, int colnum, float sz, float rot)
		{
			position = pos;
			colorNumber = colnum;
			size = sz;
			rotation = rot;
		}
	}

	void Start()
	{
		occupyPaintColors = Enumerable.Range(0, paintTextures.Length).Select(i => new Color(i * occupyPaintColorStep, 0, 0)).ToArray();
		paintBuffer = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.Default);
		paintBuffer.wrapMode = TextureWrapMode.Clamp;
		if (occupyPaintTexture != null)
		{
			InitOccupationBuffer();
		}

		// 모든 오브젝트의 렌더러를 가지고 오는 루프
		foreach (var r in GetComponentsInChildren<Renderer>())
		{
			// 기존 오브젝트들의 매터리얼 정보를 가지고와서
			// 페인트 쉐이더를 적용하고 쉐이더 정보에 맞게 매터리얼을 변경하는 함수
			r.materials = r.materials.Select(m => NewPaintableMaterial(m)).ToArray();
		}
		Clear();
		if (occupyPaintTexture != null)
		{
			DrawOccupationMask(fieldSize * 0.5f);
		}
	}

	void Update()
	{
		if (newDrawPositions.Count > 0)
		{
			Draw(newDrawPositions);
			newDrawPositions.Clear();
		}
	}

	void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawWireCube(Vector3.zero, new Vector3(fieldSize, 0f, fieldSize));
	}

	public void Clear()
	{
		Clear(paintBuffer);
		Clear(occupationBuffer);
		newDrawPositions.Clear();
	}

	private void Clear(RenderTexture target)
	{
		if (target == null)
		{
			return;
		}
		RenderTexture.active = target;
		GL.Clear(false, true, new Color(0f, 0f, 0f, 0f));
		RenderTexture.active = null;
	}

	public void InitOccupationBuffer()
	{
		occupationBuffer = new RenderTexture(occupationBufferSize, occupationBufferSize, 16, RenderTextureFormat.Default);
		occupationBuffer.anisoLevel = 0;
		occupationBuffer.filterMode = FilterMode.Point;
		occupationBuffer.autoGenerateMips = false;
		occupationBufferTexture = new Texture2D(occupationBufferSize, occupationBufferSize);
		occupationBufferTexture.anisoLevel = 0;
		occupationBufferTexture.filterMode = FilterMode.Point;
	}

	public void DrawOccupationMask(float orthoSize)
	{
		var camobj = new GameObject();
		var cam = camobj.AddComponent<Camera>();
		cam.orthographic = true;
		cam.orthographicSize = orthoSize;
		cam.clearFlags = CameraClearFlags.Color | CameraClearFlags.Depth;
		cam.backgroundColor = Color.black;
		cam.nearClipPlane = -100;
		cam.farClipPlane = 100f;
		cam.cullingMask = 1 << LayerMask.NameToLayer("Wall");
		cam.transform.position = new Vector3(0, 0.1f, 0);
		cam.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
		cam.transform.localScale = new Vector3(1f, 1f, 0f);

		cam.targetTexture = occupationBuffer;
		var save = RenderTexture.active;
		RenderTexture.active = occupationBuffer;
		GL.Clear(true, true, new Color(0f, 0f, 0f, 0f));
		cam.Render();
		ApplyOccupationTexture();
		IterateAllPixel(occupationBufferTexture, (c) =>
		{
			if (c.r != 0 || c.b != 0 || c.g != 0)
				paintableOccupationPixelNum++;
		});
		GL.Clear(false, true, new Color(0f, 0f, 0f, 0f));
		RenderTexture.active = save;
		cam.targetTexture = null;
		Destroy(camobj);
	}

	private Material NewPaintableMaterial(Material mat)
	{
		var newMat = new Material(paintShader);
		newMat.mainTexture = mat.mainTexture;
		newMat.mainTextureOffset = mat.mainTextureOffset;
		newMat.mainTextureScale = mat.mainTextureScale;
		newMat.SetTexture("_PaintTex", paintBuffer);
		newMat.SetVector("_PaintOffset", fieldOffset);
		newMat.SetFloat("_PaintScale", 1f / fieldSize);
		return newMat;
	}

	public void AddNewPaint(Vector3 position, int textureId, float size, float rotation)
	{
		newDrawPositions.Add(new PaintData(position, textureId, size, rotation));
	}

	private void Draw(IEnumerable<PaintData> paintDataList)
	{
		RenderTexture.active = paintBuffer;
		Draw(resolution, paintDataList, pd => paintTextures[pd.colorNumber], pd => new Color(0.5f, 0.5f, 0.5f, 0.5f), null);
		RenderTexture.active = null;

		if (occupationBuffer != null)
		{
			RenderTexture.active = occupationBuffer;
			Draw(occupationBufferSize, paintDataList, pd => occupyPaintTexture, pd => occupyPaintColors[pd.colorNumber], occupyPaintMaterial);
			ApplyOccupationTexture();
			RenderTexture.active = null;
		}

		CalcOccupationRate();
	}

	private void ApplyOccupationTexture()
	{
		occupationBufferTexture.ReadPixels(new Rect(0, 0, occupationBufferSize, occupationBufferSize), 0, 0);
		occupationBufferTexture.Apply();
	}

	private void Draw(int resolution, IEnumerable<PaintData> paintDataList, Func<PaintData, Texture2D> texFunc, Func<PaintData, Color> colFunc, Material paintMat)
	{
		var srcSize = new Rect(0, 0, 1, 1);
		GL.PushMatrix();
		GL.LoadPixelMatrix(0, resolution, resolution, 0);
		foreach (var pd in paintDataList)
		{
			var psiz = resolution * pd.size / fieldSize;
			var texHalf = new Vector3(psiz * 0.5f, psiz * 0.5f, 0f);
			var trs = Matrix4x4.TRS(WorldToPaintLocal(pd.position, resolution), Quaternion.Euler(0f, 0f, pd.rotation), Vector3.one);
			var sft = Matrix4x4.TRS(-texHalf, Quaternion.identity, Vector3.one);
			GL.MultMatrix(trs * sft);
			Graphics.DrawTexture(new Rect(0, 0, psiz, psiz), texFunc(pd), srcSize, 0, 0, 0, 0, colFunc(pd), paintMat);
		}
		GL.PopMatrix();
	}

	private Vector3 WorldToPaintLocal(Vector3 pos, float resolution)
	{
		var xzpos = new Vector3(pos.x, -pos.z, 0f) / fieldSize + new Vector3(fieldOffset.x, fieldOffset.y);
		return xzpos * resolution + new Vector3(0f, 0f, paintDrawDepth);
	}

	private void CalcOccupationRate()
	{
		var counts = new int[occupyPaintColors.Length];
		IterateAllPixel(occupationBufferTexture, (pcol) =>
		{
			if (pcol.a != 0)
			{
				var idx = (int)(pcol.r / occupyPaintColorStep);
				if (idx >= 0)
				{
					counts[idx]++;
				}
			}
		});
		occupationRate = counts.Select(c => (float)c / paintableOccupationPixelNum).ToArray();
	}

	private void IterateAllPixel(Texture2D texture, Action<Color> pixelAction)
	{
		if (texture == null)
		{
			return;
		}
		for (var i = 0; i < occupationBufferSize; ++i)
		{
			for (var j = 0; j < occupationBufferSize; ++j)
			{
				var pcol = occupationBufferTexture.GetPixel(i, j);
				pixelAction(pcol);
			}
		}
	}

	public SplatterState GetSplatterState(int playerNumber, Vector3 position)
	{
		if (occupationBufferTexture == null)
			return SplatterState.None;

		var lp = WorldToPaintLocal(position, occupationBufferSize);
		var pcol = occupationBufferTexture.GetPixel((int)lp.x, occupationBufferSize - (int)lp.y);
		if (pcol == Color.black)
		{
			return SplatterState.None;
		}
		else if (pcol == occupyPaintColors[playerNumber])
		{
			return SplatterState.Mine;
		}
		else
		{
			return SplatterState.NotMine;
		}
	}
	public float GetOccupationRate(int playerNumber)
	{
		if (occupationRate != null && 0 <= playerNumber && playerNumber < occupationRate.Length)
		{
			return occupationRate[playerNumber];
		}
		else
		{
			return 0f;
		}
	}
}

public enum SplatterState
{
	None,
	Mine,
	NotMine,
}