using UdonSharp;
using UnityEngine;

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

    #region CACHE   
    private Team teamScored;
    private Team teamWon;
    #endregion

    [ContextMenu("StartMatch")]
    public void StartMatch()
    {
        roundNumber = 1;
        round.PrepareRound(firstTeam, secondTeam);
        scoreBoard.ShowMatchRound(roundNumber);

        // Show UI
        scoreBoard.ShowMatchType(matchType);
        scoreBoard.ShowTeams(firstTeam, secondTeam);

        scoreBoard.ShowFirstTeamScore(firstTeam.GetScore(), scoreForWin);
        scoreBoard.ShowSecondTeamScore(secondTeam.GetScore(), scoreForWin);
    }

    // Would you look at this update doing Rounds job because events are a joke and don't exist in Udon
    private void Update()
    {
        if (round.IsInProgress())
        {
            scoreBoard.ShowMatchTime(round.GetTimeText());
            scoreBoard.ShowBetweenTimers("");
        }
        else
        {
            scoreBoard.ShowBetweenTimers(round.GetBetweenTimeText());
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

            if (firstTeam == teamScored) scoreBoard.ShowFirstTeamScore(teamScored.GetScore(), scoreForWin);
            if (secondTeam == teamScored) scoreBoard.ShowSecondTeamScore(teamScored.GetScore(), scoreForWin);
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