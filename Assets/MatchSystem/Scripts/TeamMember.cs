using ThrowingSystem;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

public class TeamMember : UdonSharpBehaviour
{
    public VRCPlayerApi playerAPI;
    public string playerName;
    public bool immobilized = false;
    public bool hit = false;
    public VRCPlayerApi gotHitByPlayer;

    private void Update()
    {
        if(playerAPI != null)
            this.transform.position = playerAPI.GetPosition();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<ThrowingObject>() && 
            other.GetComponent<ThrowingObject>().GetPlayerOwner() != playerAPI)
        {
            gotHitByPlayer = other.GetComponent<ThrowingObject>().GetPlayerOwner();
            hit = true;
        }
    }

    public void SetImmobilized(bool immobilized)
    {
        if(playerAPI != null)
            playerAPI.Immobilize(immobilized);
        this.immobilized = immobilized;
    }

    public bool IsImmobilized()
    {
        return immobilized;
    }
}
