using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;

public class Match : UdonSharpBehaviour
{
    [SerializeField] private ScoreBoard scoreBoard;

    [SerializeField] private Round round;

    [SerializeField] private Team firstTeam;
    [SerializeField] private Team secondTeam;

    [SerializeField] private ThrowingSystem.ThrowingSystem throwingSystem;

    [SerializeField] private string matchType;

    [SerializeField] private int scoreForPointCondition;
    [SerializeField] private int scoreForWin;

    [UdonSynced] private int roundNumber;
    [UdonSynced] private bool isInProgress = false;

    #region CACHE   
    private Team teamScored;
    private Team teamWon;
    #endregion

    public void StartMatchNetworked()
    {
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "StartMatch");
    }

    public void StartMatch()
    {
        roundNumber = 1;

        firstTeam.ResetScore();
        secondTeam.ResetScore();

        firstTeam.SetDiskColors(throwingSystem.GetDisks());
        secondTeam.SetDiskColors(throwingSystem.GetDisks());

        isInProgress = true;
        if (Networking.LocalPlayer.isMaster)
        {
            round.PrepareRound();
            RequestSerialization();
        }

        scoreBoard.ShowMatchRound(roundNumber);

        // Show UI
        scoreBoard.ShowMatchType(matchType);
        scoreBoard.ShowTeams(firstTeam, secondTeam);

        scoreBoard.ShowFirstTeamScore(firstTeam.GetScore(), scoreForWin);
        scoreBoard.ShowSecondTeamScore(secondTeam.GetScore(), scoreForWin);
    }

    private void Update()
    {
        if (!isInProgress) {

            if (Networking.GetOwner(this.gameObject) != Networking.LocalPlayer) {
                scoreBoard.ShowFirstTeamScore(firstTeam.GetScore(), scoreForWin);
                scoreBoard.ShowSecondTeamScore(secondTeam.GetScore(), scoreForWin);
                scoreBoard.ShowMatchRound(roundNumber);

                if (firstTeam.GetScore() >= scoreForWin || secondTeam.GetScore() >= scoreForWin)
                {
                    teamWon = firstTeam.GetScore() >= scoreForWin ? firstTeam : secondTeam;
                    scoreBoard.ShowWinner(teamWon.GetMembers(), teamWon.GetColor());
                }
            }
            return;

        }

        if (round.IsInProgress())
        {
            scoreBoard.ShowMatchTime(round.GetTimeText());
            scoreBoard.ShowBetweenTimers("");
        }
        else
        {
            scoreBoard.ShowBetweenTimers(round.GetBetweenTimeText());
        }

        if (Networking.GetOwner(this.gameObject) == Networking.LocalPlayer)
        {
            CheckIfPlayerHit();
        }
        else
        {
            scoreBoard.ShowFirstTeamScore(firstTeam.GetScore(), scoreForWin);
            scoreBoard.ShowSecondTeamScore(secondTeam.GetScore(), scoreForWin);
            scoreBoard.ShowMatchRound(roundNumber);

            if (firstTeam.GetScore() >= scoreForWin || secondTeam.GetScore() >= scoreForWin)
            {
                teamWon = firstTeam.GetScore() >= scoreForWin ? firstTeam : secondTeam;
                scoreBoard.ShowWinner(teamWon.GetMembers(), teamWon.GetColor());
            }
        }
    }

    public void OnPlayerHit(TeamMember playerHit)
    {
        if (playerHit.IsImmobilized() && !Networking.LocalPlayer.isInstanceOwner) return;

        playerHit.SetImmobilized(true);
        playerHit.OnHit();
        playerHit.RequestSerialization();

        if (PointCondition())
        {
            teamScored = firstTeam.PlayerInTeam(playerHit) ? secondTeam : firstTeam;
            teamScored.AddScore(scoreForPointCondition);

            if (firstTeam == teamScored) scoreBoard.ShowFirstTeamScore(teamScored.GetScore(), scoreForWin);
            if (secondTeam == teamScored) scoreBoard.ShowSecondTeamScore(teamScored.GetScore(), scoreForWin);
        }

        if(firstTeam.GetScore() >= scoreForWin || secondTeam.GetScore() >= scoreForWin)
        {
            teamWon = firstTeam.GetScore() >= scoreForWin ? firstTeam : secondTeam;
            scoreBoard.ShowWinner(teamWon.GetMembers(), teamWon.GetColor());

            EndMatch();
            return;
        }

        if (firstTeam.IsOut() || secondTeam.IsOut())
        {
            round.StopRound();
            roundNumber++;
            RequestSerialization();
            scoreBoard.ShowMatchRound(roundNumber);
        }
    }

    public virtual bool PointCondition()
    {
        return firstTeam.IsOut() || secondTeam.IsOut();
    }

    public void EndMatch()
    {
        isInProgress = false;
        throwingSystem.SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "DespawnDisks");
        round.StopRound();
        firstTeam.DisbandTeam();
        secondTeam.DisbandTeam();
        RequestSerialization();
    }

    public void CheckIfPlayerHit()
    {
        foreach(TeamMember firstTeamMember in firstTeam.GetMembers())
        {
            if (firstTeamMember.hit && secondTeam.IsPlayerInTeam(firstTeamMember.gotHitByPlayer))
            {
                if (!firstTeamMember.isOut)
                {
                    firstTeamMember.SetModelColor(secondTeam.GetColor());
                    OnPlayerHit(firstTeamMember);
                    firstTeamMember.hit = false;
                    return;
                }
            }
            firstTeamMember.hit = false;
        }

        foreach (TeamMember secondTeamMember in secondTeam.GetMembers())
        {
            if (secondTeamMember.hit && firstTeam.IsPlayerInTeam(secondTeamMember.gotHitByPlayer))
            {
                if (!secondTeamMember.isOut)
                {
                    secondTeamMember.SetModelColor(firstTeam.GetColor());
                    OnPlayerHit(secondTeamMember);
                    secondTeamMember.hit = false;
                    return;
                }
            }
            secondTeamMember.hit = false;
        }
    }
}