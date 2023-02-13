using System.Collections.Generic;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class Team : UdonSharpBehaviour
{
    private string teamName;
    private Color teamColor;
    private int teamScore;
    private List<TeamMember> players;
    private List<GameObject> spawns;

    public void Respawn() {
        
        for(int i = 0; i < players.Count; i++)
        {
            players[i].playerAPI.TeleportTo(spawns[i].transform.position, spawns[i].transform.rotation);
        }
    }

    public void AddScore(int score)
    {
        teamScore += score;
    }

    public void ResetScore()
    {
        teamScore = 0;
    }

    public bool IsOut()
    {
        bool result = true;
        foreach(TeamMember player in players)
        {
            if (!player.immobilized) result = false;
        }

        return result;
    }

    public bool PlayerInTeam(TeamMember player)
    {
        return players.Contains(player);
    }

    public void DisbandTeam()
    {
        foreach(TeamMember member in players)
        {

        }
    }
}
