using BaseLib.Patches.Content;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;

namespace Master_of_Mankind.Master_of_MankindCode.Chaos;

internal static class ObsessionState
{
    private static readonly SavedSpireField<CardModel, bool> IsObsessedField =
        new(() => false, "MasterOfMankind_IsObsessed");

    private static readonly SavedSpireField<CardModel, bool> AddedRetain =
        new(() => false, "MasterOfMankind_ObsessionAddedRetain");

    public static bool IsObsessed(CardModel card) => IsObsessedField.Get(card);

    public static void Apply(CardModel card)
    {
        if (IsObsessed(card))
            return;

        IsObsessedField.Set(card, true);
        if (!card.Keywords.Contains(CardKeyword.Retain))
        {
            CardCmd.ApplyKeyword(card, CardKeyword.Retain);
            AddedRetain.Set(card, true);
        }

        if (!card.EnergyCost.CostsX)
            card.EnergyCost.AddUntilPlayed(1, reduceOnly: false);
    }

    public static void Clear(CardModel card)
    {
        if (!IsObsessed(card))
            return;

        IsObsessedField.Set(card, false);
        if (AddedRetain.Get(card))
        {
            CardCmd.RemoveKeyword(card, CardKeyword.Retain);
            AddedRetain.Set(card, false);
        }
    }

    public static void RestoreAfterTransformation(CardModel card)
    {
        if (!IsObsessed(card))
            return;

        if (AddedRetain.Get(card) && !card.Keywords.Contains(CardKeyword.Retain))
            CardCmd.ApplyKeyword(card, CardKeyword.Retain);
        if (!card.EnergyCost.CostsX)
            card.EnergyCost.AddUntilPlayed(1, reduceOnly: false);
    }
}
