using System.Collections.Generic;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class Team : UdonSharpBehaviour
{
    [SerializeField] private string teamName;
    [SerializeField] private Color teamColor;
    [SerializeField] private TeamMember[] players;
    /*{
        get
        {
            int j = 0;

            for (int i = 0; i < players.Length; i++)
            {
                if (players[i].playerAPI != null)
                {
                    j++;
                }
            }

            TeamMember[] members = new TeamMember[j];
            j = 0;

            for (int i = 0; i < players.Length; i++)
            {
                if (players[i].playerAPI != null)
                {
                    members[j] = players[i];
                    j++;
                }
            }

            return members;
        }

        set
        {
            players = value;
        }
    }*/
    [SerializeField] private GameObject[] spawns;

    private int teamScore;

    public void Respawn() 
    {
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i].playerAPI != null)
            {
                players[i].playerAPI.TeleportTo(spawns[i].transform.position, spawns[i].transform.rotation);
                players[i].SetImmobilized(true);
            }
        }
    }

    public void FreeMembers()
    {
        for (int i = 0; i < players.Length; i++)
        {
            players[i].SetImmobilized(false);
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

    public Color GetColor()
    {
        return teamColor;
    }

    public bool IsPlayerInTeam(VRCPlayerApi player)
    {
        foreach (TeamMember member in players)
        {
            if (member.playerAPI == player) return true;
        }

        return false;
    }
    
    public TeamMember[] GetMembers()
    {
        return players;
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
