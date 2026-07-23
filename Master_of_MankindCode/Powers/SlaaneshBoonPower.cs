using BaseLib.Hooks;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace Master_of_Mankind.Master_of_MankindCode.Powers;

public sealed class SlaaneshBoonPower : Master_of_MankindPower, IMaxHandSizeModifier
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public int ModifyMaxHandSize(Player player, int currentMaxHandSize) =>
        ReferenceEquals(Owner.Player, player) ? currentMaxHandSize + 5 : currentMaxHandSize;
}
