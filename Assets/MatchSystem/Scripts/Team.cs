
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class Team : UdonSharpBehaviour
{
    private string teamName;
    private Color teamColor;
    private int teamScore;
    private VRCPlayerApi[] players;
}
