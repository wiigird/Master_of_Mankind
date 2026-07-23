using HarmonyLib;
using Master_of_Mankind.Master_of_MankindCode.Chaos;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Master_of_Mankind.Master_of_MankindCode.Patches;

[HarmonyPatch(typeof(ArtifactPower), nameof(ArtifactPower.TryModifyPowerAmountReceived))]
internal static class ChaosMarkArtifactPatch
{
    [HarmonyPrefix]
    private static bool Prefix(
        PowerModel canonicalPower,
        decimal amount,
        ref decimal modifiedAmount,
        ref bool __result)
    {
        if (canonicalPower is not IChaosMark)
            return true;

        modifiedAmount = amount;
        __result = false;
        return false;
    }
}
