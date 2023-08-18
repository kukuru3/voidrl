using System.Collections.Generic;
using System.Security.Permissions;
using UnityEngine;
using Void;

namespace Scanner.AppContext {
    public class TestUtil {
        static void TestProfile(decimal speedInC, decimal accelInG) {
            var profile = SpaceMath.GetProfile(speedInC, accelInG);
            var d = SpaceMath.FormatDistance(profile.distanceOfFullAcceleration);
            var t = SpaceMath.FormatTime(profile.timeToFullAcceleration);
            var distanceToAlphaCentauriMeters = SpaceMath.LightYearsToMeters(4.1m);
            var maxSpeedInMetersPerSecond = speedInC * SpaceMath.c;

            var accelInSIUnits = accelInG * SpaceMath.g;

            var travelEstimate = SpaceMath.EstimateTravelTime(distanceToAlphaCentauriMeters, maxSpeedInMetersPerSecond, accelInSIUnits);
            var timeToReachMaxV = (travelEstimate.totalTime - travelEstimate.coastTime) / 2;
            
            var accelerationDistance = accelInSIUnits * timeToReachMaxV * timeToReachMaxV / 2;
            
            var str = $"At {accelInG}g acceleration (and theoretical limit of {speedInC:P3}c), we will reach Alpha Centauri in {SpaceMath.FormatTime(travelEstimate.totalTime)}, accelerating then decelerating all the way. Midway velocity will be {travelEstimate.maxV/SpaceMath.c:P3}, which will take {SpaceMath.FormatDistance(accelerationDistance)}";
            if (travelEstimate.coastTime > 0) {
                
                // var timeOfReachingMaxV = travelEstimate.maxV / accelInSIUnits;
                str = $"At {accelInG}g acceleration (and theoretical limit of {speedInC:P3}c), we will reach Alpha Centauri in {SpaceMath.FormatTime(travelEstimate.totalTime)}. Acceleration stage will take {SpaceMath.FormatTime(timeToReachMaxV)}, max velocity will be reached after {SpaceMath.FormatDistance(accelerationDistance)}; "
                    + $"This will be followed by a coasting period of {SpaceMath.FormatTime(travelEstimate.coastTime)} of going at {travelEstimate.maxV/SpaceMath.c:P3}c, then deceleration.";
            }
            Debug.Log(str);

        }

        // complex miner rendezvous:
        // - colony ship sends out miners as soon as the asteroid is detected
        // - miners accelerate and decelerate to reach the asteroid ASAP
        // - (miners are assumed to have thrust much greater than mass of the ship, so accel and decel are limited at 1g in this stage due to pilots)
        // - miners mine the asteroid for X amount of hours
        // - miners leave the asteroid and catch up to the main ship


        // POINT ZERO : ship sends out miners.

        // FIRST: determine the time it takes for miners to reach the comet
        // given: a, R (distance to comet), v (spd of colony ship)

        // t = 2 * x + v/a
        // x =  ( (-v) + sqrt(v*v / 2 + aR) ) / 2
        // t =  [ v + sqr(2*v*v - 4aR) ] / 2

        public struct MinerRendezvousConfig {
            public decimal detectionDistanceInAU;
            public decimal minerAccelerationCapInG;
            public decimal colonyShipVInC;
        }

        public struct MinerRendezvousResult {
            public decimal timeOvertakeZero;
            public decimal detectionTime;
            public decimal minerOvertake;
            public decimal timeToTouchdown;
            public decimal timeAccelBurn;
        }

        public struct CatchUpConfig {
            public decimal minerInitialOvertake;
            public decimal minerTimeToMineInHours;
            public decimal ladenMinerAcceleration;
            public decimal colonyShipVinC;
        }

        public struct CatchUpResult {
            public decimal idealCatchupDistance;
            public decimal idealCatchupTime;
            public decimal rendezvousTime;
            public decimal rendezvousDistanceFromAsteroid;
            public decimal leewayWaitTime;
        }

        delegate decimal DecimalOp(decimal input);

        static MinerRendezvousResult? GetRendezvous(MinerRendezvousConfig config) {
            DecimalOp root = SpaceMath.Root;

            var R = config.detectionDistanceInAU * SpaceMath.AU;
            var v = config.colonyShipVInC * SpaceMath.c;
            var a = config.minerAccelerationCapInG * SpaceMath.g;
            var C = v * v / a / 2;

            if (C > R) return null;

            var t2 = v / a;

            var timeOvertakeZero = t2 * (1m + 2m.Root());

            var detectionTime = R / v;

            var timeX = (-v + root(v * v / 2 + a * R)) / a;

            var timeRendezvous = timeX + timeX + t2;

            var travelDistanceAfterRelease_Miner = 2 * v * timeX + a * timeX * timeX + v * v / a / a / 2;
            var travelDistanceAfterRelease_CS = v * timeRendezvous;


            // if released at detection time:
            // var t1ImmediateRelease = (detectionTime - t2) / 2;

            // todo: this conflates detection time with time of touchdown.

            
            
            //var travelDistanceAfterRelease_CS = v * (2m * t1ImmediateRelease + t2);
            //var travelDistanceAfterRelease_Miner = 2 * v * t1ImmediateRelease + a * t1ImmediateRelease * t1ImmediateRelease + v * v / a / 2;

            var overtake = travelDistanceAfterRelease_Miner - travelDistanceAfterRelease_CS;

            return new MinerRendezvousResult { 
                timeOvertakeZero = timeOvertakeZero, 
                detectionTime = detectionTime, 
                timeAccelBurn = timeX,
                timeToTouchdown = timeRendezvous,
                minerOvertake = overtake
            };
        }

        static CatchUpResult GetCatchUp(CatchUpConfig config) {            
            var v = config.colonyShipVinC * SpaceMath.c;
            var a = config.ladenMinerAcceleration * SpaceMath.g;
            var tm = config.minerTimeToMineInHours * 3600;

            var timeToReachV = v / a;
            var distanceToReachV = v * v / a / 2; 

            var colonyShipPos = -config.minerInitialOvertake ;
            colonyShipPos += tm * v;
            colonyShipPos += timeToReachV * v; // at acceleration end

            var auperday = 86400 * v / SpaceMath.AU;
            Debug.Log($"Colony ship crosses {auperday} AU per day ({1/auperday} days to cross 1 AU)");

            if (colonyShipPos > distanceToReachV) {

                var distanceToOvercome = colonyShipPos - distanceToReachV;
                // at this point, we can just dispense with v, and consider things from a stationary frame of reference.
                var additionalTHalf = (distanceToOvercome /a).Root(); // this belies some 2s being cancelled out , tHalf = root (2sHalf/a)

                return new CatchUpResult {
                    idealCatchupDistance = distanceToReachV,
                    idealCatchupTime = timeToReachV,
                    leewayWaitTime = 0,
                    rendezvousTime = timeToReachV + 2 * additionalTHalf,
                    rendezvousDistanceFromAsteroid = distanceToReachV + a * additionalTHalf * additionalTHalf,
                };

            } else {
                // we need to spend AT LEAST "distanceToReachV" to accelerate to v. But if we do that, the colony ship will be behind us!
                var extraDistance = distanceToReachV - colonyShipPos;
                
                var extraTime = extraDistance / v;

                return new CatchUpResult {
                    idealCatchupDistance = distanceToReachV,
                    idealCatchupTime = timeToReachV,
                    leewayWaitTime = extraTime,
                    rendezvousTime = timeToReachV,
                    rendezvousDistanceFromAsteroid = distanceToReachV,
                };
            }
        }

        [UnityEditor.MenuItem("Void/SPACE MATH : Rendezvous with miners")]
        static public void RunMinerShipsTests() {

            void RunAndPrint(decimal detectionDistanceInAU, decimal minerAccelerationCapInG, decimal colonyShipVinC, decimal timeToMineInHours, decimal minerAccelerationLaden) {
                var config = new MinerRendezvousConfig { detectionDistanceInAU = detectionDistanceInAU, minerAccelerationCapInG = minerAccelerationCapInG, colonyShipVInC = colonyShipVinC };
                var r = GetRendezvous(config);
                if (r == null) {
                    Debug.Log($"Could not rendezvous at {SpaceMath.FormatDistance(detectionDistanceInAU * SpaceMath.AU)}, with accel {minerAccelerationCapInG}g and colony ship v at {colonyShipVinC:P2}c; ship is going too fast!");
                } else {
                    var str = $"At {SpaceMath.FormatDistance(detectionDistanceInAU * SpaceMath.AU)}, with accel {minerAccelerationCapInG}g and colony ship v at {colonyShipVinC:P2}c. ";
                    // str += $"\nAt {minerAccelerationCapInG}g, miners spend {SpaceMath.FormatTime(r.Value.t1)} accelerating, then {SpaceMath.FormatTime(r.Value.t1 + r.Value.t2)} decelerating, for a total of {SpaceMath.FormatTime(r.Value.fullTime)}";
                    // str += $"\nBreak zero point would be for partial time X to be {SpaceMath.FormatTime(r.Value.breakZeroT1)}";

                    str += $"\n Break-even point would be to release the miners {SpaceMath.FormatTime(r.Value.timeOvertakeZero)} ahead of flyby. With given detection range, we have {SpaceMath.FormatTime(r.Value.detectionTime)} time til overtake";
                    if (r.Value.timeOvertakeZero > r.Value.detectionTime) { 
                        str += $"\n If miners would be released IMMEDIATELY, then the ship would already have made flyby by the time they reach the comet, and would be {SpaceMath.FormatDistance(-r.Value.minerOvertake)} ahead ";
                    } else {
                        str += $"\n If miners would be released IMMEDIATELY, then they would overtake the ship by {SpaceMath.FormatDistance(r.Value.minerOvertake)}";
                    }

                    str += $"\n With constant burn they would touch down after {SpaceMath.FormatTime(r.Value.timeToTouchdown)} of which accel stage would comprise {SpaceMath.FormatTime(r.Value.timeAccelBurn)}";

                    var catchup = GetCatchUp(new CatchUpConfig { 
                        colonyShipVinC = colonyShipVinC,
                        ladenMinerAcceleration = minerAccelerationLaden,
                        minerInitialOvertake = r.Value.minerOvertake,
                        minerTimeToMineInHours = timeToMineInHours
                    });

                    str += $"\nThe miner needs {SpaceMath.FormatTime(catchup.idealCatchupTime)} to accelerate to {colonyShipVinC:P3}c. It will take {SpaceMath.FormatTime(catchup.rendezvousTime)} to rendezvous with the ship at a distance {SpaceMath.FormatDistance(catchup.rendezvousDistanceFromAsteroid)} from asteroid";
                    if (catchup.leewayWaitTime > 0) { str += $"(the miner has time to spare and will wait for {SpaceMath.FormatTime(catchup.leewayWaitTime)})"; }

                    

                    Debug.Log(str);
                }
            }

            //RunAndPrint(1, 1, 0.001m);
            //RunAndPrint(10, 1, 0.001m);
            //RunAndPrint(100, 1, 0.001m);
            // RunAndPrint(detectionDistanceInAU: 100, minerAccelerationCapInG: 1, colonyShipVinC: 0.01m, timeToMineInHours: 100, minerAccelerationLaden: 0.1m);
            RunAndPrint(detectionDistanceInAU: 100, minerAccelerationCapInG: 1, colonyShipVinC: 0.01m, timeToMineInHours: 24 * 40, minerAccelerationLaden: 0.001m);
            RunAndPrint(detectionDistanceInAU: 10, minerAccelerationCapInG: 1, colonyShipVinC: 0.01m, timeToMineInHours: 1000, minerAccelerationLaden: 0.001m);
            RunAndPrint(detectionDistanceInAU: 100, minerAccelerationCapInG: 1, colonyShipVinC: 0.03m, timeToMineInHours: 100, minerAccelerationLaden: 0.01m);
            //void RunAndPrint(decimal vColonyShipInC, decimal emptyMinerAccelerationInG, decimal massRatioOfMiningShip, decimal timeToMineOneMassRatioInHours) {
            //    var data = RunMinerShips(0.1m, 100, 100, 1);    
            //    Debug.Log(
            //        $"At {vColonyShipInC:P2}c, mass ratio {massRatioOfMiningShip} and time-to-mine one mass ration of {timeToMineOneMassRatioInHours}h, time is:" +
            //        $"decel: {SpaceMath.FormatTime(data.tDecel)}, mining: {SpaceMath.FormatTime(data.tDecel)}, catch-up: {SpaceMath.FormatTime(data.tRendezvous)}"
            //    );
            //}

            //RunAndPrint(0.01m, 100, 1000, 1);
        }

        //static internal MinerRendezvousData RunMinerShips(decimal vColonyShipInC, decimal emptyMinerAccelerationInG, decimal massRatioOfMiningShip, decimal timeToMineOneMassRatioInHours) {
        //    // d = distance of colony ship

        //    var q = emptyMinerAccelerationInG * SpaceMath.g;
        //    if (q > SpaceMath.g) q = SpaceMath.g; // we PROHIBIT the miners to decelerate faster than 1g because pilots.
             
        //    var a = q / (massRatioOfMiningShip + 1); // acceleration back to ship
        //    var v = vColonyShipInC * SpaceMath.c;

        //    var dDeceleration = v * v / 2 * q; // distance gained while ships are decelerating
            
        //    var m1 = timeToMineOneMassRatioInHours * 3600; // time to mine 1 mass ratio in seconds
        //    var m  = m1 * massRatioOfMiningShip; // m = mining time total

        //    var dMining = v * m;

        //    var d0 = dDeceleration + dMining;

        //    // and here it gets tricky! the colony ship goes d0 + v * t
        //    // in the same time the miner goes from rest, therefore a * t * t / 2
        //    // so d0 + v * t = a * t * t / 2
            
        //    var term = v * v + 2 * a * d0;
        //    var tRendezvous = (  v + (decimal)System.Math.Sqrt((double)term)  ) / a;

        //    return new MinerRendezvousData {
        //        tDecel = v / q,
        //        tMine = m,
        //        tRendezvous = tRendezvous
        //    };
        //}

        [UnityEditor.MenuItem("Void/SPACE MATH : time to reach Alpha Centauri")]
        static public void RunTimeToReach() {
            TestProfile(0.1m, 0.0001m);
            TestProfile(0.1m, 0.001m);
            TestProfile(0.1m, 0.01m);
            TestProfile(0.1m, 0.1m);

            TestProfile(0.0005m, 1m); // 0.05% c, fastest man made probe to date
            TestProfile(0.001m, 1m); // 0.1%c
            TestProfile(0.003m, 1m); // 0.3%c
            TestProfile(0.005m, 1m); // 0.5%c
            TestProfile(0.0075m, 1m); // 0.75%c
            TestProfile(0.01m, 1m); // 1% c
            TestProfile(0.03m, 1m); // 3% c
            TestProfile(0.05m, 1m); // 5% c
            TestProfile(0.1m, 1m);  // 10% c

            TestProfile(0.01m, 0.001m); // 1% c
            TestProfile(0.03m, 0.001m); // 3% c
            TestProfile(0.05m, 0.001m); // 5% c
            TestProfile(0.1m, 0.001m);  // 10% c
        }


        
    }
}

