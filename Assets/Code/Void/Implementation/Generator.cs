
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Void.Entities;
using Void.Entities.Components;

namespace Void.Impl {
    public class InitialGenerator {
        public Gameworld GenerateWorld() {
            var world = new Gameworld();
            var ship = world.CreateNewEntity();

            ship.Attach<Kinetics>();
            ship.Attach<Facing>();
            ship.Attach<ShipLink>();

            var bubbleE = world.CreateNewEntity();
            var bubble = bubbleE.Attach<TacticalShipBubble>();
            bubble.Include(ship);

            // describes the position and facing of the bubble entity within the frame of reference of VICINITY.
            bubbleE.Attach<Kinetics>(); 

            return world;
        }
    }

    public class StarmapGenerator {
        public Starmap GenerateStarmap(Gameworld gw) {
            var starmapEntity = gw.CreateNewEntity();
            var starmap = starmapEntity.Attach<Starmap>();

            var solE = gw.CreateNewEntity();
            var solO = new StellarObject() { name = "Sol" };
            solO.AttachTo(solE);
            starmap.Include(solE);

            foreach (var so in LoadStarmapFromFile()) {
                var e = gw.CreateNewEntity();
                so.AttachTo(e);
                starmap.Include(e);
            }
            return starmap;
        }

        // this is of course incredibly hardcoded for the time being

        IEnumerable<StellarObject> LoadStarmapFromFile() {
            var file = Core.IO.FileOps.GetFile("stellar_neighbours.txt");
            var fi = new FileInfo(file);
            if (!fi.Exists) {
                throw new System.InvalidOperationException($"Did not find galactic neighbourhood file {file}");
            }

            var lines = File.ReadAllLines(fi.FullName);
            foreach (var line in lines) {
                var dataPoints = line.Split(',');
                var so = TryParseStellarObject(dataPoints);
                if (so != null) yield return so;
            }
        }

        StellarObject TryParseStellarObject(string[] datapoints) {
            var name = datapoints[0].Trim();

            var xyz = new Vector3();
            try { 
                xyz = new Vector3( 
                    float.Parse(datapoints[1].Trim()), 
                    float.Parse(datapoints[2].Trim()),
                    float.Parse(datapoints[3].Trim())
                );
            } catch (FormatException) {
                Debug.LogWarning($"Skiping {datapoints[0]} as data invalid");
                return default;
            }

            var type = datapoints[4].Trim();

            if (type == "S") return new StellarObject {
                name = name,
                galacticPosition = xyz,
            };

            return default;
        }

    }
    
}