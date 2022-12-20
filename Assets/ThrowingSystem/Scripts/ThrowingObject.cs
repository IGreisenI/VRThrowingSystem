using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace ThrowingSystem
{
    public class ThrowingObject : UdonSharpBehaviour
    {
        [Header("Prefabs and Scene Objects")]
        [SerializeField] private BoxCollider arenaCollider;
        [SerializeField] private GameObject guardObject;

        [Header("General Settings")]
        [SerializeField] private float returnTimeSeconds;
        [SerializeField] private float gravityMultiplier;
        [SerializeField] private float returnMultiplier;
        [SerializeField] private float distanceThreshold;
        [SerializeField] private float diskSpeedLimit;
        [SerializeField] public bool autoBlock;
        [SerializeField] public bool thrown;

        [Header("Desktop Settings")]
        [SerializeField] private float desktopThrowMultiplyer;

        [Header("VR Settings")]
        [SerializeField] private float playerSpeedMultiplyer;
        [SerializeField] private float vrThrowMultiplyer;

        [Header("Avaliability")]
        [SerializeField] public bool avaliableToBeAssinged;
        [SerializeField] public bool grabbed;

        [Header("Hide")]
        public HumanBodyBones hand;
        public Vector3 _handSpeed;
        public bool _blocking;
        public Vector3 desktopOffset;
        public VRCPlayerApi _localPlayer;

        private Rigidbody rb;
        private bool _isUserInVR;

        private Vector3 _previousHandPos;

        #region BulletDrop
        public float elapsedTime = 0;
        private int predictionStepsPerFrame = 36;
        private Vector3 diskVelocity = Vector3.zero;
        #endregion

        #region Cache
        Vector3 _relPoint;
        Vector3 handPos;
        #endregion

        public void Initialize(VRCPlayerApi localPlayer, HumanBodyBones hand, Vector3 desktopOffset)
        {
            _localPlayer = localPlayer;
            this.hand = hand;
            _isUserInVR = localPlayer.IsUserInVR();
            rb = GetComponent<Rigidbody>();

            thrown = true;
            _blocking = false;
            elapsedTime = returnTimeSeconds + 0.01f;

            this.desktopOffset = desktopOffset;
        }

        private void FixedUpdate()
        {
            if (thrown && elapsedTime <= returnTimeSeconds)
            {
                ThrowingPhysics();
            }

            // Check for input when in player hand and calculate current arm speed
            if (!thrown)
            {
                if (_isUserInVR) 
                {
                    CalculateArmSpeed();
                }
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

        private void ThrowingPhysics()
        {
            Vector3 point1 = this.transform.position;
            float stepSize = 1.0f / predictionStepsPerFrame;
            for (float step = 0; step < 1; step += stepSize)
            {
                diskVelocity += Physics.gravity * gravityMultiplier * stepSize * Time.deltaTime;

                Vector3 directionNorm = ((point1 + diskVelocity * stepSize * Time.deltaTime) - point1).normalized;
                RaycastForBounce(point1, directionNorm, stepSize);

                Vector3 point2 = point1 + diskVelocity * stepSize * Time.deltaTime;

                // In case that the disk is out of bounds it gets placed in bounced and into play
                if (!arenaCollider.bounds.Contains(point2))
                {
                    diskVelocity = Vector3.Reflect(diskVelocity, (arenaCollider.ClosestPointOnBounds(point2) - point2).normalized);

                    point2 = arenaCollider.ClosestPointOnBounds(point2) + diskVelocity * stepSize * Time.deltaTime;
                }

                this.transform.position = point2;

                point1 = point2;
            }
            elapsedTime += Time.deltaTime;
        }

        public void ReturnObjectToHand(Vector3 origin)
        {
            float _distance = Vector3.Distance(transform.position, origin);
            float _returnSpeed = _distance / returnMultiplier;
            GetComponent<Rigidbody>().velocity = Vector3.zero;

            if (_distance >= distanceThreshold)
            {
                GetComponent<SphereCollider>().isTrigger = true;
                transform.SetPositionAndRotation(origin, Quaternion.identity);
            }
            else if(_distance > 0)
            {
                transform.position = Vector3.Lerp(transform.position, origin, Time.deltaTime / _returnSpeed);
            }

        }

        private void CalculateArmSpeed()
        {
            _relPoint = _localPlayer.GetBonePosition(hand) - _localPlayer.GetPosition();
            handPos = new Vector3(Vector3.Dot(_relPoint, transform.right), Vector3.Dot(_relPoint, transform.up), Vector3.Dot(_relPoint, transform.forward));
        
            _handSpeed = (_relPoint * 1000 - _previousHandPos * 1000);

            _previousHandPos = _relPoint;
        }

        public void ThrowDesktop(VRCPlayerApi.TrackingData tData)
        {
            transform.rotation = new Quaternion();

            diskVelocity = tData.rotation * Vector3.forward * desktopThrowMultiplyer;
            if (diskVelocity.magnitude > diskSpeedLimit) diskVelocity = diskVelocity.normalized * diskSpeedLimit;

            thrown = true;
            elapsedTime = 0f;
        }

        public void ThrowVR()
        {
            transform.rotation = new Quaternion();

            diskVelocity = _localPlayer.GetVelocity() * playerSpeedMultiplyer + _handSpeed * vrThrowMultiplyer;
            if (diskVelocity.magnitude > diskSpeedLimit) diskVelocity = diskVelocity.normalized * diskSpeedLimit;

            thrown = true;
            elapsedTime = 0f;
        }

        public void GuardVR(bool isGuarding)
        {
            if (isGuarding)
            {
                var handRotation = _localPlayer.GetBoneRotation(hand);
                transform.rotation = handRotation;

                guardObject.SetActive(true);
            }
            else
            {
                transform.rotation = Quaternion.identity;
                guardObject.SetActive(false);
            }
        }

        public void GuardDesktop(bool guard)
        {
            this.guardObject.SetActive(guard);
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
            avaliableToBeAssinged = false;

        }
    }
}