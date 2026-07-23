using Master_of_Mankind.Master_of_MankindCode.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Master_of_Mankind.Master_of_MankindCode.Powers;

public sealed class MasterOfMankindPower : Master_of_MankindPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Block", 0m)];

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!ReferenceEquals(cardPlay.Card.Owner, Owner.Player)
            || cardPlay.Resources.EnergySpent < 2
            || Owner.Player?.PlayerCombatState is not { } playerState
            || PlayerCombatMemory.MasterPowerTriggeredThisTurn(Owner.Player, playerState.TurnNumber))
            return;

        PlayerCombatMemory.MarkMasterPowerTriggered(Owner.Player, playerState.TurnNumber);
        Flash();
        await CardPileCmd.Draw(choiceContext, Amount, Owner.Player);

        if (cardPlay.Card.Type == CardType.Attack)
        {
            await CreatureCmd.GainBlock(
                Owner,
                DynamicVars["Block"].BaseValue,
                ValueProp.Unpowered,
                cardPlay);
        }
    }
}
