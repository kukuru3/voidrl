
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

            var solSS = new SubstellarObjectDeclaration() { name = "Sol", type = StellarSubobjects.MainSequenceStar };
            var solSSO = gw.CreateNewEntity();
            solSS.AttachTo(solSSO);
            

            solO.Include(solSSO);

            var substellars = LoadStarmapFromFile();
            var parents = substellars.Where(s => StellarObject.IsStellarRootType(s.type)).ToArray();

            Dictionary<SubstellarObjectDeclaration, StellarObject> lookup = new();

            SubstellarObjectDeclaration TryLocateParent(SubstellarObjectDeclaration child, float tolerance) {
                foreach (var parent in lookup) {
                    if (parent.Value.name == child.name) continue;
                    var d = Vector3.SqrMagnitude(child.galacticPosition - parent.Value.galacticPosition);
                    if (d <= tolerance) {
                        return parent.Key;
                    }
                }    
                return null;
            }

            void CreateNewFrom(SubstellarObjectDeclaration newPrimary) {
                var stellarObjectEntity = gw.CreateNewEntity();
                var so = new StellarObject() { galacticPosition = newPrimary.galacticPosition, name = newPrimary.name };
                so.AttachTo(stellarObjectEntity);
                lookup[newPrimary] = so;
                var subObj = gw.CreateNewEntity();
                newPrimary.AttachTo(subObj);
                so.Include(subObj);

                starmap.Include(stellarObjectEntity);
            }

            void AssignToParent(SubstellarObjectDeclaration item, SubstellarObjectDeclaration primaryOfParent) {
                lookup.TryGetValue(primaryOfParent, out var parent);
                if (parent != null) {
                    var subObj = gw.CreateNewEntity();
                    item.AttachTo(subObj);
                    parent.Include(subObj);
                    lookup[item] = parent;
                } else {
                    Debug.LogWarning($"Could not assign {item.name} to {primaryOfParent.name}");
                }
            }

            foreach (var ss in substellars) {                
                var parent = TryLocateParent(ss, 0.5f);
                if (parent != null) {
                    AssignToParent(ss, parent);
                } else {
                    CreateNewFrom(ss);
                }
            }

            foreach (var e in starmap.ListContainedEntities()) {
                var so = e.Get<StellarObject>();
                if (so.ContainedSubstellars.Count() == 0) continue;
                var namesOfSubstellars = so.ContainedSubstellars.Select(ss => ss.name);
                var prefix = FindCommonPrefix(namesOfSubstellars);
                if (prefix.Length > 0) {
                    so.name = prefix.Trim();
                }
            }

            return starmap;
        }

        static string FindCommonPrefix(IEnumerable<string> samples) {
        
            var commonPrefix = new string(
                samples.First().Substring(0, samples.Min(s => s.Length))
                .TakeWhile((c, i) => samples.All(s => s[i] == c)).ToArray());
            return commonPrefix;
        }

        // this is of course incredibly hardcoded for the time being

        IEnumerable<SubstellarObjectDeclaration> LoadStarmapFromFile() {
            var file = Core.IO.FileOps.GetFile("stellar_neighbours.txt");
            var fi = new FileInfo(file);
            if (!fi.Exists) {
                throw new System.InvalidOperationException($"Did not find galactic neighbourhood file {file}");
            }

            var lines = File.ReadAllLines(fi.FullName);
            foreach (var line in lines) {
                var dataPoints = line.Split(',');

                var so = TryParseSubstellarObject(dataPoints);
                if (so != null) yield return so;
            }
        }


        SubstellarObjectDeclaration TryParseSubstellarObject(string[] datapoints) {
            if (datapoints.Length < 5) return default;
            var name = datapoints[0].Trim();

            var typeStr = datapoints[4].Trim();
            var type = typeStr switch {
                "S" => StellarSubobjects.MainSequenceStar, 
                "WD" => StellarSubobjects.WhiteDwarf, 
                "BD" => StellarSubobjects.BrownDwarf, 
                "PJ" => StellarSubobjects.JovianPlanet, 
                "PN" => StellarSubobjects.NeptunianPlanet, 
                "PT" => StellarSubobjects.TerrestrialPlanet, 
                _ => StellarSubobjects.Unconfirmed
            };

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

            var spectral = datapoints[5].Trim();

            var sequence = ExtractSpectralType(spectral);

            return new SubstellarObjectDeclaration() {
                galacticPosition = xyz,
                name = name,
                type = type,
                starSequence = sequence,
            };
        }

        private StarTypes ExtractSpectralType(string spectralString) {
            if (spectralString.Length < 2) return StarTypes.Unknown;
            var ss = spectralString[0];

            if (spectralString.EndsWith(" V") || spectralString.EndsWith(" Ve")) {
                switch (ss) {
                    case 'O': return StarTypes.BlueMainSequence;
                    case 'B': return StarTypes.BlueMainSequence;
                    case 'A': return StarTypes.WhiteMainSequence;
                    case 'F': return StarTypes.YellowWhiteMainSequence;
                    case 'G': return StarTypes.YellowDwarf;
                    case 'K': return StarTypes.OrangeDwarf;
                    case 'M': return StarTypes.RedDwarf;
                }
            } else {
                 switch (ss) {
                    case 'O': return StarTypes.BlueGiant;
                    case 'B': return StarTypes.BlueGiant;
                    case 'A': return StarTypes.WhiteGiant;
                    case 'F': return StarTypes.YellowWhiteGiant;
                    case 'G': return StarTypes.YellowGiant;
                    case 'K': return StarTypes.OrangeGiant;
                    case 'M': return StarTypes.RedGiant;
                }
            }
            Debug.Log($"Stumped by spectral type `{spectralString}`");
            return StarTypes.Unknown;
        }

        //StellarObject TryParseStellarObject(string[] datapoints) {
        //    if (datapoints.Length < 5) return default;
        //    var name = datapoints[0].Trim();

        //    var xyz = new Vector3();
        //    try { 
        //        xyz = new Vector3( 
        //            float.Parse(datapoints[1].Trim()), 
        //            float.Parse(datapoints[2].Trim()),
        //            float.Parse(datapoints[3].Trim())
        //        );
        //    } catch (FormatException) {
        //        Debug.LogWarning($"Skiping {datapoints[0]} as data invalid");
        //        return default;
        //    }

        //    var type = datapoints[4].Trim();

        //    if (type == "S") return new StellarObject {
        //        name = name,
        //        galacticPosition = xyz,
        //    };

        //    return default;
        //}

    }
    
}