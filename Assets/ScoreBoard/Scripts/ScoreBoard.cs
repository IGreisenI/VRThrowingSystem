
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ScoreBoard : UdonSharpBehaviour
{
    [SerializeField] private TextMeshProUGUI[] firstTeamScore;
    [SerializeField] private TextMeshProUGUI[] secondTeamScore;
    [SerializeField] private TextMeshProUGUI[] timers;
    [SerializeField] private TextMeshProUGUI[] winnerName;
    [SerializeField] private TextMeshProUGUI matchType;
    [SerializeField] private TextMeshProUGUI matchRound;

    private void UpdateScore(int firstTeam, int secondTeam)
    {
        
    }

    private void SetMatchType(string matchType)
    {
        this.matchType.text = matchType;
    }

    private void SetMatchRound(int round)
    {
        this.matchRound.text = round.ToString();
    }

}
