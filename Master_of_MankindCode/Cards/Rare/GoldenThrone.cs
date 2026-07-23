using BaseLib.Utils;
using Master_of_Mankind.Master_of_MankindCode.Character;
using Master_of_Mankind.Master_of_MankindCode.Powers;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Master_of_Mankind.Master_of_MankindCode.Cards.Rare;

/// <summary>
/// 한국어 이름: 황금 옥좌
/// 효과: 매 턴 에너지, 드로우, 방어 중 하나를 선택하는 희귀 파워입니다.
/// 수치 조절: 선택 카드와 GoldenThronePower의 수치 및 OnUpgrade 비용을 변경합니다.
/// </summary>
[Pool(typeof(Master_of_MankindCardPool))]
public sealed class GoldenThrone : Master_of_MankindCard
{
    public GoldenThrone() : base(3, CardType.Power, CardRarity.Rare, TargetType.None) { }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<GoldenThronePower>(1m)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) =>
        await CommonActions.ApplySelf<GoldenThronePower>(choiceContext, this);

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
