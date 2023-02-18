using System.Collections.Generic;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class Match : UdonSharpBehaviour
{
    [SerializeField] private ScoreBoard scoreBoard;

    [SerializeField] private Round round;

    [SerializeField] private Team firstTeam;
    [SerializeField] private Team secondTeam;

    [SerializeField] private string matchType;

    [SerializeField] private int scoreForPointCondition;
    [SerializeField] private int scoreForWin;

    private int roundNumber;
    private bool isInProgress;

    #region CACHE   
    private Team teamScored;
    private Team teamWon;
    #endregion

    [ContextMenu("StartMatch")]
    public void StartMatch()
    {
        roundNumber = 1;
        round.PrepareRound(firstTeam, secondTeam);
        isInProgress = true;

        // Show UI
        scoreBoard.ShowMatchType(matchType);
        scoreBoard.ShowTeams(firstTeam, secondTeam);
    }

    private void Update()
    {
        if (round == null || !isInProgress) return;

        if (round.IsInProgress())
        {
            scoreBoard.ShowMatchTime(round.GetTimeText());
        }
        else
        {
            scoreBoard.ShowBeteenTimers(round.GetBetweenTimeText());
        }

        CheckIfPlayerHit();
    }

    public void OnPlayerHit(TeamMember playerHit)
    {
        if (playerHit.IsImmobilized()) return;

        playerHit.SetImmobilized(true);

        if (PointCondition())
        {
            teamScored = firstTeam.PlayerInTeam(playerHit) ? secondTeam : firstTeam;
            teamScored.AddScore(scoreForPointCondition);

            if (firstTeam == teamScored) scoreBoard.ShowFirstTeamScore(teamScored.GetScore());
            if (secondTeam == teamScored) scoreBoard.ShowSecondTeamScore(teamScored.GetScore());
        }

        if (firstTeam.IsOut() || secondTeam.IsOut())
        {
            round.PrepareRound(firstTeam, secondTeam);
            roundNumber++;
            scoreBoard.ShowMatchRound(roundNumber);
        }

        if(firstTeam.GetScore() >= scoreForWin || secondTeam.GetScore() >= scoreForWin)
        {
            teamWon = firstTeam.GetScore() >= scoreForWin ? firstTeam : secondTeam;
            scoreBoard.ShowWinner(teamWon.GetMembers(), teamWon.GetColor());
            
            EndMatch();
        }
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

    public void CheckIfPlayerHit()
    {
        foreach(TeamMember teamMember in firstTeam.GetMembers())
        {
            if (teamMember.hit && secondTeam.IsPlayerInTeam(teamMember.gotHitByPlayer))
            {
                OnPlayerHit(teamMember);
            }
        }

        foreach (TeamMember teamMember in secondTeam.GetMembers())
        {
            if (teamMember.hit && firstTeam.IsPlayerInTeam(teamMember.gotHitByPlayer))
            {
                OnPlayerHit(teamMember);
            }
        }
    }
}