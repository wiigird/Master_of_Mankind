using BaseLib.Utils;
using Master_of_Mankind.Master_of_MankindCode.Character;
using Master_of_Mankind.Master_of_MankindCode.Powers;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Master_of_Mankind.Master_of_MankindCode.Cards.Uncommon;

/// <summary>
/// 한국어 이름: 인내의 방패
/// 효과: 직전 턴에 보존한 카드 한 장당 다음 턴 시작에 방어도 3을 얻는 고급 파워 카드입니다.
/// 수치 조절: BlockPerRetainedCard와 UpgradeBlock을 변경하면 카드당 방어도가 바뀝니다.
/// </summary>
[Pool(typeof(Master_of_MankindCardPool))]
public sealed class ShieldOfPatience : Master_of_MankindCard
{
    // 밸런스 조정: 보존 카드 한 장당 방어도와 강화 시 추가 방어도입니다.
    private const decimal BlockPerRetainedCard = 3m;
    private const decimal UpgradeBlock = 1m;

    public ShieldOfPatience() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.None) { }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new PowerVar<ShieldOfPatiencePower>(BlockPerRetainedCard)];

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) =>
        CommonActions.ApplySelf<ShieldOfPatiencePower>(choiceContext, this);

    protected override void OnUpgrade() =>
        DynamicVars["ShieldOfPatiencePower"].UpgradeValueBy(UpgradeBlock);
}
