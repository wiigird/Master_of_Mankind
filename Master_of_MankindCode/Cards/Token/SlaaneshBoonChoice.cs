using BaseLib.Utils;
using Master_of_Mankind.Master_of_MankindCode.Character;
using Master_of_Mankind.Master_of_MankindCode.Powers;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;

namespace Master_of_Mankind.Master_of_MankindCode.Cards.Token;

/// <summary>
/// 한국어 이름: 슬라네쉬의 권능
/// 효과: 슬라네쉬와 집착 표식을 한 단계 얻는 선택용 카드입니다.
/// 수치 조절: ChaosControllerPower와 ObsessionState의 단계별 수치를 변경합니다.
/// </summary>
[Pool(typeof(Master_of_MankindCardPool))]
public sealed class SlaaneshBoonChoice()
    : Master_of_MankindCard(0, CardType.Skill, CardRarity.Token, TargetType.None, false)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<SlaaneshBoonPower>(),
        HoverTipFactory.FromPower<ObsessionMarkPower>()
    ];

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) => Task.CompletedTask;
}
