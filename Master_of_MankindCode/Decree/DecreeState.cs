using BaseLib.Patches.Content;
using BaseLib.Utils;
using Master_of_Mankind.Master_of_MankindCode.Cards;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;

namespace Master_of_Mankind.Master_of_MankindCode.Decree;

internal static class DecreeState
{
    private static readonly SavedSpireField<DecreeCard, int> PreparedTurn =
        new(() => -1, "MasterOfMankind_PreparedTurn");

    private static readonly SavedSpireField<DecreeCard, int> TargetCombatId =
        new(() => -1, "MasterOfMankind_TargetCombatId");

    private static readonly SavedSpireField<DecreeCard, int> EnergySpent =
        new(() => 0, "MasterOfMankind_EnergySpent");

    private static readonly SavedSpireField<DecreeCard, int> EnergyValue =
        new(() => 0, "MasterOfMankind_EnergyValue");

    private static readonly SavedSpireField<DecreeCard, int> StarsSpent =
        new(() => 0, "MasterOfMankind_StarsSpent");

    private static readonly SavedSpireField<DecreeCard, int> StarValue =
        new(() => 0, "MasterOfMankind_StarValue");

    public static CardPile? GetPile(Player player)
    {
        PlayerCombatState? playerState = player.PlayerCombatState;
        return playerState is null
            ? null
            : CustomPiles.GetCustomPile(playerState, DecreePile.DecreePileType);
    }

    public static void Prepare(DecreeCard card, CardPlay cardPlay, int turnNumber)
    {
        PreparedTurn.Set(card, turnNumber);
        TargetCombatId.Set(card, cardPlay.Target?.CombatId is { } id ? checked((int)id) : -1);
        EnergySpent.Set(card, cardPlay.Resources.EnergySpent);
        EnergyValue.Set(card, cardPlay.Resources.EnergyValue);
        StarsSpent.Set(card, cardPlay.Resources.StarsSpent);
        StarValue.Set(card, cardPlay.Resources.StarValue);
    }

    public static bool IsPrepared(DecreeCard card) => PreparedTurn.Get(card) >= 0;

    public static PreparedDecree ToPrepared(DecreeCard card) => new(
        card,
        TargetCombatId.Get(card),
        new ResourceInfo
        {
            EnergySpent = EnergySpent.Get(card),
            EnergyValue = EnergyValue.Get(card),
            StarsSpent = StarsSpent.Get(card),
            StarValue = StarValue.Get(card)
        },
        PreparedTurn.Get(card));

    public static void Clear(DecreeCard card)
    {
        PreparedTurn.Set(card, -1);
        TargetCombatId.Set(card, -1);
        EnergySpent.Set(card, 0);
        EnergyValue.Set(card, 0);
        StarsSpent.Set(card, 0);
        StarValue.Set(card, 0);
    }
}
