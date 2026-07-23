using BaseLib.Abstracts;
using Master_of_Mankind.Master_of_MankindCode.Relics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;

namespace Master_of_Mankind.Master_of_MankindCode.Events;

public abstract class ChaosTemptationEvent : CustomEventModel
{
    // These events are selected by ForceFirstChaosTemptationPatch rather than the normal shuffled pool.
    public override bool IsAllowed(IRunState runState) => false;

    protected async Task Accept<T>() where T : RelicModel
    {
        await RelicCmd.Obtain<T>(Owner!);
        SetEventFinished(PageDescription("ACCEPT"));
    }

    protected void UpgradeRandom(CardType type)
    {
        List<CardModel> candidates = Owner!.Deck.Cards
            .Where(card => card.IsUpgradable && card.Type == type)
            .ToList();
        CardModel? chosen = Rng.NextItem(candidates);
        if (chosen != null)
            CardCmd.Upgrade(chosen);
    }
}

public sealed class KhorneTemptation : ChaosTemptationEvent
{
    public override string CustomInitialPortraitPath =>
        $"{MainFile.ResPath}/images/events/temptation_khorne.png";

    protected override IReadOnlyList<EventOption> GenerateInitialOptions() =>
    [
        new EventOption(this, Reject, $"{Id.Entry}.pages.INITIAL.options.REJECT"),
        new EventOption(this, AcceptKhorne, $"{Id.Entry}.pages.INITIAL.options.ACCEPT",
            HoverTipFactory.FromRelic(ModelDb.Relic<SealOfKhorne>()))
    ];

    private Task Reject()
    {
        UpgradeRandom(CardType.Attack);
        SetEventFinished(PageDescription("REJECT"));
        return Task.CompletedTask;
    }

    private Task AcceptKhorne() => Accept<SealOfKhorne>();
}

public sealed class TzeentchTemptation : ChaosTemptationEvent
{
    public override string CustomInitialPortraitPath =>
        $"{MainFile.ResPath}/images/events/temptation_tzeentch.png";

    protected override IReadOnlyList<EventOption> GenerateInitialOptions() =>
    [
        new EventOption(this, Reject, $"{Id.Entry}.pages.INITIAL.options.REJECT"),
        new EventOption(this, AcceptTzeentch, $"{Id.Entry}.pages.INITIAL.options.ACCEPT",
            HoverTipFactory.FromRelic(ModelDb.Relic<SealOfTzeentch>()))
    ];

    private Task Reject()
    {
        UpgradeRandom(CardType.Skill);
        SetEventFinished(PageDescription("REJECT"));
        return Task.CompletedTask;
    }

    private Task AcceptTzeentch() => Accept<SealOfTzeentch>();
}

public sealed class NurgleTemptation : ChaosTemptationEvent
{
    public override string CustomInitialPortraitPath =>
        $"{MainFile.ResPath}/images/events/temptation_nurgle.png";

    protected override IReadOnlyList<EventOption> GenerateInitialOptions() =>
    [
        new EventOption(this, Reject, $"{Id.Entry}.pages.INITIAL.options.REJECT"),
        new EventOption(this, AcceptNurgle, $"{Id.Entry}.pages.INITIAL.options.ACCEPT",
            HoverTipFactory.FromRelic(ModelDb.Relic<SealOfNurgle>()))
    ];

    private async Task Reject()
    {
        decimal amount = Math.Ceiling(Owner!.Creature.MaxHp * 0.30m);
        await CreatureCmd.Heal(Owner.Creature, amount);
        SetEventFinished(PageDescription("REJECT"));
    }

    private Task AcceptNurgle() => Accept<SealOfNurgle>();
}

public sealed class SlaaneshTemptation : ChaosTemptationEvent
{
    public override string CustomInitialPortraitPath =>
        $"{MainFile.ResPath}/images/events/temptation_slaanesh.png";

    protected override IReadOnlyList<EventOption> GenerateInitialOptions() =>
    [
        new EventOption(this, Reject, $"{Id.Entry}.pages.INITIAL.options.REJECT"),
        new EventOption(this, AcceptSlaanesh, $"{Id.Entry}.pages.INITIAL.options.ACCEPT",
            HoverTipFactory.FromRelic(ModelDb.Relic<SealOfSlaanesh>()))
    ];

    private async Task Reject()
    {
        await EmperorEventHelpers.RemoveChosen(this);
        SetEventFinished(PageDescription("REJECT"));
    }

    private Task AcceptSlaanesh() => Accept<SealOfSlaanesh>();
}
