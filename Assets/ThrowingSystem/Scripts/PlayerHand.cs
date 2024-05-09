using System;
using UdonSharp;
using Unity.Collections;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon.Common;

namespace ThrowingSystem
{
    public class PlayerHand : UdonSharpBehaviour
    {
        [Header("Desktop Settings")]
        [SerializeField] private float desktopPlayerSpeedMultiplyer;
        [SerializeField] private float desktopThrowMultiplyer;

        [Header("VR Settings")]
        [SerializeField] private float vrPlayerSpeedMultiplyer;
        [SerializeField] private float vrThrowMultiplyer;

        private bool occupied;
        private bool autoBlock;
        private Vector3 desktopOffset;
        private ThrowingObject disk;
        private HumanBodyBones hand;
        private KeyCode desktopInput;
        private string vrInput;
        private float handSpeedThreshold;
        private float buttonReleaseThreshold;

        private bool grabbing;
        private Vector3 _handSpeed;
        private Vector3 _previousHandOffset;

        #region CACHE
        private const float handSpeedCoefficient = 1000f;
        private Vector3 returnPoint;
        private Vector3 _relativeHandOffset = Vector3.zero;
        private Vector3 _handPos = Vector3.zero;
        #endregion

        public PlayerHand Init(bool occupied, bool autoBlock, Vector3 desktopOffset, ThrowingObject disk, HumanBodyBones hand, KeyCode desktopInput, string vrInput,
                                float handSpeedThreshold, float buttonReleaseThreshold)
        {
            this.occupied = occupied;
            this.autoBlock = autoBlock;
            this.desktopOffset = desktopOffset;
            this.disk = disk;
            this.hand = hand;
            this.desktopInput = desktopInput;
            this.vrInput = vrInput;
            this.handSpeedThreshold = handSpeedThreshold;
            this.buttonReleaseThreshold = buttonReleaseThreshold;

            return this;
        }

        private void Update()
        {
            CalculateHandSpeed();
        }

        private void CalculateHandSpeed()
        {
            _relativeHandOffset = Networking.LocalPlayer.GetBonePosition(hand) - Networking.LocalPlayer.GetBonePosition(HumanBodyBones.Chest);

            // This is calculated so players can't just rotate and throw
            //_handPos = new Vector3(Vector3.Dot(_relativeHandOffset, transform.right), Vector3.Dot(_relativeHandOffset, transform.up), Vector3.Dot(_relativeHandOffset, transform.forward));

            _handSpeed = (_relativeHandOffset - _previousHandOffset) * handSpeedCoefficient;

            _previousHandOffset = _relativeHandOffset;
        }

        public void HandlePlayerInput(VRCPlayerApi.TrackingData headTrackingData, PlayerHand otherHand, bool playerSpeedThresholdExceeded, bool isInVR)
        {
            if (occupied)
            {
                if (CheckThrowing(playerSpeedThresholdExceeded))
                {
                    Vector3 diskVelocity = isInVR ? CalculateVRVelocity() : CalculateDesktopVelocity(headTrackingData);
                    disk.Throw(diskVelocity);
                    occupied = false;
                }
                else
                {
                    bool isBlocking = CheckBlocking();
                    bool shouldRotate = isInVR && isBlocking;

                    disk.SetRotation(shouldRotate ? Networking.LocalPlayer.GetBoneRotation(hand) : Quaternion.identity);
                    disk.SetBlocking(isBlocking);
                }
            }
            else 
            {
                if (CheckSwapingDisks(otherHand))
                {
                    SwapDisk(otherHand);
                }
            }
        }

        public bool CheckThrowing(bool playerSpeedThresholdExceeded)
        {
            if (Input.GetKeyUp(desktopInput) && (playerSpeedThresholdExceeded || _handSpeed.magnitude > 0f) || IsVrThrowInput(playerSpeedThresholdExceeded))
            {
                grabbing = false;
                return true;
            }
            return false;
        }

        public bool CheckBlocking()
        {
            if (Input.GetKey(desktopInput) || Input.GetAxisRaw(vrInput) > 0.7)
            {
                grabbing = true;
                return true;
            }
            else
            {
                grabbing = false;
                return autoBlock;
            }
        }

        public bool CheckSwapingDisks(PlayerHand otherHand)
        {
            return (Input.GetKey(desktopInput) || Input.GetAxisRaw(vrInput) > 0.7) && otherHand.occupied;
        }

        private void SwapDisk(PlayerHand otherHand)
        {
            disk.SetHand(otherHand.hand);

            ThrowingObject temp = disk;
            disk = otherHand.disk;
            otherHand.disk = temp;

            occupied = true;
            otherHand.occupied = false;
        }

        public void ReturnDisk(VRCPlayerApi _player, VRCPlayerApi.TrackingData headTrackingData, float hapticDuration, float hapticAmplitude, float hapticFrequency)
        {
            if (!disk.IsReturning()) return;

            if (disk.Airborne && !occupied)
            {
                disk.SetHand(hand);
            }

            if (_player.IsUserInVR())
            {
                returnPoint = _player.GetBonePosition(hand);
            }
            else
            {
                returnPoint = headTrackingData.position + (headTrackingData.rotation * desktopOffset);
            }

            if (disk.ReturnObjectToHand(returnPoint, hand))
            {
                occupied = true;
                _player.PlayHapticEventInHand(GetPickupHand(), hapticDuration, hapticAmplitude, hapticFrequency);
            }
        }

        public Vector3 CalculateVRVelocity()
        {
            return (Networking.LocalPlayer.GetVelocity() * vrPlayerSpeedMultiplyer + _handSpeed * vrThrowMultiplyer) / 1000f;
        }

        public Vector3 CalculateDesktopVelocity(VRCPlayerApi.TrackingData headTrackingData)
        {
            Vector3 playerLookDirection = Networking.LocalPlayer.GetRotation() * Vector3.forward;
            float absVelocityLookDirScalar = Mathf.Abs(Vector3.Dot(Networking.LocalPlayer.GetVelocity().normalized, playerLookDirection));

            float velocity = 0;
            if (absVelocityLookDirScalar > 0.1f)
            {
                velocity = Mathf.Abs(Networking.LocalPlayer.GetVelocity().magnitude);
            }

            return (headTrackingData.rotation * Vector3.forward * (velocity * desktopPlayerSpeedMultiplyer) + headTrackingData.rotation * Vector3.forward * desktopThrowMultiplyer) / 1000f;
        }

        private VRCPickup.PickupHand GetPickupHand()
        {
            if (hand == HumanBodyBones.RightHand)
            {
                return VRC_Pickup.PickupHand.Right;
            }
            else if (hand == HumanBodyBones.LeftHand)
            {
                return VRC_Pickup.PickupHand.Left;
            }
            else
            {
                return VRC_Pickup.PickupHand.None;
            }
        }

        public bool IsHandMovingFastEnough()
        {
            return _handSpeed.magnitude > handSpeedThreshold;
        }

        private bool IsVrThrowInput(bool playerSpeedThresholdExceeded)
        {
            return (Input.GetAxisRaw(vrInput) > buttonReleaseThreshold && Input.GetAxisRaw(vrInput) <= 0.7 && (IsHandMovingFastEnough() || playerSpeedThresholdExceeded) && grabbing);
        }

        public void EmptyHand()
        {
            disk.ResetDisk();
            disk = null;
            occupied = false;
        }

        public bool HasDisk()
        {
            return disk.gameObject.activeSelf;
        }
    }
}