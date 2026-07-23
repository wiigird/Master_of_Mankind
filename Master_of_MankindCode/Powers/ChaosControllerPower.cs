using Master_of_Mankind.Master_of_MankindCode.Cards.Token;
using Master_of_Mankind.Master_of_MankindCode.Chaos;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Master_of_Mankind.Master_of_MankindCode.Powers;

public sealed class ChaosControllerPower : Master_of_MankindPower
{
    // 코른의 신성한 숫자: 단계마다 얻는 활력입니다.
    private const decimal KhorneVigorPerLevel = 8m;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterEnergyReset(Player player)
    {
        if (!ReferenceEquals(Owner.Player, player) || Owner.IsDead)
            return;

        if (Owner.GetPower<SlaaneshBoonPower>()?.Amount >= 2)
            await PlayerCmd.GainEnergy(1m, player);

        int khorne = Owner.GetPower<KhorneBoonPower>()?.Amount ?? 0;
        if (khorne > 0)
        {
            await PowerCmd.Apply<VigorPower>(
                new ThrowingPlayerChoiceContext(),
                Owner,
                khorne * KhorneVigorPerLevel,
                Owner,
                null,
                silent: false);
        }

        int nurgle = Owner.GetPower<NurgleBoonPower>()?.Amount ?? 0;
        if (nurgle > 0 && Owner.CombatState is { } combatState)
        {
            var context = new ThrowingPlayerChoiceContext();
            var enemies = combatState.GetOpponentsOf(Owner).Where(creature => creature.IsAlive).ToList();
            await PowerCmd.Apply<WeakPower>(context, enemies, 1m, Owner, null, silent: false);
            await PowerCmd.Apply<VulnerablePower>(context, enemies, 1m, Owner, null, silent: false);
            if (nurgle >= 2)
                await CreatureCmd.GainBlock(Owner, 9m, ValueProp.Unpowered, null);
        }
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (!ReferenceEquals(Owner.Player, player) || Owner.IsDead)
            return;

        int tzeentch = Owner.GetPower<TzeentchBoonPower>()?.Amount ?? 0;
        if (tzeentch > 0)
        {
            await CardPileCmd.Draw(choiceContext, tzeentch, player);
            int transformed = await ChaosTransformationService.PerformChosenTransformations(
                choiceContext,
                player,
                tzeentch);
            int deception = Owner.GetPower<DeceptionMarkPower>()?.Amount ?? 0;
            await ChaosTransformationService.PerformForcedTransformations(
                choiceContext,
                player,
                Math.Min(transformed, deception));
        }

        await ChooseBoons(choiceContext, Amount, null);
    }

    public override async Task BeforeFlush(PlayerChoiceContext choiceContext, Player player)
    {
        if (!ReferenceEquals(Owner.Player, player))
            return;

        await ChaosTransformationService.RevertHand(player);
        Owner.GetPower<ObsessionMarkPower>()?.ApplyAtTurnEnd();
    }

    public override async Task AfterCardChangedPiles(
        CardModel card,
        PileType oldPileType,
        AbstractModel? clonedBy) =>
        await ChaosTransformationService.RevertIfResolved(card, oldPileType);

    public async Task ChooseBoons(
        PlayerChoiceContext choiceContext,
        int count,
        CardModel? cardSource)
    {
        Player? player = Owner.Player;
        if (player is null)
            return;

        for (int i = 0; i < count; i++)
        {
            List<CardModel> choices = CreateBoonChoices(player);
            if (choices.Count == 0)
                return;

            LocString prompt = new("static_hover_tips", "MASTER_OF_MANKIND-CHAOS_BOON_PROMPT");
            CardSelectorPrefs prefs = new(prompt, 1)
            {
                Cancelable = false,
                PretendCardsCanBePlayed = true
            };
            CardModel? selected = (await CardSelectCmd.FromSimpleGrid(
                    choiceContext,
                    choices,
                    player,
                    prefs))
                .FirstOrDefault();
            if (selected is null)
                return;

            Flash();
            await ApplyBoon(choiceContext, selected, cardSource);
        }
    }

    private List<CardModel> CreateBoonChoices(Player player)
    {
        List<CardModel> choices = [];
        if ((Owner.GetPower<KhorneBoonPower>()?.Amount ?? 0) < 2)
            choices.Add(CreateChoice<KhorneBoonChoice>(player));
        if ((Owner.GetPower<TzeentchBoonPower>()?.Amount ?? 0) < 2)
            choices.Add(CreateChoice<TzeentchBoonChoice>(player));
        if ((Owner.GetPower<NurgleBoonPower>()?.Amount ?? 0) < 2)
            choices.Add(CreateChoice<NurgleBoonChoice>(player));
        if ((Owner.GetPower<SlaaneshBoonPower>()?.Amount ?? 0) < 2)
            choices.Add(CreateChoice<SlaaneshBoonChoice>(player));
        return choices;
    }

    private static CardModel CreateChoice<T>(Player player) where T : CardModel
    {
        CardModel card = ModelDb.Card<T>().ToMutable();
        card.Owner = player;
        return card;
    }

    private async Task ApplyBoon(
        PlayerChoiceContext choiceContext,
        CardModel selected,
        CardModel? cardSource)
    {
        switch (selected)
        {
            case KhorneBoonChoice:
                await ApplyPair<KhorneBoonPower, WrathMarkPower>(choiceContext, cardSource);
                break;
            case TzeentchBoonChoice:
                await ApplyPair<TzeentchBoonPower, DeceptionMarkPower>(choiceContext, cardSource);
                break;
            case NurgleBoonChoice:
                await ApplyPair<NurgleBoonPower, CorruptionMarkPower>(choiceContext, cardSource);
                break;
            case SlaaneshBoonChoice:
                await ApplyPair<SlaaneshBoonPower, ObsessionMarkPower>(choiceContext, cardSource);
                break;
        }
    }

    private async Task ApplyPair<TBoon, TMark>(
        PlayerChoiceContext choiceContext,
        CardModel? cardSource)
        where TBoon : PowerModel
        where TMark : PowerModel
    {
        await PowerCmd.Apply<TBoon>(choiceContext, Owner, 1m, Owner, cardSource, silent: false);
        await PowerCmd.Apply<TMark>(choiceContext, Owner, 1m, Owner, cardSource, silent: false);
    }
}
