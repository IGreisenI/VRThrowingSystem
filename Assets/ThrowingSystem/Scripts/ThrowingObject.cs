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
        [SerializeField] private float returnMultiplierAirborne;
        [SerializeField] private float returnMultiplierInHand;
        [SerializeField] private float distanceThreshold;
        [SerializeField] private float diskSpeedLimit;

        [SerializeField] [UdonSynced] private bool airborne = true;
        public bool Airborne { get { return airborne; } set { airborne = value; } }

        [Header("Visibility")]
        [SerializeField] private Material thrownMat;
        [SerializeField] private Material returnMat;

        [UdonSynced] private bool _blocking = false;
        private VRCPlayerApi _localPlayer;
        private HumanBodyBones hand;

        #region BulletDrop
        [UdonSynced] private float airBounceTime = 0;
        private int predictionStepsPerFrame = 6;
        private float stepSize;
        [UdonSynced] private float returnMultiplier;
        [UdonSynced] private Vector3 diskVelocity = Vector3.zero; 
        #endregion

        #region Syncing
        [UdonSynced] private Vector3 nextPoint = Vector3.zero; 
        private Vector3 nextPointClient = Vector3.zero;
        [UdonSynced] private Vector3 returnOrigin = Vector3.zero;
        [UdonSynced] private Vector3 diskRotation = Vector3.zero;
        #endregion

        public ThrowingObject Initialize(VRCPlayerApi localPlayer, HumanBodyBones hand)
        {
            _localPlayer = localPlayer;

            this.hand = hand;
            stepSize = 1f / predictionStepsPerFrame;
            airBounceTime = returnTimeSeconds + 0.01f;
            returnMultiplier = returnMultiplierAirborne;

            RequestSerialization();
            return this;
        }

        private void FixedUpdate()
        {
            if (_localPlayer != null && _localPlayer.IsOwner(this.gameObject))
            {
                if (Airborne && !IsReturning())
                {
                    ThrowingPhysics(out nextPoint);
                }
            }
            // Ran by remote players
            else if (!IsReturning())
            {
                ThrowingPhysics(out nextPointClient);
            }
            else
            {
                InterpolateReturn(returnOrigin);
            }
        }
        
        public override void OnDeserialization()
        {
            nextPointClient = nextPoint;

            transform.rotation = Quaternion.Euler(diskRotation);

            guardObject.SetActive(_blocking);

            this.GetComponent<MeshRenderer>().material = !Airborne ? thrownMat : returnMat;
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

            for (float step = 0f; step < 1f; step += stepSize)
            {
                diskVelocity += Physics.gravity * gravityMultiplier * stepSize * Time.deltaTime;

                RaycastForBounce(currPoint, diskVelocity.normalized, stepSize);

                nextPoint = currPoint + diskVelocity;

                // In case that the disk is out of bounds it gets placed in bounced and into play
                if (!arenaCollider.bounds.Contains(nextPoint))
                {
                    diskVelocity = Vector3.Reflect(diskVelocity, (arenaCollider.ClosestPointOnBounds(nextPoint) - nextPoint).normalized);

                    nextPoint = arenaCollider.ClosestPointOnBounds(nextPoint) + diskVelocity;
                }

                currPoint = nextPoint;
            }
            airBounceTime += Time.deltaTime;
            
            RequestSerialization();

            this.transform.position = nextPoint;
        }

        public void InterpolateDiskThrow(Vector3 updatePoint)
        {
            Vector3 currPoint = this.transform.position;

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
            airBounceTime += Time.deltaTime;

            this.transform.position = updatePoint;
        }

        /// <summary>
        /// Moves the object closer to the hand, if object has reached the hand it returns true as there are no events in Udon
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="hand"></param>
        /// <returns></returns>
        public bool ReturnObjectToHand(Vector3 origin, HumanBodyBones hand)
        {
            returnOrigin = origin;

            InterpolateReturn(returnOrigin);

            RequestSerialization();

            // Check if object has returned to the origin, if so then 
            if (Vector3.Distance(transform.position, returnOrigin) < 0.1f && Airborne)
            {
                OnReturnToHand();
                if(this.hand == hand)
                    return this.hand == hand;
            }
            return false;
        }

        public void InterpolateReturn(Vector3 target)
        {
            float _distance = Vector3.Distance(transform.position, target);
            float _returnSpeed = _distance / returnMultiplier;

            if (_distance >= distanceThreshold)
            {
                transform.SetPositionAndRotation(target, Quaternion.identity);
                nextPoint = target;
            }
            else if (_distance > 0)
            {
                nextPoint = Vector3.Lerp(transform.position, target, Time.deltaTime / _returnSpeed);
            }

            this.transform.position = nextPoint;
        }

        public void Throw(Vector3 velocity)
        {
            diskVelocity = Vector3.ClampMagnitude(velocity, diskSpeedLimit);
            transform.rotation = new Quaternion();

            SetBlocking(false);
            Airborne = true;
            airBounceTime = 0f;
            returnMultiplier = returnMultiplierAirborne;

            RequestSerialization();
        }

        public void SetBlocking(bool isBlocking)
        {
            _blocking = isBlocking;

            guardObject.SetActive(isBlocking);
        }

        public bool IsReturning()
        {
            return airBounceTime > returnTimeSeconds;
        }

        public void SetRotation(Quaternion handRotation)
        {
            diskRotation = handRotation.eulerAngles;
            transform.rotation = handRotation;
            RequestSerialization();
        }

        public void ResetDisk()
        {
            airborne = true;
            gameObject.SetActive(false);
        }

        public void SetThrown(bool thrown)
        {
            this.Airborne = thrown;
            RequestSerialization();
        }

        public void SetHand(HumanBodyBones hand)
        {
            this.hand = hand;
        }

        public void OnReturnToHand()
        {
            returnMultiplier = returnMultiplierInHand;
            SetThrown(false);
        }
    }
}