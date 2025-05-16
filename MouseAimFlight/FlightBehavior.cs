/*
Copyright (c) 2016, ferram4, tetryds
All rights reserved.
*/

using System.Collections.Generic;
using MouseAimFlight.FlightModes;
using UnityEngine;

namespace MouseAimFlight
{
    class FlightBehavior
    {
        readonly List<Flight> modes;

        int activeMode;

        public FlightBehavior() //Hardcoded Behaviors listing, can be made dynamic
        {
            modes = new List<Flight>
            {
                new NormalFlight(),
                new CruiseFlight(),
                new AggressiveFlight()
            };

        }

        public ErrorData Simulate(Transform vesselTransform, Transform velocityTransform, Vector3 targetPosition, Vector3 upDirection, float upWeighting, Vessel vessel)
        {
            return modes[activeMode].Simulate(vesselTransform, velocityTransform, targetPosition, upDirection, upWeighting, vessel);
        }

        public void NextBehavior()
        {
            activeMode++;
            if (activeMode >= modes.Count)
                activeMode = 0;
        }

        public string GetBehaviorName()
        {
            return modes[activeMode].GetFlightMode();
        }
    }

    public readonly struct ErrorData
    {
        public readonly float pitchError;
        public readonly float rollError;
        public readonly float yawError;

        public ErrorData(float p, float r, float y)
        {
            pitchError = p;
            rollError = r;
            yawError = y;
        }
    }
}