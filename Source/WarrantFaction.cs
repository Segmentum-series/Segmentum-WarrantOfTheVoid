using RimWorld;
using Verse;
using System.Collections.Generic;

namespace seg
{
    public class CompProperties_WarrantWallGoodwill : CompProperties
    {
        public List<FactionDef> factions;
        public int goodwillAmount = 25;

        public CompProperties_WarrantWallGoodwill()
        {
            this.compClass = typeof(CompWarrantWallGoodwill);
        }
    }

    public class CompWarrantWallGoodwill : ThingComp
    {
        public CompProperties_WarrantWallGoodwill Props => (CompProperties_WarrantWallGoodwill)this.props;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            Log.Message("[WOTV] PostSpawnSetup fired — applying goodwill");

            foreach (var factionDef in Props.factions)
            {
                Faction f = Find.FactionManager.FirstFactionOfDef(factionDef);
                if (f != null)
                {
                    Faction.OfPlayer.TryAffectGoodwillWith(f, Props.goodwillAmount);
                    Log.Message($"[WOTV] Added {Props.goodwillAmount} goodwill with {f.Name}");
                }
                else
                {
                    Log.Message($"[WOTV] No faction found for {factionDef.defName}");
                }
            }
        }

        public override void PostDestroy(DestroyMode mode, Map map)
        {
            base.PostDestroy(mode, map);

            Log.Message("[WOTV] PostDestroy fired — removing goodwill");

            foreach (var factionDef in Props.factions)
            {
                Faction f = Find.FactionManager.FirstFactionOfDef(factionDef);
                if (f != null)
                {
                    Faction.OfPlayer.TryAffectGoodwillWith(f, -Props.goodwillAmount);
                    Log.Message($"[WOTV] Removed {Props.goodwillAmount} goodwill with {f.Name}");
                }
                else
                {
                    Log.Message($"[WOTV] No faction found for {factionDef.defName}");
                }
            }
        }
    }
}