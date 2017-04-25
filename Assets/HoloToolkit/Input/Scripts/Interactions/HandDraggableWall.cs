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

        private Camera mainCamera;

        private bool isDragging;
        private Vector3 initialObjPosition;
        private Vector3 initialHandVector;
        private Vector3 prevHandPosition;       // for smoothing

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
        }

        private void Update()
        {
            if (isDragging)
            {
                UpdateDragging();
            }
        }

        /// <summary>
        /// Starts dragging the object.
        /// </summary>
        public void StartDragging()
        {
            Vector3 initialHandPosition;
            currentInputSource.TryGetPosition(currentInputSourceId, out initialHandPosition);

            // set member variables
            isDragging = true;
            initialObjPosition = HostTransform.position;
            initialHandVector = initialHandPosition - mainCamera.transform.position;
            prevHandPosition = initialHandPosition;

            // Add self as a modal input handler, to get all inputs during the manipulation
            //InputManager.Instance.PushModalInputHandler(gameObject);
            InputManager.Instance.AddMultiModalInputHandler(currentInputSourceId, gameObject);
            RepositionManager.Instance.SetWallMode(gameObject, WallStatusModes.DRAGGING);

            StartedDragging.RaiseEvent();
        }

        /// <summary>
        /// Update the position of the object being dragged.
        /// </summary>
        private void UpdateDragging()
        {
            float smoothingRatio = RepositionManager.Instance.SmoothingRatio;
            // current hand position
            Vector3 handPosition;
            currentInputSource.TryGetPosition(currentInputSourceId, out handPosition);

            // smoothing hand position
            handPosition = handPosition * smoothingRatio + prevHandPosition * (1 - smoothingRatio);
            
            Vector3 handVector = handPosition - mainCamera.transform.position;
            Vector3 handMovement = handVector - initialHandVector;

            // calculate hand movement along the normal of the wall object
            handMovement = HostTransform.transform.InverseTransformDirection(handMovement);
            handMovement = Vector3.Scale(handMovement, Vector3.forward);
            handMovement = HostTransform.transform.TransformDirection(handMovement);

            HostTransform.position = initialObjPosition + handMovement * RepositionManager.Instance.GetWallMovementScale(gameObject.GetInstanceID());

            // TODO clamp position with minimum distance to wall with respect to wall normal

            prevHandPosition = handPosition;
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
            RepositionManager.Instance.SetWallMode(gameObject, WallStatusModes.IDLE);

            isDragging = false;
            currentInputSource = null;

            HostTransform.position = initialObjPosition;
            StoppedDragging.RaiseEvent();
        }

        public void OnFocusEnter()
        {
        }

        public void OnFocusExit()
        {
        }

        public void OnInputUp(InputEventData eventData)
        {
            if (currentInputSource != null && eventData.SourceId == currentInputSourceId)
            {
                StopDragging();
            }
        }

        public void OnInputDown(InputEventData eventData)
        {
            if (!eventData.InputSource.SupportsInputInfo(eventData.SourceId, SupportedInputInfo.Position))
            {
                // The input source must provide positional data for this script to be usable
                return;
            }

            if (isDragging)
            {
                // We're already handling drag input, so we can't start a new drag operation.
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
