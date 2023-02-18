using System.Collections.Generic;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace ThrowingSystem {
    public class TriggerForThrowingPlayer : UdonSharpBehaviour
    {
        [SerializeField] Transform throwingPlayers;
        [SerializeField] GameObject throwingPlayerPrefab;

        public override void OnPlayerTriggerEnter(VRCPlayerApi player)
        {
            Networking.SetOwner(player, this.gameObject);
            if (player == Networking.LocalPlayer)
            {
                ThrowingPlayer tP = Instantiate(throwingPlayerPrefab, transform.position, Quaternion.identity, throwingPlayers).GetComponent<ThrowingPlayer>();
                Networking.SetOwner(player, tP.gameObject);
                gameObject.SetActive(false);
            }
        }
    }
}