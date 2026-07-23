using BaseLib.Utils;
using Master_of_Mankind.Master_of_MankindCode.Character;
using Master_of_Mankind.Master_of_MankindCode.Keywords;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Master_of_Mankind.Master_of_MankindCode.Cards.Rare;

/// <summary>
/// 한국어 이름: 익스터미나투스
/// 효과: 모든 적에게 큰 피해를 주고 소멸하는 희귀 칙령입니다.
/// 수치 조절: CanonicalVars와 OnUpgrade의 피해 수치를 변경합니다.
/// </summary>
[Pool(typeof(Master_of_MankindCardPool))]
public sealed class Exterminatus : DecreeCard
{
    public Exterminatus() : base(3, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies) { }

    public override HashSet<CardKeyword> CanonicalKeywords =>
        [EmperorKeywords.Decree, CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(42m, ValueProp.Move)];

    protected override async Task OnExecuteDecree(PlayerChoiceContext choiceContext, CardPlay cardPlay) =>
        await CommonActions.CardAttack(this, cardPlay)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(14m);
}
