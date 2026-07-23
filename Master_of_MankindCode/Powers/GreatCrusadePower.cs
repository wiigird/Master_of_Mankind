using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Master_of_Mankind.Master_of_MankindCode.Powers;

public sealed class GreatCrusadePower : Master_of_MankindPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Block", 0m),
        new DynamicVar("Damage", 0m)
    ];

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (!ReferenceEquals(Owner.Player, player))
            return;

        decimal block = DynamicVars["Block"].BaseValue;
        if (block <= 0m)
            return;

        Flash();
        await CreatureCmd.GainBlock(Owner, block, ValueProp.Unpowered, null);
    }

    public async Task AfterDecreeExecuted(PlayerChoiceContext choiceContext)
    {
        decimal damage = DynamicVars["Damage"].BaseValue;
        if (damage <= 0m || Owner.CombatState is not { } combatState)
            return;

        Flash();
        await CreatureCmd.Damage(
            choiceContext,
            combatState.GetOpponentsOf(Owner).Where(creature => creature.IsAlive),
            damage,
            ValueProp.Unpowered,
            Owner);
    }
}
