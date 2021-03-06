﻿// Copyright (c) Microsoft Corporation. All rights reserved.
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
        [Tooltip("Transform that will be dragged. Defaults to the object of the component.")]
        public Transform HostTransform;

        private float doubleTapDuration = 0.3f;

        private Camera mainCamera;
        private int instanceId;
        private Vector3 initialObjPosition;
        private Vector3 lockedObjPosition;
        private Vector3 initialHandVector;
        private Vector3 prevHandPosition;       // for smoothing
        private float lastInputUpTime = 0f;

        private IInputSource currentInputSource = null;
        private uint currentInputSourceId;

        private void Start()
        {
            if (HostTransform == null)
            {
                HostTransform = transform;
            }

            mainCamera = Camera.main;
            instanceId = gameObject.GetInstanceID();
        }

        private void OnDestroy()
        {
            if (currentInputSource != null)
            {
                // Remove self as a modal input handler
                InputManager.Instance.RemoveMultiModalInputHandler(currentInputSourceId);
            }
        }

        private void Update()
        {
            if (RepositionManager.Instance.IsWallAvailable
                && RepositionManager.Instance.GetWallStatusMode(instanceId) == WallStatusModes.DRAGGING
                && currentInputSource != null)
            {
                UpdateDragging();
            }
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

            Vector3 newWallPosition = lockedObjPosition + handMovement * RepositionManager.Instance.GetWallMovementScale(gameObject.GetInstanceID());

            Transform initWallTransform = RepositionManager.Instance.GetWallInitObject(gameObject.GetInstanceID()).transform;

            Vector3 initRefWallPos = initWallTransform.InverseTransformPoint(newWallPosition);
            Vector3 initRefCameraPos = initWallTransform.InverseTransformPoint(RepositionManager.Instance.GetCameraMinDistToWallPosition());

            float initRefWallZ = initRefWallPos.z;
            float initRefCameraZ = initRefCameraPos.z;
            /*
            if (Mathf.Sign(initRefWallZ * initRefCameraZ) < 0 && Mathf.Abs(initRefWallZ) > Mathf.Abs(initRefCameraZ))
            {
                initRefWallPos.z = -initRefCameraPos.z;
            }
            */
            if (initRefWallZ * initRefCameraZ < 0)
            {
                initRefWallPos.z = 0;
            }
            else if (Mathf.Sign(initRefWallZ * initRefCameraZ) > 0 && Mathf.Abs(initRefWallZ) > Mathf.Abs(initRefCameraZ))
            {
                initRefWallPos.z = initRefCameraPos.z;
            }

            newWallPosition = initWallTransform.TransformPoint(initRefWallPos);

            HostTransform.position = newWallPosition;

            prevHandPosition = handPosition;
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
                if (RepositionManager.Instance.GetWallStatusMode(instanceId) == WallStatusModes.DRAGGING)
                {
                    // Remove self as a modal input handler
                    InputManager.Instance.RemoveMultiModalInputHandler(currentInputSourceId);
                    RepositionManager.Instance.SetWallMode(gameObject, WallStatusModes.LOCKED);

                    currentInputSource = null;
                    currentInputSourceId = 0;
                }
            }
            lastInputUpTime = Time.unscaledTime;
        }

        public void OnInputDown(InputEventData eventData)
        {
            if (ExperimentManager.Instance.IsOnCountdown())
            {
                return;
            }

            if (!eventData.InputSource.SupportsInputInfo(eventData.SourceId, SupportedInputInfo.Position))
            {
                // The input source must provide positional data for this script to be usable
                return;
            }

            if (RepositionManager.Instance.GetWallStatusMode(instanceId) == WallStatusModes.DRAGGING)
            {
                // We're already handling drag input, so we can't start a new drag operation.
                return;
            }

            if (RepositionManager.Instance.GetWallStatusMode(instanceId) == WallStatusModes.IDLE)
            {
                currentInputSource = eventData.InputSource;
                currentInputSourceId = eventData.SourceId;

                // Add self as a modal input handler, to get all inputs during the manipulation
                InputManager.Instance.AddMultiModalInputHandler(currentInputSourceId, gameObject);
                RepositionManager.Instance.SetWallMode(gameObject, WallStatusModes.DRAGGING);

                // calculate initial positions
                Vector3 initialHandPosition;
                currentInputSource.TryGetPosition(currentInputSourceId, out initialHandPosition);
                initialObjPosition = HostTransform.position;
                lockedObjPosition = HostTransform.position;
                initialHandVector = initialHandPosition - mainCamera.transform.position;
                prevHandPosition = initialHandPosition;
            }
            else if (RepositionManager.Instance.GetWallStatusMode(instanceId) == WallStatusModes.LOCKED)
            {
                if (Time.unscaledTime - lastInputUpTime < doubleTapDuration)
                {
                    OnInputDoubleTapped(eventData);
                }
                else
                {
                    currentInputSource = eventData.InputSource;
                    currentInputSourceId = eventData.SourceId;

                    // Add self as a modal input handler, to get all inputs during the manipulation
                    InputManager.Instance.AddMultiModalInputHandler(currentInputSourceId, gameObject);
                    RepositionManager.Instance.SetWallMode(gameObject, WallStatusModes.DRAGGING);

                    // calculate initial positions
                    Vector3 initialHandPosition;
                    currentInputSource.TryGetPosition(currentInputSourceId, out initialHandPosition);
                    lockedObjPosition = HostTransform.position;
                    initialHandVector = initialHandPosition - mainCamera.transform.position;
                    prevHandPosition = initialHandPosition;
                }
            }
        }

        public void OnInputDoubleTapped(InputEventData eventData)
        {
            if (RepositionManager.Instance.GetWallStatusMode(instanceId) == WallStatusModes.LOCKED)
            {
                HostTransform.position = initialObjPosition;
                RepositionManager.Instance.SetWallMode(gameObject, WallStatusModes.IDLE);
            }
        }

        public void OnSourceDetected(SourceStateEventData eventData)
        {
            // nothing to do
        }

        public void OnSourceLost(SourceStateEventData eventData)
        {
            if (currentInputSource != null && eventData.SourceId == currentInputSourceId)
            {
                if (RepositionManager.Instance.GetWallStatusMode(instanceId) == WallStatusModes.DRAGGING)
                {
                    // Remove self as a modal input handler
                    //InputManager.Instance.PopModalInputHandler();
                    InputManager.Instance.RemoveMultiModalInputHandler(currentInputSourceId);
                    RepositionManager.Instance.SetWallMode(gameObject, WallStatusModes.LOCKED);
                }
            }
        }
    }
}
