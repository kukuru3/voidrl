using System.Collections.Generic;
using Core.Calculations;
using Core.Units;
using K3;
using Scanner.Charting;
using UnityEngine;
using static Core.Units.Mass;

namespace Scanner.Windows {
    internal class MissionProfile : MonoBehaviour {
        [SerializeField] GameObject sliderPrefab;
        [SerializeField] GameObject checkboxPrefab;

        [SerializeField] Vector3 start;
        [SerializeField] Vector3 columnOffset;
        [SerializeField] Vector3 elementOffsetInsideColumn;
        [SerializeField] int elementsPerColumn;

        [SerializeField] TMPro.TMP_Text resultLabel;
        [SerializeField] TMPro.TMP_Text massLabel;
        [SerializeField] AreaChart massDistribution;

        List<GameObject> elements = new List<GameObject>();

        NumberSlider projAreaPerMan;
        private NumberSlider areaMultiplier;
        private NumberSlider nominalCrew;
        private NumberSlider overpopulation;
        private NumberSlider distanceToTarget;
        private Toggle infiniteFuel;
        private NumberSlider propellantMassTotal;
        private NumberSlider engineMass;
        private NumberSlider exhaustVelocity;
        private NumberSlider propellantFlow;
        private NumberSlider engineHeatFactor;
        private NumberSlider structuralMass;
        private NumberSlider maxToleratedSpeed;
        private NumberSlider passiveShieldingM;
        private Toggle propellantAsShielding;
        private NumberSlider percentageShielded;
        private NumberSlider activeShieldingAtt;
        private NumberSlider cosmicRays;

        private NumberSlider engineCount;

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

            var numEngines = Mathf.FloorToInt(engineCount.NumericValue);
            var totalMassOfEngines = new Mass((decimal)engineMass.NumericValue * numEngines, MassUnits.t);
            var structureM = new Mass((decimal)structuralMass.NumericValue, MassUnits.kt);
            var dryMassKg = 
                totalMassOfEngines +
                structureM + 
                totalShieldingMass;

            var propellantMass = new Mass((decimal)propellantMassTotal.NumericValue, MassUnits.Mt);
            var totalWetMass = dryMassKg + propellantMass;

            var distance = new Distance((decimal)distanceToTarget.NumericValue, DistanceUnits.LightYear);
            var maxV = new Velocity((decimal)maxToleratedSpeed.NumericValue, VelocityUnits.C);

            var thrustOfSingleEngine = (decimal)(exhaustVelocity.NumericValue * propellantFlow.NumericValue);
            var thrustOfAllEngines = thrustOfSingleEngine * numEngines;

            var pctDry = totalShieldingMass.ValueSI / dryMassKg.ValueSI;

            var massRatio = totalWetMass.ValueSI / dryMassKg.ValueSI;

            var lnM = massRatio.Ln();

            var thrustPowerOfSingleEngine = (decimal)(propellantFlow.NumericValue * exhaustVelocity.NumericValue * exhaustVelocity.NumericValue / 2);

            massDistribution.ClearEntries();
            massDistribution.AddEntry("Engines", (float)totalMassOfEngines.As(MassUnits.kt),    new Color32(250, 150, 120, 255));
            massDistribution.AddEntry("Structure", (float)structureM.As(MassUnits.kt), new Color32(150, 150, 120, 255));
            massDistribution.AddEntry("Shielding", (float)totalShieldingMass.As(MassUnits.kt), new Color32(55, 100, 100, 255));
            massDistribution.AddEntry("Propellant", (float)propellantMass.As(MassUnits.kt), new Color32(240, 140, 8, 255));

            massLabel.text = $"<color=#122>{totalWetMass.As(MassUnits.Mt):f1}Mt";

            
            var bellRadius = (thrustPowerOfSingleEngine / 1000000m * (decimal)engineHeatFactor.NumericValue).Root() * 0.13m;
            
            s += $"THRUST: {numEngines} x <color=#fc2>{thrustOfSingleEngine/1000:F0}kN</color>; TOTAL = <color=#fc2>{thrustOfAllEngines/1000000:F2}MN</color>; Mass Ratio: <color=#6c0>{massRatio:f}</color>\r\n";
            s += $"Fp = {numEngines} x <color=#f24>{thrustPowerOfSingleEngine:G2}</color> ; v<sub>e</sub>=<color=#c40>{new Velocity((decimal)exhaustVelocity.NumericValue).As(VelocityUnits.C):p2}c</color>)";
            s += $"Engine bells: {numEngines} x {bellRadius:f0}m\r\n";

            s += $"Crew total: {pplActual:f0}\r\n";
            s += $"Radiation Shielding mass: <color=#122>{totalShieldingMass}</color> ({pctDry:P1} of dry mass)\r\n";
            s += $"Total mass: <color=#122>{totalWetMass}</color> (Ship:<color=#122>{dryMassKg}</color> , Propellant:<color=#122>{ propellantMass }</color>)\r\n";
            

            var propFlow = new Core.CustomSIValue((decimal)propellantFlow.NumericValue * numEngines, "kg/s");
            var vExhaust = new Velocity((decimal)exhaustVelocity.NumericValue);

            var calculation = TravelTimeCalculator.CalculateComplexWithRootFinding(distance, dryMassKg, propellantMass, vExhaust, propFlow);
            var totalTime = new TimeSI(calculation.TotalTime);
            var turnoverV = new Velocity(calculation.turnoverV);
            var propExpenditure = new Mass(calculation.isBrachistochrone ? (calculation.progradeBurnTime + calculation.retrogradeBurnTime) * propFlow.ValueSI : propellantMass.ValueSI);

            s += $"Total time to reach target <color=#f4c>{distance}</color> away: <size=200><b><color=#cd2>{totalTime}</color></b></size>\r\n";
            
            if (calculation.isBrachistochrone) {
                s+= $"The movement is <color=#a31>Brachistochrone</color> - turnover at <color=#cd2>{new TimeSI(calculation.progradeBurnTime)}</color> at <color=#2af>{turnoverV}</color>\r\n";
                s+= $"During this time, <color=#ea3>{propExpenditure}</color> of propellant will be expended, and <color=#ea3>{propellantMass - propExpenditure}</color> remaining\r\n";
            } else {
                s+= $"Acceleration burn: <color=#cd2>{new TimeSI(calculation.progradeBurnTime)}</color>, retro burn <color=#cd2>{new TimeSI(calculation.retrogradeBurnTime)}</color>\r\n";
                s+= $"The ship will coast for <color=#cd2>{new TimeSI(calculation.coastTime)}</color> at <color=#2af>{turnoverV}</color>\r\n";
                s+= $"During this time, all of <color=#ea3>{propExpenditure}</color> propellant will be expended\r\n";
            }

            var structureShieldingBonus = structureM.ValueSI * 0.6m / (decimal)habitatSurface / 1000;

            var propellantShieldingBonus = 0m;

            if (propellantAsShielding.ToggleState)
                propellantShieldingBonus = propellantMass.ValueSI / (1m + lnM) * 0.9m / (decimal)habitatSurface / 1000; // propellant exhausted during voyage
            
            // dosage:
            var msvperyear = cosmicRays.NumericValue;
            msvperyear *= (float)System.Math.Pow(System.Math.E, -(double)passiveShieldingM.NumericValue - (double)structureShieldingBonus - (double)propellantShieldingBonus);
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
            nominalCrew         = GenerateSlider("habitation", 5000, 100000, 50000, "ppl", logarithmic: true);
            overpopulation      = GenerateSlider("sardine", 1f, 5f, 1f, "", "p0");
            distanceToTarget    = GenerateSlider("distance", 1, 30, 20f, "ly", "f2", logarithmic: true);

            propellantMassTotal = GenerateSlider("propellant", 0.1f, 10, 1, "Mt", "f2");
            engineCount         = GenerateSlider("num engines", 1, 20, 5, "", "f0");
            engineMass          = GenerateSlider("engine m", 50, 500000, 50000, "t", "f0", logarithmic: true); 
            exhaustVelocity     = GenerateSlider("exhaust V", 1e4f, 1e8f, 1e7f, "ms<sup>-1</sup>", "G2", logarithmic: true);
            propellantFlow      = GenerateSlider("prop flow", 0.001f, 200, 0.7f, "kg/s", "f5", logarithmic: true); 
            engineHeatFactor    = GenerateSlider("engine heat", 0, 1, 0.2f, "", "p0");

            structuralMass      = GenerateSlider("struct m", 50, 1000, 500, "kt", "f0");
            maxToleratedSpeed   = GenerateSlider("tolerated v", 0.01f, 0.8f, 0.3f, "c", "p2", logarithmic: true);
            passiveShieldingM   = GenerateSlider("shielding", 0f, 4.5f, 1f, "t/m<sup>2</sup>", "f1");
            propellantAsShielding = GenerateCheckbox("Propellant radshield");
            percentageShielded  = GenerateSlider("pct shielded", 0f, 1f, 0.5f, "", "p0");
            activeShieldingAtt  = GenerateSlider("active Shld", 0, 1, 0, "-", "p0");

            cosmicRays          = GenerateSlider("cosmic rays", 100, 2000, 600, "mSv/yr", "f0");

            try { 
                exhaustVelocity.GetComponent<Slider>().SetValueExternal(0.77f); // D-He3 fusion values
                propellantFlow.GetComponent<Slider>().SetValueExternal(0.557f);
            } catch (System.Exception) {

            }
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

        NumberSlider GenerateSlider(string name, float min, float max, float initialSliderValue, string suffix, string format = "f0", bool logarithmic = false) {
            var go = Instantiate(sliderPrefab, transform); 
            var sld = go.GetComponent<NumberSlider>();
            sld.GetComponent<Slider>().SetCaption(name);
            sld.min = min; sld.max = max; sld.suffix = suffix; sld.format = format;
            sld.logarithmic = logarithmic;
            AddElement(go);
            sld.SetSliderTFromValue(initialSliderValue);
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
