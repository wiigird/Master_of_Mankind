using BaseLib.Utils;
using Master_of_Mankind.Master_of_MankindCode.Character;
using Master_of_Mankind.Master_of_MankindCode.Foresight;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Master_of_Mankind.Master_of_MankindCode.Cards.Uncommon;

/// <summary>
/// 한국어 이름: 필연적 승리
/// 효과: 대상의 공개된 미래 행동 수에 비례해 피해가 증가합니다.
/// 수치 조절: CanonicalVars의 기본 피해, 행동당 추가 피해, 최대 적용 수를 변경합니다.
/// </summary>
[Pool(typeof(Master_of_MankindCardPool))]
public sealed class InevitableVictory : Master_of_MankindCard
{
    public InevitableVictory() : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy) { }

    protected override bool ShouldGlowGoldInternal => CombatState is { } state
        && ForesightPredictionService.AnyRevealedActionIsKnown(state);

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ..MakeCalculatedDamage(
            14,
            CountRevealedActions,
            mult: 4,
            props: ValueProp.Move)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await CommonActions.CardAttack(this, cardPlay)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["CalculationBase"].UpgradeValueBy(4m);
        DynamicVars["ExtraDamage"].UpgradeValueBy(1m);
    }

    private static decimal CountRevealedActions(CardModel card, Creature? target) =>
        target is null ? 0m : Math.Min(ForesightPredictionService.CountRevealedActions(target), 3);
}
