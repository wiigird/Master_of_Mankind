using Master_of_Mankind.Master_of_MankindCode.Retain;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Master_of_Mankind.Master_of_MankindCode.Powers;

public sealed class ArmourOfOneTrueKingPower : Master_of_MankindPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Block", 0m)];

    public override async Task BeforeFlush(PlayerChoiceContext choiceContext, Player player)
    {
        if (!ReferenceEquals(Owner.Player, player) || player.PlayerCombatState is not { } playerState)
            return;

        List<CardModel> hand = playerState.Hand.Cards.ToList();
        int selectionCount = Math.Min(Amount, hand.Count);
        if (selectionCount <= 0)
            return;

        LocString prompt = new("static_hover_tips", "MASTER_OF_MANKIND-ARMOUR_RETAIN_PROMPT");
        CardSelectorPrefs prefs = new(prompt, selectionCount, selectionCount)
        {
            Cancelable = false,
            PretendCardsCanBePlayed = true
        };
        IEnumerable<CardModel> selected = await CardSelectCmd.FromSimpleGrid(
            choiceContext,
            hand,
            player,
            prefs);
        foreach (CardModel card in selected)
            card.GiveSingleTurnRetain();
    }

    public override Task AfterFlush(
        PlayerChoiceContext choiceContext,
        Player player,
        IReadOnlyCollection<CardModel> flushedCards,
        IReadOnlyCollection<CardModel> retainedCards)
    {
        if (!ReferenceEquals(Owner.Player, player))
            return Task.CompletedTask;

        foreach (CardModel card in flushedCards)
            ArmourRetainState.SetRetained(card, false);
        foreach (CardModel card in retainedCards)
            ArmourRetainState.SetRetained(card, true);

        return Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        CardModel card = cardPlay.Card;
        if (!ReferenceEquals(card.Owner, Owner.Player) || !ArmourRetainState.IsRetained(card))
            return;

        ArmourRetainState.SetRetained(card, false);
        if (Owner.Player is not { } player || player.PlayerCombatState is not { } playerState)
            return;

        int turnNumber = playerState.TurnNumber;
        if (ArmourRetainState.HasTriggeredThisTurn(player, turnNumber))
            return;

        ArmourRetainState.MarkTriggered(player, turnNumber);
        Flash();
        await CreatureCmd.GainBlock(
            Owner,
            DynamicVars["Block"].BaseValue,
            ValueProp.Unpowered,
            cardPlay);
    }
}
