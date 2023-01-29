using System;
using System.Collections.Generic;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;

namespace ThrowingSystem
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class ThrowingObject : UdonSharpBehaviour
    {
        [Header("Prefabs and Scene Objects")]
        [SerializeField] private BoxCollider arenaCollider;
        [SerializeField] private GameObject guardObject;

        [Header("General Settings")]
        [SerializeField] [UdonSynced] private float returnTimeSeconds;
        [SerializeField] private float gravityMultiplier;
        [SerializeField] private float returnMultiplier;
        [SerializeField] private float distanceThreshold;
        [SerializeField] private float diskSpeedLimit;
        [SerializeField] [UdonSynced] private bool thrown;

        public float ReturnMultiplier { get { return returnMultiplier; } set { returnMultiplier = value; } }
        public bool Thrown { get { return thrown; } set { thrown = value; } }

        [Header("Desktop Settings")]
        [SerializeField] private float desktopThrowMultiplyer;

        [Header("VR Settings")]
        [SerializeField] private float playerSpeedMultiplyer;
        [SerializeField] private float vrThrowMultiplyer;

        [Header("Avaliability")]
        [SerializeField] [UdonSynced] private bool avaliableToBeAssinged;
        [SerializeField] [UdonSynced] private bool grabbed;

        public bool AvaliableToBeAssinged { get { return avaliableToBeAssinged; } set { avaliableToBeAssinged = value; } }
        public bool Grabbed { get { return grabbed; } set { grabbed = value; } }

        [Header("Visibility")]
        [SerializeField] private Material thrownMat;
        [SerializeField] private Material returnMat;

        [UdonSynced] private bool _blocking;
        private VRCPlayerApi _localPlayer;

        public HumanBodyBones hand { get; set; }
        public Vector3 _handSpeed { get; set; }
        public Vector3 desktopOffset { get; set; }

        [UdonSynced] private bool _isUserInVR;
        private Vector3 _previousHandPos;

        #region BulletDrop
        [UdonSynced] private float elapsedTime = 0;
        private int predictionStepsPerFrame = 6;
        [UdonSynced] private Vector3 diskVelocity = Vector3.zero;
        #endregion

        #region Syncing
        [UdonSynced] private Vector3 nextPoint = Vector3.zero;
        private Vector3 updatePoint = Vector3.zero;

        [UdonSynced] private Vector3 returnOrigin = Vector3.zero;
        [UdonSynced] private Vector3 vrDiskRotation = Vector3.zero;
        #endregion

        #region Cache
        [UdonSynced] private Vector3 _relPoint = Vector3.zero;
        [UdonSynced] private Vector3 handPos = Vector3.zero;
        #endregion

        public void Initialize(VRCPlayerApi localPlayer, HumanBodyBones hand, Vector3 desktopOffset)
        {
            _localPlayer = localPlayer;

            this.hand = hand;
            _isUserInVR = localPlayer.IsUserInVR();

            Thrown = true;
            _blocking = false;
            elapsedTime = returnTimeSeconds + 0.01f;

            this.desktopOffset = desktopOffset;
            RequestSerialization();
        }

        private void FixedUpdate()
        {
            if (_localPlayer != null && _localPlayer.IsOwner(this.gameObject))
            {
                if (Thrown && elapsedTime <= returnTimeSeconds)
                {
                    ThrowingPhysics();
                }

                // Check for input when in player hand and calculate current arm speed
                if (!Thrown && _isUserInVR)
                {
                    CalculateArmSpeed();
                }
            }
            else if (!IsReturning())
            {
                InterpolateDiskThrow(updatePoint);
            }
            else if (IsReturning())
            {
                InterpolateReturn(returnOrigin);
            }
        }
        
        public override void OnDeserialization()
        {
            if(this.transform.position != nextPoint)
            {
                updatePoint = nextPoint;
            }

            if (_isUserInVR && _blocking && !Thrown)
            {
                transform.rotation = Quaternion.Euler(vrDiskRotation);
            }
            else
            {
                transform.rotation = Quaternion.identity;
            }

            guardObject.SetActive(_blocking);


            if (!Networking.IsOwner(gameObject))
            {
                this.GetComponent<MeshRenderer>().material = !Thrown ? thrownMat : returnMat;
            }
        }

        private void RaycastForBounce(Vector3 position, Vector3 direction, float stepDistance)
        {
            RaycastHit[] hits;
            hits = Physics.RaycastAll(position, direction, stepDistance);

            foreach(RaycastHit hit in hits)
            {
                if (hit.collider != null && hit.collider.name != null && hit.collider.name.Contains("Wall"))
                {
                    diskVelocity = Vector3.Reflect(diskVelocity, hits[1].normal);
                    break;
                }
            }
        }

        public void ThrowingPhysics()
        {
            Vector3 currPoint = this.transform.position;
            float stepSize = 1f / predictionStepsPerFrame;
            for (float step = 0; step < 1; step += stepSize)
            {
                diskVelocity += Physics.gravity * gravityMultiplier * stepSize * Time.deltaTime;

                Vector3 directionNorm = ((currPoint + diskVelocity * stepSize * Time.deltaTime) - currPoint).normalized;
                RaycastForBounce(currPoint, directionNorm, stepSize);

                nextPoint = currPoint + diskVelocity * stepSize * Time.deltaTime;

                // In case that the disk is out of bounds it gets placed in bounced and into play
                if (!arenaCollider.bounds.Contains(nextPoint))
                {
                    diskVelocity = Vector3.Reflect(diskVelocity, (arenaCollider.ClosestPointOnBounds(nextPoint) - nextPoint).normalized);

                    nextPoint = arenaCollider.ClosestPointOnBounds(nextPoint) + diskVelocity * stepSize * Time.deltaTime;
                }

                currPoint = nextPoint;
            }
            elapsedTime += Time.deltaTime;
            
            RequestSerialization();

            this.transform.position = nextPoint;
        }

        public void InterpolateDiskThrow(Vector3 updatePoint)
        {
            Vector3 currPoint = this.transform.position;
            float stepSize = 1f / predictionStepsPerFrame;
            for (float step = 0; step < 1; step += stepSize)
            {
                diskVelocity += Physics.gravity * gravityMultiplier * stepSize * Time.deltaTime;

                Vector3 directionNorm = ((currPoint + diskVelocity * stepSize * Time.deltaTime) - currPoint).normalized;
                RaycastForBounce(currPoint, directionNorm, stepSize);

                updatePoint = currPoint + diskVelocity * stepSize * Time.deltaTime;

                // In case that the disk is out of bounds it gets placed in bounced and into play
                if (!arenaCollider.bounds.Contains(updatePoint))
                {
                    diskVelocity = Vector3.Reflect(diskVelocity, (arenaCollider.ClosestPointOnBounds(updatePoint) - updatePoint).normalized);

                    updatePoint = arenaCollider.ClosestPointOnBounds(updatePoint) + diskVelocity * stepSize * Time.deltaTime;
                }

                currPoint = updatePoint;
            }
            elapsedTime += Time.deltaTime;

            this.transform.position = updatePoint;
        }

        public void ReturnObjectToHand(Vector3 origin)
        {
            returnOrigin = origin;
            float _distance = Vector3.Distance(transform.position, origin);
            float _returnSpeed = _distance / ReturnMultiplier;
            GetComponent<Rigidbody>().velocity = Vector3.zero;

            if (_distance >= distanceThreshold)
            {
                transform.SetPositionAndRotation(origin, Quaternion.identity);
                nextPoint = origin;
            }
            else if(_distance > 0)
            {
                nextPoint = Vector3.Lerp(transform.position, origin, Time.deltaTime / _returnSpeed);
            }

            RequestSerialization();
            this.transform.position = nextPoint;
        }

        public void InterpolateReturn(Vector3 origin)
        {
            GetComponent<Rigidbody>().velocity = Vector3.zero;
            float _distance = Vector3.Distance(transform.position, origin);
            float _returnSpeed = _distance / ReturnMultiplier;
            transform.position = Vector3.Lerp(transform.position, origin, Time.deltaTime / _returnSpeed);
        }

        private void CalculateArmSpeed()
        {
            _relPoint = _localPlayer.GetBonePosition(hand) - _localPlayer.GetPosition();
            handPos = new Vector3(Vector3.Dot(_relPoint, transform.right), Vector3.Dot(_relPoint, transform.up), Vector3.Dot(_relPoint, transform.forward));

            _handSpeed = (_relPoint * 1000 - _previousHandPos * 1000);

            _previousHandPos = _relPoint;
        }

        public void Throw(VRCPlayerApi.TrackingData tData)
        {
            if (_isUserInVR)
            {
                diskVelocity = _localPlayer.GetVelocity() * playerSpeedMultiplyer + _handSpeed * vrThrowMultiplyer;
            }
            else
            {
                diskVelocity = tData.rotation * Vector3.forward * desktopThrowMultiplyer;
            }

            transform.rotation = new Quaternion();

            if (diskVelocity.magnitude > diskSpeedLimit) this.diskVelocity = diskVelocity.normalized * diskSpeedLimit;

            Thrown = true;
            elapsedTime = 0f;
            ReturnMultiplier = 20f;
            RequestSerialization();
        }

        public void Guard(bool isGuarding)
        {
            _blocking = isGuarding;
            transform.rotation = Quaternion.identity;

            if (isGuarding)
            {
                if (_isUserInVR)
                {
                    var handRotation = _localPlayer.GetBoneRotation(hand);
                    vrDiskRotation = handRotation.eulerAngles;
                    transform.rotation = handRotation;
                }
            }
            guardObject.SetActive(isGuarding);
        }

        public bool IsReturning()
        {
            return elapsedTime > returnTimeSeconds;
        }

        public VRCPlayerApi GetPlayer()
        {
            return _localPlayer;
        }

        public void ResetDisk()
        {
            thrown = true;
            AvaliableToBeAssinged = true;
            gameObject.SetActive(false);
        }

        public void SetThrown(bool thrown)
        {
            this.Thrown = thrown;
            RequestSerialization();
        }
    }
}