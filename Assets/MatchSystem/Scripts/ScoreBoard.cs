
using UdonSharp;
using UnityEngine;
using TMPro;

public class ScoreBoard : UdonSharpBehaviour
{
    [SerializeField] private TextMeshProUGUI[] firstTeamScore;
    [SerializeField] private TextMeshProUGUI[] firstTeamMembers;
    [SerializeField] private TextMeshProUGUI[] secondTeamScore;
    [SerializeField] private TextMeshProUGUI[] secondTeamMembers;
    [SerializeField] private TextMeshProUGUI[] matchTimers;
    [SerializeField] private TextMeshProUGUI[] betweenTimers;
    [SerializeField] private TextMeshProUGUI[] winnerName;
    [SerializeField] private TextMeshProUGUI matchType;
    [SerializeField] private TextMeshProUGUI matchRound;

    public void ShowFirstTeamScore(int score)
    {
        for(int i = 0; i < firstTeamScore.Length; i++)
        {
            firstTeamScore[i].text = score.ToString();
        }
    }

    public void ShowSecondTeamScore(int score)
    {
        for (int i = 0; i < secondTeamScore.Length; i++)
        {
            secondTeamScore[i].text = score.ToString();
        }
    }

    public void ShowMatchTime(string timeText)
    {
        for (int i = 0; i < matchTimers.Length; i++)
        {
            matchTimers[i].text = timeText.ToString();
        }
    }

    public void ShowBeteenTimers(string timeText)
    {
        for (int i = 0; i < betweenTimers.Length; i++)
        {
            betweenTimers[i].text = timeText.ToString();
        }
    }

    public void ShowWinner(TeamMember[] winners, Color color)
    {
        for (int i = 0; i < winnerName.Length; i++)
        {
            winnerName[i].text = "Winner";
            winnerName[i].color = color;
        }
    }

    public void ShowMatchType(string matchType)
    {
        this.matchType.text = matchType;
    }

    public void ShowMatchRound(int round)
    {
        this.matchRound.text = round.ToString();
    }

    public void ShowTeams(Team firstTeam, Team secondTeam)
    {       
        string words = "";

        foreach(TeamMember member in firstTeam.GetMembers())
        {
            words += member.playerAPI.displayName + "\n";
        }

        for (int i = 0; i < firstTeamMembers.Length; i++)
        {
            firstTeamMembers[i].text = words;
            firstTeamMembers[i].color = firstTeam.GetColor();
        }

        words = "";
        foreach (TeamMember member in secondTeam.GetMembers())
        {
            words += member.playerAPI.displayName + "\n";
        }

        for (int i = 0; i < secondTeamMembers.Length; i++)
        {
            secondTeamMembers[i].text = words;
            secondTeamMembers[i].color = secondTeam.GetColor();
        }

        SetTextColor(firstTeamScore, firstTeam.GetColor());
        SetTextColor(secondTeamScore, secondTeam.GetColor());
    }

    public void SetTextColor(TextMeshProUGUI[] textFields, Color color)
    {
        for (int i = 0; i < textFields.Length; i++)
        {
            textFields[i].color = color;
        }
    }
}