using System.Collections.Generic;
using System.Linq;
using Core.h3x;
using K3.Hex;
using UnityEngine;

namespace Scanner.HexShip {
    // for now, just one root per frame.
    // later, possibly more, with branches etc.
    public class ConstructibleRoot : MonoBehaviour {
        Hex3SparseGrid<ConstructibleModuleTile> grid = new Hex3SparseGrid<ConstructibleModuleTile>();
    }

    [System.Serializable]
    public struct H3Ref {
        [SerializeField] internal int q;
        [SerializeField] internal int r;
        [SerializeField] internal int z;
    } 

    // ( -r, 0) => goingup

    public class Prohibition {
        internal Hex3 foo;
    }

    public class ConstructibleModuleDeclaration {
        internal string name;
        internal string[] tags;
        internal List<Hex3> tiles = new();
        internal List<AttachmentRule> attachments = new();
    }


    public class AttachmentRule {
        internal Hex3 direction;
        internal List<string> requiredTags;
        internal List<string> prohibitedTags;
    }

    public static class HexagonalPlayground {
        static AttachmentRule TagRule(this Hex3 direction, string requiredTags) {
            return new AttachmentRule {
                direction = direction,
                requiredTags = requiredTags.Split(',').ToList(),
            };
        }
        static internal Hex3 Inflate(this Hex hex, int z = 0) => new Hex3(hex, z);
        static internal IEnumerable<Hex3> Inflate(this IEnumerable<Hex> hexes, int z = 0) => hexes.Select(h => h.Inflate(z));

        static List<AttachmentRule> WithTags(this IEnumerable<Hex3> directions, string requiredTags) => directions.Select(foo => TagRule(foo, requiredTags)).ToList();

        static string[] ToTagList(this string commaSeparated) => commaSeparated.Split(',', System.StringSplitOptions.RemoveEmptyEntries);

        static public IEnumerable<ConstructibleModuleDeclaration> DeclareModules() {
            var allNeighbours = Hex3Util.DirectNeighbours((0,0,0)).ToList();
            var hexNeighbours = Hexes.Neighbours.Inflate(0);
            var axialNeighbours = new List<Hex3> { (0,0,-1), (0,0,1)};

            var structureModule = new ConstructibleModuleDeclaration {
                name = "Structure", 
                tags = "structure".ToTagList(),
                tiles = new List<Hex3> { (0,0,0) },
                attachments = allNeighbours.WithTags(""),
            };

            yield return structureModule;

            var spineModule = new ConstructibleModuleDeclaration {
                name = "Spine",
                tags = "spine".ToTagList(),
                tiles = new List<Hex3> { (0,0,0) },
                attachments = axialNeighbours.WithTags("spine")
            };
            spineModule.attachments.AddRange(hexNeighbours.Select(h => new AttachmentRule() {direction = h, prohibitedTags = { "spine" }  }));
            
            yield return spineModule;  
            
            var engineModule = new ConstructibleModuleDeclaration {
                name = "Engine",
                tags = "engine".ToTagList(),
                tiles = new List<Hex3> { (0,0,0) },
                attachments = axialNeighbours.WithTags("engine")
            };
        }
    }
}