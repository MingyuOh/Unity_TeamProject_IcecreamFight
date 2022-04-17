using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Generic 싱글톤
public abstract class Singleton<T> where T :class {
	protected static T instance = null;
	public static T Instance
	{
		get
		{
			if(instance == null)
			{
				instance = System.Activator.CreateInstance(typeof(T)) as T;

				if (instance == null)
				{
					Debug.LogError(typeof(T) + " is not found");
				}
			}
			return instance;
		}
	}
}
