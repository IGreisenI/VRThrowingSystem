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
        [SerializeField] private AudioSource diskAudioSource;

        [Header("General Settings")]
        [SerializeField] private float airBounceTimeLimit;
        [SerializeField] private float gravityMultiplier;
        [SerializeField] private float returnMultiplierAirborne;
        [SerializeField] private float returnMultiplierInHand;
        [SerializeField] private float distanceThreshold;
        [SerializeField] private float diskSpeedLimit;
        [SerializeField] private AnimationCurve returnCurve;

        [Header("Visibility")]
        [SerializeField] private Material thrownMat;
        [SerializeField] private Material returnMat;

        [UdonSynced] private bool airborne = true;
        public bool Airborne { get { return airborne; } set { airborne = value; } }
        [UdonSynced] private bool _blocking = false;
        private VRCPlayerApi _localPlayer;
        private HumanBodyBones hand;

        #region BulletDrop
        [UdonSynced] private float airBounceTime = 0f;
        [UdonSynced] private float returnTime = 0f;
        private int predictionStepsPerFrame = 6;
        private float stepSize { get { return 1f / predictionStepsPerFrame; } }
        [UdonSynced] private float returnMultiplier;
        [UdonSynced] private Vector3 diskVelocity = Vector3.zero; 
        #endregion

        #region Syncing
        [UdonSynced] private Vector3 objectActualPosition = Vector3.zero;
        [UdonSynced] private Vector3 returnOrigin = Vector3.zero;
        [UdonSynced] private Vector3 diskRotation = Vector3.zero;
        #endregion

        public ThrowingObject Initialize(VRCPlayerApi localPlayer, HumanBodyBones hand)
        {
            _localPlayer = localPlayer;

            this.hand = hand;
            airBounceTime = airBounceTimeLimit + 0.01f;
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
                    ThrowingPhysics();
                }
            }
            // Ran by remote players
            else if (!IsReturning())
            {
                ThrowingPhysics();
            }
            else
            {
                InterpolateReturn(returnOrigin);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
        }

        public override void OnDeserialization()
        {
            if (Vector3.Distance(objectActualPosition, transform.position) > 1f && Airborne)
            {
                transform.position = objectActualPosition;
            }

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
                    diskVelocity = Vector3.Reflect(diskVelocity, hit.normal);
                    break;
                }
            }
        }

        public void ThrowingPhysics()
        {
            Vector3 currPoint = this.transform.position;
            Vector3 nextPoint = Vector3.zero;

            for (float step = 0f; step < 1f; step += stepSize)
            {
                diskVelocity += Physics.gravity * (gravityMultiplier / 1000) * stepSize * Time.deltaTime;

                RaycastForBounce(currPoint, diskVelocity.normalized, diskVelocity.magnitude);

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

            objectActualPosition = nextPoint;

            RequestSerialization();

            transform.position = nextPoint;
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
                objectActualPosition = target;
            }
            else if (_distance > 2f)
            {
                diskVelocity += (target - transform.position).normalized * returnCurve.Evaluate(returnTime / 5);
                objectActualPosition = transform.position + diskVelocity;
            }
            else
            {
                objectActualPosition = Vector3.Lerp(transform.position, target, Time.deltaTime / _returnSpeed);
            }

            returnTime += Time.deltaTime;
            this.transform.position = objectActualPosition;
        }

        public void Throw(Vector3 velocity)
        {
            SetRotation(Quaternion.identity);
            SetBlocking(false);

            diskVelocity = Vector3.ClampMagnitude(velocity, diskSpeedLimit);
            returnMultiplier = returnMultiplierAirborne;
            airBounceTime = 0f;
            returnTime = 0f;
            Airborne = true;

            RequestSerialization();
        }

        public void SetBlocking(bool isBlocking)
        {
            _blocking = isBlocking;

            guardObject.SetActive(isBlocking);
        }

        public bool IsReturning()
        {
            return airBounceTime > airBounceTimeLimit;
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