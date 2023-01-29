using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UdonSharp;
using VRC.SDK3.Components;
using VRC.SDKBase;

namespace ThrowingSystem
{
    public class PlayerHand
    {
        private bool occupancy;
        private ThrowingObject disk;
        private HumanBodyBones bone;
        private VRCPickup.PickupHand pickupHand;
        private KeyCode desktopInput;
        private string vrInput;

        public PlayerHand(bool occupancy, ThrowingObject disk, HumanBodyBones bone, VRCPickup.PickupHand pickupHand, KeyCode desktopInput, string vrInput)
        {
            this.occupancy = occupancy;
            this.disk = disk;
            this.bone = bone;
            this.pickupHand = pickupHand;
            this.desktopInput = desktopInput;
            this.vrInput = vrInput;
        }

        public void EmptyHand()
        {
            disk.ResetDisk();
            disk = null;
            occupancy = false;
        }
    }
}
