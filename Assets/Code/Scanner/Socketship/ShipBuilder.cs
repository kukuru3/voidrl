using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Scanner.Socketship {

    public class Database {
        internal List<PartDeclaration> parts;
        internal PartDeclaration GetPart(string id) => parts.Find(p => p.name == id);
    }

    public class ShipBuilder {

        public Database database;
        public Ship ship;        
        public Part ghostPart;

        public ShipBuilder()
        {
            database = new(); database.parts = new(); database.parts.AddRange(Hardcoder.HardcodePartDeclarations());
            ship = new Ship();
            ship.ConnectionMade += ConnectionMade;
        }

        public event Action<Part> AddedRootPart;
        public event Action<Connexion> ConnectionMade;

        public void GenerateShipPart(string part) {
            var pd = database.GetPart(part);
            if (pd == null) throw new InvalidOperationException($"no id `{part}`");
            var pp = new Part(pd, ship);
            ship.rootParts.Add(pp);
            AddedRootPart?.Invoke(pp);
        }

        public bool CanConnect(Contact a, Contact b) {
            if (a == null || b == null) return false;
            if (a.decl is PlugDecl && b.decl is SocketDecl) return CanConnectTrue(a, b);
            if (b.decl is SocketDecl && a.decl is PlugDecl) return CanConnectTrue(b, a);
            return false;
        }

        private bool CanConnectTrue(Contact plug, Contact socket) {
            var plugD = (PlugDecl)plug.decl;
            foreach (var pc in plugD.plugCriteria)
                if (!pc.Pass(plug, socket)) 
                    return false;
            
            return true;
        }

        public IEnumerable<Contact> GetAllShipContacts() {
            var allParts = ship.ListAllParts();
            return allParts.SelectMany(p => p.contacts);
        }

        public IEnumerable<Contact> GetShipPlugs() {
            foreach (var c in GetAllShipContacts()) 
                if (c.decl is PlugDecl && c.connection == null) yield return c;
        }

        public IEnumerable<Contact> GetFreeShipSockets() {
            foreach (var c in GetAllShipContacts())
                if (c.decl is SocketDecl && c.connection == null) yield return c;
        }

        public bool IsRoot(Part part) {
            if (part.ship == null) return true;
            return part.ship.rootParts.Contains(part);
        }

        public IEnumerable<ContactMatch> ListContactMatches(Part phantomPart) {
            var phantomPartPlugs = phantomPart.contacts.Where(c => c.decl is PlugDecl && c.connection == null);

            foreach (var socket in GetFreeShipSockets()) {
                foreach (var plug in phantomPartPlugs) {
                    if (CanConnect(plug, socket)) yield return new ContactMatch() { plug = plug, socket = socket };
                }
            }
        }

        public IEnumerable<ContactMatch> ListAllPossibleContactMatches() {
            foreach (var blueprint in database.parts) {
                var phantomPart = new Part(blueprint, null);
                foreach (var match in ListContactMatches(phantomPart)) {
                    yield return match;
                }
            }
        }
    }

    public class ContactMatch {
        public Contact plug;
        public Contact socket;
    }

    public class Ship
    {
        internal List<Part> rootParts = new();
        internal List<Connexion> connections = new();

        public event Action<Connexion> ConnectionMade;

        public void Connect(Contact plugSide, Contact socketSide) {
            if (plugSide.connection != null) throw new InvalidOperationException("Reconnect disallowed");
            if (socketSide.connection != null) throw new InvalidOperationException("Reconnect disallowed");


            var connexion = new Connexion(plugSide, socketSide);
            plugSide.connection = connexion;
            socketSide.connection = connexion;
            connections.Add(connexion);

            plugSide.part.ship = this;

            ConnectionMade?.Invoke(connexion);
        }

        public List<Part> ListAllParts() {
            var list = new List<Part>();

            var q = new Queue<Part>();
            foreach (var p in rootParts) q.Enqueue(p);
            while (q.Count > 0) {
                var part = q.Dequeue();
                list.Add(part);

                var outgoingSockets = part.contacts.Where(c => c.decl is SocketDecl && c.connection != null);
                foreach (var s in outgoingSockets) {
                    q.Enqueue(s.connection.plug.part);
                }
            }

            return list;
        }
    }

    public class Part {

        public Vector2 rootOffset;
        public readonly PartDeclaration declaration;
        public Ship ship; // can be modified!

        internal List<Contact> contacts = new();

        public Contact FindAttachedPlug() {
            foreach (var c in contacts) if (c.decl is PlugDecl && c.connection != null) return c;
            return default;
        }

        public int PlugDepth() {
            if (ship == null) return default;
            var plug = FindAttachedPlug();
            if (plug == null) return 0;            
            return plug.connection.socket.part.PlugDepth() + 1;
            
        }

        public Vector2 ResultingPosition() {
            if (ship == null) return default;
            var plug = FindAttachedPlug();
            if (plug == null) {
                return rootOffset;
            } else {
                var socket = plug.connection.socket;
                var parentPartPosition = socket.part.ResultingPosition();
                // Parent + parent's socket offset = this pos + my plug offset
                // therefore, 
                var myPos = parentPartPosition + socket.decl.offset - plug.decl.offset;
                return myPos;
                // return plug.connection.socket.part.ResultingPosition() + plug.decl.offset;
            }
        }

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

    public class Connexion {
        public readonly Contact plug;
        public readonly Contact socket;

        public Connexion(Contact plug, Contact socket) {
            this.plug = plug;
            this.socket = socket;
        }
    }

    public class Contact {
        public readonly ContactDecl decl;
        public readonly Part part;
        internal Connexion connection;

        public override string ToString() {
            if (decl is PlugDecl plugD) return $"{part.declaration.name}: Plug {plugD.tags.FirstOrDefault()}";
            if (decl is SocketDecl sockD) return $"{part.declaration.name}: Socket {sockD.tags.FirstOrDefault()}";
            return "unknown";
        }

        public Contact(ContactDecl decl, Part part) {
            this.decl = decl;
            this.part = part;
        }
    }
}
