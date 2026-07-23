using BaseLib.Patches.Content;
using BaseLib.Utils;
using Master_of_Mankind.Master_of_MankindCode.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace Master_of_Mankind.Master_of_MankindCode.Retain;

internal static class ArmourRetainState
{
    private static readonly SavedSpireField<CardModel, bool> WasRetained =
        new(() => false, "MasterOfMankind_WasRetained");

    public static bool IsRetained(CardModel card) => WasRetained.Get(card);

    public static void SetRetained(CardModel card, bool retained) => WasRetained.Set(card, retained);

    public static bool HasTriggeredThisTurn(Player player, int turnNumber) =>
        PlayerCombatMemory.ArmourTriggeredThisTurn(player, turnNumber);

    public static void MarkTriggered(Player player, int turnNumber) =>
        PlayerCombatMemory.MarkArmourTriggered(player, turnNumber);
}
