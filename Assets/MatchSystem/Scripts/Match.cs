using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class Match : UdonSharpBehaviour
{
    private Round currentRound;
    private int roundNumber;

    private Team firstTeam;
    private Team secondTeam;
    
    private int firstTeamScore;
    private int secondTeamScore;

    private void IncreaseScore(Team team, int score)
    {

    }

    private void PlayerHit(VRCPlayerApi player)
    {

    }

    private void WinCondition()
    {

    }
}
