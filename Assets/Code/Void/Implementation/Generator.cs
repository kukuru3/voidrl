
using Void.Entities;
using Void.Entities.Components;

namespace Void.Impl {
    public class InitialGenerator {
        public void GenerateInitial() {
            var gw = new Gameworld();
            var ship = gw.CreateNewEntity();

            ship.Attach<Kinetics>();
            ship.Attach<Facing>();
            ship.Attach<ShipLink>();

            var bubbleE = gw.CreateNewEntity();
            var bubble = bubbleE.Attach<TacticalShipBubble>();
            bubble.Include(ship);

            // describes the position and facing of the bubble entity within the frame of reference of VICINITY.
            bubbleE.Attach<Kinetics>(); 
        }
    }
    
}