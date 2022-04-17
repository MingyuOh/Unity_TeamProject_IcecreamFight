using System.Collections;
using UnityEngine;

// Mono 싱글톤
public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
{
	private static bool shuttingDown = false;
	private static object syncObj = new object();
	private static T instance = null;
	public static bool recycleObj = true;
	public static T Instance
	{
		get
		{
			if(shuttingDown == true)
			{
				return null;
			}

			lock(syncObj)
			{
				if(instance == null)
				{
					instance = FindObjectOfType(typeof(T)) as T;

					if(instance == null)
					{
						instance = new GameObject(typeof(T).ToString(), typeof(T)).GetComponent<T>();
						DontDestroyOnLoad(instance);
					}
				}
				return instance;
			}
		}
	}

	protected virtual void Awake()
	{
		if (instance == null)
		{
			instance = this as T;

			if(recycleObj == true)
			{
				DontDestroyOnLoad(instance);
			}
		}
		else if (instance != this)
		{
			DestroyObject();
		}
	}

	private void DestroyObject()
	{
		if (Application.isPlaying)
			Destroy(this);
		else
			DestroyImmediate(this);
	}

	public virtual void Init() { }

	private void OnDestroy()
	{
		shuttingDown = true;
	}

	private void OnApplicationQuit()
	{
		shuttingDown = true;
	}
}
