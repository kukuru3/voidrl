using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Scanner.Megaship {

    internal class Module : MonoBehaviour {
        [field:SerializeField] public string Name { get; private set; }
        internal Ship Ship { get; set; }
    }

    internal static class ModuleUtilities {
        public static IEnumerable<Module> AllConnectedModules(Module m) {
            if (m.Ship == null) return Enumerable.Empty<Module>();

            HashSet<Module> allOthers = new();
            foreach (var contact in m.Ship.Contacts) {
                if (contact.ModuleParticipatesInContact(m)) {
                    allOthers.UnionWith(contact.OtherModulesInContact(m));
                }
            }
            return allOthers;
        }

        public static bool ModuleParticipatesInContact(this Contact c, Module m) {
            foreach (var link in c.links) {
                if (link.a.Module == m) return true;
                if (link.b.Module == m) return true;
            }
            return false;
        }

        public static IEnumerable<Module> OtherModulesInContact(this Contact c, Module m) {
            var others = new HashSet<Module>();
            foreach (var link in c.links) {
                if (link.a.Module == m) others.Add(link.b.Module);
                if (link.b.Module == m) others.Add(link.a.Module);
            }
            return others;
        }
    }


    internal enum Polarities {
        Male,
        Female,
        TwoWay,
    }

    internal interface ILinkable {
        IEnumerable<Contact> ParticipatesInContacts { get; }
        Module Module { get; }
    }

    // a Point Linkable
    internal class Plug : MonoBehaviour, ILinkable {
        [field:SerializeField] public Polarities Polarity { get; private set; }

        public IEnumerable<Contact> ParticipatesInContacts => throw new NotImplementedException();

        public Module Module { get;set; }
    }

    // a group of one or more LINKS, connecting any number of modules in between
    internal class Contact {
        public readonly Link[] links;
        public Contact(IEnumerable<Link> links) {
            this.links = links.ToArray();
        }
    }

    // any connection between two LINKABLES
    internal class Link {
        internal readonly ILinkable a; 
        internal readonly ILinkable b;
        public Link(ILinkable a, ILinkable b) {
            this.a = a;
            this.b = b;
        }
    }
    

    internal class Ship : MonoBehaviour {

        List<Module> rootModules = new();
        List<Contact> contacts = new();
        internal IEnumerable<Contact> Contacts => contacts;

        List<Module> resolvedModuleList = null;

        public IEnumerable<Module> AllShipModules() {
            if (resolvedModuleList == null) ResolveModuleList();
            return resolvedModuleList;
        }

        public void AddRootModule(Module m) {
            if (!rootModules.Contains(m)) rootModules.Add(m);
            InvalidateModuleList();
        }

        public void AddContact(Contact c) {
            if (!contacts.Contains(c)) contacts.Add(c);
            InvalidateModuleList();
        }

        public void RemoveContact(Contact c) {
            if (contacts.Remove(c))
                InvalidateModuleList();
        }

        private void InvalidateModuleList() => resolvedModuleList = null;

        private void ResolveModuleList() {
            var closedList = new HashSet<Module>(rootModules);
            var queue = new Queue<Module>(rootModules);

            var l = new List<Module>();
            while (queue.Count > 0) {
                var activeModule = queue.Dequeue();
                l.Add(activeModule);
                var connectedToThisModule = ModuleUtilities.AllConnectedModules(activeModule);
                foreach (var o in connectedToThisModule) {
                    if (closedList.Contains(o)) continue;
                    queue.Enqueue(o);
                    closedList.Add(o);
                }
            }
            this.resolvedModuleList = l;
        }
    }
}
