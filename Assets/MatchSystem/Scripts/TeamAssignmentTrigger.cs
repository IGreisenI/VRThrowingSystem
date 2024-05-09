using ThrowingSystem;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;

public class TeamAssignmentTrigger : UdonSharpBehaviour
{
    [SerializeField] private VRCObjectPool pool;

    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        if (!Networking.LocalPlayer.isMaster) return;

        foreach (TeamMember member in pool.GetComponentsInChildren<TeamMember>())
        {
            if (member.playerName == player.displayName)
                return;
        }

        AssingTeamMemeber(player);
        gameObject.SetActive(false);
    }

    public TeamMember AssingTeamMemeber(VRCPlayerApi player)
    {
        GameObject obj;
        do
        {
            obj = pool.TryToSpawn();
        } while (obj == null);

        obj.GetComponent<TeamMember>().playerAPI = player;
        obj.GetComponent<TeamMember>().playerName = player.displayName;
        obj.GetComponent<TeamMember>().RequestSerialization();
        return null;
    }
}