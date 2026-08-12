using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using RimWorld.QuestGen;
using Verse.AI;
using Verse.Sound;

#nullable disable
namespace seg
{
    public class QuestNode_GiveSpecificThing : QuestNode
    {
        public SlateRef<string> inSignal;
        public SlateRef<string> thingDefName;
        public SlateRef<int?> stackCount;
        public SlateRef<string> customLetterLabel;
        public SlateRef<string> customLetterText;

        protected override bool TestRunInt(Slate slate) => true;
       protected override void RunInt()
        {
            string raw = this.inSignal.GetValue(QuestGen.slate);

            if (raw.Contains(".AllEnemiesDefeated"))
            {
                raw = "site.AllEnemiesDefeated";
            }

            string signal = QuestGenUtility.HardcodedSignalWithQuestID(raw);

            string defName = this.thingDefName.GetValue(QuestGen.slate);
            int count = this.stackCount.GetValue(QuestGen.slate) ?? 1;
            string label = this.customLetterLabel.GetValue(QuestGen.slate);
            string text = this.customLetterText.GetValue(QuestGen.slate);

            if (defName.NullOrEmpty())
            {
                Log.Error("ksdebug: thingDefName is empty.");
                return;
            }

            ThingDef def = DefDatabase<ThingDef>.GetNamedSilentFail(defName);
            if (def == null)
            {
                Log.Error($"ksdebug: ThingDef '{defName}' not found.");
                return;
            }

            QuestPart_SpawnThing qp = new QuestPart_SpawnThing();
            qp.inSignal = signal;
            qp.thingDef = def;
            qp.stackCount = count;
            qp.customLetterLabel = label;
            qp.customLetterText = text;
            QuestGen.quest.AddPart(qp);
        }
            
            }

    public class QuestPart_SpawnThing : QuestPart
    {
        public string inSignal;
        public ThingDef thingDef;
        public int stackCount = 1;
        public string customLetterLabel;
        public string customLetterText;
        private bool alreadySpawned;

        public override void Notify_QuestSignalReceived(Signal signal)
        {
            if (alreadySpawned)
                return;

            if (signal.tag != inSignal)
                return;

            alreadySpawned = true;

            Map targetMap = Find.AnyPlayerHomeMap;

            if (targetMap == null)
            {
                if (signal.args.TryGetArg<Map>("map", out Map m1))
                    targetMap = m1;
                else if (signal.args.TryGetArg<Map>("mapParent", out Map m2))
                    targetMap = m2;
            }

            if (targetMap == null)
            {
                if (Find.Maps != null && Find.Maps.Count > 0)
                    targetMap = Find.Maps[0];
            }

            if (targetMap == null)
            {
                Log.Error("[ksdebug] cant find a map!");
                return;
            }

            Thing thing = ThingMaker.MakeThing(thingDef);
            thing.stackCount = Math.Max(1, Math.Min(stackCount, thingDef.stackLimit));

            IntVec3 dropCell = DropCellFinder.TradeDropSpot(targetMap);
            if (!GenPlace.TryPlaceThing(thing, dropCell, targetMap, ThingPlaceMode.Near))
            {
                GenPlace.TryPlaceThing(thing, targetMap.Center, targetMap, ThingPlaceMode.Near);
            }

            if (!customLetterLabel.NullOrEmpty() || !customLetterText.NullOrEmpty())
            {
                string letterLabel = customLetterLabel.NullOrEmpty() ? "Reward" : customLetterLabel;
                string letterText = customLetterText.NullOrEmpty() ? $"You received: {thing.LabelCap}." : customLetterText;
                Find.LetterStack.ReceiveLetter(letterLabel, letterText, LetterDefOf.PositiveEvent);
            }
            else
            {
              return;
            }
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref inSignal, "inSignal");
            Scribe_Defs.Look(ref thingDef, "thingDef");
            Scribe_Values.Look(ref stackCount, "stackCount", 1);
            Scribe_Values.Look(ref customLetterLabel, "customLetterLabel");
            Scribe_Values.Look(ref customLetterText, "customLetterText");
            Scribe_Values.Look(ref alreadySpawned, "alreadySpawned", false);
        }
    }
}   