using BaseLib.Utils;
using Master_of_Mankind.Master_of_MankindCode.Character;
using Master_of_Mankind.Master_of_MankindCode.Foresight;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Master_of_Mankind.Master_of_MankindCode.Cards.Common;

/// <summary>
/// 한국어 이름: 예지의 일격
/// 효과: 피해를 주고 대상의 다음 행동이 공격이면 약화를 부여합니다.
/// 수치 조절: CanonicalVars의 피해 및 약화 수치와 OnUpgrade를 변경합니다.
/// </summary>
[Pool(typeof(Master_of_MankindCardPool))]
public sealed class PrescientStrike : Master_of_MankindCard
{
    public PrescientStrike() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy) { }

    protected override bool ShouldGlowGoldInternal => CombatState is { } state
        && ForesightPredictionService.AnyNextRevealedActionIsAttack(state);

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(7m, ValueProp.Move),
        new PowerVar<WeakPower>(1m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        bool shouldApplyWeak = ForesightPredictionService.IsNextRevealedActionAttack(cardPlay.Target);

        await CommonActions.CardAttack(this, cardPlay)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);

        if (shouldApplyWeak && cardPlay.Target.IsAlive)
            await CommonActions.Apply<WeakPower>(choiceContext, cardPlay.Target, this);
    }

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromPower<WeakPower>()];

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3m);
}
