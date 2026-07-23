using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Master_of_Mankind.Master_of_MankindCode.Powers;

public sealed class ImperialTruthPower : Master_of_MankindPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (!ReferenceEquals(Owner.Player, player)
            || Owner.GetPower<ArtifactPower>() is not null)
            return;

        Flash();
        await PowerCmd.Apply<ArtifactPower>(
            choiceContext,
            Owner,
            1m,
            Owner,
            null,
            silent: false);
    }
}
