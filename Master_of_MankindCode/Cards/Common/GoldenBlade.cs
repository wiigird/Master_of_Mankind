using BaseLib.Utils;
using Master_of_Mankind.Master_of_MankindCode.Character;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Master_of_Mankind.Master_of_MankindCode.Cards.Common;

/// <summary>
/// 한국어 이름: 황금 칼날
/// 효과: 모든 적에게 피해를 주는 일반 광역 공격 카드입니다.
/// 수치 조절: BaseDamage와 UpgradeDamage를 변경하면 기본 및 강화 피해량이 바뀝니다.
/// </summary>
[Pool(typeof(Master_of_MankindCardPool))]
public sealed class GoldenBlade : Master_of_MankindCard
{
    // 밸런스 조정: 기본 피해량과 강화 시 추가 피해량입니다.
    private const decimal BaseDamage = 6m;
    private const decimal UpgradeDamage = 3m;

    public GoldenBlade() : base(1, CardType.Attack, CardRarity.Common, TargetType.AllEnemies) { }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(BaseDamage, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CommonActions.CardAttack(this, cardPlay)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(UpgradeDamage);
}
