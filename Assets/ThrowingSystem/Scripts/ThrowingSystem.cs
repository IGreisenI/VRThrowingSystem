
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
        [SerializeField] public Vector3 desktopLeftOffset;
        [SerializeField] public Vector3 desktopRightOffset;

        [Header("List of cached disks")]
        [SerializeField] private GameObject[] disks;

        public ThrowingObject SpawnDisk(VRCPlayerApi player)
        {
            Networking.SetOwner(player, pool.gameObject);
            GameObject obj = pool.TryToSpawn();
            if (obj != null)
            {
                Networking.SetOwner(player, obj);
                obj.GetComponent<ThrowingObject>().RequestSerialization();
                return obj.GetComponent<ThrowingObject>();
            }
            else return null;
        }

        public void DespawnDisks()
        {
            foreach(ThrowingObject disk in pool.GetComponentsInChildren<ThrowingObject>())
            {
                if (disk.gameObject.activeSelf)
                {
                    disk.ResetDisk();
                    pool.Return(disk.gameObject);
                }
            }
        }

        public ThrowingObject[] GetDisks()
        {
            return pool.GetComponentsInChildren<ThrowingObject>();
        }
    }
}