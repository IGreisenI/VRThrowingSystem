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
    private TeamMember[] players;
    private GameObject[] spawns;

    public void Respawn() {
        
        for(int i = 0; i < players.Length; i++)
        {
            players[i].playerAPI.TeleportTo(spawns[i].transform.position, spawns[i].transform.rotation);
        }
    }

    public void AddScore(int score)
    {
        teamScore += score;
    }

    public int GetScore()
    {
        return teamScore;
    }

    public void ResetScore()
    {
        teamScore = 0;
    }

    public bool IsOut()
    {
        foreach(TeamMember player in players)
        {
            if (!player.immobilized) return false;
        }

        return true;
    }

    /// <summary>
    /// Can't use contains so we do this
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    public bool PlayerInTeam(TeamMember player)
    {
        foreach (TeamMember member in players)
        {
            if (member == player) return true;
        }

        return false;
    }

    public void DisbandTeam()
    {
        foreach(TeamMember member in players)
        {

        }
    }
}
