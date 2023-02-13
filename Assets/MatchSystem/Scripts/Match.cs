using System.Collections.Generic;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class Match : UdonSharpBehaviour
{
    [SerializeField] private int pointsForHit;
    [SerializeField] private int pointsForRoundWin;

    private Round round;
    private int roundNumber;

    private Team firstTeam;
    private Team secondTeam;

    public void StartMatch()
    {
        roundNumber = 1;
        round.StartRound(firstTeam, secondTeam);
    }

    public void PlayerHit(TeamMember playerHit)
    {
        if (playerHit.IsImmobilized()) return;

        playerHit.SetImmobilized(true);

        if (PointCondition())
        {
            if (firstTeam.PlayerInTeam(playerHit)) secondTeam.AddScore(pointsForHit);
            else firstTeam.AddScore(pointsForHit);
        }

        if (firstTeam.IsOut() || secondTeam.IsOut())
        {
            round.StartRound(firstTeam, secondTeam);
            roundNumber++;
        }
    }

    public virtual bool PointCondition()
    {
        return true;
    }

    public void EndMatch()
    {
        
    }
}