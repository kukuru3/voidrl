using System;
using System.Collections.Generic;
using UnityEngine;

namespace Void.Entities.Components {
    public class Kinetics : Component {
        public Vector3 position;
        public Vector3 velocity;
    }

    public class Mineable : Component {

    }

    public class Facing : Component { // a comet has velocity, but not facing per se
        public Quaternion rotation;
        public Quaternion spin;
    }

    public class ShipLink : Component {
        
    }

    // hierarchical handle: the ship's presence in VICINITY
    public class TacticalShipBubble : Container {
        
    }

    public abstract class Container : Component {

        List<BelongsToContainer> contains = new();

        public IEnumerable<Entity> ListContainedEntities() { 
            foreach (var link in contains) yield return link.Entity;
        } 

        HashSet<Entity> _belongToMe = null;
        public bool Contains(Entity entity) {
            RebuildCachesIfNeeded();
            return _belongToMe.Contains(entity);
        }

        private void RebuildCachesIfNeeded() {
            if (_belongToMe == null) {
                _belongToMe = new HashSet<Entity>(); foreach (var e in ListContainedEntities()) _belongToMe.Add(e);
            }
        }

        public void Include(Entity e) {
            var existingLink = GetLink(e);
            if (existingLink == null) {
                var link = CreateLink(e);
                contains.Add(link);
                InvalidateCaches();
            }
        }

        private void InvalidateCaches() {
            _belongToMe = null;
        }

        BelongsToContainer CreateLink(Entity e) { 
            var l = new BelongsToContainer(this);
            EntityComponentAttachment.AttachTo(l, e);
            return l;
        }

        BelongsToContainer GetLink(Entity e) {
            foreach (var l in e.ListComponents<BelongsToContainer>()) {
                if (l.container == this) { return l; }
            }
            return null;
        }
    }

    public class BelongsToContainer : Component {
        public readonly Container container;
        public BelongsToContainer(Container container)
        {
            this.container = container;
        }
    }
}
