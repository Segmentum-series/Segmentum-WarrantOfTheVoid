using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using RimWorld.Planet;

namespace seg
{
    public class GenStep_PirateSpawner : GenStep_Scatterer
    {
        public override int SeedPart => 931842770;

        protected override void ScatterAt(IntVec3 loc, Map map, GenStepParams parms, int count = 1)
        {
            var facDef = DefDatabase<FactionDef>.GetNamed("Seg_WOTV_ImperiumPirates", true);
            var faction = Find.FactionManager.FirstFactionOfDef(facDef);

            if (faction == null)
            {
                FactionGeneratorParms facparms = new FactionGeneratorParms
                {
                    factionDef = facDef
                };
                faction = FactionGenerator.NewGeneratedFaction(facparms);
                Find.FactionManager.Add(faction);
            }

            var pumps = map.listerBuildings.AllBuildingsNonColonistOfDef(ThingDef.Named("PollutionPump"));
            foreach (var pump in pumps)
            {
                IntVec3 spawnLoc = pump.Position;

                Pawn renegade = PawnGenerator.GeneratePawn(
                    DefDatabase<PawnKindDef>.GetNamed("Seg_WOTV_Renegade"),
                    faction
                );

                GenSpawn.Spawn(renegade, spawnLoc, map);
                LordMaker.MakeNewLord(faction,new LordJob_DefendBase(faction, spawnLoc, 999999, false),map).AddPawn(renegade);
                pump.Destroy();
            }

            var monitors = map.listerBuildings.AllBuildingsNonColonistOfDef(ThingDef.Named("VitalsMonitor"));
            foreach (var monitor in monitors)
            {
                IntVec3 spawnLoc = monitor.Position;

                Pawn captain = PawnGenerator.GeneratePawn(
                    DefDatabase<PawnKindDef>.GetNamed("Seg_WOTV_PirateCaptain"),
                    faction
                );

                GenSpawn.Spawn(captain, spawnLoc, map);

                LordMaker.MakeNewLord(
                    faction,
                    new LordJob_DefendBase(faction, spawnLoc, 999999, false),
                    map
                ).AddPawn(captain);

                monitor.Destroy();
            }
        }
    }
}