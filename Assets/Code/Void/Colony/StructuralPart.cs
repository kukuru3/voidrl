namespace Void.Colony {

    // a part of the colony. Can be anything from struts to rotating cylinders.
    internal class StructuralPart {
    }

    internal class Slot {

    }

    internal class Module {
        public string ID { get; set; }

        
    }

    internal class ModularShip {
        K3.Collections.SimpleTree<Module> tree = new();

        public ModularShip() {
            tree.CreateRoot(new Module() { ID = "ship root"});
        }
    }

    // root
    //  spine slot
    //  spine slot
    //  spine slot
    //  spine slot
    //  spine slot
    //  spine slot

}
