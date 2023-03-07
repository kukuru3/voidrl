using Scanner.AppContext;
using Void.ColonyShip;
using Void.Data;

namespace Scanner.Loading {
    internal class BasicShipBlueprintConstructor: K3.Modules.Component<GameModule> {
        // create spine
        // add the reactor assembly rearmost
        //  add engines to the reactor
        // add vertebrae as appropriate
        // add ring A: 2x habitat, biomass module, administration
        // add ring B (zero-g): heavy industry, hangar bay, cargo bays; zero-g fabricator facility, 
        // add ring C: auxilliary habitation, auxilliary biomass module, water reserve

        // add reservoir assembly, with 4 huge reservoir slots of which 2 are constructed:
        //  - large water reserve
        //  - fuel reserve

        // biomass module consumes energy and OUTPUTS food. Surplus food can be processed into RATIONS; otherwise it is lost.
        // it has a closed ecosystem of biomass and water with little wastage

        // 

    }

    internal class HardcodedDataInitializer : K3.Modules.Component<CoreModule> {
        protected override void Launch() {
            
            var dataRepo = new DataRepository();
            
            dataRepo.resources.Add(new SymbolDeclaration { id = "materials"});
            dataRepo.resources.Add(new SymbolDeclaration { id = "energy"});
            dataRepo.resources.Add(new SymbolDeclaration { id = "biomass"});

            dataRepo.structures.Add(new StructureDeclaration {
                id = "spine", 
                providesSlots = (SlotTypes.Axis, 6)
            });

            dataRepo.structures.Add(new StructureDeclaration { 
                id = "vertebra",
                Cost = "materials:100",
                occupies = SlotTypes.Axis,
            });

            // struts, ring, reactor,

            dataRepo.structures.Add(new StructureDeclaration {
               id = "lateral_strut",
               Cost = "materials:100",
               occupies = SlotTypes.Axis,
               providesSlots = (SlotTypes.Berth, 2)
            });

            dataRepo.structures.Add(new StructureDeclaration {
                id = "ring",
                Cost = "materials:200",
                occupies = SlotTypes.Axis, 
                providesSlots = StructureSlottingDeclaration.Build((SlotTypes.RingInterior, 12), (SlotTypes.RingExterior, 2)),                
            });
                        
            dataRepo.structures.Add(new StructureDeclaration {
                id = "reactor",
                Cost = "materials:8000, fusion_fuel:300",
                occupies = SlotTypes.Axis,
                providesSlots =  StructureSlottingDeclaration.Build((SlotTypes.Engines, 2), (SlotTypes.Facilities, 2)),
            });

            dataRepo.structures.Add(new StructureDeclaration {
               id = "habitat_module",
               Cost = "materials:200,biomass:50",
               occupies = (SlotTypes.RingInterior, 2),
            });

            dataRepo.structures.Add(new StructureDeclaration {
               id = "heavy_industry_module",
               Cost = "materials:300",
               occupies = (SlotTypes.RingInterior, 3),
            });

            dataRepo.structures.Add(new StructureDeclaration {
                id = "biomass_module",
                Cost = "materials:200, water:1000",
                occupies = (SlotTypes.RingInterior, 3),
            });

            dataRepo.structures.Add(new StructureDeclaration {
                id = "fusion_ion_engine",
                Cost = "materials:3200",
                occupies = (SlotTypes.Engines, 1),
            });
        }
    }
}
