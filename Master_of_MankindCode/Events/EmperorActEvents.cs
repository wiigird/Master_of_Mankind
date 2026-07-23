using BaseLib.Abstracts;
using Master_of_Mankind.Master_of_MankindCode.Cards.Rare;
using Master_of_Mankind.Master_of_MankindCode.Cards.Uncommon;
using Master_of_Mankind.Master_of_MankindCode.Character;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace Master_of_Mankind.Master_of_MankindCode.Events;

internal static class EmperorEventHelpers
{
    public static bool IsEmperorRun(IRunState runState) =>
        runState.Players.All(player => player.Character is Emperor);

    public static async Task AddCard<T>(EventModel eventModel, bool upgraded = false)
        where T : CardModel
    {
        CardModel card = eventModel.Owner!.RunState.CreateCard(ModelDb.Card<T>(), eventModel.Owner);
        if (upgraded && card.IsUpgradable)
            CardCmd.Upgrade(card);

        CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(card, PileType.Deck), 2f);
    }

    public static async Task UpgradeChosen(EventModel eventModel, int requestedCount)
    {
        int available = eventModel.Owner!.Deck.Cards.Count(card => card.IsUpgradable);
        int count = Math.Min(requestedCount, available);
        if (count <= 0)
            return;

        CardSelectorPrefs prefs = new(CardSelectorPrefs.UpgradeSelectionPrompt, count);
        foreach (CardModel card in await CardSelectCmd.FromDeckForUpgrade(eventModel.Owner, prefs))
            CardCmd.Upgrade(card);
    }

    public static void UpgradeRandom(EventModel eventModel, CardType type, CardRarity? rarity = null)
    {
        List<CardModel> candidates = eventModel.Owner!.Deck.Cards
            .Where(card => card.IsUpgradable)
            .Where(card => card.Type == type)
            .Where(card => rarity == null || card.Rarity == rarity)
            .ToList();
        CardModel? chosen = eventModel.Rng.NextItem(candidates);
        if (chosen != null)
            CardCmd.Upgrade(chosen);
    }

    public static async Task RemoveChosen(EventModel eventModel)
    {
        if (eventModel.Owner!.Deck.Cards.Count == 0)
            return;

        CardSelectorPrefs prefs = new(CardSelectorPrefs.RemoveSelectionPrompt, 1);
        List<CardModel> selected = (await CardSelectCmd.FromDeckForRemoval(eventModel.Owner, prefs)).ToList();
        await CardPileCmd.RemoveFromDeck(selected);
    }
}

public sealed class PrimarchSignals : CustomEventModel
{
    public override string CustomInitialPortraitPath =>
        $"{MainFile.ResPath}/images/events/primarch_signals.png";

    public override bool IsAllowed(IRunState runState) =>
        runState.CurrentActIndex == 0 && EmperorEventHelpers.IsEmperorRun(runState);

    protected override IReadOnlyList<EventOption> GenerateInitialOptions() =>
    [
        new EventOption(this, Search,
            $"{Id.Entry}.pages.INITIAL.options.SEARCH"),
        new EventOption(this, Leave,
            $"{Id.Entry}.pages.INITIAL.options.LEAVE")
    ];

    private async Task Search()
    {
        Func<Task>? discovery = Rng.NextItem(new Func<Task>[]
        {
            RevealHorus,
            RevealSanguinius,
            RevealGuilliman,
            RevealPerturabo
        });

        await (discovery ?? RevealHorus)();
    }

    private Task Leave()
    {
        SetEventFinished(PageDescription("LEAVE"));
        return Task.CompletedTask;
    }

    private Task RevealHorus()
    {
        SetEventState(PageDescription("HORUS_FOUND"),
        [
            new EventOption(this, ClaimHorus,
                $"{Id.Entry}.pages.HORUS_FOUND.options.CLAIM",
                [
                    .. HoverTipFactory.FromCardWithCardHoverTips<TriumphAtUllanor>(),
                    .. HoverTipFactory.FromCardWithCardHoverTips<Doubt>()
                ])
        ]);
        return Task.CompletedTask;
    }

    private Task RevealSanguinius()
    {
        SetEventState(PageDescription("SANGUINIUS_FOUND"),
        [
            new EventOption(this, ClaimSanguinius,
                $"{Id.Entry}.pages.SANGUINIUS_FOUND.options.CLAIM")
        ]);
        return Task.CompletedTask;
    }

    private Task RevealGuilliman()
    {
        SetEventState(PageDescription("GUILLIMAN_FOUND"),
        [
            new EventOption(this, ClaimGuilliman,
                $"{Id.Entry}.pages.GUILLIMAN_FOUND.options.CLAIM")
        ]);
        return Task.CompletedTask;
    }

    private Task RevealPerturabo()
    {
        SetEventState(PageDescription("PERTURABO_FOUND"),
        [
            new EventOption(this, ClaimPerturabo,
                $"{Id.Entry}.pages.PERTURABO_FOUND.options.CLAIM",
                HoverTipFactory.FromCardWithCardHoverTips<Doubt>())
        ]);
        return Task.CompletedTask;
    }

    private async Task ClaimHorus()
    {
        await EmperorEventHelpers.AddCard<TriumphAtUllanor>(this);
        await CardPileCmd.AddCurseToDeck<Doubt>(Owner!);
        SetEventFinished(PageDescription("HORUS"));
    }

    private async Task ClaimSanguinius()
    {
        await CreatureCmd.GainMaxHp(Owner!.Creature, 8m);
        SetEventFinished(PageDescription("SANGUINIUS"));
    }

    private async Task ClaimGuilliman()
    {
        await PlayerCmd.GainGold(150m, Owner!);
        await EmperorEventHelpers.UpgradeChosen(this, 1);
        SetEventFinished(PageDescription("GUILLIMAN"));
    }

    private async Task ClaimPerturabo()
    {
        await EmperorEventHelpers.UpgradeChosen(this, 2);
        await CardPileCmd.AddCurseToDeck<Doubt>(Owner!);
        SetEventFinished(PageDescription("PERTURABO"));
    }
}

public sealed class LorgarsQuestion : CustomEventModel
{
    public override string CustomInitialPortraitPath =>
        $"{MainFile.ResPath}/images/events/lorgars_question.png";

    public override bool IsAllowed(IRunState runState) =>
        runState.CurrentActIndex == 1 && EmperorEventHelpers.IsEmperorRun(runState);

    protected override IReadOnlyList<EventOption> GenerateInitialOptions() =>
    [
        new EventOption(this, DenyDivinity,
            $"{Id.Entry}.pages.INITIAL.options.DENY_DIVINITY",
            HoverTipFactory.FromCardWithCardHoverTips<ImperialTruth>()),
        new EventOption(this, UseTheFaith,
            $"{Id.Entry}.pages.INITIAL.options.USE_THE_FAITH"),
        new EventOption(this, EntrustMalcador,
            $"{Id.Entry}.pages.INITIAL.options.ENTRUST_MALCADOR")
    ];

    private async Task DenyDivinity()
    {
        await EmperorEventHelpers.AddCard<ImperialTruth>(this, upgraded: true);
        SetEventFinished(PageDescription("DENY_DIVINITY"));
    }

    private async Task UseTheFaith()
    {
        await PlayerCmd.GainGold(200m, Owner!);
        UpgradeRandomRarity(CardRarity.Common);
        UpgradeRandomRarity(CardRarity.Uncommon);
        await CreatureCmd.LoseMaxHp(new ThrowingPlayerChoiceContext(), Owner!.Creature, 6m, isFromCard: false);
        SetEventFinished(PageDescription("USE_THE_FAITH"));
    }

    private async Task EntrustMalcador()
    {
        await EmperorEventHelpers.RemoveChosen(this);
        await CreatureCmd.Damage(
            new ThrowingPlayerChoiceContext(),
            Owner!.Creature,
            8m,
            ValueProp.Unblockable | ValueProp.Unpowered,
            null,
            null);
        SetEventFinished(PageDescription("ENTRUST_MALCADOR"));
    }

    private void UpgradeRandomRarity(CardRarity rarity)
    {
        List<CardModel> candidates = Owner!.Deck.Cards
            .Where(card => card.IsUpgradable && card.Rarity == rarity)
            .ToList();
        CardModel? chosen = Rng.NextItem(candidates);
        if (chosen != null)
            CardCmd.Upgrade(chosen);
    }
}
