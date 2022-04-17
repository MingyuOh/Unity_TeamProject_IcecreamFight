using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplatterManager : MonoSingleton<SplatterManager>
{
	public List<Texture> icecreamTextureList;
	public List<Texture2D> icecreamTexture2DList;

	public Texture2D splatTexture;

	private float[,] splatTextureInfos;

	protected override void Awake()
	{
		base.Awake();
	}

	// Start is called before the first frame update
	void Start()
    {
		int width = splatTexture.width;
		int height = splatTexture.height;
		Color[] pixels = splatTexture.GetPixels();
		splatTextureInfos = new float[width, height];
		int count = 0;

		for (int x = 0; x < width; x++)
		{
			for(int y = 0; y < height; y++)
			{
				splatTextureInfos[x, y] = pixels[count].a;
				count++;
			}
		}

		for (int textureCount = 0; textureCount < icecreamTextureList.Count; textureCount++)
		{
			icecreamTexture2DList.Add(new Texture2D(icecreamTextureList[textureCount].width, icecreamTextureList[textureCount].height, TextureFormat.RGBA32, false));
		}
		icecreamTextureList.Clear();
	}

    public float[,] GetSplatTextureInfos()
	{
		if(splatTextureInfos != null)
			return splatTextureInfos;

		return null;
	}
}
