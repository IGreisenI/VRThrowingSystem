using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;

namespace ThrowingSystem
{
    public class ThrowingPlayer : UdonSharpBehaviour
    {
        [SerializeField] private Transform spawnPoint;

        [Header("Disk Settings")]
        [SerializeField] private bool autoBlock;

        [Header("Haptic Feedback Settings")]
        [SerializeField] private float hapticDuration;
        [SerializeField] private float hapticAmplitude;
        [SerializeField] private float hapticFrequency;

        [Header("VR Input Settings")]
        [SerializeField] private float handSpeedThreshold;
        [SerializeField] private float playerSpeedThreshold;
        [Range(0.01f, 0.5f)]
        [SerializeField] private float buttonReleaseThreshold;

        private PlayerHand leftPlayerHand;
        private PlayerHand rightPlayerHand;

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

        #region INPUT
        private KeyCode desktopLeftThrow;
        private KeyCode desktopRightThrow;
        private string vrThrowLeftInput;
        private string vrThrowRightInput;
        #endregion

        #region CACHE
        private ThrowingObject tempDisk;
        VRCPlayerApi.TrackingData tData;
        private Vector3 returnOrigin;
        #endregion

        public void Initialize(KeyCode desktopRightInput, KeyCode desktopLeftInput, string vrRightInput, string vrLeftInput,
            Vector3 desktopLeftOffset, Vector3 desktopRightOffset, ThrowingObject firstDisk, ThrowingObject secondDisk)
        {
            this.desktopLeftThrow = desktopLeftInput;
            this.desktopRightThrow = desktopRightInput;
            this.vrThrowLeftInput = vrLeftInput;
            this.vrThrowRightInput = vrRightInput;

            this.desktopLeftOffset = desktopLeftOffset;
            this.desktopRightOffset = desktopRightOffset;

            this.firstDisk = firstDisk.GetComponent<ThrowingObject>();
            this.secondDisk = secondDisk.GetComponent<ThrowingObject>();

            leftPlayerHand = new PlayerHand(false, firstDisk, HumanBodyBones.LeftHand, VRC_Pickup.PickupHand.Left, desktopLeftInput, vrLeftInput);
            rightPlayerHand = new PlayerHand(false, secondDisk, HumanBodyBones.RightHand, VRC_Pickup.PickupHand.Right, desktopRightInput, vrRightInput);
        }

        internal void ResetDisks()
        {
            if (this.firstDisk == null || this.secondDisk == null) return;

            this.firstDisk.gameObject.SetActive(false);
            this.secondDisk.gameObject.SetActive(false);
            this.firstDisk.AvaliableToBeAssinged = true;
            this.secondDisk.AvaliableToBeAssinged = true;
            this.firstDisk.SetThrown(true);
            this.secondDisk.SetThrown(true);
            firstDisk = null;
            secondDisk = null;
        }

        public override void OnPlayerTriggerStay(VRCPlayerApi player)
        {
            if (!player.isLocal) Destroy(this.gameObject);

            GameObject throwingSystem = GameObject.Find("ThrowingSystem");
            ThrowingSystem ts = throwingSystem.GetComponent<ThrowingSystem>();

            if (ts != null)
            {
                _player = player;
                _isUserInVR = _player.IsUserInVR();

                Initialize(ts.desktopRightInput, ts.desktopLeftInput, ts.vrRightInput, ts.vrLeftInput, ts.desktopLeftOffset, ts.desktopRightOffset, ts.spawnDisk(player), ts.spawnDisk(player));

                firstDisk.Initialize(_player, HumanBodyBones.LeftHand, desktopLeftOffset);
                leftDisk = firstDisk;
                
                secondDisk.Initialize(_player, HumanBodyBones.RightHand, desktopRightOffset);
                rightDisk = secondDisk;

                leftHand = false;
                rightHand = false;

                GetComponent<BoxCollider>().enabled = false;
            }
        }

        private void FixedUpdate()
        {
            if (_player == null) return;
            if (firstDisk == null || secondDisk == null) return;
            
            tData = _player.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);

            // Check for input when in player hand
            if (_isUserInVR)
            {
                if (leftHand)
                {
                    CheckVRInput(vrThrowLeftInput, leftDisk);
                    if (leftDisk.Thrown) leftHand = false;
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
                    if (rightDisk.Thrown) rightHand = false;
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
                    if (leftDisk.Thrown) leftHand = false;
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
                    if (rightDisk.Thrown) rightHand = false;
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

        private void CheckVRInput(string _vrThrowInput, ThrowingObject disk)
        {

            if (Input.GetAxisRaw(_vrThrowInput) > buttonReleaseThreshold && Input.GetAxisRaw(_vrThrowInput) <= 0.7)
            {
                if (disk._handSpeed.magnitude > handSpeedThreshold && disk.Grabbed)
                {
                    disk.Guard(false);
                    disk.Throw(new VRCPlayerApi.TrackingData());
                }
                disk.Grabbed = false;
            }
            else if (Input.GetAxisRaw(_vrThrowInput) > 0.7)
            {
                disk.Guard(true);
                disk.Grabbed = true;
            }
            else if (Input.GetAxisRaw(_vrThrowInput) <= buttonReleaseThreshold)
            {
                if ((disk._handSpeed.magnitude > handSpeedThreshold || _player.GetVelocity().magnitude > playerSpeedThreshold) && disk.Grabbed)
                {
                    disk.Guard(false);
                    disk.Throw(new VRCPlayerApi.TrackingData());
                }
                else if (autoBlock)
                {
                    disk.Guard(true);
                }
                else
                {
                    disk.Guard(false);
                }
                disk.Grabbed = false;
            }

            disk.RequestSerialization();
        }

        private void CheckDesktopInput(KeyCode desktopThrow, ThrowingObject disk, VRCPlayerApi.TrackingData tData)
        {
            if (Input.GetKeyUp(desktopThrow))
            {
                disk.Guard(false);
                disk.Throw(tData);
            }
            else if (Input.GetKey(desktopThrow))
            {
                disk.Guard(true);
            }
            else if (autoBlock)
            {
                disk.Guard(true);
            }
            else
            {
                disk.Guard(false);
            }
        }

        private void ReturnDisk(ThrowingObject disk, VRCPlayerApi.TrackingData tData)
        {
            if (!disk.IsReturning() || _player == null) return;

            if (!leftHand && disk.Thrown)
            {
                disk.hand = HumanBodyBones.LeftHand;
                disk.desktopOffset = desktopLeftOffset;
            }
            else if (!rightHand && disk.Thrown)
            {
                disk.hand = HumanBodyBones.RightHand;
                disk.desktopOffset = desktopRightOffset;
            }

            if (_isUserInVR)
            {
                returnOrigin = _player.GetBonePosition(disk.hand);
            }
            else
            {
                returnOrigin = _player.GetBonePosition(HumanBodyBones.Head) + (tData.rotation * disk.desktopOffset);
            }

            disk.ReturnObjectToHand(returnOrigin);

            if (Vector3.Distance(disk.gameObject.transform.position, returnOrigin) < 0.1f && disk.Thrown)
            {
                disk.SetThrown(false);
                disk.ReturnMultiplier = 200f;

                if (disk.hand == HumanBodyBones.LeftHand)
                {
                    leftHand = true;
                    leftDisk = disk;
                    _player.PlayHapticEventInHand(VRC_Pickup.PickupHand.Left, hapticDuration, hapticAmplitude, hapticFrequency);
                }
                if (disk.hand == HumanBodyBones.RightHand)
                {
                    rightHand = true;
                    rightDisk = disk;
                    _player.PlayHapticEventInHand(VRC_Pickup.PickupHand.Right, hapticDuration, hapticAmplitude, hapticFrequency);
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
