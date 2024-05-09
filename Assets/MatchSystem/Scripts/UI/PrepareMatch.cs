using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class PrepareMatch : UdonSharpBehaviour
{
    [SerializeField] private GameObject[] objectsToEnable;
    [SerializeField] private Transform teleportLocation;

    public void EnableObjects()
    {
        foreach(GameObject obj in objectsToEnable)
        {
            obj.SetActive(true);
        }

        Networking.LocalPlayer.TeleportTo(teleportLocation.position, Quaternion.identity);
    }
}
