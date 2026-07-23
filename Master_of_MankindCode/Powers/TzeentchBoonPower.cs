using MegaCrit.Sts2.Core.Entities.Powers;

namespace Master_of_Mankind.Master_of_MankindCode.Powers;

public sealed class TzeentchBoonPower : Master_of_MankindPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
}
