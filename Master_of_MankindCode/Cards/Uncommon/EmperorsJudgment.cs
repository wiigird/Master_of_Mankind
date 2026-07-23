using BaseLib.Utils;
using Master_of_Mankind.Master_of_MankindCode.Character;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Master_of_Mankind.Master_of_MankindCode.Cards.Uncommon;

/// <summary>
/// 한국어 이름: 황제의 심판
/// 효과: 에너지를 모두 소모하고 에너지당 피해를 합산해 모든 적에게 한 번씩 줍니다.
/// 수치 조절: DamagePerEnergy와 UpgradeDamage를 변경하면 에너지당 기본 및 강화 피해가 바뀝니다.
/// </summary>
[Pool(typeof(Master_of_MankindCardPool))]
public sealed class EmperorsJudgment : Master_of_MankindCard
{
    // 밸런스 조정: 소모한 에너지 1당 피해와 강화 시 추가 피해입니다.
    private const decimal DamagePerEnergy = 6m;
    private const decimal UpgradeDamage = 2m;

    public EmperorsJudgment() : base(0, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies) { }

    protected override bool HasEnergyCostX => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(DamagePerEnergy, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (CombatState is null)
            return;

        decimal totalDamage = DynamicVars.Damage.BaseValue * ResolveEnergyXValue();
        await DamageCmd.Attack(totalDamage)
            .FromCard(this)
            .TargetingAllOpponents(CombatState)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(UpgradeDamage);
}
