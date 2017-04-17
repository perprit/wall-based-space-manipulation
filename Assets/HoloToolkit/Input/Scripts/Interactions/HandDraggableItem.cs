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

        public bool IsDraggingEnabled = true;

        private Camera mainCamera;
        private bool isDragging;

        private Vector3 initCameraPosition;
        private Vector3 initObjPosition;

        private Vector3 initHandPosition;
        private Vector3 initHandVector;
        private Vector3 prevHandPosition;   // for smoothing

        private Vector3 draggingPosition;
        private Quaternion draggingRotation;

        private IInputSource currentInputSource = null;
        private uint currentInputSourceId;

        private float smoothingRatio;

        private void Start()
        {
            if (HostTransform == null)
            {
                HostTransform = transform;
            }

            mainCamera = Camera.main;
            smoothingRatio = RepositionManager.Instance.SmoothingRatio;
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

            RepositionManager.Instance.StartReposition(currentInputSource, currentInputSourceId, gameObject, DraggableType.Item);

            // Add self as a modal input handler, to get all inputs during the manipulation
            //InputManager.Instance.PushModalInputHandler(gameObject);
            InputManager.Instance.AddMultiModalInputHandler(currentInputSourceId, gameObject);

            isDragging = true;
            
            currentInputSource.TryGetPosition(currentInputSourceId, out initHandPosition);
            prevHandPosition = initHandPosition;
            initHandVector = initHandPosition - mainCamera.transform.position;
            initCameraPosition = mainCamera.transform.position;
            initObjPosition = HostTransform.position;

            StartedDragging.RaiseEvent();
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
            Vector3 handPosition;
            currentInputSource.TryGetPosition(currentInputSourceId, out handPosition);
            
            // hand position after smoothing
            handPosition = handPosition * smoothingRatio + prevHandPosition * (1 - smoothingRatio);

            Vector3 headMovement = mainCamera.transform.position - initCameraPosition;

            Vector3 handVector = handPosition - mainCamera.transform.position;
            Vector3 handMovement = handVector - initHandVector;

            Vector3 newObjPosition = initObjPosition + headMovement + handMovement;
            Vector3 newObjVector = newObjPosition - mainCamera.transform.position;
            float newObjDist = Vector3.Magnitude(newObjVector);

            HostTransform.position = initObjPosition + headMovement + handMovement * newObjDist * 2f;

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

            isDragging = false;
            currentInputSource = null;

            RepositionManager.Instance.StopReposition(currentInputSourceId, DraggableType.Item);
            StoppedDragging.RaiseEvent();
        }

        public void OnFocusEnter()
        {
            if (!IsDraggingEnabled)
            {
                return;
            }
        }

        public void OnFocusExit()
        {
            if (!IsDraggingEnabled)
            {
                return;
            }
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
