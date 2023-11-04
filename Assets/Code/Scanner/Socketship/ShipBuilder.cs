using System;
using System.Collections.Generic;

namespace Scanner.Socketship {
    public class ShipBuilder {

        public Ship _currentShip;
        List<PartDeclaration> database;

        public ShipBuilder()
        {
            database = new(); database.AddRange(Hardcoder.HardcodePartDeclarations());
        }
    }

    public class Ship
    {
        internal List<Part> parts = new();
    }

    public class Part {
        public readonly PartDeclaration declaration;
        public Ship ship; // can be modified!

        internal List<Contact> contacts = new();

        public Part(PartDeclaration decl, Ship ship) {
            declaration = decl;
            this.ship = ship;
            GenerateContactInstances();
        }

        private void GenerateContactInstances() {
            foreach (var cdecl in declaration.Contacts) {
                var c = new Contact(cdecl, this);
                contacts.Add(c);
            }
        }
    }

    public class Contact {
        public readonly ContactDecl decl;
        public readonly Part part;

        public Contact(ContactDecl decl, Part part) {
            this.decl = decl;
            this.part = part;
        }
    }
}
