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
        AssingTeamMemeber(player);  
    }

    public TeamMember AssingTeamMemeber(VRCPlayerApi player)
    {
        GameObject obj = pool.TryToSpawn();
        if (obj != null)
        {
            obj.GetComponent<TeamMember>().playerAPI = player;
            return obj.GetComponent<TeamMember>();
        }
        else return null;
    }
}