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

        [Header("Hands")]
        [SerializeField] private PlayerHand leftPlayerHand;
        [SerializeField] private PlayerHand rightPlayerHand;

        private bool PlayerSpeedThresholdExceeded { get { return _player.GetVelocity().magnitude > playerSpeedThreshold; } }

        private VRCPlayerApi _player;
        private bool _isUserInVR;

        #region CACHE
        private VRCPlayerApi.TrackingData headTrackingData;
        #endregion

        internal void ResetDisks()
        {
            if (this.leftPlayerHand == null || this.rightPlayerHand == null) return;

            leftPlayerHand.EmptyHand();
            rightPlayerHand.EmptyHand();
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

                ThrowingObject firstDisk = ts.spawnDisk(player).Initialize(_player, HumanBodyBones.LeftHand);
                ThrowingObject secondDisk = ts.spawnDisk(player).Initialize(_player, HumanBodyBones.RightHand);

                leftPlayerHand.Init(false, ts.desktopLeftOffset, firstDisk, HumanBodyBones.LeftHand, ts.desktopLeftInput, ts.vrLeftInput, handSpeedThreshold, buttonReleaseThreshold);
                rightPlayerHand.Init(false, ts.desktopRightOffset, secondDisk, HumanBodyBones.RightHand, ts.desktopRightInput, ts.vrRightInput, handSpeedThreshold, buttonReleaseThreshold);

                GetComponent<BoxCollider>().enabled = false;
            }
        }

        private void FixedUpdate()
        {
            if (_player == null) return;
            if (leftPlayerHand == null || rightPlayerHand == null) return;
            
            headTrackingData = _player.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);

            leftPlayerHand.HandlePlayerInput(headTrackingData, rightPlayerHand, PlayerSpeedThresholdExceeded, _isUserInVR, autoBlock);
            rightPlayerHand.HandlePlayerInput(headTrackingData, leftPlayerHand, PlayerSpeedThresholdExceeded, _isUserInVR, autoBlock);

            leftPlayerHand.ReturnDisk(_player, headTrackingData, hapticDuration, hapticAmplitude, hapticFrequency);
            rightPlayerHand.ReturnDisk(_player, headTrackingData, hapticDuration, hapticAmplitude, hapticFrequency);

        }

        private void OnDestroy()
        {
            ResetDisks();
        }
    }
}