using Master_of_Mankind.Master_of_MankindCode.Chaos;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Master_of_Mankind.Master_of_MankindCode.Powers;

public sealed class CorruptionMarkPower : Master_of_MankindPower, IChaosMark
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (Owner.Side != side || Owner.IsDead)
            return;

        Flash();
        await CreatureCmd.SetCurrentHp(Owner, Owner.CurrentHp - Amount);
    }
}
