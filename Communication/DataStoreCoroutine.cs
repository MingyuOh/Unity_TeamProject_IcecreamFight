using NCMB;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Communication
{
	public class DataStoreCoroutine : MonoBehaviour
	{
		// DB에서 오브젝트 리스트 찾는 함수
		public IEnumerator FindAsyncCoroutine(NCMBQuery<NCMBObject> query, UnityAction<NCMBException> errorCallback)
		{
			bool isConnecting = true;

			List<NCMBObject> objectList = new List<NCMBObject>();

			query.FindAsync((List<NCMBObject> _objectList, NCMBException e) =>
			{
				if(e != null)
				{
					errorCallback(e);
				}
				else
				{
					objectList = _objectList;
				}

				isConnecting = false;
			});

			while(isConnecting == true)
			{
				yield return null;
			}

			yield return objectList;
		}

		// DB에 오브젝트를 저장하는 함수
		public IEnumerator SaveAsyncCoroutine(NCMBObject _object, UnityAction<NCMBException> errorCallback)
		{
			bool isConnecting = true;

			_object.SaveAsync((NCMBException e) =>
			{
				if(e != null)
				{
					errorCallback(e);
				}

				isConnecting = false;
			});

			while(isConnecting == true)
			{
				yield return null;
			}

			yield return _object;
		}

		// DB에서 클래스 네임과 일치하는 데이터들의 수를 가져오는 함수
		public IEnumerator CountCoroutine(string className, UnityAction<NCMBException> errorCallback)
		{
			bool isConnecting = true;

			int count = 0;

			NCMBQuery<NCMBObject> query = new NCMBQuery<NCMBObject>(className);

			query.CountAsync((int _count, NCMBException e) =>
			{
				if(e != null)
				{
					// 통신에 실패한 경우는 타이틀로 다시 돌아간다.
					errorCallback(e);
				}
				else
				{
					count = _count;
				}

				isConnecting = false;
			});

			while(isConnecting == true)
			{
				yield return null;
			}

			yield return count;
		}

		// 비동기 처리에서 개체를 검색하는 함수
		public IEnumerator FetchAsyncCoroutine(NCMBObject _object, UnityAction<NCMBException> errorCallback)
		{
			bool isConnecting = true;

			_object.FetchAsync((NCMBException e) =>
			{
				if(e != null)
				{
					errorCallback(e);
				}

				isConnecting = false;
			});

			while(isConnecting == true)
			{
				yield return null;
			}
		}

		// DB에서 해당 객체를 삭제하는 함수
		public IEnumerator DeleteAsyncCoroutine(NCMBObject _object, UnityAction<NCMBException> errorCallback)
		{
			bool isConnecting = true;

			_object.DeleteAsync((NCMBException e) =>
			{
				if(e != null)
				{
					errorCallback(e);
				}

				isConnecting = false;
			});

			while(isConnecting == true)
			{
				yield return null;
			}
		}
	}
}