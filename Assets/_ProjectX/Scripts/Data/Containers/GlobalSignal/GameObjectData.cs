﻿using UnityEngine;

namespace Data.Containers.GlobalSignal
{
    public class GameObjectData : GlobalSignalBaseData
    {
        public GameObject gameObject;

        public GameObjectData()
        {

        }

        public GameObjectData(GameObject gameObject)
        {
            this.gameObject = gameObject;
        }
    }
}