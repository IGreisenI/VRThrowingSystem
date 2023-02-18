using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class Round : UdonSharpBehaviour
{
    private bool isInProgress;
    private string betweenText;

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
    }

    public void PrepareRound(Team firstTeam, Team secondTeam)
    {
        betweenText = "Continuing... ";
        roundTime = 0f;

        // Do starting text
        // We don't talk about this, why are there no abstraction tools in Udon?
        this.firstTeam = firstTeam;
        this.secondTeam = secondTeam;
        
        firstTeam.Respawn();
        secondTeam.Respawn();
    }

    public void StartRound()
    {
        roundTime = 0f;

        firstTeam.FreeMembers();
        secondTeam.FreeMembers();
    }

    public string GetTimeText()
    {
        return (int)(roundTime / 60f) + " [min] " + (roundTime % 60f) + "[sec]";
    }

    public string GetBetweenTimeText()
    {
        return betweenText + ((int)(3f - roundTime)).ToString();
    }

    public void StopRound()
    {
        roundTime = 0f;
        betweenText = "Restarting... ";
    }

    public bool IsInProgress()
    {
        return isInProgress;
    }
}