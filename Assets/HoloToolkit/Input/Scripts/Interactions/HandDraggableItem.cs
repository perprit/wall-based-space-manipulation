// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using System;
using ManipulateWalls;
using System.Collections.Generic;

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
        [Tooltip("Transform that will be dragged. Defaults to the object of the component.")]
        public Transform HostTransform;

        private Camera mainCamera;
        private bool isDragging;

        private Vector3 initCameraPosition;
        private Vector3 initObjPosition;

        private Vector3 initHandPosition;
        private Vector3 initHandVector;
        private Vector3 prevHandPosition;   // for smoothing
        private Vector3 prevObjPosition;   // for smoothing

        private float minRatio = 2.5f;
        private float maxRatio = 12f;
        private float distMul = 1.65f;
        private float velMul = 16f;

        private IInputSource currentInputSource;
        private uint currentInputSourceId;

        private float smoothingRatio;
        
        private void Start()
        {
            if (HostTransform == null)
            {
                HostTransform = transform;
            }

            mainCamera = Camera.main;
            currentInputSource = null;
            currentInputSourceId = 0;
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

            if (isDragging)
            {
                return;
            }

            RepositionManager.Instance.SetItemMode(gameObject, ItemStatusModes.DRAGGING);

            // Add self as a modal input handler, to get all inputs during the manipulation
            //InputManager.Instance.PushModalInputHandler(gameObject);
            InputManager.Instance.AddMultiModalInputHandler(currentInputSourceId, gameObject);

            isDragging = true;
            
            currentInputSource.TryGetPosition(currentInputSourceId, out initHandPosition);
            prevHandPosition = initHandPosition;
            initHandVector = initHandPosition - mainCamera.transform.position;
            initCameraPosition = mainCamera.transform.position;
            initObjPosition = HostTransform.position;
        }

        /// <summary>
        /// Update the position of the object being dragged.
        /// </summary>
        private void UpdateDragging()
        {
            // Hololens-like object movement
            Vector3 handPosition;
            currentInputSource.TryGetPosition(currentInputSourceId, out handPosition);

            // hand position after smoothing
            handPosition = handPosition * smoothingRatio + prevHandPosition * (1 - smoothingRatio);

            Vector3 headMovement = mainCamera.transform.position - initCameraPosition;

            Vector3 handVector = handPosition - mainCamera.transform.position;
            Vector3 handMovement = handVector - initHandVector;

            Vector3 newObjPosition = initObjPosition + headMovement + handMovement;

            if (ExperimentManager.Instance.InteractionType == InteractionType.CONST)
            {
                // CONST, deprecated
                newObjPosition = initObjPosition + headMovement + handMovement * 5.5f;
            }
            else if (ExperimentManager.Instance.InteractionType == InteractionType.ADAPT)
            {
                // ADAPT
                float velocity = Vector3.Magnitude(handMovement) / Time.smoothDeltaTime;
                velocity = UtilFunctions.Instance.QuadraticInOut(velocity * velMul, minRatio, maxRatio);
                newObjPosition = initObjPosition + headMovement + handMovement * velocity;
                initObjPosition = newObjPosition;
                initCameraPosition = mainCamera.transform.position;
                initHandVector = handVector;
            }
            else if (ExperimentManager.Instance.InteractionType == InteractionType.DIST)
            {
                // DIST
                float newObjDist = Vector3.Magnitude(prevObjPosition - mainCamera.transform.position);
                newObjPosition = initObjPosition + headMovement + handMovement * Mathf.Clamp(newObjDist * distMul, minRatio, Mathf.Infinity);
                initObjPosition = newObjPosition;
                initCameraPosition = mainCamera.transform.position;
                initHandVector = handVector;
            }

            // clamp movement vector with wall objects
            Vector3 eyeToObjDirection = newObjPosition - mainCamera.transform.position;
            RaycastHit hit;
            // raycast only on SpatialMapping layer
            if (Physics.Raycast(mainCamera.transform.position, Vector3.Normalize(eyeToObjDirection), out hit,
                Vector3.Magnitude(eyeToObjDirection), 1 << LayerMask.NameToLayer("SpatialMapping")))
            {
                newObjPosition = hit.point;
            }

            HostTransform.position = newObjPosition;
            prevObjPosition = newObjPosition;
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

            RepositionManager.Instance.SetItemMode(gameObject, ItemStatusModes.IDLE);
        }

        public void OnFocusEnter()
        {
        }

        public void OnFocusExit()
        {
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

            /*
            GameObject wall = ExperimentManager.Instance.GetWallObject();
            if (RepositionManager.Instance.GetWallStatusMode(wall.GetInstanceID()) == WallStatusModes.LOCKED)
            {
                wall.transform.position = RepositionManager.Instance.GetWallInitObject(wall.GetInstanceID()).transform.position;
                GameObject initObj = RepositionManager.Instance.GetItemInitObject(gameObject.GetInstanceID());
                initObj.transform.position = gameObject.transform.position;
                RepositionManager.Instance.SetWallMode(wall, WallStatusModes.IDLE);
            }
            */

            currentInputSource = eventData.InputSource;
            currentInputSourceId = eventData.SourceId;
            
            StartDragging();
        }

        public void OnInputDoubleTapped(InputEventData eventData)
        {
            // do nothing
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

        public void SetConst(string str)
        {
            string[] tokens = str.Split(' ');
            minRatio = float.Parse(tokens[0]);
            maxRatio = float.Parse(tokens[1]);
            velMul = float.Parse(tokens[2]);
            distMul = float.Parse(tokens[3]);
            Debug.Log(minRatio + ", " + maxRatio + ", " + velMul + ", " + distMul);
        }
    }
}
