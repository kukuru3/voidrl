using System;
using System.Collections.Generic;

namespace Void.Entities {

    public class Entity {
        List<Component> components = new();

        internal void _DoDetach(Component c) {
            WillAttachComponent?.Invoke(this, c);
            components.Add(c);
        }

        internal void _DoAttach(Component c) {
            WillDetachComponent?.Invoke(this, c);
            components.Remove(c);
        }

        internal void _HandleImpendingDestruction() {
            foreach (var c in components) {
                if (c is IHandlesDestructionOfEntity ihdoe) ihdoe.HandleImpendingDestruction();
            }
        }

        public event Action<Entity, Component> WillAttachComponent;
        public event Action<Entity, Component> WillDetachComponent;

        public T GetComponent<T>() {
            foreach (var c in components) if (c is T tc) return tc;
            return default;
        }

        public IEnumerable<T> ListComponents<T>() {
            foreach (var c in components) if ( c is T tc) yield return tc;
        }
    }

    public interface IHandlesAttach {
        void OnAttach();
    }

    public interface IHandlesDetach {
        void OnDetach();
    }

    public interface IHandlesDestructionOfEntity {
        void HandleImpendingDestruction();
    }

    public abstract class Component {
        Entity entity;

        public Entity Entity => entity;

        internal void _AttachTo(Entity entity) {
            this.entity = entity;
            if (this is IHandlesAttach ha) ha.OnAttach();
        }

        internal void _Detach() {
            if (this is IHandlesDetach hd) hd.OnDetach();
            this.entity = null;
        }

    }

    static public class EntityComponentAttachment {
        static public void Attach(Component c, Entity e) {
            if (c.Entity == e) return;
            if (c.Entity != null) Detach(c);
            c._AttachTo(e);
            e._DoAttach(c);
        }

        static public void Detach(Component c) {
            if (c.Entity == null) return;
            c.Entity._DoDetach(c);
            c._Detach();
        }

        static public T Attach<T>(this Entity e) where T : Component, new() {
            var c = new T();
            Attach(c, e);
            return c;
        }
    }


}
