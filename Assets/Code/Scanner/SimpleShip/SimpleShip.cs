using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Scanner.SimpleShip {

    public class ColonyShip {
        List<ShipModule> modules = new();
        public ShipModule CreateModule() { 
            var m = new ShipModule();
            modules.Add(m);
            return m;
        }

        public ShipModule GetModule(int index) => modules[index];
    }

    // a MODULE is where the tiles go.
    public class ShipModule {
        public string name;

        List<Tile> tiles = new();

        public IEnumerable<Structure> structures { get {  foreach (var t in tiles) if (t.structure != null) yield return t.structure; } }

        public Tile CreateTile(int x, int y) {
            var t = new Tile(this, new Vector2Int(x, y));
            tiles.Add(t);
            return t;
        }

        public Tile GetTile(int x, int y) {
            var p = new Vector2Int(x, y);
            return tiles.FirstOrDefault(t => t.localPosition == p);
        }

        public Structure CreateStructure(StructureDeclaration decl, int x, int y) {
            var s = new Structure(decl, x, y);
            GetTile(x,y).structure = s;
            return s;
        }
    }

    public class Tile {
        public readonly ShipModule module;
        public readonly Vector2Int localPosition;
        public Structure structure;

        public Tile(ShipModule module, Vector2Int position) {
            this.module = module;
            this.localPosition = position;
        }
    }


    public class Structure { 
        public readonly StructureDeclaration declaration;
        public readonly Vector2Int position;

        public Structure(StructureDeclaration declaration, int x, int y) {
            this.declaration = declaration;
            position = new(x, y);
        }
    }

    public struct PlacementDeclaration {
        public int width;
        public int height;
        public bool canRotate;
        public PlacementDeclaration(int w, int h, bool canRotate = false) {
            width = w;
            height = h;
            this.canRotate = canRotate;
        }
    }

    public static class RuleRepo {
        public static List<StructureDeclaration> structures = new();
    }

    public class StructureDeclaration {
        public string id;
    }

    public class Hardcoder {
        private static ColonyShip currentShip;

        static ShipModule GenerateGridModule(string moduleName, int w, int h, Vector2 offset) {
            var m = currentShip.CreateModule();
            for (var x = 0; x < w; x++) for (var y = 0; y < h; y++) { m.CreateTile(x, y); }
            return m;
        }

        public static ColonyShip CreateHardcodedShip() {
            currentShip = new ColonyShip();

            GenerateGridModule("propulsion", 2, 3, default);
            GenerateGridModule("habitat",    2, 3, Vector2.right * 3);
            GenerateGridModule("operations", 2, 3, Vector2.right * 6);

            FillModule(0, 
                "propulsion",       "storage",
                "",                 "reactor",
                "propulsion",       "storage"
            );

            FillModule(1,
                "habitation",       "",
                "habitation",       "hydroponics",
                "habitation",       ""
            );

            FillModule(2, 
                "hangar",           "field-generator",
                "weapons",          "operations",
                "engineering",      "ouroboros-core"
            );

            return currentShip;
        }

        private static void FillModule(int moduleID, params string[] grid) { 
            var module = currentShip.GetModule(moduleID);
            for (var i = 0; i < grid.Length; i++) {
                var x = i % 2;
                var y = i / 2;
                if (!string.IsNullOrWhiteSpace(grid[i])) {
                    var decl = new StructureDeclaration { id = grid[i] };
                    module.CreateStructure(decl, x, y);
                }
            }
        }

        private static void CreateStructure(int module, int x, int y, string id) {
            var s = RuleRepo.structures.Find(s => s.id == id);
            if (s == null) throw new Exception($"No structure with id {id}");
            var moduleInstance = currentShip.GetModule(module);
            moduleInstance.CreateStructure(s, x, y);
        }

        public static void InitializeStructures() {
            foreach (var id in new[] { 
                "propulsion", "reactor", "storage", "reactor", "field-generator", 
                "ouroboros-core",
                "habitation", "hydroponics", "science", "engineering", "hangar" 
            }) { 
                var s = new StructureDeclaration { id = id };
                RuleRepo.structures.Add(s);
            }
        }


    }
}
