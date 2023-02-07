using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class Round : UdonSharpBehaviour
{
    private float roundTime;

    private void Start()
    {
        roundTime = 0;
    }

    private void Update()
    {
        roundTime += Time.deltaTime;
    }

    public void OnPlayerHit()
    {
        
    }

    public void CheckConidition()
    {

    }
}
