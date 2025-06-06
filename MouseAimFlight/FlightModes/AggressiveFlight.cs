﻿using System;
using UnityEngine;

namespace MouseAimFlight.FlightModes
{
    class AggressiveFlight : Flight
    {
        const string FlightMode = "Aggressive Flight";

        public override ErrorData Simulate(Transform vesselTransform, Transform velocityTransform, Vector3 targetPosition, Vector3 upDirection, float upWeighting, Vessel vessel)
        {
            Vector3d srfVel = vessel.srf_velocity;
            if (srfVel != Vector3d.zero)
            {
                velocityTransform.rotation = Quaternion.LookRotation(srfVel, -vesselTransform.forward);
            }
            velocityTransform.rotation = Quaternion.AngleAxis(90, velocityTransform.right) * velocityTransform.rotation;

            Vector3d targetDirection = vesselTransform.InverseTransformDirection(targetPosition - vessel.CurrentCoM).normalized;
            Vector3d targetDirectionYaw = targetDirection;

            float sideslip = (float)Math.Asin(Vector3.Dot(vesselTransform.right, vessel.srf_velocity.normalized)) * Mathf.Rad2Deg;

            float pitchError = (float)Math.Asin(Vector3d.Dot(Vector3d.back, VectorUtils.Vector3dProjectOnPlane(targetDirection, Vector3d.right))) * Mathf.Rad2Deg;
            float yawError = (float)Math.Asin(Vector3d.Dot(Vector3d.right, VectorUtils.Vector3dProjectOnPlane(targetDirectionYaw, Vector3d.forward))) * Mathf.Rad2Deg;

            //roll
            Vector3 currentRoll = -vesselTransform.forward;

            Vector3 rollTarget = (targetPosition + Mathf.Clamp(upWeighting * (100f - Math.Abs(yawError * 1.6f) - (pitchError * 2.8f)), 0, float.PositiveInfinity) * upDirection) -
                                 vessel.CoM;

            rollTarget = Vector3.ProjectOnPlane(rollTarget, vesselTransform.up);

            float rollError = VectorUtils.SignedAngle(currentRoll, rollTarget, vesselTransform.right) - sideslip * (float)Math.Sqrt(vessel.srf_velocity.magnitude) / 5;

            float pitchDownFactor = pitchError * (10 / ((float)Math.Pow(yawError, 2) + 10f) - 0.1f);
            rollError += Math.Sign(rollError) * Math.Abs(Mathf.Clamp(pitchDownFactor, -15, 0));

            //pitchError -= Math.Abs(Mathf.Clamp(rollError, -pitchError, +pitchError) / 3);

            pitchError += 1 - Mathf.Exp(-pitchError);

            ErrorData behavior = new ErrorData(pitchError, rollError, yawError);

            return behavior;
        }

        public override string GetFlightMode()
        {
            return FlightMode;
        }
    }
}