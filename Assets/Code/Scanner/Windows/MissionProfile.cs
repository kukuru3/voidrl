using System.Collections.Generic;
using Core.Calculations;
using Core.Units;
using Scanner.Charting;
using UnityEngine;
using static Core.Units.Mass;

namespace Scanner.Windows {
    internal class MissionProfile : MonoBehaviour {
        [SerializeField] GameObject sliderPrefab;
        [SerializeField] GameObject checkboxPrefab;
        [SerializeField] GameObject selectorPrefab;

        [SerializeField] Vector3 start;
        [SerializeField] Vector3 columnOffset;
        [SerializeField] Vector3 elementOffsetInsideColumn;
        [SerializeField] int elementsPerColumn;

        [SerializeField] TMPro.TMP_Text resultLabel;
        [SerializeField] TMPro.TMP_Text massLabel;
        [SerializeField] Chart massDistribution;

        List<GameObject> elements = new List<GameObject>();

        // private NumberSlider projAreaPerMan;
        // private NumberSlider areaMultiplier;
        private NumberSlider nominalCrew;
        private NumberSlider overpopulation;
        private Selector distanceSelector;
        // private NumberSlider distanceToTarget;
        private Toggle infiniteFuel;

        private NumberSlider massRatioSlider;
        // private NumberSlider propellantMassTotal;

        private Selector engineSelector;

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
        // private NumberSlider cosmicRays;

        private NumberSlider engineCount;

        private void Start() {
            GenerateAll();
        }

        private void RecalculateAll() {

            string s = "";
            
            var pplNominal = nominalCrew.NumericValue;
            var pplActual = pplNominal * overpopulation.NumericValue;
            // var surface1 = projAreaPerMan.NumericValue * areaMultiplier.NumericValue;

            var surface1 = 75; // m2 per person, 3x3m room, 2.5m ceiling

            var habitatSurface = surface1 * pplNominal;

            var shieldedFraction = percentageShielded.NumericValue;
            var totalShieldingMass = new Mass((decimal)(passiveShieldingM.NumericValue * habitatSurface * shieldedFraction), MassUnits.t);

            var numEngines = Mathf.FloorToInt(engineCount.NumericValue);

            var isCustomEngine = engineSelector.Selected.data == null;
            EngineDeclaration engineParams = default;

            bool canTweakPropFlow = false;

            

            if (engineSelector.Selected.data is EngineDeclaration decl) {
                engineParams = decl;
            } else {
                engineParams.propellantFlow = propellantFlow.NumericValue;
                engineParams.propellantFlowVariableBonus = 0f;
                engineParams.exhaustVelocity = exhaustVelocity.NumericValue;
                engineParams.engineMass = engineMass.NumericValue;
                engineParams.heatFactor = engineHeatFactor.NumericValue;
            }

            if (!isCustomEngine && (engineParams.propellantFlowVariableBonus > float.Epsilon)) {
                propellantFlow.min = engineParams.propellantFlow;
                propellantFlow.max = engineParams.propellantFlowVariableBonus;
                canTweakPropFlow = true;
            }

            foreach (var e in new[] { propellantFlow, exhaustVelocity, engineMass, engineHeatFactor }) {
                e.gameObject.SetActive(isCustomEngine);
            }

            if ( canTweakPropFlow ) { 
                propellantFlow.gameObject.SetActive(true);
                engineParams.propellantFlow = propellantFlow.NumericValue;
            }



            var totalMassOfEngines = new Mass((decimal)engineParams.engineMass * numEngines, MassUnits.t);
            var structureM = new Mass((decimal)structuralMass.NumericValue, MassUnits.kt);
            var dryMassKg = 
                totalMassOfEngines +
                structureM + 
                totalShieldingMass;

            var propellantMass = dryMassKg * (decimal)(massRatioSlider.NumericValue - 1f);

            // var propellantMass = new Mass((decimal)propellantMassTotal.NumericValue, MassUnits.Mt);
            var totalWetMass = dryMassKg + propellantMass;
            
            var distance = new Distance((decimal)(float)distanceSelector.Selected.data, DistanceUnits.LightYear);
            var maxV = new Velocity((decimal)maxToleratedSpeed.NumericValue, VelocityUnits.C);

            var thrustOfSingleEngine = (decimal)(engineParams.exhaustVelocity * engineParams.propellantFlow);

            var thrustOfAllEngines = thrustOfSingleEngine * numEngines;

            var pctDry = totalShieldingMass.ValueSI / dryMassKg.ValueSI;

            var massRatio = totalWetMass.ValueSI / dryMassKg.ValueSI;

            var lnM = massRatio.Ln();

            var thrustPowerOfSingleEngine = (decimal)(engineParams.propellantFlow * engineParams.exhaustVelocity * engineParams.exhaustVelocity / 2);

            massDistribution.ClearEntries();
            massDistribution.AddEntry("Engines", (float)totalMassOfEngines.As(MassUnits.kt),    new Color32(250, 30, 30, 255));
            massDistribution.AddEntry("Structure", (float)structureM.As(MassUnits.kt), new Color32(150, 250, 120, 255));
            massDistribution.AddEntry("Shielding", (float)totalShieldingMass.As(MassUnits.kt), new Color32(55, 130, 130, 255));
            massDistribution.AddEntry("Propellant", (float)propellantMass.As(MassUnits.kt), new Color32(240, 140, 8, 255));

            massLabel.text = $"<color=#122>{dryMassKg.As(MassUnits.Mt):f1}Mt</color>\r\n+\r\n<color=#f81>{propellantMass.As(MassUnits.Mt):f1}Mt</color>";

            
            var bellRadius = (thrustPowerOfSingleEngine / 1000000m * (decimal)engineParams.heatFactor).Root() * 0.13m;

            var a0 = new Accel(thrustOfAllEngines/totalWetMass.ValueSI);

            var Δv = new Velocity((decimal)engineParams.exhaustVelocity * massRatio.Ln());

            var terawattOutput = thrustPowerOfSingleEngine / 10e12m;
            
            s += $"Thrust: {numEngines} x <color=#fc2>{thrustOfSingleEngine/1000:F0}kN</color>; TOTAL = <color=#fc2>{thrustOfAllEngines/1000000:F2}MN</color>; Mass Ratio: <color=#6c0>{massRatio:f}</color>; a0={a0}\r\n";
            s += $"Fp = <color=#f24>{numEngines * terawattOutput:f1}TW</color> ; v<sub>e</sub>=<color=#c40>{new Velocity((decimal)engineParams.exhaustVelocity).As(VelocityUnits.C):p2}c</color>)";
            s += $"Engine bells: {numEngines} x {bellRadius:f0}m\r\n";

            s += $"Δv = {Δv}\r\n";

            s += $"Crew total: {pplActual:f0}\r\n";
            s += $"Radiation Shielding mass: <color=#122>{totalShieldingMass}</color> ({pctDry:P1} of dry mass)\r\n";
            s += $"Total mass: <color=#122>{totalWetMass}</color> (Ship:<color=#122>{dryMassKg}</color> , Propellant:<color=#122>{ propellantMass }</color>)\r\n";
            

            var propFlow = new Core.CustomSIValue((decimal)engineParams.propellantFlow * numEngines, "kg/s");
            var vExhaust = new Velocity((decimal)engineParams.exhaustVelocity);

            var calculation = TravelTimeCalculator.CalculateComplexWithRootFinding(distance, dryMassKg, propellantMass, vExhaust, propFlow, maxV);
            var totalTime = new TimeSI(calculation.TotalTime);
            var turnoverV = new Velocity(calculation.turnoverV);


            var propExpenditure = new Mass((calculation.progradeBurnTime + calculation.retrogradeBurnTime) * propFlow.ValueSI);

            s += $"Total time to reach target <color=#f4c>{distance}</color> away: <size=200><b><color=#cd2>{totalTime}</color></b></size>\r\n";
            
            var leftoverPropellant = (propellantMass.ValueSI - propExpenditure.ValueSI) / propellantMass.ValueSI > 0.02m;

            if (calculation.isBrachistochrone) {
                s+= $"The movement is <color=#a31>Brachistochrone</color> - turnover at <color=#cd2>{new TimeSI(calculation.progradeBurnTime)}</color> at <color=#2af>{turnoverV}</color>\r\n";
            } else {
                s+= $"Acceleration burn: <color=#cd2>{new TimeSI(calculation.progradeBurnTime)}</color>, retro burn <color=#cd2>{new TimeSI(calculation.retrogradeBurnTime)}</color>\r\n";
                s+= $"The ship will coast for <color=#cd2>{new TimeSI(calculation.coastTime)}</color> at <color=#2af>{turnoverV}</color>\r\n";
            }

            if (leftoverPropellant) { 
                s+= $"During this time, <color=#ea3>{propExpenditure}</color> of propellant will be expended, and <color=#ea3>{propellantMass - propExpenditure}</color> remaining\r\n";
            } else { 
                s+= $"During this time, all of <color=#ea3>{propExpenditure}</color> propellant will be expended\r\n";
            }

            var structureShieldingBonus = structureM.ValueSI * 0.6m / (decimal)habitatSurface / 1000;

            var propellantShieldingBonus = 0m;

            if (propellantAsShielding.ToggleState)
                propellantShieldingBonus = propellantMass.ValueSI * 0.5m / (decimal)habitatSurface / 1000; // propellant exhausted during voyage
            
            // dosage:
            // var msvperyear = cosmicRays.NumericValue;

            var msvperyear = 600f;

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

            // projAreaPerMan      = GenerateSlider("habitat PA", 10, 50, 25, "m<sup>2</sup> / person");
            // areaMultiplier      = GenerateSlider("PA->surface", 1, 20, 3, "x", "f1");
            nominalCrew         = GenerateSlider("habitation", 5000, 100000, 50000, "ppl", logarithmic: true);
            overpopulation      = GenerateSlider("sardine", 1f, 5f, 1f, "", "p0");

            // distanceToTarget    = GenerateSlider("distance", 0.05f, 30, 20f, "ly", "f2", logarithmic: true);

            distanceSelector = GenerateSelector();
            distanceSelector.AddItem("Oort cloud", 0.15f);
            distanceSelector.AddItem("1 Light Year", 1f);
            distanceSelector.AddItem("Proxima Centauri", 4.2f);
            distanceSelector.AddItem("Barnard's Star", 	5.96f);
            distanceSelector.AddItem("Sirius", 8.709f);
            distanceSelector.AddItem("Epsilon Eridani", 10.5f);
            distanceSelector.AddItem("Tau Ceti", 11.91f);

            massRatioSlider     = GenerateSlider("m ratio", 1.1f, 8f, 3f, "x", "f1");

            structuralMass      = GenerateSlider("struct m", 50, 1000, 500, "kt", "f0", logarithmic: true);
            maxToleratedSpeed   = GenerateSlider("tolerated v", 0.01f, 0.8f, 0.3f, "c", "p2", logarithmic: true);
            passiveShieldingM   = GenerateSlider("shielding", 0f, 4.5f, 1f, "t/m<sup>2</sup>", "f1");
            propellantAsShielding = GenerateCheckbox("Propellant radshield");
            percentageShielded  = GenerateSlider("pct shielded", 0f, 1f, 0.8f, "", "p0");
            activeShieldingAtt  = GenerateSlider("active Shld", 0, 1, 0, "-", "p0");

            engineCount         = GenerateSlider("num engines", 1, 20, 5, "", "f0");

            engineSelector = GenerateSelector();
            engineSelector.AddItem("Engine: Daedalus", new EngineDeclaration(1e7f, 0.7f, heatFactor: 0.2f, engineMass: 5e4f));
            engineSelector.AddItem("Ouroboros Drive", new EngineDeclaration(4e7f, 0.9f, heatFactor: 0.05f, engineMass: 3e5f, propFlowBonus: 9.1f));
            engineSelector.AddItem("Antimatter beam", new EngineDeclaration(1e8f, 1f, heatFactor: 0.1f,  engineMass: 1e4f));
            engineSelector.AddItem("Custom Engine", null);

            engineMass          = GenerateSlider("engine m", 50, 500000, 50000, "t", "f0", logarithmic: true); 
            exhaustVelocity     = GenerateSlider("exhaust V", 1e4f, 1e8f, 1e7f, "ms<sup>-1</sup>", "G2", logarithmic: true);
            propellantFlow      = GenerateSlider("prop flow", 0.001f, 200, 0.7f, "kg/s", "f5", logarithmic: true); 
            engineHeatFactor    = GenerateSlider("engine heat", 0, 1, 0.2f, "", "p0");


            // cosmicRays          = GenerateSlider("cosmic rays", 100, 2000, 600, "mSv/yr", "f0");

            try { 
                //exhaustVelocity.GetComponent<Slider>().SetValueExternal(0.77f); // D-He3 fusion values
                //propellantFlow.GetComponent<Slider>().SetValueExternal(0.557f);
            } catch (System.Exception) {

            }
        }
        struct EngineDeclaration {
            public EngineDeclaration(float eV, float propFlow, float engineMass = 5000f, float heatFactor = 0.5f, float propFlowBonus = 0f)
            {
                this.exhaustVelocity = eV;
                this.propellantFlow = propFlow;
                this.propellantFlowVariableBonus = propFlowBonus;
                this.engineMass = engineMass;
                this.heatFactor = heatFactor;
            }
            public float exhaustVelocity;
            public float propellantFlow;
            public float propellantFlowVariableBonus;
            public float heatFactor;
            public float engineMass;
        }


        private void Update() {
            var x = -1;
            for (var i = 0; i < elements.Count; i++) {
                var element = elements[i];
                if (element.activeInHierarchy) x++;
                var column   = x / elementsPerColumn;   
                var inColumn = x % elementsPerColumn;
                var pos = start + columnOffset * column + elementOffsetInsideColumn * inColumn;
                element.transform.position = pos;
            }
        }

        Selector GenerateSelector() {
            var go = Instantiate(selectorPrefab, transform);
            var sel = go.GetComponent<Selector>();
            AddElement(go);
            sel.IndexChanged += _ => RecalculateAll();
            return sel;
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
