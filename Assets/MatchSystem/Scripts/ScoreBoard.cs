
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public struct MatchState
{
    int firstTeamScore;
    int secondTeamScore;
    float timers;
    string winnerName;
    string matchType;
    int matchRound;
}
public class ScoreBoard : UdonSharpBehaviour
{
    [SerializeField] private TextMeshProUGUI[] firstTeamScore;
    [SerializeField] private TextMeshProUGUI[] secondTeamScore;
    [SerializeField] private TextMeshProUGUI[] timers;
    [SerializeField] private TextMeshProUGUI[] winnerName;
    [SerializeField] private TextMeshProUGUI matchType;
    [SerializeField] private TextMeshProUGUI matchRound;

    public void UpdateScoreboard(MatchState matchState)
    {

    }
}