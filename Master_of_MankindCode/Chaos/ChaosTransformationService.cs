using Master_of_Mankind.Master_of_MankindCode.Cards;
using Master_of_Mankind.Master_of_MankindCode.Retain;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Random;

namespace Master_of_Mankind.Master_of_MankindCode.Chaos;

internal static class ChaosTransformationService
{
    public static async Task<int> PerformChosenTransformations(
        PlayerChoiceContext choiceContext,
        Player player,
        int maxSelections)
    {
        List<CardModel> eligible = GetEligibleHandCards(player);
        int max = Math.Min(maxSelections, eligible.Count);
        if (max <= 0)
            return 0;

        LocString prompt = new("static_hover_tips", "MASTER_OF_MANKIND-TZEENTCH_CARD_PROMPT");
        CardSelectorPrefs prefs = new(prompt, 0, max)
        {
            RequireManualConfirmation = true,
            PretendCardsCanBePlayed = true
        };
        List<CardModel> selected = (await CardSelectCmd.FromSimpleGrid(
                choiceContext,
                eligible,
                player,
                prefs))
            .ToList();

        int transformedCount = 0;
        foreach (CardModel original in selected)
        {
            if (await ChooseAndTransform(choiceContext, player, original))
                transformedCount++;
        }

        return transformedCount;
    }

    public static async Task PerformForcedTransformations(
        PlayerChoiceContext choiceContext,
        Player player,
        int count)
    {
        Rng rng = player.RunState.Rng.CombatCardSelection;
        for (int i = 0; i < count; i++)
        {
            List<CardModel> eligible = GetEligibleHandCards(player);
            if (eligible.Count == 0)
                return;

            CardModel original = eligible[rng.NextInt(eligible.Count)];
            List<CardModel> candidates = GetCandidates(player, original);
            if (candidates.Count == 0)
                continue;

            CardModel replacement = CreateReplacement(
                player,
                original,
                candidates[rng.NextInt(candidates.Count)]);
            await Transform(original, replacement);
        }
    }

    public static async Task RevertHand(Player player)
    {
        List<CardModel> transformed = player.PlayerCombatState?.Hand.Cards
            .Where(ChaosTransformationState.IsTransformed)
            .ToList() ?? [];
        foreach (CardModel card in transformed)
            await Revert(card);
    }

    public static async Task RevertIfResolved(CardModel card, PileType oldPileType)
    {
        if (oldPileType != PileType.Play
            || card.Pile?.Type is not (PileType.Discard or PileType.Exhaust)
            || !ChaosTransformationState.IsTransformed(card))
            return;

        await Revert(card);
    }

    private static async Task<bool> ChooseAndTransform(
        PlayerChoiceContext choiceContext,
        Player player,
        CardModel original)
    {
        List<CardModel> candidates = GetCandidates(player, original);
        if (candidates.Count == 0)
            return false;

        Rng rng = player.RunState.Rng.CombatCardGeneration;
        rng.Shuffle(candidates);
        List<CardModel> choices = candidates
            .Take(3)
            .Select(candidate => CreateReplacement(player, original, candidate))
            .ToList();

        LocString prompt = new("static_hover_tips", "MASTER_OF_MANKIND-TZEENTCH_REPLACEMENT_PROMPT");
        CardSelectorPrefs prefs = new(prompt, 1)
        {
            Cancelable = false,
            PretendCardsCanBePlayed = true
        };
        CardModel? replacement = (await CardSelectCmd.FromSimpleGrid(
                choiceContext,
                choices,
                player,
                prefs))
            .FirstOrDefault();
        return replacement is not null && await Transform(original, replacement);
    }

    private static List<CardModel> GetEligibleHandCards(Player player) =>
        player.PlayerCombatState?.Hand.Cards
            .Where(card => card.Type is CardType.Attack or CardType.Skill)
            .Where(card => !ChaosTransformationState.IsTransformed(card))
            .ToList() ?? [];

    private static List<CardModel> GetCandidates(Player player, CardModel original) =>
        player.Character.CardPool
            .GetUnlockedCards(player.UnlockState, player.RunState.CardMultiplayerConstraint)
            .Where(card => card is Master_of_MankindCard)
            .Where(card => card.ShouldShowInCardLibrary && card.CanBeGeneratedInCombat)
            .Where(card => card.Type == original.Type && card.Rarity == original.Rarity)
            .Where(card => card.Id != original.Id)
            .GroupBy(card => card.Id)
            .Select(group => group.First())
            .ToList();

    private static CardModel CreateReplacement(Player player, CardModel original, CardModel candidate)
    {
        var cardScope = original.CardScope
                        ?? throw new InvalidOperationException($"{original.Id.Entry} has no card scope.");
        CardModel replacement = cardScope.CreateCard(candidate, player);
        for (int i = 0; i < original.CurrentUpgradeLevel && replacement.IsUpgradable; i++)
            CardCmd.Upgrade(replacement, CardPreviewStyle.None);
        return replacement;
    }

    private static async Task<bool> Transform(CardModel original, CardModel replacement)
    {
        ChaosTransformationState.SetOriginal(replacement, original.ToSerializable());
        CardPileAddResult? result = await CardCmd.Transform(original, replacement, CardPreviewStyle.None);
        return result is { success: true };
    }

    private static async Task Revert(CardModel transformed)
    {
        var save = ChaosTransformationState.TakeOriginal(transformed);
        if (save is null)
            return;

        CardModel original = CardModel.FromSerializable(save);
        var cardScope = transformed.CardScope
                        ?? throw new InvalidOperationException($"{transformed.Id.Entry} has no card scope.");
        cardScope.AddCard(original, transformed.Owner);
        TemporaryRetainState.RestoreAfterTransformation(original);
        ObsessionState.RestoreAfterTransformation(original);
        await CardCmd.Transform(transformed, original, CardPreviewStyle.None);
    }
}
