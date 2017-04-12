// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using ManipulateWalls;
using System;

namespace HoloToolkit.Unity.InputModule
{
    /// <summary>
    /// Component that allows dragging an object with your hand on HoloLens.
    /// Dragging is done by calculating the angular delta and z-delta between the current and previous hand positions,
    /// and then repositioning the object based on that.
    /// </summary>
    public class HandDraggableWall : MonoBehaviour,
                                 IFocusable,
                                 IInputHandler,
                                 ISourceStateHandler
    {
        /// <summary>
        /// Event triggered when dragging starts.
        /// </summary>
        public event Action StartedDragging;

        /// <summary>
        /// Event triggered when dragging stops.
        /// </summary>
        public event Action StoppedDragging;

        [Tooltip("Transform that will be dragged. Defaults to the object of the component.")]
        public Transform HostTransform;

        [Tooltip("Should the object be kept upright as it is being dragged?")]
        public bool IsKeepUpright = false;

        [Tooltip("Should the object be oriented towards the user as it is being dragged?")]
        public bool IsOrientTowardsUser = true;

        public bool IsDraggingEnabled = true;

        private Camera mainCamera;
        private bool isDragging;
        private bool isGazed;
        private Vector3 objRefForward;
        private float objRefDistance;
        private Quaternion gazeAngularOffset;
        private float handRefDistance;
        private Vector3 objRefGrabPoint;
        private Vector3 initialObjDirection;

        private Vector3 initialHostTransformPosition;
        private Quaternion initialHostTransformRotation;

        private Vector3 initialHandPosition;
        private Vector3 initialObjPosition;

        private Vector3 draggingPosition;
        private Quaternion draggingRotation;

        private IInputSource currentInputSource = null;
        private uint currentInputSourceId;

        private void Start()
        {
            if (HostTransform == null)
            {
                HostTransform = transform;
            }

            mainCamera = Camera.main;
        }

        private void OnDestroy()
        {
            if (isDragging)
            {
                StopDragging();
            }

            if (isGazed)
            {
                OnFocusExit();
            }
        }

        private void Update()
        {
            if (IsDraggingEnabled && isDragging)
            {
                UpdateDragging();
            }
        }

        /// <summary>
        /// Starts dragging the object.
        /// </summary>
        public void StartDragging()
        {
            if (!IsDraggingEnabled)
            {
                return;
            }

            if (isDragging)
            {
                return;
            }

            RepositionManager.Instance.StartReposition(currentInputSource, currentInputSourceId, gameObject, DraggableType.Wall);

            // Add self as a modal input handler, to get all inputs during the manipulation
            InputManager.Instance.PushModalInputHandler(gameObject);
            //InputManager.Instance.AddMultiModalInputHandler(currentInputSourceId, gameObject);

            isDragging = true;
            //GazeCursor.Instance.SetState(GazeCursor.State.Move);
            //GazeCursor.Instance.SetTargetObject(HostTransform);

            Vector3 gazeHitPosition = GazeManager.Instance.HitInfo.point;
            Vector3 handPosition;
            currentInputSource.TryGetPosition(currentInputSourceId, out initialHandPosition);
            currentInputSource.TryGetPosition(currentInputSourceId, out handPosition);

            Vector3 pivotPosition = GetHandPivotPosition();
            handRefDistance = Vector3.Magnitude(handPosition - pivotPosition);
            objRefDistance = Vector3.Magnitude(gazeHitPosition - pivotPosition);

            // HostTransform : the selected object
            Vector3 objForward = HostTransform.forward;

            // Store where the object was grabbed from
            // gaze hit -> object position (in camera coord)
            objRefGrabPoint = mainCamera.transform.InverseTransformDirection(HostTransform.position - gazeHitPosition);

            // pivot -> gaze hit (in world)
            Vector3 objDirection = Vector3.Normalize(gazeHitPosition - pivotPosition);
            // pivot -> hand (in world)
            Vector3 handDirection = Vector3.Normalize(handPosition - pivotPosition);

            initialObjDirection = Vector3.Normalize(objDirection);  // in world coord

            // obj forward (in camera)
            objForward = mainCamera.transform.InverseTransformDirection(objForward);       // in camera space
            // pivot -> gaze hit (in camera)
            objDirection = mainCamera.transform.InverseTransformDirection(objDirection);   // in camera space
            // pivot -> hand (in camera)
            handDirection = mainCamera.transform.InverseTransformDirection(handDirection); // in camera space
            
            objRefForward = objForward;

            // Store the initial offset between the hand and the object, so that we can consider it when dragging
            gazeAngularOffset = Quaternion.FromToRotation(handDirection, objDirection);
            draggingPosition = gazeHitPosition;

            initialHostTransformPosition = HostTransform.position;
            initialHostTransformRotation = HostTransform.rotation;
            initialObjPosition = HostTransform.position;

            StartedDragging.RaiseEvent();
        }

        /// <summary>
        /// Gets the pivot position for the hand, which is approximated to the base of the neck.
        /// </summary>
        /// <returns>Pivot position for the hand.</returns>
        private Vector3 GetHandPivotPosition()
        {
            Vector3 pivot = Camera.main.transform.position + new Vector3(0, -0.2f, 0) - Camera.main.transform.forward * 0.2f; // a bit lower and behind
            return pivot;
        }

        /// <summary>
        /// Enables or disables dragging.
        /// </summary>
        /// <param name="isEnabled">Indicates whether dragging shoudl be enabled or disabled.</param>
        public void SetDragging(bool isEnabled)
        {
            if (IsDraggingEnabled == isEnabled)
            {
                return;
            }

            IsDraggingEnabled = isEnabled;

            if (isDragging)
            {
                StopDragging();
            }
        }

        /// <summary>
        /// Update the position of the object being dragged.
        /// </summary>
        private void UpdateDragging()
        {
            // current hand position
            Vector3 newHandPosition;
            currentInputSource.TryGetPosition(currentInputSourceId, out newHandPosition);

            // pivot: my neck
            Vector3 pivotPosition = GetHandPivotPosition();

            // pivot -> hand direction (in world coord)
            Vector3 newHandDirection = Vector3.Normalize(newHandPosition - pivotPosition);

            Vector3 handMoveDirection = newHandPosition - initialHandPosition;
            handMoveDirection = HostTransform.transform.InverseTransformDirection(handMoveDirection);
            handMoveDirection = Vector3.Scale(handMoveDirection, Vector3.forward);
            float handMoveMagnitude = Vector3.Magnitude(handMoveDirection);
            handMoveDirection = Vector3.Normalize(handMoveDirection);
            handMoveDirection = HostTransform.transform.TransformDirection(handMoveDirection);
            //HostTransform.position = initialObjPosition + handMoveDirection * handMoveMagnitude * DistanceScale;
            // TODO scaling method is not mathematically correct
            HostTransform.position = initialObjPosition + handMoveDirection * handMoveMagnitude * RepositionManager.Instance.GetWallMovementScale() * 2.5f;

            if (IsKeepUpright)
            {
                Quaternion upRotation = Quaternion.FromToRotation(HostTransform.up, Vector3.up);
                HostTransform.rotation = upRotation * HostTransform.rotation;
            }
        }

        /// <summary>
        /// Stops dragging the object.
        /// </summary>
        public void StopDragging()
        {
            if (!isDragging)
            {
                return;
            }

            // Remove self as a modal input handler
            InputManager.Instance.PopModalInputHandler();
            //InputManager.Instance.RemoveMultiModalInputHandler(currentInputSourceId);
            RepositionManager.Instance.StopReposition(currentInputSourceId, DraggableType.Wall);

            isDragging = false;
            currentInputSource = null;

            HostTransform.position = initialHostTransformPosition;
            HostTransform.rotation = initialHostTransformRotation;
            StoppedDragging.RaiseEvent();
        }

        public void OnFocusEnter()
        {
            if (!IsDraggingEnabled)
            {
                return;
            }

            if (isGazed)
            {
                return;
            }

            isGazed = true;
        }

        public void OnFocusExit()
        {
            if (!IsDraggingEnabled)
            {
                return;
            }

            if (!isGazed)
            {
                return;
            }

            isGazed = false;
        }

        public void OnInputUp(InputEventData eventData)
        {
            if (currentInputSource != null &&
                eventData.SourceId == currentInputSourceId)
            {
                StopDragging();
            }
        }

        public void OnInputDown(InputEventData eventData)
        {
            if (isDragging)
            {
                // We're already handling drag input, so we can't start a new drag operation.
                return;
            }

            if (!eventData.InputSource.SupportsInputInfo(eventData.SourceId, SupportedInputInfo.Position))
            {
                // The input source must provide positional data for this script to be usable
                return;
            }

            currentInputSource = eventData.InputSource;
            currentInputSourceId = eventData.SourceId;
            StartDragging();
        }

        public void OnSourceDetected(SourceStateEventData eventData)
        {
            // Nothing to do
        }

        public void OnSourceLost(SourceStateEventData eventData)
        {
            if (currentInputSource != null && eventData.SourceId == currentInputSourceId)
            {
                StopDragging();
            }
        }
    }
}
