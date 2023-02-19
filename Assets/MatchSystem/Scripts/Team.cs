using System.Collections.Generic;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class Team : UdonSharpBehaviour
{
    [SerializeField] private string teamName;
    [SerializeField] private Color teamColor;
    [SerializeField] private TeamMember[] _players;
    private TeamMember[] Players
    {
        get
        {
            int j = 0;

            for (int i = 0; i < _players.Length; i++)
            {
                if (_players[i].playerAPI != null && _players[i].enabled)
                {
                    j++;
                }
            }

            TeamMember[] members = new TeamMember[j];
            j = 0;

            for (int i = 0; i < _players.Length; i++)
            {
                if (_players[i].playerAPI != null && _players[i].enabled)
                {
                    members[j] = _players[i];
                    j++;
                }
            }

            return members;
        }

        set
        {
            _players = value;
        }
    }
    [SerializeField] private GameObject[] spawns;

    private int teamScore;

    public void Respawn() 
    {
        Debug.Log(Players.Length);

        for (int i = 0; i < Players.Length; i++)
        {
            if (Players[i].playerAPI != null)
            {
                Players[i].playerAPI.TeleportTo(spawns[i].transform.position, spawns[i].transform.rotation);
                Players[i].SetImmobilized(true);
            }
        }
    }

    public void FreeMembers()
    {
        for (int i = 0; i < Players.Length; i++)
        {
            Players[i].SetImmobilized(false);
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
        foreach (TeamMember member in Players)
        {
            if (member.playerAPI == player) return true;
        }

        return false;
    }
    
    public TeamMember[] GetMembers()
    {
        return Players;
    }
 
    public bool IsOut()
    {
        foreach(TeamMember player in Players)
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
        foreach (TeamMember member in Players)
        {
            if (member == player) return true;
        }

        return false;
    }

    public void DisbandTeam()
    {
        foreach(TeamMember member in Players)
        {

        }
    }
}
