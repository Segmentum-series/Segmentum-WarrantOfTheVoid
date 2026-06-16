using Verse;
using RimWorld;
using RimWorld.QuestGen;
namespace seg
{
public class Seg_WOTV_WarrantTimer : GameComponent
{
    private bool questFired = false;
    private int startTick = -1;

    public Seg_WOTV_WarrantTimer(Game game)
    {
    }

    public override void StartedNewGame()
    {
        startTick = Find.TickManager.TicksGame;
    }

    public override void LoadedGame()
    {
        // just to be safe, set the start tick if it wasnt set at game start for whatever fuckin reason
        if (startTick < 0)
            startTick = Find.TickManager.TicksGame;
    }

    public override void GameComponentTick()
    {
        if (questFired)
            return;
        if (Faction.OfPlayer == null)
            return;
        // only fires for the RT faction, to avoid accidentally triggering for players who have the mod but aren't playing the RT scenario
        if (Faction.OfPlayer.def.defName != "Seg_WOTV_PlayerRT")
            return;
        if (startTick < 0)
            return;
        // Wait one in-game year 
        if (Find.TickManager.TicksGame - startTick < 3600000)
            return;
        QuestScriptDef questDef = DefDatabase<QuestScriptDef>.GetNamed("Seg_WOTV_warrantBuildingscenario");
        Slate slate = new Slate();
        QuestUtility.GenerateQuestAndMakeAvailable(questDef, slate);

        questFired = true;
    }

    public override void ExposeData()
    {
        Scribe_Values.Look(ref questFired, "Seg_WOTV_questFired", false);
        Scribe_Values.Look(ref startTick, "Seg_WOTV_startTick", -1);
    }
}
}