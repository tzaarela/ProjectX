using System;
using System.Collections.Generic;
using Data.Enums;
using Mirror;
using UnityEngine;

namespace Managers
{
	public class ObjectPoolsManager : NetworkBehaviour
	{
		[Serializable]
		public class Pool
		{
			public ObjectPoolType poolType;
			public GameObject prefab;
			public int startSize;
			public Transform parent;
		}

		public List<Pool> pools;
		private Dictionary<ObjectPoolType, Queue<GameObject>> poolDictionary;

		private void Awake()
		{
			print("ObjectPoolsManager provided to ServiceLocator");
			ServiceLocator.ProvideObjectPoolsManager(this);
		}

		private void Start()
		{
			poolDictionary = new Dictionary<ObjectPoolType, Queue<GameObject>>();

			foreach (Pool pool in pools)
			{
				Queue<GameObject> objectPool = new Queue<GameObject>();

				for (int i = 0; i < pool.startSize; i++)
				{
					GameObject obj = Instantiate(pool.prefab, pool.parent);
					obj.SetActive(false);
					objectPool.Enqueue(obj);
				}

				poolDictionary.Add(pool.poolType, objectPool);
			}
		}
		
		public GameObject SpawnFromPool(ObjectPoolType poolType)
		{
			if (poolDictionary[poolType].Count > 0) {
				GameObject objFromPool = poolDictionary[poolType].Dequeue();
				objFromPool.SetActive(true);
				// objFromPool.transform.parent = null;  - NECESSARY?!?
				return objFromPool;
			}
			
			Pool entry = pools.Find(pool => pool.poolType == poolType);
			if (entry != null)
			{
				print(poolType + "-Pool was empty - Instantiating!");
				GameObject instObj = Instantiate(entry.prefab);
				return instObj;
			}
			
			Debug.LogError(poolType + "-Pool was empty and couldn't find Pool to instantiate from!");
			return null;
		}

		public void ReturnToPool(ObjectPoolType poolType, GameObject obj)
		{
			obj.SetActive(false);
			poolDictionary[poolType].Enqueue(obj);
		}

		private void OnDestroy()
		{
			ServiceLocator.ProvideObjectPoolsManager(null);
		}
	}
}