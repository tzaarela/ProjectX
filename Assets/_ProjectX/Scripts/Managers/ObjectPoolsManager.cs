﻿using System;
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
		
		public override void OnStartServer()
		{
			base.OnStartServer();

			poolDictionary = new Dictionary<ObjectPoolType, Queue<GameObject>>();

			foreach (Pool pool in pools)
			{
				Queue<GameObject> objectPool = new Queue<GameObject>();

				for (int i = 0; i < pool.startSize; i++)
				{
					GameObject obj = Instantiate(pool.prefab);
					NetworkServer.Spawn(obj);
					obj.SetActive(false);
					RpcDeactivateObject(obj);
					objectPool.Enqueue(obj);
				}

				poolDictionary.Add(pool.poolType, objectPool);
			}
		}

		[ClientRpc]
		private void RpcDeactivateObject(GameObject obj)
		{
			obj.SetActive(false);
		}
		
		public GameObject SpawnFromPool(ObjectPoolType poolType)
		{
			if (poolDictionary[poolType].Count > 0)
			{
				GameObject objFromPool = poolDictionary[poolType].Dequeue();
				objFromPool.SetActive(objFromPool);
				//objFromPool.transform.parent = null;
				objFromPool.SetActive(true);
				RpcActivateObject(objFromPool);
				return objFromPool;
			}
			
			Pool entry = pools.Find(pool => pool.poolType == poolType);
			if (entry != null)
			{
				print(poolType + "-Pool was empty - Instantiating!");
				GameObject instObj = Instantiate(entry.prefab);
				NetworkServer.Spawn(instObj);
				return instObj;
			}
			
			Debug.LogError(poolType + "-Pool was empty and couldn't find Pool to instantiate from!");
			return null;
		}

		[Server]
		public void ReturnToPool(ObjectPoolType poolType, GameObject obj)
		{
			obj.SetActive(false);
			RpcDeactivateObject(obj);
			poolDictionary[poolType].Enqueue(obj);
		}

		[ClientRpc]
		private void RpcActivateObject(GameObject obj)
		{
			obj.SetActive(true);
		}

		private void OnDestroy()
		{
			ServiceLocator.ProvideObjectPoolsManager(null);
		}
	}
}