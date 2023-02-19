using UdonSharp;
using UnityEngine;

public class Round : UdonSharpBehaviour
{
    private bool isInProgress = false;
    private string betweenText = "Preparing... ";

    private float roundTime;

    private Team firstTeam;
    private Team secondTeam;

    private void Start()
    {
        roundTime = 0;
    }

    private void Update()
    {
        roundTime += Time.deltaTime;
        if(3f - roundTime < 0f && !isInProgress)
        {
            StartRound();
        }
    }

    public void PrepareRound(Team firstTeam, Team secondTeam)
    {
        isInProgress = false;
        betweenText = "Preparing... ";
        roundTime = 0f;

        // Do starting text
        // We don't talk about this, why are there no abstraction tools in Udon?
        this.firstTeam = firstTeam;
        this.secondTeam = secondTeam;

        this.firstTeam.Respawn();
        this.secondTeam.Respawn();
    }

    public void StartRound()
    {
        isInProgress = true;
        roundTime = 0f;

        if(firstTeam)
            this.firstTeam.FreeMembers();
        
        if(secondTeam)
            this.secondTeam.FreeMembers();
    }

    public string GetTimeText()
    {
        return (int)(roundTime / 60f) + " [min] " + (roundTime % 60f).ToString("00.0") + "[sec]";
    }

    public string GetBetweenTimeText()
    {
        if ((3f - roundTime) < 0) return "";

        return $"{betweenText} {((int)(3f - roundTime))}";
    }

    public void StopRound()
    {
        isInProgress = false;
        roundTime = 0f;
        betweenText = "Restarting... ";
    }

    public bool IsInProgress()
    {
        return isInProgress;
    }
}