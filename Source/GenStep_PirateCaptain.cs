
using RimWorld;
using RimWorld.Planet;
using System.Linq;
using Verse;
using RimWorld.QuestGen;

#nullable disable
namespace seg;
public class QuestNode_GetImperiumPirates : QuestNode
    {
        [NoTranslate]
        public SlateRef<string> storeAs;

        protected override void RunInt()
        {
            Faction f = Find.FactionManager.FirstFactionOfDef(FactionDef.Named("Seg_WOTV_ImperiumPirates"));
            QuestGen.slate.Set(storeAs.GetValue(QuestGen.slate), f);
        }

        protected override bool TestRunInt(Slate slate)
        {
            return Find.FactionManager.FirstFactionOfDef(FactionDef.Named("Seg_WOTV_ImperiumPirates")) != null;
        }
    }
