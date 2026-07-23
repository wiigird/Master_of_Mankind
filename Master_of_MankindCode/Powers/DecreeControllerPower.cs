using Master_of_Mankind.Master_of_MankindCode.Decree;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Rooms;

namespace Master_of_Mankind.Master_of_MankindCode.Powers;

public sealed class DecreeControllerPower : Master_of_MankindPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    protected override bool IsVisibleInternal => false;
    public override bool ShouldPlayVfx => false;

    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player) =>
        ReferenceEquals(Owner.Player, player)
            ? DecreeManager.ResolveReady(choiceContext, player)
            : Task.CompletedTask;

    public override Task AfterCombatEnd(CombatRoom room)
    {
        if (Owner.Player is { } player)
            DecreeManager.Clear(player);

        return Task.CompletedTask;
    }
}
