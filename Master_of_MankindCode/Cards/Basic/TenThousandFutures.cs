using BaseLib.Utils;
using Master_of_Mankind.Master_of_MankindCode.Character;
using Master_of_Mankind.Master_of_MankindCode.Extensions;
using Master_of_Mankind.Master_of_MankindCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Master_of_Mankind.Master_of_MankindCode.Cards.Basic;

/// <summary>
/// 한국어 이름: 만 갈래의 미래
/// 효과: 미래 행동 3개를 공개하고 턴 종료 시 카드를 최대 2장 보존하며, 강화 시 모든 적에게 약화와 취약을 부여합니다.
/// 수치 조절: CardCost, PredictionDepth와 CanonicalVars의 Power 수치를 변경합니다.
/// </summary>
[Pool(typeof(Master_of_MankindCardPool))]
public sealed class TenThousandFutures()
    : Master_of_MankindCard(CardCost, CardType.Power, CardRarity.Ancient, TargetType.None)
{
    // 밸런스 조정: 카드 비용과 공개할 미래 행동 수는 아래 두 상수에서 변경합니다.
    private const int CardCost = 1;
    private const decimal PredictionDepth = 3m;

    public override HashSet<CardKeyword> CanonicalKeywords => [CardKeyword.Innate];

    public override string PortraitPath => "ten_thousand_futures.png".BigCardImagePath();
    public override string CustomPortraitPath => "ten_thousand_futures.png".BigCardImagePath();

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new PowerVar<TenThousandFuturesPower>(PredictionDepth), new DynamicVar("Power", 2m)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CommonActions.ApplySelf<TenThousandFuturesPower>(choiceContext, this);

        if (!IsUpgraded || CombatState is not { } combatState)
            return;

        foreach (var enemy in combatState.HittableEnemies.ToList())
        {
            await PowerCmd.Apply<WeakPower>(choiceContext, enemy, DynamicVars["Power"].BaseValue, Owner.Creature, this);
            await PowerCmd.Apply<VulnerablePower>(choiceContext, enemy, DynamicVars["Power"].BaseValue, Owner.Creature, this);
        }
    }
}
