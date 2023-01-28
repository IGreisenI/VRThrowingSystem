
using System;
using System.Linq;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;

namespace ThrowingSystem
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Continuous)]
    public class ThrowingSystem : UdonSharpBehaviour
    {
        [Header("Input Settings")]
        [SerializeField] public KeyCode desktopLeftInput = KeyCode.Mouse0;
        [SerializeField] public KeyCode desktopRightInput = KeyCode.Mouse1;
        [SerializeField] public string vrLeftInput = "Oculus_CrossPlatform_PrimaryHandTrigger";
        [SerializeField] public string vrRightInput = "Oculus_CrossPlatform_SecondaryHandTrigger";

        [Header("Disk Settings")]
        [SerializeField] private VRCObjectPool pool;
        [SerializeField] private GameObject objectPrefab;
        [SerializeField] public Vector3 desktopLeftOffset;
        [SerializeField] public Vector3 desktopRightOffset;

        [Header("ThrowingPlayer")]
        [SerializeField] private GameObject throwingPlayerPrefab;

        [Header("List of cached disks")]
        [SerializeField] private GameObject[] disks;
        private int position = 0;

        [Header("List of cached throwing player")]
        [SerializeField] private GameObject[] throwingPlayersGameObjects;

        public void InitThrowingPlayer(VRCPlayerApi player)
        {
            GameObject disk1 = FindAvaliableDisk();
            GameObject disk2 = FindAvaliableDisk();
        }

        public GameObject FindAvaliableDisk()
        {
            for (int i = 0 + position; i < disks.Length; i++)
            {
                if (disks[i].GetComponent<ThrowingObject>() != null && disks[i].GetComponent<ThrowingObject>().avaliableToBeAssinged)
                {
                    disks[i].GetComponent<ThrowingObject>().avaliableToBeAssinged = false;
                    position++;
                    return disks[i];
                }
            }
            return null;
        }

        public void MakeDiskAvaliable(VRCPlayerApi player)
        {
            for (int i = 0; i < disks.Length; i++)
            {
                if (disks[i].GetComponent<ThrowingObject>().GetPlayer() == player)
                {
                    disks[i].GetComponent<ThrowingObject>().ResetDisk();
                }
            }   
        }

        public ThrowingPlayer FindAvaliableThrowingPlayerObject()
        {
            for (int i = 0; i < throwingPlayersGameObjects.Length; i++)
            {
                if (throwingPlayersGameObjects[i].GetComponent<ThrowingPlayer>().GetPlayer() == null)
                {
                    throwingPlayersGameObjects[i].SetActive(true);
                    return throwingPlayersGameObjects[i].GetComponent<ThrowingPlayer>();
                }
            }
            return null;
        }

        public GameObject spawnDisk(VRCPlayerApi player)
        {
            Networking.SetOwner(player, pool.gameObject);
            GameObject obj = pool.TryToSpawn();
            if (obj != null)
            {
                Networking.SetOwner(player, obj);
                obj.GetComponent<ThrowingObject>().RequestSerialization();
                return obj;
            }
            else return null;
        }
    }
}