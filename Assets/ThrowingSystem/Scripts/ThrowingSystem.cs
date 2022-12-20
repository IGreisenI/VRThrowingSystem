
using System;
using System.Linq;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace ThrowingSystem
{
    public class ThrowingSystem : UdonSharpBehaviour
    {
        [Header("Input Settings")]
        [SerializeField] public KeyCode desktopLeftInput = KeyCode.Mouse0;
        [SerializeField] public KeyCode desktopRightInput = KeyCode.Mouse1;
        [SerializeField] public string vrLeftInput = "Oculus_CrossPlatform_PrimaryHandTrigger";
        [SerializeField] public string vrRightInput = "Oculus_CrossPlatform_SecondaryHandTrigger";

        [Header("Disk Settings")]
        [SerializeField] private GameObject objectPrefab;
        [SerializeField] public Vector3 desktopLeftOffset;
        [SerializeField] public Vector3 desktopRightOffset;

        [Header("ThrowingPlayer")]
        [SerializeField] private GameObject throwingPlayerPrefab;

        [Header("List of cached disks")]
        [SerializeField] private GameObject[] disks;

        [Header("List of cached throwing player")]
        [SerializeField] private GameObject[] throwingPlayersGameObjects;

        public void InitThrowingPlayer(VRCPlayerApi player)
        {
            GameObject disk1 = FindAvaliableDisk();
            GameObject disk2 = FindAvaliableDisk();

            /*ThrowingPlayer throwingPlayer = FindAvaliableThrowingPlayerObject();
            if(throwingPlayer != null)
                throwingPlayer.Initialize(player, objectPrefab, desktopRightInput, desktopLeftInput, vrRightInput, vrLeftInput, desktopLeftOffset, desktopRightOffset, disk1, disk2);*/
        }

        private void RemoveThrowingPlayer(VRCPlayerApi player)
        {
            for (int i = 0; i < throwingPlayersGameObjects.Length; i++)
            {
                if (throwingPlayersGameObjects[i].GetComponent<ThrowingPlayer>().GetPlayer() == player)
                {
                    throwingPlayersGameObjects[i].GetComponent<ThrowingPlayer>().ResetDisks();
                    throwingPlayersGameObjects[i].GetComponent<ThrowingPlayer>().SetPlayer(null);
                    throwingPlayersGameObjects[i].SetActive(false);
                    return;
                }
            }
            return;
        }

        public GameObject FindAvaliableDisk()
        {
            for (int i = 0; i < disks.Length; i++)
            {
                if (disks[i].GetComponent<ThrowingObject>().avaliableToBeAssinged)
                {
                    disks[i].GetComponent<ThrowingObject>().avaliableToBeAssinged = false;
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
    }
}