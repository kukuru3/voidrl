using System.Collections.Generic;

namespace Void.Colony {
    class Module {
        public ModuleDeclaration declarations;
    }

    class ColonyShip {
        List<Section> sections = new();
    }

    class Section {
        List<Module> modules = new();
    }


    struct ModuleDeclaration {
        public string id;
    }
}
