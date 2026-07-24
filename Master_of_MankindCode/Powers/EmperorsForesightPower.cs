using Master_of_Mankind.Master_of_MankindCode.Foresight;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Master_of_Mankind.Master_of_MankindCode.Powers;

public class EmperorsForesightPower : Master_of_MankindPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        if (Owner.CombatState is { } combatState)
            await ForesightTimelineUi.RefreshAllEnemies(combatState);
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (ReferenceEquals(Owner.Player, player) && Owner.CombatState is { } combatState)
            await ForesightTimelineUi.RefreshAllEnemies(combatState);
    }

    public override async Task AfterDeath(
        PlayerChoiceContext choiceContext,
        Creature creature,
        bool wasRemovalPrevented,
        float deathAnimLength)
    {
        if (!ReferenceEquals(Owner, creature) || wasRemovalPrevented)
            return;

        var combatState = Owner.CombatState;
        await PowerCmd.Remove(this);
        if (combatState is not null)
            await ForesightTimelineUi.RefreshAllEnemies(combatState);
    }
}
