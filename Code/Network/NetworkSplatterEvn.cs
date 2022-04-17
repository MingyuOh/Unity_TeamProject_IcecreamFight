using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;

public class NetworkSplatterEvn : PunBehaviour {

	public static NetworkSplatterEvn instance { get; private set; }
	public float paintSize = 3f;

	private int serverSplatterId = 0;
	private List<SplatterData> splatterData = new List<SplatterData>();
	private SplatterEnv splatter;
	private int clientPaintId = 0;

	void Awake()
	{
		splatter = GetComponent<SplatterEnv>();
		instance = this;
	}

	public void Start()
	{
		splatter.Clear();
	}

	public void Splatter(Vector3 position, int colorNumber, float paintScale)
	{ // 서버만 실행. 0,1,2...와 같이 번호를 붙여서 순서대로 clientのRPC를 호출
		if (PhotonNetwork.isMasterClient == true)
		{
			RpcPaint(serverSplatterId++, position, colorNumber, paintScale, Random.Range(0, 360));
		}
	}

	[PunRPC]
	public void RpcPaint(int id, Vector3 position, int colorNumber, float paintScale, int rotation)
	{ // 서버의 호출을 받아 id 순으로 PaintEnv에 paint를 한다
		splatterData.Add(new SplatterData(id, position, colorNumber, paintScale, rotation));
	}

	void Update()
	{
		while (true)
		{
			var pd = splatterData.Find(d => d.id == clientPaintId);
			if (pd == null)
			{
				return;
			}
			splatter.AddNewPaint(pd.position, pd.colorNumber, paintSize * pd.paintScale, pd.rotation);
			clientPaintId++;
		}
	}

	public SplatterState GetSplatterState(int colorNumber, Vector3 position)
	{
		return splatter.GetSplatterState(colorNumber, position);
	}

	/// <summary>
	/// 페인트탄에 의한 한 개분 데이터
	/// </summary>
	private class SplatterData
	{
		public int id;
		public Vector3 position;
		public int colorNumber;
		public float paintScale;
		public int rotation;
		public SplatterData(int id, Vector3 position, int colorNumber, float paintScale, int rotation)
		{
			this.id = id;
			this.position = position;
			this.colorNumber = colorNumber;
			this.paintScale = paintScale;
			this.rotation = rotation;
		}
	}
}
