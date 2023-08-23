﻿using System.Collections.Generic;
using Core.Calculations;
using Core.Units;
using K3;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using static Core.Units.Mass;

namespace Scanner.Windows {
    internal class NumbersPlayground : MonoBehaviour {
        [SerializeField] GameObject sliderPrefab;
        [SerializeField] GameObject checkboxPrefab;

        [SerializeField] Vector3 start;
        [SerializeField] Vector3 columnOffset;
        [SerializeField] Vector3 elementOffsetInsideColumn;
        [SerializeField] int elementsPerColumn;
        [SerializeField] TMPro.TMP_Text resultLabel;

        List<GameObject> elements = new List<GameObject>();

        NumberSlider projAreaPerMan;
        private NumberSlider areaMultiplier;
        private NumberSlider nominalCrew;
        private NumberSlider overpopulation;
        private NumberSlider distanceToTarget;
        private Toggle brachisto;
        private NumberSlider propellantMassTotal;
        private NumberSlider engineMass;
        private NumberSlider exhaustVelocity;
        private NumberSlider propellantFlow;
        private NumberSlider engineHeatFactor;
        private NumberSlider structuralMass;
        private NumberSlider maxToleratedSpeed;
        private NumberSlider passiveShieldingM;
        private NumberSlider percentageShielded;
        private NumberSlider activeShieldingAtt;
        private NumberSlider cosmicRays;

        private void Start() {
            GenerateAll();
        }

        private void RecalculateAll() {

            string s = "";
            
            var pplNominal = nominalCrew.NumericValue;
            var pplActual = pplNominal * overpopulation.NumericValue;
            var surface1 = projAreaPerMan.NumericValue * areaMultiplier.NumericValue;
            var habitatSurface = surface1 * pplNominal;
            var shieldedFraction = percentageShielded.NumericValue;
            var totalShieldingMass = new Mass((decimal)(passiveShieldingM.NumericValue * habitatSurface * shieldedFraction), MassUnits.t);
            var engineM = new Mass((decimal)engineMass.NumericValue, MassUnits.t);
            var structureM = new Mass((decimal)structuralMass.NumericValue, MassUnits.kt);
            var dryMassKg = 
                engineM +
                structureM + 
                totalShieldingMass;

            var propellantMass = new Mass((decimal)propellantMassTotal.NumericValue, MassUnits.Mt);
            var totalWetMass = dryMassKg + propellantMass;

            var isInfiniteFuel = brachisto.ToggleState;

            var distance = new Distance((decimal)distanceToTarget.NumericValue, DistanceUnits.LightYear);
            var maxV = new Velocity((decimal)maxToleratedSpeed.NumericValue, VelocityUnits.C);

            var thrust = (decimal)(exhaustVelocity.NumericValue * propellantFlow.NumericValue);

            var pctDry = totalShieldingMass.ValueSI / dryMassKg.ValueSI;

            var massRatio = totalWetMass.ValueSI / dryMassKg.ValueSI;

            s += $"THRUST: <color=#fc2>{thrust/1000:F0}kN</color> | <color=#fc2>{thrust/1000000:F2}MN</color>(v<sub>e</sub>=<color=#c40>{new Velocity((decimal)exhaustVelocity.NumericValue).As(VelocityUnits.C):p2}c</color>) ; Mass Ratio: <color=#6c0>{massRatio:f}</color>\r\n";
            

            s += $"Crew total: {pplActual:f0}\r\n";
            s += $"Radiation Shielding mass: <color=#122>{totalShieldingMass}</color> ({pctDry:P1} of dry mass)\r\n";
            s += $"Total mass: <color=#122>{totalWetMass}</color> (Ship:<color=#122>{dryMassKg}</color> , Propellant:<color=#122>{ propellantMass }</color>)\r\n";

            var calc = Brachistochrone.CalculateBrachistochrone(dryMassKg, propellantMass, distance, new Velocity((decimal)exhaustVelocity.NumericValue),  new Core.CustomSIValue((decimal)propellantFlow.NumericValue, "kg/s"), isInfiniteFuel);
            var totalTime = calc.burnTimePrograde + calc.burnTimeRetrograde + calc.coastTime;

            var propPercent = calc.propellantExpenditure.ValueSI / propellantMass.ValueSI;
            var collectedPropellant =  calc.propellantExpenditure - propellantMass ;

            s += $"Brachisto-estimate: Accelerating at <color=#129>{calc.acceleration}</color> (with correction factor <color=#36f>{calc.estimatedAccelerationFactor:f2}</color>)),"
              + $"distance <color=#f4c>{distance}</color> reached in <color=#ff0>{totalTime}</color>";

            if (calc.coastTime.ValueSI > 100)
                s += $", of which <color=#ff0>{calc.coastTime}</color> spent coasting at maximum velocity of <color=#2af>{calc.topV}</color>.";
            else 
                s += $", followed by immediate turnover at <color=#2af>{calc.topV}</color>";

            s += "\r\n";

            s+= $"<color=#ea3>{calc.propellantExpenditure} ({propPercent:p0})</color> propellant expended.";

            if (collectedPropellant.ValueSI > 100)
                s+= $" <color=#ea3>({collectedPropellant}</color> will have to be MINED from the Oort cloud along the way)";
            s += "\r\n";

            var structureShieldingBonus = structureM.ValueSI * 0.6m / (decimal)habitatSurface / 1000;
            
            // dosage:
            var msvperyear = cosmicRays.NumericValue;
            msvperyear *= (float)System.Math.Pow(System.Math.E, -(double)passiveShieldingM.NumericValue - (double)structureShieldingBonus);
            // active magnetic shielding:
            msvperyear *= 1f - (activeShieldingAtt.NumericValue * 0.85f); // 85% are charged particles, 15% are neutrons and photons, which aren't affected by active shielding

            var xrays = msvperyear / 0.03f;
            s += $"Each person absorbs: <color=#020>{msvperyear:f1}mSv</color> (or <color=#020>{xrays:f0} chest x-rays</color>) each year.\r\n";

            // cancer incidence on Earth: 2*10e-3 per year
            // "1 year lost per Sievert of exposure in a population"
            var mortFactor = msvperyear / 800f; // so we put in 900 as a pessimistic estimate
            var q = mortFactor / (1f + mortFactor); // this can be proven with some trigonometry on the graph basically.
            var baseLifeExpectancy = 80;
            var lifeExpectancy = (decimal)(baseLifeExpectancy * (1f - q));
            s += $"Life expectancy: {lifeExpectancy:f1}\r\n";

            var radiationCancerCasesBruteForcePerYear = pplActual * q / baseLifeExpectancy;
            var baselineCancerRate = pplActual / 500; 
            var cancerRateIncreaseDueToRadiation = radiationCancerCasesBruteForcePerYear / baselineCancerRate;
            s += $"est. radiation cancer cases: {(int)(radiationCancerCasesBruteForcePerYear)}/year (+{cancerRateIncreaseDueToRadiation:P0} increase) \r\n";
            var yearsInTransit = totalTime.As(TimeUnits.Years);
            var gens = yearsInTransit / 25m;
            s += $"A total of {gens:f0} generations would live and die on the ship. {(gens-1)*(decimal)pplActual:f0} would be born. Of those, {totalTime.As(TimeUnits.Years) * (decimal)radiationCancerCasesBruteForcePerYear:f0} would have died from cancer due to radiation during the voyage.";
            resultLabel.text = s;
            
        }

        void GenerateAll() {

            // INPUT SLIDERS:

            projAreaPerMan      = GenerateSlider("habitat PA", 10, 50, 25, "m<sup>2</sup> / person");
            areaMultiplier      = GenerateSlider("PA->surface", 1, 20, 3, "x", "f1");
            nominalCrew         = GenerateSlider("crew", 5000, 100000, 50000, ""); nominalCrew.logarithmic = true;
            overpopulation      = GenerateSlider("sardine", 1f, 5f, 1f, "", "p0");
            distanceToTarget    = GenerateSlider("distance", 1, 30, 20f, "ly", "f2"); distanceToTarget.logarithmic = true;
            brachisto           = GenerateCheckbox("Infinite fuel");
            propellantMassTotal = GenerateSlider("propellant", 0.1f, 10, 1, "Mt", "f2");
            engineMass          = GenerateSlider("engine m", 1, 50000, 20000, "t", "f0"); engineMass.logarithmic = true;
            exhaustVelocity     = GenerateSlider("exhaust V", 10000, 50000000, 40000000, "ms<sup>-1</sup>", "G2"); exhaustVelocity.logarithmic = true;

            
            // engineThrust        = GenerateSlider("THRUST", 10, 50000000, 5000000, "N", "f0"); engineThrust.logarithmic = true;
            propellantFlow      = GenerateSlider("prop flow", 0.00001f, 150, 100, "kg/s", "f5"); propellantFlow.logarithmic = true;
            engineHeatFactor    = GenerateSlider("engine heat", 0, 1, 0.2f, "", "p0");

            structuralMass      = GenerateSlider("struct m", 50, 1000, 500, "kt", "f0");

            maxToleratedSpeed   = GenerateSlider("tolerated v", 0.01f, 0.8f, 0.3f, "c", "p2"); maxToleratedSpeed.logarithmic = true;

            passiveShieldingM   = GenerateSlider("shielding", 0f, 4.5f, 1f, "t/m<sup>2</sup>", "f1");
            percentageShielded  = GenerateSlider("pct shielded", 0f, 1f, 0.5f, "", "p0");

            activeShieldingAtt  = GenerateSlider("active Shld", 0, 1, 0, "-", "p0");

            cosmicRays          = GenerateSlider("cosmic rays", 100, 2000, 600, "mSv/yr", "f0");

            exhaustVelocity.GetComponent<Slider>().SetValueExternal(0.77f); // D-He3 fusion values
            propellantFlow.GetComponent<Slider>().SetValueExternal(0.52f);
        }



        private void Update() {
            for (var i = 0; i < elements.Count; i++) {
                var element = elements[i];
                var column   = i / elementsPerColumn;
                var inColumn = i % elementsPerColumn;
                var pos = start + columnOffset * column + elementOffsetInsideColumn * inColumn;
                element.transform.position = pos;
            }
        }

        NumberSlider GenerateSlider(string name, float min, float max, float initialSliderValue, string suffix, string format = "f0") {
            var go = Instantiate(sliderPrefab, transform); 
            var sld = go.GetComponent<NumberSlider>();
            sld.GetComponent<Slider>().SetCaption(name);
            sld.min = min; sld.max = max; sld.suffix = suffix; sld.format = format;
            AddElement(go);
            sld.GetComponent<Slider>().SetValueExternal(initialSliderValue.Map(min, max, 0f, 1f));
            sld.GetComponent<Slider>().ValueChanged += _ => RecalculateAll();
            return sld;
        }

        Toggle GenerateCheckbox(string name) {
            var go = Instantiate(checkboxPrefab, transform);
            var chk = go.GetComponent<Toggle>();
            chk.SetCaption(name);
            AddElement(go);
            chk.ValueChanged += RecalculateAll;
            return chk;
        }

        private void AddElement(GameObject go) {
            var index = elements.Count;
            elements.Add(go);
            var column   = index / elementsPerColumn;
            var inColumn = index % elementsPerColumn;

            var pos = start + columnOffset * column + elementOffsetInsideColumn * inColumn;
            go.transform.position = pos;
        }
    }
}
