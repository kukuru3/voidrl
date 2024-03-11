using System.Collections.Generic;
using UnityEngine;
using Void.Entities;

namespace Void {
    public class Gameworld {

        List<Entity> entities = new List<Entity>();

        public Entity CreateNewEntity() {
            var e = new Entity();
            entities.Add(e);
            return e;
        }

        public void Destroy(Entity e) {
            if (!entities.Contains(e)) {
                Debug.LogWarning($"Entity {e} not in list");
                return;
            }
            e._HandleImpendingDestruction();
            entities.Remove(e);
        }

        public Gameworld() {           
            
        }
    }

    public class GeneralTester {
        public static string ReturnFoo() => "foo";
        public static string ReturnBar() => "bar";
    }
}
