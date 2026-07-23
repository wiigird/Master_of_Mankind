using HarmonyLib;
using Master_of_Mankind.Master_of_MankindCode.Events;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;

namespace Master_of_Mankind.Master_of_MankindCode.Patches;

[HarmonyPatch(typeof(ActModel), nameof(ActModel.PullNextEvent))]
internal static class ForceFirstChaosTemptationPatch
{
    [HarmonyPrefix]
    private static bool ForceFirstActThreeEvent(RunState runState, ref EventModel __result)
    {
        if (runState.CurrentActIndex != 2 || !EmperorEventHelpers.IsEmperorRun(runState))
            return true;

        EventModel[] temptations =
        [
            ModelDb.Event<KhorneTemptation>(),
            ModelDb.Event<TzeentchTemptation>(),
            ModelDb.Event<NurgleTemptation>(),
            ModelDb.Event<SlaaneshTemptation>()
        ];

        if (temptations.Any(temptation => runState.VisitedEventIds.Contains(temptation.Id)))
            return true;

        EventModel selected = runState.Rng.UpFront.NextItem(temptations) ?? temptations[0];
        runState.AddVisitedEvent(selected);
        __result = selected;
        return false;
    }
}
