using BaseLib.Utils;
using Master_of_Mankind.Master_of_MankindCode.Character;
using Master_of_Mankind.Master_of_MankindCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Master_of_Mankind.Master_of_MankindCode.Cards.Uncommon;

/// <summary>
/// 한국어 이름: 하나의 진정한 갑주
/// 효과: 턴 종료 시 카드를 보존하고 보존 카드 사용 시 방어도를 얻는 파워입니다.
/// 수치 조절: CanonicalVars의 파워 및 방어도와 OnUpgrade 수치를 변경합니다.
/// </summary>
[Pool(typeof(Master_of_MankindCardPool))]
public sealed class ArmourOfOneTrueKing : Master_of_MankindCard
{
    public ArmourOfOneTrueKing() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.None) { }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<ArmourOfOneTrueKingPower>(1m),
        new DynamicVar("Block", 4m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArmourOfOneTrueKingPower? power = await PowerCmd.Apply<ArmourOfOneTrueKingPower>(
            choiceContext,
            Owner.Creature,
            1m,
            Owner.Creature,
            this,
            silent: false);
        if (power is not null)
            power.DynamicVars["Block"].UpgradeValueBy(DynamicVars["Block"].BaseValue);
    }

    protected override void OnUpgrade() => DynamicVars["Block"].UpgradeValueBy(2m);
}
