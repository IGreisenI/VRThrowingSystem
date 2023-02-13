using System.Collections.Generic;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class Match : UdonSharpBehaviour
{
    [SerializeField] private ScoreBoard scoreBoard;

    [SerializeField] private int scoreForPointCondition;
    [SerializeField] private int scoreForWin;

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
            if (firstTeam.PlayerInTeam(playerHit)) secondTeam.AddScore(scoreForPointCondition);
            else firstTeam.AddScore(scoreForPointCondition);
        }

        if(firstTeam.GetScore() >= scoreForWin || secondTeam.GetScore() >= scoreForWin)
        {
            EndMatch();
        }

        if (firstTeam.IsOut() || secondTeam.IsOut())
        {
            round.StartRound(firstTeam, secondTeam);
            roundNumber++;
        }

        scoreBoard.UpdateScoreboard();
    }

    public virtual bool PointCondition()
    {
        return true;
    }

    public void EndMatch()
    {
        roundNumber = 1;
        round.StopRound();
        firstTeam.DisbandTeam();
        secondTeam.DisbandTeam();
    }
}