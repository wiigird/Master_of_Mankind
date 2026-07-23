using HarmonyLib;
using Master_of_Mankind.Master_of_MankindCode.Cards.Basic;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using EmperorCharacter = Master_of_Mankind.Master_of_MankindCode.Character.Emperor;
using EmperorBurningBlade = Master_of_Mankind.Master_of_MankindCode.Relics.BurningBlade;
using EmperorRadiantBurningBlade = Master_of_Mankind.Master_of_MankindCode.Relics.RadiantBurningBlade;

namespace Master_of_Mankind.Master_of_MankindCode.Patches;

[HarmonyPatch(typeof(ArchaicTooth), "GetTranscendenceStarterCard")]
internal static class EmperorArchaicToothStarterCardPatch
{
    [HarmonyPostfix]
    private static void Postfix(Player player, ref CardModel? __result)
    {
        if (__result is null && player.Character is EmperorCharacter)
            __result = player.Deck.Cards.OfType<EmperorsForesight>().FirstOrDefault();
    }
}

[HarmonyPatch(typeof(ArchaicTooth), "GetTranscendenceTransformedCard")]
internal static class EmperorArchaicToothTransformedCardPatch
{
    [HarmonyPrefix]
    private static bool Prefix(CardModel starterCard, ref CardModel __result)
    {
        if (starterCard is not EmperorsForesight)
            return true;

        TenThousandFutures replacement =
            starterCard.Owner.RunState.CreateCard<TenThousandFutures>(starterCard.Owner);
        if (starterCard.IsUpgraded)
            CardCmd.Upgrade(replacement);

        __result = replacement;
        return false;
    }
}

[HarmonyPatch(typeof(TouchOfOrobas), nameof(TouchOfOrobas.GetUpgradedStarterRelic))]
internal static class EmperorTouchOfOrobasPatch
{
    [HarmonyPostfix]
    private static void Postfix(RelicModel starterRelic, ref RelicModel __result)
    {
        if (starterRelic is EmperorBurningBlade)
            __result = ModelDb.Relic<EmperorRadiantBurningBlade>();
    }
}
