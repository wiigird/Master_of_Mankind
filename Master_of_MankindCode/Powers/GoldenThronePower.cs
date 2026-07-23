using Master_of_Mankind.Master_of_MankindCode.Cards.Token;
using Master_of_Mankind.Master_of_MankindCode.Combat;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Master_of_Mankind.Master_of_MankindCode.Powers;

public sealed class GoldenThronePower : Master_of_MankindPower
{
    private enum ThroneChoice
    {
        None = -1,
        Energy,
        Draw,
        Block
    }

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

    // 밸런스 조정: 자신의 턴 종료 시 회복하는 체력입니다.
    protected override IEnumerable<DynamicVar> CanonicalVars => [new HealVar(4m)];

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (Owner.Side != side || Owner.IsDead)
            return;

        Flash();
        await CreatureCmd.Heal(Owner, DynamicVars.Heal.BaseValue);
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (!ReferenceEquals(Owner.Player, player))
            return;

        int instanceIndex = Owner.GetPowerInstances(Id)
            .OfType<GoldenThronePower>()
            .ToList()
            .IndexOf(this);
        ThroneChoice previous = (ThroneChoice)PlayerCombatMemory.GetGoldenThronePreviousChoice(
            player,
            instanceIndex);
        List<CardModel> choices = [];
        if (previous != ThroneChoice.Energy)
            choices.Add(CreateChoice<GoldenThroneEnergyChoice>(player));
        if (previous != ThroneChoice.Draw)
            choices.Add(CreateChoice<GoldenThroneDrawChoice>(player));
        if (previous != ThroneChoice.Block)
            choices.Add(CreateChoice<GoldenThroneBlockChoice>(player));

        LocString prompt = new("static_hover_tips", "MASTER_OF_MANKIND-GOLDEN_THRONE_PROMPT");
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

        ThroneChoice choice = selected switch
        {
            GoldenThroneEnergyChoice => ThroneChoice.Energy,
            GoldenThroneDrawChoice => ThroneChoice.Draw,
            GoldenThroneBlockChoice => ThroneChoice.Block,
            _ => ThroneChoice.None
        };
        if (choice == ThroneChoice.None)
            return;

        PlayerCombatMemory.SetGoldenThronePreviousChoice(player, instanceIndex, (int)choice);
        Flash();
        switch (choice)
        {
            case ThroneChoice.Energy:
                await PlayerCmd.GainEnergy(1m, player);
                break;
            case ThroneChoice.Draw:
                await CardPileCmd.Draw(choiceContext, 1m, player);
                break;
            case ThroneChoice.Block:
                await CreatureCmd.GainBlock(player.Creature, 8m, ValueProp.Unpowered, null);
                break;
        }
    }

    private static CardModel CreateChoice<T>(Player player) where T : CardModel
    {
        CardModel card = ModelDb.Card<T>().ToMutable();
        card.Owner = player;
        return card;
    }
}
