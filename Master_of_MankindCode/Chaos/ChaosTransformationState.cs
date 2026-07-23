using BaseLib.Patches.Content;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace Master_of_Mankind.Master_of_MankindCode.Chaos;

internal static class ChaosTransformationState
{
    private static readonly SavedSpireField<CardModel, SerializableCard?> OriginalCard =
        new(() => null, "MasterOfMankind_ChaosOriginalCard");

    public static bool IsTransformed(CardModel card) => OriginalCard.Get(card) is not null;

    public static void SetOriginal(CardModel transformedCard, SerializableCard originalCard) =>
        OriginalCard.Set(transformedCard, originalCard);

    public static SerializableCard? TakeOriginal(CardModel transformedCard)
    {
        SerializableCard? original = OriginalCard.Get(transformedCard);
        OriginalCard.Set(transformedCard, null);
        return original;
    }
}
