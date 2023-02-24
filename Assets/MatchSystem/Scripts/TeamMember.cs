using ThrowingSystem;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

public class TeamMember : UdonSharpBehaviour
{
    [SerializeField] private GameObject spawnPoint;
    [SerializeField] private GameObject onHitModel;
    [SerializeField] private AudioSource onHitAudio;

    public VRCPlayerApi playerAPI;
    public VRCPlayerApi gotHitByPlayer;
    [UdonSynced] public string playerName;
    [UdonSynced] public bool immobilized = false;
    [UdonSynced] public bool isOut = false;
    [UdonSynced] public bool hit = false;
    [UdonSynced] public string gotHitByPlayerName;

    [UdonSynced] public Vector3 playerPos;

    private void Update()
    {
        if(playerAPI != null) { 
            playerPos = playerAPI.GetPosition();
            transform.position = playerPos;
        }
        
        RequestSerialization();
    }

    public override void OnDeserialization()
    {
        this.transform.position = playerPos;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<ThrowingObject>() && !isOut &&
            other.GetComponent<ThrowingObject>().Airborne &&
            other.GetComponent<ThrowingObject>().GetPlayerName() != playerName)
        {
            gotHitByPlayer = Networking.GetOwner(other.GetComponent<ThrowingObject>().gameObject);
            gotHitByPlayerName = other.GetComponent<ThrowingObject>().GetPlayerName();
            hit = true;
        }
    }

    public void SetModelColor(Color color)
    {
        onHitModel.GetComponent<MeshRenderer>().material.color = color;
    }

    public void OnHit()
    {
        isOut = true;
        onHitModel.SetActive(true);
        onHitAudio.Play();
    }

    public void SetImmobilized(bool immobilized)
    {
        this.immobilized = immobilized;
        RequestSerialization();
        if (playerAPI != null)
        {
            if (immobilized)
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "Imm");
            else
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "Free");
        }
    }

    public bool IsImmobilized()
    {
        return immobilized;
    }

    public void Imm()
    {
        if(Networking.LocalPlayer.displayName == playerName)
        {
            Networking.LocalPlayer.Immobilize(true);
        }
    }

    public void Free()
    {
        if (Networking.LocalPlayer.displayName == playerName)
        {
            Networking.LocalPlayer.Immobilize(false);
        }
    }

    public void TeleportOut()
    {
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "TpOut");
    }

    public void Teleport()
    {
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "Tp");
    }

    public void Tp()
    {
        if (Networking.LocalPlayer.displayName == playerName)
        {
            Networking.LocalPlayer.TeleportTo(spawnPoint.transform.position, spawnPoint.transform.rotation);
        }
    }

    public void TpOut()
    {
        if (Networking.LocalPlayer.displayName == playerName)
        {
            Networking.LocalPlayer.TeleportTo(new Vector3(10, 2, 0), Quaternion.identity);
        }
    }

    public void OnResetRound()
    {
        immobilized = false;
        isOut = false;
        hit = false;
        gotHitByPlayer = null;
        gotHitByPlayerName = null;
        onHitModel.SetActive(false);
        RequestSerialization();
    }

    public void Empty()
    {
        OnResetRound();
        playerAPI = null;
        playerName = null;
    }
}
