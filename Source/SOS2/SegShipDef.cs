using HarmonyLib;
using RimWorld;
using Verse;
using SaveOurShip2;

namespace seg
{
    public class ShipDefWithAI : ShipDef
    {
        public ShipAI forcedAI = ShipAI.none;
    }

    [StaticConstructorOnStartup]
    public static class Bootstrap
    {
        static Bootstrap()
        {
            new Harmony("seg.SaveOurShip2.ExtendedAI").PatchAll();
        }
    }

    [HarmonyPatch(typeof(ShipMapComp), "SpawnEnemyShipMap")]
    public static class Patch_SpawnEnemyShipMap_SetAI
    {
        static void Postfix(Map __result)
        {
            if (__result == null) return;
            var comp = __result.GetComponent<ShipMapComp>();
            if (comp == null) return;

            var def = GetShipDef(comp);
            if (def is ShipDefWithAI shipDefWithAI && shipDefWithAI.forcedAI != ShipAI.none)
                comp.ShipMapAI = shipDefWithAI.forcedAI;
        }

        static ShipDef GetShipDef(ShipMapComp comp)
        {
            foreach (var kvp in comp.ShipsOnMap)
            {
                var cache = kvp.Value;
                if (cache == null || cache.IsWreck || cache.Core == null) continue;
                var coreName = cache.Core.def.defName;
                if (coreName == null) continue;

                foreach (var d in DefDatabase<ShipDef>.AllDefs)
                {
                    if (d == null || d.parts == null) continue;
                    foreach (var p in d.parts)
                        if (p.shapeOrDef == coreName)
                            return d;
                }
            }
            return null;
        }
    }
}