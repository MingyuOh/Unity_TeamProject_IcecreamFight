using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplatterReceiver : MonoBehaviour
{
	private int texSize = 256;
	private static object syncObj = new object();
	private bool splatFlag = false;
	private Texture2D texture;
	private Material material;

	// Start is called before the first frame update
	void Start()
	{
		Renderer r = GetComponent<Renderer>();
		if (r == null)
			return;

		foreach (Material mat in r.materials)
		{
			if (mat.shader.name.Contains("Custom/SplatterShader"))
			{
				material = mat;
				break;
			}
		}

		if (material != null)
		{
			texture = new Texture2D(texSize, texSize);
			for (int i = 0; i < texSize; i++)
			{
				for (int j = 0; j < texSize; j++)
				{
					texture.SetPixel(i, j, new Color(0, 0, 0, 0));
				}
			}

			texture.Apply();

			material.SetTexture("_SplatTex", texture);
			splatFlag = true;
		}
	}

	public void OnDrawSplatter(Vector2 hitTextureCoordinate, int index , float[,] splatTexture)
	{
		// coord는 충돌된 hit.textureCoord2
		if (splatFlag == true)
		{
			lock(syncObj)
			{
				// Splat텍스처의 가로 / 세로 크기
				int splatTextureWidth = splatTexture.GetLength(0);
				int splatTextureHeight = splatTexture.GetLength(1);

				// Splatting을 위해 생성했던 texSize 크기를 가진 텍스처에
				// 오브젝트와 충돌된 텍스처 좌표를 적용하여
				// 이 텍스처의 비율에 맞게 같은 위치에서의 센터값을 구함
				int textureCenterWidth = (int)(hitTextureCoordinate.x * texSize) - (splatTextureWidth / 2);
				int textureCenterHeight = (int)(hitTextureCoordinate.y * texSize) - (splatTextureHeight / 2);

				// Splatting 텍스처의 범위에 미달 되지않게 값을 보간
				int interpolatedTextureCenterWidth = textureCenterWidth > 0 ? textureCenterWidth : 0;
				int interpolatedTextureCenterHeight = textureCenterHeight > 0 ? textureCenterHeight : 0;

				// Splatting 텍스처의 크기(texSize - 1)
				int textureWidth = texture.width - 1;
				int textureHeight = texture.height - 1;

				// Splat 텍스처가 Splatting 텍스처에 초과되지 않게 하기 위함
				int interpolatedSplatTextureWidthInSplattingTexture = ((interpolatedTextureCenterWidth + splatTextureWidth) < (textureWidth)) ?
					(interpolatedTextureCenterWidth + splatTextureWidth) : (textureWidth);
				interpolatedSplatTextureWidthInSplattingTexture -= interpolatedTextureCenterWidth;

				int interpolatedSplatTextureHeightInSplattingTexture = ((interpolatedTextureCenterHeight + splatTextureHeight) < (textureHeight)) ?
					(interpolatedTextureCenterHeight + splatTextureHeight) : (textureHeight);
				interpolatedSplatTextureHeightInSplattingTexture -= interpolatedTextureCenterHeight;

				// Splatting Texture에서 센터값과 
				// 보간된 SplatTexture가 뿌려질 픽세들을 가지고 옮
				Color[] pixels = texture.GetPixels(
					interpolatedTextureCenterWidth,
					interpolatedTextureCenterHeight,
					interpolatedSplatTextureWidthInSplattingTexture,
					interpolatedSplatTextureHeightInSplattingTexture);

				// Texture     =>        Texture2D 
				//          ( 변환 )
				Texture2D icecreamTexture2D = SplatterManager.Instance.icecreamTexture2DList[index];
				RenderTexture renderTexture = new RenderTexture(icecreamTexture2D.width, icecreamTexture2D.height, 32);
				Graphics.Blit(icecreamTexture2D, renderTexture);
				RenderTexture.active = renderTexture;

				icecreamTexture2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
				icecreamTexture2D.Apply();

				int count = 0;
				for (int x = 0; x < interpolatedSplatTextureWidthInSplattingTexture; x++)
				{
					for (int y = 0; y < interpolatedSplatTextureHeightInSplattingTexture; y++)
					{
						float a = splatTexture[x, y];
						if (a == 1)
						{
							pixels[count] = icecreamTexture2D.GetPixel(x, y);
						}
						else
						{
							Color color = pixels[count];
							// 결과 컬러는 알파에 기반한 텍스처에 스플래시 텍스처를 추가 한 것입니다.
							Color newColor = Color.Lerp(color, icecreamTexture2D.GetPixel(x, y), a);
							//하지만 결과 알파는 alphas의 합계입니다 (투명한 색을 추가하면 기본 색이 투명 해져서는 안됩니다)
							newColor.a = pixels[count].a + a;
							pixels[count] = newColor;
						}
						count++;
					}
				}

				// 텍스처 적용
				texture.SetPixels(
					interpolatedTextureCenterWidth,
					interpolatedTextureCenterHeight,
					interpolatedSplatTextureWidthInSplattingTexture,
					interpolatedSplatTextureHeightInSplattingTexture,
					pixels);
				texture.Apply();
			}
		}
	}
}
