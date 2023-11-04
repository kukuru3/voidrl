using System.Collections.Generic;
using UnityEngine;

namespace Scanner.Socketship {
    public class PartDeclaration {
        public string name;
        List<ContactDecl> contacts = new();
        public IEnumerable<ContactDecl> Contacts => contacts;

        public void Register(ContactDecl decl) => contacts.Add(decl);
    }

    public abstract class ContactDecl {
        public Vector2 offset;
        internal HashSet<string> tags = new();
    }

    // for a part to be inserted, its PLUG needs to go into a SOCKET

    public class SocketDecl : ContactDecl {
        
    }

    public class PlugDecl : ContactDecl {
        public List<IPlugCriterion> plugCriteria = new();
    }

    public interface IPlugCriterion {
        bool Pass(PlugDecl plug, SocketDecl socket);
    }

    class BothHaveTag : IPlugCriterion {
        public string tag;

        public bool Pass(PlugDecl plug, SocketDecl socket) => plug.tags.Contains(tag) && socket.tags.Contains(tag);
    }

    static internal class Extensions {

        public static PlugDecl AddPlug(this PartDeclaration decl) {
            var plug = new PlugDecl {
                plugCriteria = new List<IPlugCriterion>()
            };
            decl.Register(plug);
            return plug;
        }

        public static SocketDecl AddSocket(this PartDeclaration decl) {
            var socket = new SocketDecl();
            decl.Register(socket);
            return socket;
        }

        public static PlugDecl WithCriterion(this PlugDecl contact, IPlugCriterion crit) {
            contact.plugCriteria.Add(crit);
            return contact;
        }
        public static T WithOffset<T>(this T contact, Vector2 offset) where T : ContactDecl {
            contact.offset = offset;
            return contact;
        }
        public static T WithOffset<T>(this T contact, float x, float y) where T : ContactDecl {
            contact.offset = new Vector2(x,y);
            return contact;
        }

        public static T WithTag<T>(this T contact, string tag) where T : ContactDecl { contact.tags.Add(tag); return contact; }
        public static T WithTags<T>(this T contact, IEnumerable<string> tags) where T : ContactDecl { contact.tags.UnionWith(tags); return contact; }
        public static T WithTags<T>(this T contact, params string[] tags) where T : ContactDecl { contact.tags.UnionWith(tags); return contact; }

        public static PartDeclaration CreatePart(this IList<PartDeclaration> list, string name) {
            var pd = new PartDeclaration() { name = name }; 
            return pd;
        }

    }

    public static class Hardcoder {
        public static IEnumerable<PartDeclaration> HardcodePartDeclarations() {

            var parts = new List<PartDeclaration>();

            var spine = parts.CreatePart("Spine");
            spine.AddPlug()
                .WithTags("spine-ext")
                .WithCriterion(new BothHaveTag() { tag = "spine-ext" })
                .WithOffset(1, 0);

            spine.AddSocket()
                .WithTags("spine-ext")
                .WithOffset(-1, 0);

            spine.AddSocket()
                .WithTag("spine-attach")
                .WithOffset(0,0);

            
            var engineBlock = parts.CreatePart("Engine block");
            engineBlock.AddSocket()
                .WithTags("spine-ext")
                .WithOffset(1, 0);

            
            var meteorShield = parts.CreatePart("Meteor Shield");
            meteorShield.AddPlug()
                .WithOffset(-1, 0)
                .WithTags("spine-ext")
                .WithCriterion(new BothHaveTag() { tag = "spine-ext" });

            // small ring
            var smallRing = parts.CreatePart("Small Ring");
            smallRing.AddPlug()
                .WithOffset(0, 0)
                .WithTags("spine-attach")
                .WithCriterion(new BothHaveTag() { tag = "spine-attach"});

            smallRing.AddSocket()
                .WithTag("ring-shielding")
                .WithOffset(0, 0);

            smallRing.AddSocket()
                .WithTag("ring-surface")
                .WithOffset(0,1);

            for (var i = 0; i < 6; i++) 
                smallRing.AddSocket().WithOffset(0,-i).WithTags("facility");

            // radiators:
            var radiator = parts.CreatePart("Radiator");
            radiator.AddPlug()
                .WithTag("ring-surface")
                .WithCriterion(new BothHaveTag() { tag = "ring-surface"})
                .WithOffset(0, 0);

            return parts;
        }
    }
}
