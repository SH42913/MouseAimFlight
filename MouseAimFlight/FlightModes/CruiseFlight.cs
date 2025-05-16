using System;
using UnityEngine;

namespace MouseAimFlight.FlightModes
{
    class CruiseFlight : Flight
    {
        const string FlightMode = "Cruise Flight";

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

            Vector3d target = (targetPosition - vessel.CurrentCoM).normalized;

            float sideslip = (float)Math.Asin(Vector3.Dot(vesselTransform.right, vessel.srf_velocity.normalized)) * Mathf.Rad2Deg;

            float pitchErrorHorizon = ((float)Math.Acos(Vector3.Dot(vesselTransform.up, vessel.upAxis)) - (float)Math.Acos(Vector3.Dot(target, vessel.upAxis))) * Mathf.Rad2Deg;
            float pitchErrorTarget = (float)Math.Asin(Vector3d.Dot(Vector3d.back, VectorUtils.Vector3dProjectOnPlane(targetDirection, Vector3d.right))) * Mathf.Rad2Deg;

            float yawError = 1.5f * (float)Math.Asin(Vector3d.Dot(Vector3d.right, VectorUtils.Vector3dProjectOnPlane(targetDirectionYaw, Vector3d.forward))) * Mathf.Rad2Deg;

            float pitchError = (pitchErrorHorizon + pitchErrorTarget) / 2f;

            //roll
            Vector3 currentRoll = -vesselTransform.forward;

            Vector3 rollTarget = (targetPosition + Mathf.Clamp(2 * upWeighting * (100f - Math.Abs(yawError * 1.8f)), 0, float.PositiveInfinity) * upDirection) - vessel.CurrentCoM;

            rollTarget = Vector3.ProjectOnPlane(rollTarget, vesselTransform.up);

            float rollError = VectorUtils.SignedAngle(currentRoll, rollTarget, vesselTransform.right) - sideslip * (float)Math.Sqrt(vessel.srf_velocity.magnitude) / 5;

            float pitchDownFactor = pitchError * (1 / ((yawError * yawError) / 1000 + 1));
            rollError += Math.Sign(rollError) * Math.Abs(Mathf.Clamp(pitchDownFactor, -20, 0));

            ErrorData behavior = new ErrorData(pitchError, rollError, yawError);

            return behavior;
        }

        public override string GetFlightMode()
        {
            return FlightMode;
        }
    }
}