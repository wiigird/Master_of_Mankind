using BaseLib.Utils;
using Master_of_Mankind.Master_of_MankindCode.Character;
using Master_of_Mankind.Master_of_MankindCode.Keywords;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Master_of_Mankind.Master_of_MankindCode.Cards.Uncommon;

/// <summary>
/// 한국어 이름: 절대 명령
/// 효과: 보존되는 단일 피해 칙령입니다.
/// 수치 조절: CanonicalVars와 OnUpgrade의 피해 수치를 변경합니다.
/// </summary>
[Pool(typeof(Master_of_MankindCardPool))]
public sealed class AbsoluteCommand : DecreeCard
{
    public AbsoluteCommand() : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy) { }

    public override HashSet<CardKeyword> CanonicalKeywords =>
        [EmperorKeywords.Decree, CardKeyword.Retain];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(27m, ValueProp.Move)];

    protected override async Task OnExecuteDecree(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await CommonActions.CardAttack(this, cardPlay)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(8m);
}
