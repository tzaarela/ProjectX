using System;
using System.Collections.Generic;
using Data.Containers.GlobalSignal;
using Data.Enums;
using Data.Interfaces;
using Mirror;
using UnityEngine;

namespace Managers
{
	public class ObjectPoolsManager : NetworkBehaviour, IReceiveGlobalSignal
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

		// NetworkIdentity = !ServerOnly
		[Server]
		public override void OnStartServer()
		{
			if (!isServer)
				return;
			
			GlobalMediator.Instance.Subscribe(this);
			
			print("ObjectPoolsManager provided to ServiceLocator");
			ServiceLocator.ProvideObjectPoolsManager(this);
		}
		
		[Server]
		private void CreatePool()
		{
			print("PoolCreation!");
			poolDictionary = new Dictionary<ObjectPoolType, Queue<GameObject>>();

			foreach (Pool pool in pools)
			{
				Queue<GameObject> objectPool = new Queue<GameObject>();

				for (int i = 0; i < pool.startSize; i++)
				{
					GameObject obj = Instantiate(pool.prefab, pool.parent);
					obj.SetActive(false);
					NetworkServer.Spawn(obj);
					objectPool.Enqueue(obj);
				}

				poolDictionary.Add(pool.poolType, objectPool);
			}
		}

		[Server]
		public GameObject SpawnFromPool(ObjectPoolType poolType)
		{
			if (poolDictionary[poolType].Count > 0)
			{
				GameObject objFromPool = poolDictionary[poolType].Dequeue();
				//objFromPool.transform.parent = null;
				objFromPool.SetActive(true);
				RpcActivateObject(objFromPool);
				return objFromPool;
			}
			
			Pool entry = pools.Find(pool => pool.poolType == poolType);
			if (entry != null)
			{
				print(poolType + "-Pool was empty - Instantiating!");
				GameObject instObj = Instantiate(entry.prefab, entry.parent);
				NetworkServer.Spawn(instObj);
				poolDictionary[poolType].Enqueue(instObj);
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
		private void RpcDeactivateObject(GameObject obj)
		{
			obj.SetActive(false);
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

		public void ReceiveGlobal(GlobalEvent eventState, GlobalSignalBaseData globalSignalData = null)
		{
			print("Yellow!");
			if (eventState == GlobalEvent.ALL_PLAYERS_CONECTED_TO_GAME)
			{
				print("Yellow2!");
				CreatePool();
			}
		}
	}
}