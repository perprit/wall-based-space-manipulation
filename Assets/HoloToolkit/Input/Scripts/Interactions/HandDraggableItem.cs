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


        [Tooltip("Should the object be kept upright as it is being dragged?")]
        public bool IsKeepUpright = false;

        [Tooltip("Should the object be oriented towards the user as it is being dragged?")]
        public bool IsOrientTowardsUser = true;

        public bool IsDraggingEnabled = true;

        public float SmoothingRatio = 0.5f;

        private Camera mainCamera;
        private bool isDragging;
        private bool isGazed;

        private Vector3 initialCameraPosition;
        private Vector3 initialObjPosition;

        private Vector3 initialHandVector;
        private Vector3 prevHandPosition;   // for smoothing

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

            RepositionManager.Instance.StartReposition(currentInputSource, currentInputSourceId, gameObject, DraggableType.Item);

            // Add self as a modal input handler, to get all inputs during the manipulation
            InputManager.Instance.PushModalInputHandler(gameObject);
            //InputManager.Instance.AddMultiModalInputHandler(currentInputSourceId, gameObject);

            isDragging = true;
            //GazeCursor.Instance.SetState(GazeCursor.State.Move);
            //GazeCursor.Instance.SetTargetObject(HostTransform);

            Vector3 initialHandPosition;
            
            currentInputSource.TryGetPosition(currentInputSourceId, out initialHandPosition);
            initialHandVector = initialHandPosition - mainCamera.transform.position;
            prevHandPosition = initialHandPosition;
            initialCameraPosition = mainCamera.transform.position;
            initialObjPosition = HostTransform.position;

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
            handPosition = handPosition * SmoothingRatio + prevHandPosition * (1 - SmoothingRatio);

            Vector3 headMovement = mainCamera.transform.position - initialCameraPosition;
            Vector3 handVector = handPosition - mainCamera.transform.position;
            Vector3 handMovement = handVector - initialHandVector;

            float cameraToObjDist= Vector3.Magnitude(HostTransform.position - mainCamera.transform.position);
            float cameraToHandDist = Vector3.Magnitude(handPosition - mainCamera.transform.position);

            float distRatio = cameraToHandDist > 0.1f && cameraToObjDist > 0.1f ? cameraToObjDist / cameraToHandDist : 1f;
            
            HostTransform.position = initialObjPosition + headMovement + handMovement * distRatio;

            /*
            if (IsKeepUpright)
            {
                Quaternion upRotation = Quaternion.FromToRotation(HostTransform.up, Vector3.up);
                HostTransform.rotation = upRotation * HostTransform.rotation;
            }
            */

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
            InputManager.Instance.PopModalInputHandler();
            //InputManager.Instance.RemoveMultiModalInputHandler(currentInputSourceId);

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
