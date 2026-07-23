using BaseLib.Utils;
using Master_of_Mankind.Master_of_MankindCode.Character;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Master_of_Mankind.Master_of_MankindCode.Cards.Token;

/// <summary>
/// 한국어 이름: 군주의 의지
/// 효과: 황금 옥좌 선택으로 에너지 1을 얻습니다.
/// 수치 조절: OnPlay의 에너지 획득량을 변경합니다.
/// </summary>
[Pool(typeof(Master_of_MankindCardPool))]
public sealed class GoldenThroneEnergyChoice()
    : Master_of_MankindCard(0, CardType.Skill, CardRarity.Token, TargetType.None, false)
{
    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) => Task.CompletedTask;
}
