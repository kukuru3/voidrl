using System.Collections.Generic;
using Core.Units;
using K3;
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

            var accel0 = thrust / totalWetMass.ValueSI;

            var pctDry = totalShieldingMass.ValueSI / dryMassKg.ValueSI;

            s += $"THRUST: {thrust/1000:F0}kN | {thrust/1000000:F2}MN\r\n";

            s += $"Crew: {pplActual:f0}\r\n";
            s += $"Radiation Shielding mass: {totalShieldingMass} ({pctDry:P1} of dry mass)\r\n";
            s += $"Total mass: {totalWetMass} (Ship:{dryMassKg} , Propellant:{ propellantMass })\r\n";

            s += $"Surface: {habitatSurface:f0}m<sup>2</sup>\r\n";

            if (isInfiniteFuel) {
                var estimate = Void.SpaceMath.EstimateTravelTime(distance.ValueSI, maxV.ValueSI, accel0);
                var t = new TimeSI(estimate.totalTime);
                var d = new Distance(estimate.distance);
                var v = new Velocity(estimate.maxV);
                var ct = new TimeSI(estimate.coastTime);
                var fuelExpenditure = (decimal)propellantFlow.NumericValue;
                var totalFuelSpent = new Mass(t.ValueSI * fuelExpenditure);
                var fractionFuelSpent = totalFuelSpent.ValueSI / (decimal)propellantMass.ValueSI;
                s += $"Brachisto-estimate: {t} to traverse {d}. MaxV={v}. Coast time: {ct}\r\n";
                s += $"Fuel needed: {totalFuelSpent} ({fractionFuelSpent:p1} of initial propellant)\r\n";
            } else {

            }


            var structureShieldingBonus = structureM.ValueSI * 0.6m / (decimal)habitatSurface / 1000;
            
            // dosage:
            var msvperyear = cosmicRays.NumericValue;
            msvperyear *= (float)System.Math.Pow(System.Math.E, -(double)passiveShieldingM.NumericValue - (double)structureShieldingBonus);
            msvperyear *= 1f - activeShieldingAtt.NumericValue;
            s += $"Dosage: {msvperyear:f1}mSv/year\r\n";

            // cancer incidence on Earth: 2*10e-3 per year
            // "1 year lost per Sievert of exposure in a population"
            var mortFactor = msvperyear / 800f; // so we put in 900 as a pessimistic estimate
            var q = mortFactor / (1f + mortFactor); // this can be proven with some trigonometry on the graph basically.
            var baseLifeExpectancy = 80;
            var lifeExpectancy = baseLifeExpectancy * (1f - q);
            s += $"Life expectancy: {lifeExpectancy:f1}\r\n";

            var radiationCancerCasesBruteForce = pplActual * q / baseLifeExpectancy;
            var baselineCancerRate = pplActual / 500;
            var cancerRateIncreaseDueToRadiation = radiationCancerCasesBruteForce / baselineCancerRate;
            s += $"est. radiation cancer cases: {(int)(radiationCancerCasesBruteForce)}/year (+{cancerRateIncreaseDueToRadiation:P0} increase) \r\n";
            
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
            exhaustVelocity     = GenerateSlider("exhaust V", 10000, 50000000, 40000000, "ms<sup>-1</sup>"); exhaustVelocity.logarithmic = true;
            // engineThrust        = GenerateSlider("THRUST", 10, 50000000, 5000000, "N", "f0"); engineThrust.logarithmic = true;
            propellantFlow      = GenerateSlider("prop flow", 0.00001f, 150, 100, "kg/s", "f5"); propellantFlow.logarithmic = true;
            engineHeatFactor    = GenerateSlider("engine heat", 0, 1, 0.2f, "", "p0");

            structuralMass      = GenerateSlider("struct m", 50, 1000, 500, "kt", "f0");

            maxToleratedSpeed   = GenerateSlider("tolerated v", 0.01f, 0.8f, 0.3f, "c", "p2"); maxToleratedSpeed.logarithmic = true;

            passiveShieldingM   = GenerateSlider("shielding", 0f, 4.5f, 1f, "t/m<sup>2</sup>", "f1");
            percentageShielded  = GenerateSlider("pct shielded", 0f, 1f, 0.5f, "", "p0");

            activeShieldingAtt  = GenerateSlider("active Shld", 0, 1, 0, "-", "p0");

            cosmicRays          = GenerateSlider("cosmic rays", 100, 2000, 600, "mSv/yr", "f0");

            // passive shielding mass, t / m2
            // % habitats shielded
            
            // active shielding attenuation factor
            

            // intermediaries and outputs:

            // specific impulse
            // dry mass
            // wet mass
            // (fusion) engine bell radius
            // received dosage per year
            // life expectancy
            // medical cancer healing

            // refuel efficiency critical speed

            
            
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
