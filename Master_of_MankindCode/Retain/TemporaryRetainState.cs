using BaseLib.Patches.Content;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;

namespace Master_of_Mankind.Master_of_MankindCode.Retain;

internal static class TemporaryRetainState
{
    private static readonly SavedSpireField<CardModel, bool> AddedRetain =
        new(() => false, "MasterOfMankind_PerfectMomentRetain");

    private static readonly SavedSpireField<CardModel, int> CostReductions =
        new(() => 0, "MasterOfMankind_PerfectMomentCostReductions");

    public static void Apply(CardModel card)
    {
        if (!card.Keywords.Contains(CardKeyword.Retain))
        {
            CardCmd.ApplyKeyword(card, CardKeyword.Retain);
            AddedRetain.Set(card, true);
        }

        if (!card.EnergyCost.CostsX)
        {
            card.EnergyCost.AddUntilPlayed(-1, reduceOnly: true);
            CostReductions.Set(card, CostReductions.Get(card) + 1);
        }
    }

    public static bool IsActive(CardModel card) =>
        AddedRetain.Get(card) || CostReductions.Get(card) > 0;

    public static void RestoreAfterTransformation(CardModel card)
    {
        if (AddedRetain.Get(card) && !card.Keywords.Contains(CardKeyword.Retain))
            CardCmd.ApplyKeyword(card, CardKeyword.Retain);

        int reductions = CostReductions.Get(card);
        if (reductions > 0 && !card.EnergyCost.CostsX)
            card.EnergyCost.AddUntilPlayed(-reductions, reduceOnly: true);
    }

    public static void Clear(CardModel card)
    {
        if (AddedRetain.Get(card))
        {
            CardCmd.RemoveKeyword(card, CardKeyword.Retain);
            AddedRetain.Set(card, false);
        }

        CostReductions.Set(card, 0);
    }
}
