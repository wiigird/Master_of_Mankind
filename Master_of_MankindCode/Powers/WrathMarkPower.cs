using Master_of_Mankind.Master_of_MankindCode.Cards;
using Master_of_Mankind.Master_of_MankindCode.Chaos;
using Master_of_Mankind.Master_of_MankindCode.Combat;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Master_of_Mankind.Master_of_MankindCode.Powers;

public sealed class WrathMarkPower : Master_of_MankindPower, IChaosMark
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public static void RecordDecreeExecution(Player player, DecreeCard decree)
    {
        if (decree.Type != CardType.Attack || player.PlayerCombatState is not { } playerState)
            return;

        PlayerCombatMemory.RecordExecutedAttack(player, playerState.TurnNumber);
    }

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (Owner.Side != side || Owner.Player is not { } player || Owner.CombatState is not { } combatState)
            return;

        int playedAttacks = CombatManager.Instance.History.Entries
            .OfType<CardPlayFinishedEntry>()
            .Count(entry => ReferenceEquals(entry.CardPlay.Card.Owner, player)
                            && entry.CardPlay.Card.Type == CardType.Attack
                            && entry.HappenedThisTurn(combatState));
        int executedAttacks = player.PlayerCombatState is { } playerState
            ? PlayerCombatMemory.GetExecutedAttacks(player, playerState.TurnNumber)
            : 0;
        int missing = Math.Max(0, Amount - playedAttacks - executedAttacks);
        if (missing <= 0 || Owner.IsDead)
            return;

        Flash();
        await CreatureCmd.SetCurrentHp(Owner, Owner.CurrentHp - missing * 4m);
    }
}
