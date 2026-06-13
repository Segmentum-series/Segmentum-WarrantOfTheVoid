using Verse;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
namespace seg
{
    public class SegWarrantGameComponent : GameComponent
    {
        public static SegWarrantGameComponent Instance;

        private List<SegCompWarrantWall> warrants = new List<SegCompWarrantWall>();

        public SegWarrantGameComponent(Game game)
        {
            Instance = this;
        }

        public override void GameComponentTick()
        {
            for (int i = warrants.Count - 1; i >= 0; i--)
            {
                var comp = warrants[i];
                if (comp.owner != null && comp.owner.Dead)
                {
                    if (comp.heir != null)
                    {
                        ApplyTitle(comp.heir);
                    }
                    RemoveTitle(comp.owner);
                    comp.owner = null;
                }
            }
        }

        public void Notify_OwnerAssigned(SegCompWarrantWall comp)
        {
            if (!warrants.Contains(comp))
                warrants.Add(comp);

            ApplyTitle(comp.owner);
        }

        public void Notify_HeirAssigned(SegCompWarrantWall comp)
        {
            if (!warrants.Contains(comp))
                warrants.Add(comp);
        }

        public void Notify_WarrantDestroyed(SegCompWarrantWall comp)
        {
            if (comp.owner != null)
                RemoveTitle(comp.owner);

            warrants.Remove(comp);
        }

        private void ApplyTitle(Pawn pawn)
        {
            Faction faction = Find.FactionManager.FirstFactionOfDef(
                DefDatabase<FactionDef>.GetNamed("Ancients"));

            RoyalTitleDef title = DefDatabase<RoyalTitleDef>.GetNamed("Seg_WOTV_RougeTraderTitle");

            pawn.royalty.SetTitle(faction, title, grantRewards: false);
        }

        private void RemoveTitle(Pawn pawn)
        {
            Faction faction = Find.FactionManager.FirstFactionOfDef(
                DefDatabase<FactionDef>.GetNamed("Ancients"));

            pawn.royalty.SetTitle(faction, null, grantRewards: false);
        }

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref warrants, "Seg_WOTV_Warrants", LookMode.Reference);
        }
    }
 public class CompProperties_SegCompWarrantWall : CompProperties
    {
        public CompProperties_SegCompWarrantWall()
        {
            this.compClass = typeof(SegCompWarrantWall);
        }
    }
      public class SegCompWarrantWall : ThingComp
    {
        public Pawn owner;
        public Pawn heir;

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            // Assign Owner
            yield return new Command_Action
            {
                defaultLabel = "Assign Warrant Holder",
                icon = ContentFinder<Texture2D>.Get("UI/Gizmos/Seg_WOTV_Gizmo_Trader"),
                action = () =>
                {
                    List<FloatMenuOption> opts = new List<FloatMenuOption>();
                    foreach (Pawn p in this.parent.Map.mapPawns.FreeColonists)
                    {
                        opts.Add(new FloatMenuOption(p.LabelShort, () =>
                        {
                            owner = p;
                            SegWarrantGameComponent.Instance.Notify_OwnerAssigned(this);
                        }));
                    }
                    Find.WindowStack.Add(new FloatMenu(opts));
                }
                };

                    yield return new Command_Action
                    {
                    defaultLabel = "Assign Heir",
                    icon = ContentFinder<Texture2D>.Get("UI/Gizmos/Seg_WOTV_Gizmo_Heir"),
                    action = () =>
                    {
                        List<FloatMenuOption> opts = new List<FloatMenuOption>();
                        foreach (Pawn p in this.parent.Map.mapPawns.FreeColonists)
                        {
                            opts.Add(new FloatMenuOption(p.LabelShort, () =>
                            {
                                heir = p;
                                SegWarrantGameComponent.Instance.Notify_HeirAssigned(this);
                            }));
                        }
                        Find.WindowStack.Add(new FloatMenu(opts));
                    }
                };
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            base.PostDestroy(mode, previousMap);
            SegWarrantGameComponent.Instance.Notify_WarrantDestroyed(this);
        }

        public override void PostExposeData()
        {
            Scribe_References.Look(ref owner, "Seg_WOTV_WarrantOwner");
            Scribe_References.Look(ref heir, "Seg_WOTV_WarrantHeir");
        }
    }
}