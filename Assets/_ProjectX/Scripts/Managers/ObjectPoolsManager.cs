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
		// NetworkIdentity = !ServerOnly
		
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
					RpcDeactivateObject(obj);
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
		public GameObject SpawnFromPool(ObjectPoolType poolType, Vector3 position, Quaternion rotation)
		{
			if (poolDictionary[poolType].Count > 0)
			{
				GameObject objFromPool = poolDictionary[poolType].Dequeue();
				objFromPool.transform.position = position;
				objFromPool.transform.rotation = rotation;
				objFromPool.SetActive(true);
				RpcActivatePositionObject(objFromPool, position, rotation.eulerAngles);
				return objFromPool;
			}
			
			Pool entry = pools.Find(pool => pool.poolType == poolType);
			if (entry != null)
			{
				print(poolType + "-Pool was empty - Instantiating!");
				GameObject instObj = Instantiate(entry.prefab, position, rotation, entry.parent);
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
		
		[ClientRpc]
		private void RpcActivatePositionObject(GameObject obj, Vector3 position, Vector3 eulerAngles)
		{
			obj.transform.position = position;
			obj.transform.eulerAngles = eulerAngles;
			obj.SetActive(true);
		}

		[Server]
		public void ReceiveGlobal(GlobalEvent eventState, GlobalSignalBaseData globalSignalData = null)
		{
			if (eventState == GlobalEvent.ALL_PLAYERS_CONNECTED_TO_GAME)
			{
				CreatePool();
			}
		}

		[ServerCallback]
		private void OnDestroy()
		{
			// print("ObjectPoolsManager OnDestroy");
			GlobalMediator.Instance.UnSubscribe(this);
			ServiceLocator.ProvideObjectPoolsManager(null);
		}
	}
}