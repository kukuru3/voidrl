using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;

namespace Scanner.Flatship {

    public class Segment {
        public ModuleSlot Slot { get; set; }
        public Ship Owner { get; set; }

        public Vector2 offset;
    }

    [System.Diagnostics.DebuggerDisplay("{ToString()}")]
    public class ModuleSlot {

        public override string ToString() {
            return $"slot `{decl.name}`, sloted item = {Slotted?.declaration.id}";
        }

        public readonly ModuleSlotDeclaration decl;
        public ModuleSlot(ModuleSlotDeclaration decl) { 
            this.decl = decl;
        }

        public Vector2? positionOverride;

        public Module Slotted { get; set; }

        public InModuleHierarchy Parent { get; set; }
    }

    public class Ship : InModuleHierarchy {
        internal void ExecuteSlotting(Module module, ModuleSlot slot) { 
            if (module.inSlot != null) throw new System.InvalidOperationException("Module already slotted");
            if (slot.Slotted != null) throw new System.InvalidOperationException("Slot already occupied");
            slot.Slotted = module;
            module.inSlot = slot;
            SlotsUpdated();
        }

        public event Action OnSlotsUpdated;

        private void SlotsUpdated() {
            OnSlotsUpdated?.Invoke();   
        }
    }

    public class EnsureSpinalSlotsExistAlways {
        Ship ship;
        public void Attach(Ship ship) {
            this.ship = ship;
            ship.OnSlotsUpdated += OnSlotsUpdated;
        }

        private void OnSlotsUpdated() {
            // if there are no blank spinal slots, create one
            var hasBlankSpinalSlots = ship.slots.Any(slot => slot.decl.name == "spinal" && slot.Slotted == null);
            if (!hasBlankSpinalSlots) {
                var rightmostNonblankSpinalSlot = ship.slots.Where(slot => slot.decl.name == "spinal").OrderByDescending(slot => slot.positionOverride.Value.x).First();
                ship.slots.Add(new ModuleSlot(rightmostNonblankSpinalSlot.decl) { positionOverride = rightmostNonblankSpinalSlot.positionOverride.Value + new Vector2(3, 0) });
            }
        }
    }


    public class InModuleHierarchy {
        public List<ModuleSlot> slots;
        public ModuleSlot inSlot; // can be null;

        public InModuleHierarchy Parent => inSlot?.Parent;

        public IEnumerable<Module> Children { get {  
            foreach (var item in slots) if (item.Slotted != null) yield return item.Slotted; 
        } }
    }

    [System.Diagnostics.DebuggerDisplay("{ToString()}")]
    public class Module : InModuleHierarchy {
        public override string ToString() {
            // var slottedStr = inSlot == null ? "not in slot" : $"in slot `{inSlot}` of `{inSlot?.Parent}`";
            return $"{declaration.id}, ";
        }

        public readonly ModuleDeclaration declaration;
        public Module(ModuleDeclaration declaration) {
            this.declaration = declaration;
            try { 
                slots = declaration.moduleSlots.Select(x => new ModuleSlot(x)).ToList();
            } catch (Exception e) {
                throw;
            }
        }
    }

    public class RuleRepo {
        public List<ModuleDeclaration> moduleDeclarations = new();

        internal ModuleDeclaration? GetModule(string moduleID) {
            var md = moduleDeclarations.FirstOrDefault(x => x.id == moduleID);
            if (string.IsNullOrWhiteSpace(md.id)) return null;
            return md;
        }
    }

    public struct ModuleSlotDeclaration {
        public string name;
        public Vector2 relativePosition;
        public List<string> allowedTags;
    }
    public struct ModuleDeclaration {
        public string id;
        public ModuleSlotDeclaration[] moduleSlots;
        public List<string> tags;
    }

    public class ShipHardcoder {

        RuleRepo rules = new();
        Ship ship;
        public Ship CreateHardcodedShip() {

            CreateModuleDeclarations();
            var smallSpinalModuleDecl = rules.GetModule("small-spinal-module").Value;

            ship = new Ship { };
            new EnsureSpinalSlotsExistAlways().Attach(ship);

            ship.slots = new() {
                new ModuleSlot(new ModuleSlotDeclaration() { name = "spinal", allowedTags = new List<string>() { "spinal" } }) {
                    positionOverride = new Vector2(0, 0),
                },
            };

            var spine = new Module(smallSpinalModuleDecl) { };
            ship.ExecuteSlotting(spine, ship.slots[0]);

            GenerateModuleInSitu("daedalus-engine", spine);
            GenerateModuleInSitu("daedalus-engine", spine);
            GenerateModuleInSitu("daedalus-engine", spine);


            spine = new Module(smallSpinalModuleDecl) { }; ship.ExecuteSlotting(spine, ship.slots[1]);

            GenerateModuleInSitu("radiator-large", spine);
            GenerateModuleInSitu("reactor-core", spine);
            GenerateModuleInSitu("radiator-large", spine);

            spine = new Module(smallSpinalModuleDecl) { }; ship.ExecuteSlotting(spine, ship.slots[2]);
            
            GenerateModuleInSitu("crio-pumps", spine);
            GenerateModuleInSitu("storage", spine);
            GenerateModuleInSitu("storage", spine);

            spine = new Module(smallSpinalModuleDecl) { }; ship.ExecuteSlotting(spine, ship.slots[2]);
            GenerateModuleInSitu("habitation", spine);
            GenerateModuleInSitu("habitation", spine);
            GenerateModuleInSitu("hydroponics", spine);

            return ship;
        }

        private void GenerateModuleInSitu(string moduleID, ModuleSlot moduleSlot) { 
            var mdecl = rules.GetModule(moduleID);
            if (!mdecl.HasValue) throw new System.InvalidOperationException($"Module `{moduleID}` not found");
            ship.ExecuteSlotting(new Module(mdecl.Value), moduleSlot);
        }

        private void GenerateModuleInSitu(string moduleID, Module parentModule) {
            var mdecl = rules.GetModule(moduleID);
            if (!mdecl.HasValue) throw new System.InvalidOperationException($"Module `{moduleID}` not found");
            foreach (var slot in parentModule.slots) {
                if (slot.Slotted != null) continue;
                if (Compatible(slot, mdecl.Value)) {
                    ship.ExecuteSlotting(new Module(mdecl.Value), slot);
                    return;
                }
            }
            throw new System.InvalidOperationException($"No compatible slot found for module `{moduleID}`");
        }

        private bool Compatible(ModuleSlot slot, ModuleDeclaration mdecl) { 
            return slot.decl.allowedTags.Intersect(mdecl.tags).Any();
        }

        private void CreateModuleDeclarations() { 

            rules.moduleDeclarations.Add(new ModuleDeclaration {
                id = "daedalus-engine",
                tags = new() { "engine", "module" },
                moduleSlots = new ModuleSlotDeclaration[0]
            });

            rules.moduleDeclarations.Add(new ModuleDeclaration {
                id = "small-spinal-module",
                tags = new() { "spinal" },
                moduleSlots = new[] {
                    new ModuleSlotDeclaration {
                        name = "module 1",
                        allowedTags = new() { "module" },
                        relativePosition = new Vector2(0, -1),
                    },

                    new ModuleSlotDeclaration {
                        name = "module 2",
                        allowedTags = new() { "module" },
                        relativePosition = new Vector2(0, 0),
                    },

                    new ModuleSlotDeclaration {
                        name = "module 3",
                        allowedTags = new() { "module" },
                        relativePosition = new Vector2(0, 1),
                    },

                },
            });
        }
    }
}