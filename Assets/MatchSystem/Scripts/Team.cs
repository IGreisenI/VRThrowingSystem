using System.Collections.Generic;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;

public class Team : UdonSharpBehaviour
{
    [SerializeField] private string teamName;
    [SerializeField] private Color teamColor;
    [SerializeField] private TeamMember[] _players;
    [SerializeField] private GameObject[] spawns;
    [SerializeField] private VRCObjectPool teamMemberPool;

    [UdonSynced] private int teamScore;
    private TeamMember[] Players
    {
        get
        {
            int j = 0;

            for (int i = 0; i < _players.Length; i++)
            {
                if (_players[i].playerName != "" && _players[i].enabled)
                {
                    j++;
                }
            }

            TeamMember[] members = new TeamMember[j];
            j = 0;

            for (int i = 0; i < _players.Length; i++)
            {
                if (_players[i].playerName != "" && _players[i].enabled)
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

    public void Respawn()    
    {
        for (int i = 0; i < Players.Length; i++)
        {
            if (Players[i].playerAPI != null)
            {
                Players[i].OnResetRound();
                Players[i].Teleport();
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
        RequestSerialization();
    }

    public int GetScore()
    {
        return teamScore;
    }

    public void ResetScore()
    {
        teamScore = 0;
        RequestSerialization();
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
            if (!player.isOut) return false;
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
            member.TeleportOut();
            member.SetImmobilized(false);
            member.Empty();
            teamMemberPool.Return(member.gameObject);
        }
    }
}
