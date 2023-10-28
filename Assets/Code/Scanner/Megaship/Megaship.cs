using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Scanner.Megaship {

    // a megaship has structural Modules. 
    // facilities inside structural modules may also be shown as slots

    // a megaship is a graph where Modules are nodes and Plugs are edges
    // megaship also has a transform root.

    internal class Megaship : MonoBehaviour {
        internal List<Connection> connections = new();

        internal IEnumerable<Module> ListModules() {
            return connections.SelectMany(c => new[] { c.A, c.B }).Distinct();
        }
    }

    class Connection {
        public Plug a;
        public Plug b;

        public Connection(Plug a, Plug b) {
            this.a = a;
            this.b = b;
        }

        public bool DoesConnect(Plug a, Plug b) => this.a == a && this.b == b || this.a == b && this.b == a;

        public bool DoesConnect(Module m) { 
            return a.Module == m || b.Module == m;
        }

        public int ConnectionIndexOf(Plug p) {
            if (a == p) return 1;
            if (b == p) return 2;
            return 0;
        }

        public Plug GetPlugOf(Module m) {
            if (a.Module == m) return a;
            if (b.Module == m) return b;
            return null;
        }
            
        public Module A => a.Module;
        public Module B => b.Module;
    }

    internal static class ShipBuilder {
        public static void Connect(this Megaship ship, Plug a, Plug b) {            
            if (a.Connection != null) throw new System.Exception($"{a} already connected");
            if (b.Connection != null) throw new System.Exception($"{b} already connected");
            var c = ship.FindConnection(a, b);
            if (c != null) throw new System.Exception("Connection already exists");
            c = new Connection(a, b);
            ship.connections.Add(c);
            a.OnConnect(c);
            b.OnConnect(c);
        }

        public static void RemoveConnection(this Megaship ship, Connection c) {
            if (!ship.connections.Remove(c)) throw new System.Exception("Connection not found");
            c.a.OnDisconnect();
            c.b.OnDisconnect();
            ship.connections.Remove(c);
        }

        public static Connection FindConnection(this Megaship ship, Plug a, Plug b) {
            foreach (var c in ship.connections) {
                if (c.DoesConnect(a,b)) return c;
            }
            return null;
        }

        public static bool CanConnect(Plug a, Plug b) {
            if (a.PType == Plug.Polarity.None && b.PType == Plug.Polarity.None) return true;
            if (a.PType == Plug.Polarity.Out || b.PType == Plug.Polarity.In) return true;
            if (a.PType == Plug.Polarity.In || b.PType == Plug.Polarity.Out) return true;
            return false;
        }
    }

}
