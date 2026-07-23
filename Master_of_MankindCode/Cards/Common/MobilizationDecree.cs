using BaseLib.Utils;
using Master_of_Mankind.Master_of_MankindCode.Character;
using Master_of_Mankind.Master_of_MankindCode.Keywords;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Master_of_Mankind.Master_of_MankindCode.Cards.Common;

/// <summary>
/// 한국어 이름: 동원 칙령
/// 효과: 비용 없이 준비하고 집행 시 카드를 뽑은 뒤 소멸합니다.
/// 수치 조절: CanonicalVars와 OnUpgrade의 드로우 수를 변경합니다.
/// </summary>
[Pool(typeof(Master_of_MankindCardPool))]
public sealed class MobilizationDecree : DecreeCard
{
    public MobilizationDecree() : base(0, CardType.Skill, CardRarity.Common, TargetType.Self) { }

    public override HashSet<CardKeyword> CanonicalKeywords =>
        [EmperorKeywords.Decree, CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(2)];

    protected override async Task OnExecuteDecree(PlayerChoiceContext choiceContext, CardPlay cardPlay) =>
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.IntValue, Owner);

    protected override void OnUpgrade() => DynamicVars.Cards.UpgradeValueBy(1);
}
