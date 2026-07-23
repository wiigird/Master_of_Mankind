using HarmonyLib;
using Master_of_Mankind.Master_of_MankindCode.Foresight;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace Master_of_Mankind.Master_of_MankindCode.Patches;

[HarmonyPatch(typeof(NCreature), nameof(NCreature.UpdateIntent))]
internal static class ForesightUiPatches
{
    private static void Postfix(NCreature __instance)
    {
        if (__instance.Entity.CombatState is not { } combatState
            || ForesightPredictionService.GetPredictionDepth(combatState) <= 0)
            return;

        // UpdateIntent is the authoritative point where an enemy's live move changes.
        // Invalidate once here instead of rebuilding a string representation on every query.
        ForesightPredictionService.Invalidate();
        ForesightTimelineUi.RefreshDeferred(__instance);
    }
}

[HarmonyPatch(typeof(CombatManager), nameof(CombatManager.AfterCreatureAdded))]
internal static class ForesightCreatureAddedPatch
{
    private static void Postfix(Creature creature)
    {
        if (creature.IsEnemy
            && creature.CombatState is { } combatState
            && ForesightPredictionService.GetPredictionDepth(combatState) > 0)
            TaskHelper.RunSafely(ForesightTimelineUi.RefreshAllEnemies(combatState));
    }
}
