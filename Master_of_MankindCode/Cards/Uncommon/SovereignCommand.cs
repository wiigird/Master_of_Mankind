using BaseLib.Utils;
using Master_of_Mankind.Master_of_MankindCode.Character;
using Master_of_Mankind.Master_of_MankindCode.Decree;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Master_of_Mankind.Master_of_MankindCode.Cards.Uncommon;

/// <summary>
/// 한국어 이름: 군주의 명령
/// 효과: 준비된 칙령 중 원하는 수만큼 선택해 즉시 집행하고 소멸합니다.
/// 수치 조절: CardCost와 UpgradeCostReduction을 변경하면 기본 및 강화 비용이 바뀝니다.
/// </summary>
[Pool(typeof(Master_of_MankindCardPool))]
public sealed class SovereignCommand : Master_of_MankindCard
{
    // 밸런스 조정: 기본 비용과 강화 시 감소할 비용입니다.
    private const int CardCost = 1;
    private const int UpgradeCostReduction = 1;

    public SovereignCommand()
        : base(CardCost, CardType.Skill, CardRarity.Uncommon, TargetType.Self) { }

    public override HashSet<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override bool IsPlayable => DecreeManager.GetPrepared(Owner).Count > 0;

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) =>
        DecreeManager.ResolveImmediately(choiceContext, Owner);

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-UpgradeCostReduction);
}
