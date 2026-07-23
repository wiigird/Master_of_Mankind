using BaseLib.Utils;
using Master_of_Mankind.Master_of_MankindCode.Character;
using Master_of_Mankind.Master_of_MankindCode.Powers;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Master_of_Mankind.Master_of_MankindCode.Cards.Uncommon;

/// <summary>
/// 한국어 이름: 응축된 분노
/// 효과: 공격 카드가 보존된 채 턴을 넘길 때마다 그 카드의 피해량을 3 증가시키는 고급 파워 카드입니다.
/// 수치 조절: DamageBonus와 UpgradeBonus를 변경하면 기본 및 강화 피해 보너스가 바뀝니다.
/// </summary>
[Pool(typeof(Master_of_MankindCardPool))]
public sealed class CondensedWrath : Master_of_MankindCard
{
    // 밸런스 조정: 보존된 공격 카드의 피해 보너스와 강화 시 추가 보너스입니다.
    private const decimal DamageBonus = 3m;
    private const decimal UpgradeBonus = 1m;

    public CondensedWrath() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.None) { }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new PowerVar<CondensedWrathPower>(DamageBonus)];

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) =>
        CommonActions.ApplySelf<CondensedWrathPower>(choiceContext, this);

    protected override void OnUpgrade() =>
        DynamicVars["CondensedWrathPower"].UpgradeValueBy(UpgradeBonus);
}
