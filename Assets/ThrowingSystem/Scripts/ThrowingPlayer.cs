using System;
using ThrowingSystem;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Core.Config.Interfaces;
using VRC.Udon.Common;
namespace ThrowingSystem
{
    public class ThrowingPlayer : UdonSharpBehaviour
    {
        [SerializeField] private bool autoBlock;
        [Header("Disk Settings")]
        [SerializeField] private Transform spawnPoint;

        [Header("VR Input Settings")]
        [SerializeField] private float handSpeedThreshold;
        [SerializeField] private float playerSpeedThreshold;
        [Range(0.01f, 0.5f)]
        [SerializeField] private float buttonReleaseThreshold;

        private ThrowingObject firstDisk;
        private ThrowingObject secondDisk;
        private ThrowingObject leftDisk;
        private ThrowingObject rightDisk;
        public bool leftHand;
        public bool rightHand;
        private Vector3 desktopLeftOffset;
        private Vector3 desktopRightOffset;

        private VRCPlayerApi _player;
        private bool _isUserInVR;

        // Input
        private KeyCode desktopLeftThrow;
        private KeyCode desktopRightThrow;
        private string vrThrowLeftInput;
        private string vrThrowRightInput;

        private ThrowingObject tempDisk;
        VRCPlayerApi.TrackingData tData;

        public void Initialize(KeyCode desktopRightInput, KeyCode desktopLeftInput, string vrRightInput, string vrLeftInput,
            Vector3 desktopLeftOffset, Vector3 desktopRightOffset, GameObject firstDisk, GameObject secondDisk)
        {
            this.desktopLeftThrow = desktopLeftInput;
            this.desktopRightThrow = desktopRightInput;
            this.vrThrowLeftInput = vrLeftInput;
            this.vrThrowRightInput = vrRightInput;

            this.desktopLeftOffset = desktopLeftOffset;
            this.desktopRightOffset = desktopRightOffset;

            Networking.SetOwner(_player, firstDisk);
            Networking.SetOwner(_player, secondDisk);

            this.firstDisk = firstDisk.GetComponent<ThrowingObject>();
            this.secondDisk = secondDisk.GetComponent<ThrowingObject>();
        }

        internal void ResetDisks()
        {
            this.firstDisk.gameObject.SetActive(false);
            this.secondDisk.gameObject.SetActive(false);
            this.firstDisk.avaliableToBeAssinged = true;
            this.secondDisk.avaliableToBeAssinged = true;
            this.firstDisk.thrown = true;
            this.secondDisk.thrown = true;
            firstDisk = null;
            secondDisk = null;
        }

        public override void OnPlayerTriggerStay(VRCPlayerApi player)
        {
            base.OnPlayerTriggerStay(player);

            ThrowingSystem ts = GameObject.Find("ThrowingSystem").GetComponent<ThrowingSystem>();
            if (ts != null)
            {
                _player = player;
                _isUserInVR = _player.IsUserInVR();

                Initialize(ts.desktopRightInput, ts.desktopLeftInput, ts.vrRightInput, ts.vrLeftInput, ts.desktopLeftOffset, ts.desktopRightOffset, ts.FindAvaliableDisk(), ts.FindAvaliableDisk());

                firstDisk.gameObject.SetActive(true);
                firstDisk.Initialize(_player, HumanBodyBones.LeftHand, desktopLeftOffset);
                leftDisk = firstDisk;

                secondDisk.gameObject.SetActive(true);
                secondDisk.Initialize(_player, HumanBodyBones.RightHand, desktopRightOffset);
                rightDisk = secondDisk;

                leftHand = false;
                rightHand = false;


                GetComponent<BoxCollider>().enabled = false;
            }
        }

        private void FixedUpdate()
        {
            if (_player != null)
            {
                tData = _player.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);

                if (firstDisk != null && secondDisk != null)
                {
                    // Check for input when in player hand
                    if (_isUserInVR)
                    {
                        if (leftHand)
                        {
                            CheckVRInput(vrThrowLeftInput, leftDisk);
                            if (leftDisk.thrown) leftHand = false;
                        }
                        else
                        {
                            if (Input.GetAxisRaw(vrThrowLeftInput) > 0.7 && rightHand)
                            {
                                rightDisk.hand = HumanBodyBones.LeftHand;
                                rightDisk.desktopOffset = desktopLeftOffset;
                                leftHand = true;
                                rightHand = false;
                                tempDisk = leftDisk;
                                leftDisk = rightDisk;
                                rightDisk = leftDisk;
                            }
                        }
                        if (rightHand)
                        {
                            CheckVRInput(vrThrowRightInput, rightDisk);
                            if (rightDisk.thrown) rightHand = false;
                        }
                        else
                        {
                            if (Input.GetAxisRaw(vrThrowRightInput) > 0.7 && leftHand)
                            {
                                leftDisk.hand = HumanBodyBones.RightHand;
                                leftDisk.desktopOffset = desktopRightOffset;
                                rightHand = true;
                                leftHand = false;
                                tempDisk = rightDisk;
                                rightDisk = leftDisk;
                                leftDisk = rightDisk;
                            }
                        }
                    }
                    else
                    {
                        if (leftHand)
                        {
                            CheckDesktopInput(desktopLeftThrow, leftDisk, tData);
                            if (leftDisk.thrown) leftHand = false;
                        }
                        else
                        {
                            if (Input.GetKey(desktopLeftThrow) && rightHand)
                            {
                                rightDisk.hand = HumanBodyBones.LeftHand;
                                rightDisk.desktopOffset = desktopLeftOffset;
                                leftHand = true;
                                rightHand = false;
                                tempDisk = leftDisk;
                                leftDisk = rightDisk;
                                rightDisk = leftDisk;
                            }
                        }
                        if (rightHand)
                        {
                            CheckDesktopInput(desktopRightThrow, rightDisk, tData);
                            if (rightDisk.thrown) rightHand = false;
                        }
                        else
                        {
                            if (Input.GetKey(desktopRightThrow) && leftHand)
                            {
                                leftDisk.hand = HumanBodyBones.RightHand;
                                leftDisk.desktopOffset = desktopRightOffset;
                                rightHand = true;
                                leftHand = false;
                                tempDisk = rightDisk;
                                rightDisk = leftDisk;
                                leftDisk = rightDisk;
                            }
                        }
                    }

                    // Return to hands
                    tData = _player.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);
                    ReturnDisk(firstDisk, tData);
                    ReturnDisk(secondDisk, tData);
                }
            }
        }

        private void CheckVRInput(string _vrThrowInput, ThrowingObject disk)
        {

            if (Input.GetAxisRaw(_vrThrowInput) > buttonReleaseThreshold && Input.GetAxisRaw(_vrThrowInput) <= 0.7)
            {
                if (disk._handSpeed.magnitude > handSpeedThreshold && disk.grabbed)
                {
                    disk.GuardVR(false);
                    disk._blocking = false;
                    disk.ThrowVR();
                }
                disk.grabbed = false;
            }
            else if (Input.GetAxisRaw(_vrThrowInput) > 0.7)
            {
                disk.GuardVR(true);
                disk._blocking = true;
                disk.grabbed = true;
            }
            else if (Input.GetAxisRaw(_vrThrowInput) <= buttonReleaseThreshold)
            {
                if ((disk._handSpeed.magnitude > handSpeedThreshold || disk._localPlayer.GetVelocity().magnitude > playerSpeedThreshold) && disk.grabbed)
                {
                    disk.GuardVR(false);
                    disk._blocking = false;
                    disk.ThrowVR();
                }
                else if (autoBlock)
                {
                    disk.GuardVR(true);
                    disk._blocking = true;
                }
                else
                {
                    disk.GuardVR(false);
                    disk._blocking = false;
                }
                disk.grabbed = false;
            }
        }

        private void CheckDesktopInput(KeyCode desktopThrow, ThrowingObject disk, VRCPlayerApi.TrackingData tData)
        {

            if (Input.GetKeyUp(desktopThrow))
            {
                disk.GuardDesktop(false);
                disk.ThrowDesktop(tData);
            }
            else if (Input.GetKey(desktopThrow))
            {
                disk.GuardDesktop(true);
            }
            else if (autoBlock)
            {
                disk.GuardDesktop(true);
            }
            else
            {
                disk.GuardDesktop(false);
            }
        }

        private void ReturnDisk(ThrowingObject disk, VRCPlayerApi.TrackingData tData)
        {
            if (disk.IsReturning())
            {
                if (!leftHand && disk.thrown)
                {
                    disk.hand = HumanBodyBones.LeftHand;
                    disk.desktopOffset = desktopLeftOffset;
                }
                else if (!rightHand && disk.thrown)
                {
                    disk.hand = HumanBodyBones.RightHand;
                    disk.desktopOffset = desktopRightOffset;
                }

                Vector3 origin = Vector3.zero;
                if (_isUserInVR)
                {
                    origin = _player.GetBonePosition(disk.hand);
                }
                else
                {
                    origin = _player.GetBonePosition(HumanBodyBones.Head) + (tData.rotation * disk.desktopOffset);
                }

                disk.ReturnObjectToHand(origin);

                if (Vector3.Distance(disk.gameObject.transform.position, origin) == 0f && disk.thrown)
                {
                    disk.thrown = false;
                    disk.GetComponent<SphereCollider>().isTrigger = true;

                    if (disk.hand == HumanBodyBones.LeftHand)
                    {
                        leftHand = true;
                        leftDisk = disk;
                    }
                    if (disk.hand == HumanBodyBones.RightHand)
                    {
                        rightHand = true;
                        rightDisk = disk;
                    }
                }
            }
        }

        public VRCPlayerApi GetPlayer()
        {
            return _player;
        }

        public void SetPlayer(VRCPlayerApi player)
        {
            _player = player;
        }

        private void OnDestroy()
        {
            ResetDisks();
        }
    }
}
