using Master_of_Mankind.Master_of_MankindCode.Chaos;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace Master_of_Mankind.Master_of_MankindCode.Powers;

public sealed class DeceptionMarkPower : Master_of_MankindPower, IChaosMark
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;
}
