using BaseLib.Utils;
using Master_of_Mankind.Master_of_MankindCode.Character;
using Master_of_Mankind.Master_of_MankindCode.Powers;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Master_of_Mankind.Master_of_MankindCode.Cards.Basic;

/// <summary>
/// 한국어 이름: 황제의 예지
/// 효과: 모든 적의 미래 행동 3개를 공개하는 시작 파워입니다.
/// 수치 조절: CanonicalVars의 PowerVar 수치와 OnUpgrade의 비용을 변경합니다.
/// </summary>
[Pool(typeof(Master_of_MankindCardPool))]
public sealed class EmperorsForesight() : Master_of_MankindCard(3, CardType.Power, CardRarity.Basic, TargetType.None)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<EmperorsForesightPower>(3)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) =>
        await CommonActions.ApplySelf<EmperorsForesightPower>(choiceContext, this);

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
