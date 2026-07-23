using HarmonyLib;
using Master_of_Mankind.Master_of_MankindCode.Decree;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace Master_of_Mankind.Master_of_MankindCode.Patches;

[HarmonyPatch(typeof(NCombatUi), nameof(NCombatUi.Activate))]
internal static class DecreeUiActivatePatch
{
    private static void Postfix(CombatState state)
    {
        DecreeZoneUi.Initialize(state);
    }
}

[HarmonyPatch(typeof(NCombatUi), "PostCombatCleanUp")]
internal static class DecreeUiCleanupPatch
{
    private static void Prefix()
    {
        DecreeManager.ClearAll();
    }
}
