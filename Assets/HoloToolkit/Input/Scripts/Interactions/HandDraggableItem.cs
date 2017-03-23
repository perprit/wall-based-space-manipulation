// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using System;
using ManipulateWalls;

namespace HoloToolkit.Unity.InputModule
{
    /// <summary>
    /// Component that allows dragging an object with your hand on HoloLens.
    /// Dragging is done by calculating the angular delta and z-delta between the current and previous hand positions,
    /// and then repositioning the object based on that.
    /// </summary>
    public class HandDraggableItem : MonoBehaviour,
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

        [Tooltip("Scale by which hand movement in z is multipled to move the dragged object.")]
        public float DistanceScale = 2f;

        [Tooltip("Should the object be kept upright as it is being dragged?")]
        public bool IsKeepUpright = false;

        [Tooltip("Should the object be oriented towards the user as it is being dragged?")]
        public bool IsOrientTowardsUser = true;

        public bool IsDraggingEnabled = true;

        private Camera mainCamera;
        private bool isDragging;
        private bool isGazed;
        //private Vector3 objRefForward;
        //private Vector3 objRefUp;
        //private float objRefDistance;
        //private Quaternion gazeAngularOffset;
        //private float handRefDistance;
        //private Vector3 objRefGrabPoint;

        private Transform initialCameraTransform;
        private Vector3 initialHandPosition;
        private Vector3 initialObjPosition;

        //private Vector3 initialPivotPosition;

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

            // Add self as a modal input handler, to get all inputs during the manipulation
            //InputManager.Instance.PushModalInputHandler(gameObject);
            InputManager.Instance.AddMultiModalInputHandler(currentInputSourceId, gameObject);
            //Debug.Log("StartDragging, InputSourceId: " + currentInputSourceId);

            isDragging = true;
            //GazeCursor.Instance.SetState(GazeCursor.State.Move);
            //GazeCursor.Instance.SetTargetObject(HostTransform);

            //Vector3 gazeHitPosition = GazeManager.Instance.HitInfo.point;
            currentInputSource.TryGetPosition(currentInputSourceId, out initialHandPosition);

            //Vector3 pivotPosition = GetHandPivotPosition();
            //initialPivotPosition = pivotPosition;
            //handRefDistance = Vector3.Magnitude(handPosition - pivotPosition);
            //objRefDistance = Vector3.Magnitude(HostTransform.position - pivotPosition);
            //objRefDistance = Vector3.Magnitude(gazeHitPosition - pivotPosition);


            //Vector3 objForward = HostTransform.forward;
            //Vector3 objUp = HostTransform.up;

            // Store where the object was grabbed from
            //objRefGrabPoint = mainCamera.transform.InverseTransformDirection(HostTransform.position - gazeHitPosition);
            
            //Vector3 objDirection = Vector3.Normalize(gazeHitPosition - pivotPosition);            
            //Vector3 objDirection = Vector3.Normalize(HostTransform.position - pivotPosition);
            //Vector3 handDirection = Vector3.Normalize(initialHandPosition - pivotPosition);

            //objForward = mainCamera.transform.InverseTransformDirection(objForward);       // in camera space
            //objUp = mainCamera.transform.InverseTransformDirection(objUp);       		   // in camera space
            //objDirection = mainCamera.transform.InverseTransformDirection(objDirection);   // in camera space
            //handDirection = mainCamera.transform.InverseTransformDirection(handDirection); // in camera space

            //objRefForward = objForward;
            //objRefUp = objUp;

            // Store the initial offset between the hand and the object, so that we can consider it when dragging
            //gazeAngularOffset = Quaternion.FromToRotation(handDirection, objDirection);
            //draggingPosition = gazeHitPosition;

            initialCameraTransform = mainCamera.transform;
            initialObjPosition = HostTransform.position;

            StartedDragging.RaiseEvent();
        }

        /// <summary>
        /// Gets the pivot position for the hand, which is approximated to the base of the neck.
        /// </summary>
        /// <returns>Pivot position for the hand.</returns>
        /*
        private Vector3 GetHandPivotPosition()
        {
            Vector3 pivot = Camera.main.transform.position + new Vector3(0, -0.2f, 0) - Camera.main.transform.forward * 0.2f; // a bit lower and behind
            return pivot;
        }
        */

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
            Vector3 newHandPosition;
            currentInputSource.TryGetPosition(currentInputSourceId, out newHandPosition);

            DebugTextController.Instance.SetMessage(newHandPosition.ToString());

            Vector3 handMoveDirection = Vector3.Normalize(newHandPosition - initialHandPosition);
            float handMoveMagnitude = Vector3.Magnitude(newHandPosition - initialHandPosition);

            //Vector3 pivotPosition = GetHandPivotPosition();

            //pivotPosition = initialPivotPosition;

            //Vector3 newHandDirection = Vector3.Normalize(newHandPosition - pivotPosition);

            //newHandDirection = mainCamera.transform.InverseTransformDirection(newHandDirection); // in camera space
            //Vector3 targetDirection = Vector3.Normalize(gazeAngularOffset * newHandDirection);
            //targetDirection = mainCamera.transform.TransformDirection(targetDirection); // back to world space

            //newHandDirection = initialCameraTransform.InverseTransformDirection(newHandDirection); // in camera space
            //Vector3 targetDirection = Vector3.Normalize(gazeAngularOffset * newHandDirection);
            //targetDirection = initialCameraTransform.TransformDirection(targetDirection); // back to world space

            //float currenthandDistance = Vector3.Magnitude(newHandPosition - pivotPosition);

            //float distanceRatio = currenthandDistance / handRefDistance;
            //float distanceOffset = distanceRatio > 0 ? (distanceRatio - 1f) * DistanceScale : 0;
            //float targetDistance = objRefDistance + distanceOffset;

            //draggingPosition = pivotPosition + (targetDirection * targetDistance);

            if (IsOrientTowardsUser)
            {
                //draggingRotation = Quaternion.LookRotation(HostTransform.position - pivotPosition);
            }
            else
            {
                //Vector3 objForward = mainCamera.transform.TransformDirection(objRefForward); // in world space
                //Vector3 objUp = mainCamera.transform.TransformDirection(objRefUp);   // in world space
                //draggingRotation = Quaternion.LookRotation(objForward, objUp);
            }

            // Apply Final Position
            //HostTransform.position = draggingPosition + mainCamera.transform.TransformDirection(objRefGrabPoint);
            //HostTransform.position = draggingPosition + initialCameraTransform.TransformDirection(objRefGrabPoint);
            HostTransform.position = initialObjPosition + handMoveDirection * handMoveMagnitude * DistanceScale;
            //HostTransform.rotation = draggingRotation;

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
            //InputManager.Instance.PopModalInputHandler();
            InputManager.Instance.RemoveMultiModalInputHandler(currentInputSourceId);

            isDragging = false;
            currentInputSource = null;
            RepositionManager.Instance.SetInputSource(null, uint.MaxValue, RepositionManager.DraggableType.Item);
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

            //Debug.Log("OnInputDown/HandDraggableObject, SourceId: " + currentInputSourceId);

            RepositionManager.Instance.SetInputSource(currentInputSource, currentInputSourceId, RepositionManager.DraggableType.Item);
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
