using Verse;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;

namespace seg
{
    public class SegWarrantGameComponent : GameComponent
    {
        public static SegWarrantGameComponent Instance;

        private List<Thing> warrantBuildings = new List<Thing>();

        public SegWarrantGameComponent(Game game)
        {
            Instance = this;
        }

        public override void GameComponentTick()
        {
            for (int i = warrantBuildings.Count - 1; i >= 0; i--)
            {
                Thing thing = warrantBuildings[i];

                if (thing == null || thing.Destroyed)
                {
                    warrantBuildings.RemoveAt(i);
                    continue;
                }

                SegCompWarrantWall comp = thing.TryGetComp<SegCompWarrantWall>();
                if (comp == null)
                {
                    warrantBuildings.RemoveAt(i);
                    continue;
                }

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
            Thing parent = comp.parent;

            if (!warrantBuildings.Contains(parent))
                warrantBuildings.Add(parent);

            ApplyTitle(comp.owner);
        }

        public void Notify_HeirAssigned(SegCompWarrantWall comp)
        {
            Thing parent = comp.parent;

            if (!warrantBuildings.Contains(parent))
                warrantBuildings.Add(parent);
        }

        public void Notify_WarrantDestroyed(SegCompWarrantWall comp)
        {
            Thing parent = comp.parent;

            if (comp.owner != null)
                RemoveTitle(comp.owner);

            warrantBuildings.Remove(parent);
        }

        private void ApplyTitle(Pawn pawn)
        {
            Faction faction = Find.FactionManager.FirstFactionOfDef(
                DefDatabase<FactionDef>.GetNamed("Seg_WOTV_ImperiumOfMan"));

            RoyalTitleDef title = DefDatabase<RoyalTitleDef>.GetNamed("Seg_WOTV_RogueTraderTitle");

            pawn.royalty.SetTitle(faction, title, grantRewards: false, rewardsOnlyForNewestTitle: false, sendLetter: false);

            string letterTitle = "Seg_WOTV_RogueTraderTitle_LetterTitle".Translate(pawn.Named("PAWN"));
            string letterBody = "Seg_WOTV_RogueTraderTitle_LetterBody".Translate(
                pawn.Named("PAWN"),
                faction.Named("FACTION")
            );

            Find.LetterStack.ReceiveLetter(letterTitle, letterBody, LetterDefOf.PositiveEvent, pawn);
        }

        private void RemoveTitle(Pawn pawn)
        {
            Faction faction = Find.FactionManager.FirstFactionOfDef(
                DefDatabase<FactionDef>.GetNamed("Seg_WOTV_ImperiumOfMan"));

            pawn.royalty.SetTitle(faction, null, grantRewards: false, rewardsOnlyForNewestTitle: false, sendLetter: false);
        }

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref warrantBuildings, "Seg_WOTV_warrantBuildings", LookMode.Reference);
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