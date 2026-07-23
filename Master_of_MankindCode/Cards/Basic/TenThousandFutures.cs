using BaseLib.Utils;
using Master_of_Mankind.Master_of_MankindCode.Character;
using Master_of_Mankind.Master_of_MankindCode.Extensions;
using Master_of_Mankind.Master_of_MankindCode.Powers;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Master_of_Mankind.Master_of_MankindCode.Cards.Basic;

/// <summary>
/// 한국어 이름: 만 갈래의 미래
/// 효과: 미래 행동 3개를 공개하고 턴 종료 시 카드를 최대 2장 보존하는 고대 카드입니다.
/// 수치 조절: CardCost와 PredictionDepth 상수 및 파워의 최대 선택 수를 변경합니다.
/// </summary>
[Pool(typeof(Master_of_MankindCardPool))]
public sealed class TenThousandFutures()
    : Master_of_MankindCard(CardCost, CardType.Power, CardRarity.Ancient, TargetType.None)
{
    // 밸런스 조정: 카드 비용과 공개할 미래 행동 수는 아래 두 상수에서 변경합니다.
    private const int CardCost = 1;
    private const decimal PredictionDepth = 3m;

    public override int MaxUpgradeLevel => 0;
    public override HashSet<CardKeyword> CanonicalKeywords => [CardKeyword.Innate];

    public override string PortraitPath => "ten_thousand_futures.png".BigCardImagePath();
    public override string CustomPortraitPath => "ten_thousand_futures.png".BigCardImagePath();

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new PowerVar<TenThousandFuturesPower>(PredictionDepth)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) =>
        await CommonActions.ApplySelf<TenThousandFuturesPower>(choiceContext, this);
}
