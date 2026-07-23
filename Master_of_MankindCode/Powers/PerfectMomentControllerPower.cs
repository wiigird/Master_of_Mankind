using Master_of_Mankind.Master_of_MankindCode.Retain;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Master_of_Mankind.Master_of_MankindCode.Powers;

public sealed class PerfectMomentControllerPower : Master_of_MankindPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    protected override bool IsVisibleInternal => false;
    public override bool ShouldPlayVfx => false;

    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (ReferenceEquals(cardPlay.Card.Owner, Owner.Player)
            && TemporaryRetainState.IsActive(cardPlay.Card))
            TemporaryRetainState.Clear(cardPlay.Card);

        return Task.CompletedTask;
    }
}
