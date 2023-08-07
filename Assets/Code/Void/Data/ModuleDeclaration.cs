using System.Collections.Generic;
using UnityEngine;

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

    //public class StructureDeclaration : Declaration, IHasCost {        
    //    public Cargo Cost { get; set; }
    //    public StructureSlottingDeclaration providesSlots;
    //    public StructureSlottingDeclaration occupies;
    //}

    public class ItemDeclaration : Declaration, IHasCost {
        public Cargo Cost { get; set; }
    }

    public class DataRepository {
        public List<ItemDeclaration> items = new();
        public List<SymbolDeclaration> resources = new();
        //public List<StructureDeclaration> structures = new();
    }
}
