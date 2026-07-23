using BaseLib.Utils;
using Master_of_Mankind.Master_of_MankindCode.Character;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Master_of_Mankind.Master_of_MankindCode.Cards.Basic;

/// <summary>
/// 한국어 이름: 복종 칙령
/// 효과: 칙령으로 단일 피해를 주고 취약을 부여합니다.
/// 수치 조절: CanonicalVars와 OnUpgrade의 피해 및 취약 수치를 변경합니다.
/// </summary>
[Pool(typeof(Master_of_MankindCardPool))]
public sealed class ComplianceDecree : DecreeCard
{
    public ComplianceDecree() : base(1, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy) { }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(14m, ValueProp.Move),
        new PowerVar<VulnerablePower>(1)
    ];

    protected override async Task OnExecuteDecree(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await CommonActions.CardAttack(this, cardPlay)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
        await CommonActions.Apply<VulnerablePower>(choiceContext, cardPlay.Target, this);
    }

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromPower<VulnerablePower>()];

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(4m);
}
