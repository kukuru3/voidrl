using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Void.ColonyShip;

namespace Void.Data {

    public abstract class Declaration {
        public string id;
    }

    public interface IHasCost {
        Cargo Cost { get; }
    }

    public class SymbolDeclaration : Declaration {
        public string icon;
        public Color color;
    }

    public class StructureSlottingDeclaration {
        public int[] slots;        
        public int GetNumSlots(SlotTypes type) {
            return slots[(int)type];
        }

        public static StructureSlottingDeclaration Build(params SlotTypes[] types) {
            var s = new StructureSlottingDeclaration();
            s.slots = K3.Enums.MapToArray<int, SlotTypes>();
            foreach (var type in types) s.slots[(int)type]++;
            return s;   
        }

        public static StructureSlottingDeclaration Build(params (SlotTypes type, int amount)[] slots) {
            var s = new StructureSlottingDeclaration();
            s.slots = K3.Enums.MapToArray<int, SlotTypes>();
            foreach (var slot in slots) s.slots[(int)slot.type]=slot.amount;
            return s;   
        }

        public static implicit operator StructureSlottingDeclaration(SlotTypes[] types) {
            return Build(types.ToArray());
        }

        public static implicit operator StructureSlottingDeclaration(SlotTypes a) {
            return Build(new[] { a });
        }
        
        public static implicit operator StructureSlottingDeclaration((SlotTypes a, SlotTypes b) t) {
            return Build(new[] { t.a, t.b });
        }
        public static implicit operator StructureSlottingDeclaration((SlotTypes a, int count) t) {
            var s = new StructureSlottingDeclaration();
            s.slots = K3.Enums.MapToArray<int, SlotTypes>();
            s.slots[(int)t.a]=t.count;
            return s;
        }
    }
    
    public class StructureDeclaration : Declaration, IHasCost {        
        public Cargo Cost { get; set; }
        public StructureSlottingDeclaration providesSlots;
        public StructureSlottingDeclaration occupies;

    }

    public class ItemDeclaration : Declaration, IHasCost {
        public Cargo Cost { get; set; }
    }

    public class DataRepository {
        public List<SymbolDeclaration> resources = new();
        public List<StructureDeclaration> structures = new();
        public List<ItemDeclaration> items = new();
    }
}
