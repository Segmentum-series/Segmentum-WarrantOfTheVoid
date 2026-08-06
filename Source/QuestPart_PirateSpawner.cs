using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI;
namespace seg;


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
        renegade.mindState.duty= new PawnDuty(DutyDefOf.Defend, spawnLoc);
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
        captain.mindState.duty= new PawnDuty(DutyDefOf.Defend, spawnLoc);
        monitor.Destroy();
    }
}
}