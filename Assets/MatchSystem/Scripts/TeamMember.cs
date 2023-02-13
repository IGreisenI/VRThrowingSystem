
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class TeamMember : UdonSharpBehaviour
{
    public VRCPlayerApi playerAPI;
    public bool immobilized = false;

    public void SetImmobilized(bool immobilized)
    {
        this.immobilized = immobilized;
    }

    public bool IsImmobilized()
    {
        return immobilized;
    }
}
