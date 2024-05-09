using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

public class Round : UdonSharpBehaviour
{
    [SerializeField] private Team firstTeam;
    [SerializeField]private Team secondTeam;

    [UdonSynced] private bool isInProgress = false;
    [UdonSynced] private bool isRestarting = false;
    [UdonSynced] private string betweenText = "Preparing... ";

    [UdonSynced] private float roundTime = 0f;


    private void Update()
    {
        if (Networking.GetOwner(this.gameObject) != Networking.LocalPlayer) return;

        roundTime += Time.deltaTime;
        if(3f - roundTime < 0f && !isInProgress)
        {
            if (!isRestarting)
            {
                StartRound();
            }
            else
            {
                PrepareRound();
            }
        }
        if (roundTime % 0.2f > 0.18f)
        {
            RequestSerialization();
        }
    }

    public void PrepareRound()
    {
        isInProgress = false;
        isRestarting = false;
        betweenText = "Preparing... ";
        roundTime = 0f;

        this.firstTeam.Respawn();
        this.secondTeam.Respawn();

        RequestSerialization();
    }

    public void StartRound()
    {
        isInProgress = true;
        roundTime = 0f;

        if(firstTeam)
            this.firstTeam.FreeMembers();
        
        if(secondTeam)
            this.secondTeam.FreeMembers();

        RequestSerialization();
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
        isRestarting = true;

        RequestSerialization();
    }

    public bool IsInProgress()
    {
        return isInProgress;
    }
}