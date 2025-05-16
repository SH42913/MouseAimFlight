/*
Copyright (c) 2016, ferram4, tetryds
All rights reserved.
*/

using System;

namespace MouseAimFlight
{
    class AdaptivePID
    {
        readonly PID pitchPID;
        readonly PID rollPID;
        readonly PID yawPID;

        float pitchP = 0.2f, pitchI = 0.1f, pitchD = 0.08f;
        float rollP = 0.01f, rollI = 0.0f, rollD = 0.005f;
        float yawP = 0.035f, yawI = 0.1f, yawD = 0.04f;
        float upWeighting = 3f; //TODO: update external upweighting

        float pIntLimit = 0.2f, rIntLimit = 0.2f, yIntLimit = 0.1f; //initialize integral limits at 0.2

        public AdaptivePID()
        {
            pitchPID = new PID(pitchP, pitchI, pitchD);
            rollPID = new PID(rollP, rollI, rollD);
            yawPID = new PID(yawP, yawI, yawD);
        }

        public float UpWeighting(float terrainAltitude, float velocity)
        {
            if (terrainAltitude < 50)
                return (10 - 0.18f * terrainAltitude) * upWeighting;

            return upWeighting;
        }

        public Steer Simulate(float pitchError, float rollError, float yawError, UnityEngine.Vector3 angVel, float terrainAltitude, float timestep, float dynPress, float vel)
        {
            float speedFactor = vel / dynPress / 16; //More work needs to be done to sanitize speedFactor

            if (speedFactor > 1.2f)
                speedFactor = 1.2f;

            float trimFactor = (float)Math.Sqrt(speedFactor);

            //AdaptGains(pitchError, rollError, yawError, angVel, terrainAltitude, timestep, dynPress, vel, trimFactor);

            float steerPitch = pitchPID.Simulate(pitchError, angVel.x, pIntLimit * trimFactor, timestep, speedFactor);
            float steerRoll = rollPID.Simulate(rollError, angVel.y, rIntLimit, timestep, speedFactor);
            if (pitchPID.IntegralZeroed) //yaw integrals should be zeroed at the same time that pitch PIDs are zeroed, because that happens in large turns
                yawPID.ZeroIntegral();
            float steerYaw = yawPID.Simulate(yawError, angVel.z, yIntLimit, timestep, speedFactor);

            return new Steer(steerPitch, steerRoll, steerYaw);
        }
    }
    public readonly struct Steer
    {
        public readonly float pitch;
        public readonly float roll;
        public readonly float yaw;

        public Steer(float p, float r, float y)
        {
            pitch = p;
            roll = r;
            yaw = y;
        }
    }
}