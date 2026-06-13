using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;
using UnityEngine;

namespace seg
{
    public class WarrantOfTradeManager : GameComponent
    {
        private static WarrantOfTradeManager instance;
        private int nextAllowedTick = 0;
        private const int CooldownTicks = 15 * 60000;

        public static WarrantOfTradeManager Instance
        {
            get
            {
                if (instance == null)
                    instance = Current.Game.GetComponent<WarrantOfTradeManager>();
                return instance;
            }
        }

        public WarrantOfTradeManager(Game game)
        {
            instance = this;
        }

        public bool IsOnCooldown => Find.TickManager.TicksAbs < nextAllowedTick;

        public int TicksRemaining => nextAllowedTick - Find.TickManager.TicksAbs;

        public void TriggerTraderCall(Map map, TraderKindDef traderKind)
            {
                nextAllowedTick = Find.TickManager.TicksAbs + CooldownTicks;

                var parms = new IncidentParms
                {
                    target = map,
                    forced = true,
                    traderKind = traderKind
                };

                IncidentDefOf.OrbitalTraderArrival.Worker.TryExecute(parms);

                Messages.Message("Trader contacted: " + traderKind.label.CapitalizeFirst(), MessageTypeDefOf.NeutralEvent);
            }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref nextAllowedTick, "seg_warrant_nextAllowedTick", 0);
        }
    }

    public class WarrantOfTrade : Building
    {
        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn pawn)
        {
            foreach (var opt in base.GetFloatMenuOptions(pawn))
                yield return opt;

            if (!pawn.CanReach(this, PathEndMode.InteractionCell, Danger.None))
                yield break;

            if (WarrantOfTradeManager.Instance.IsOnCooldown)
            {
                int ticks = WarrantOfTradeManager.Instance.TicksRemaining;
                string time = ticks.ToStringTicksToPeriod();
                yield return new FloatMenuOption("Request trader (cooldown: " + time + ")", null);
                yield break;
            }

            yield return new FloatMenuOption(
                "Request trader",
                () =>
                {
                    List<FloatMenuOption> opts = new List<FloatMenuOption>();

                    foreach (TraderKindDef trader in DefDatabase<TraderKindDef>.AllDefs)
                    {
                        if (trader.orbital)
                        {
                            opts.Add(new FloatMenuOption(
                                trader.LabelCap,
                                () =>
                                {
                                    WarrantOfTradeManager.Instance.TriggerTraderCall(this.Map, trader);
                                }
                            ));
                        }
                    }

                    Find.WindowStack.Add(new FloatMenu(opts));
                }
            );
        }
    }

    public class JobDriver_WarrantOfTrade : JobDriver
    {
        private const int WorkDuration = 200;
        private int workDone;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, errorOnFailed: errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);

            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);

            Toil work = new Toil
            {
                defaultDuration = WorkDuration,
                defaultCompleteMode = ToilCompleteMode.Never
            };

            work.WithProgressBar(TargetIndex.A, () => (float)workDone / WorkDuration);

            work.tickAction = () =>
            {
                workDone++;
                if (workDone >= WorkDuration)
                {
                    List<FloatMenuOption> opts = new List<FloatMenuOption>();

                    foreach (TraderKindDef trader in DefDatabase<TraderKindDef>.AllDefs)
                    {
                        if (trader.orbital)
                        {
                            opts.Add(new FloatMenuOption(
                                trader.LabelCap,
                                () =>
                                {
                                    WarrantOfTradeManager.Instance.TriggerTraderCall(Map, trader);
                                }
                            ));
                        }
                    }

                    Find.WindowStack.Add(new FloatMenu(opts));
                    ReadyForNextToil();
                }
            };

            work.AddFinishAction(() =>
            {
                pawn.ClearReservationsForJob(job);
            });

            yield return work;
        }
    }
}