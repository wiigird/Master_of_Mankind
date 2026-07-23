using Master_of_Mankind.Master_of_MankindCode.Chaos;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Master_of_Mankind.Master_of_MankindCode.Powers;

public sealed class ObsessionMarkPower : Master_of_MankindPower, IChaosMark
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public void ApplyAtTurnEnd()
    {
        if (Owner.Player?.PlayerCombatState is not { } playerState)
            return;

        List<CardModel> eligible = playerState.Hand.Cards
            .Where(card => card.Type is not (CardType.Curse or CardType.Status))
            .Where(card => !ObsessionState.IsObsessed(card))
            .ToList();
        if (eligible.Count == 0)
            return;

        Owner.Player.RunState.Rng.CombatCardSelection.Shuffle(eligible);
        foreach (CardModel card in eligible.Take(Amount))
            ObsessionState.Apply(card);
        Flash();
    }

    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (ReferenceEquals(cardPlay.Card.Owner, Owner.Player))
            ObsessionState.Clear(cardPlay.Card);
        return Task.CompletedTask;
    }

    public override Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? clonedBy)
    {
        if (ReferenceEquals(card.Owner, Owner.Player)
            && card.Pile?.Type is PileType.Exhaust or PileType.None)
            ObsessionState.Clear(card);
        return Task.CompletedTask;
    }
}
