using BaseLib.Utils;
using Master_of_Mankind.Master_of_MankindCode.Character;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Master_of_Mankind.Master_of_MankindCode.Cards.Common;

/// <summary>
/// 한국어 이름: 기다리는 창격
/// 효과: 보존된 턴마다 피해량이 증가하는 공격 카드입니다.
/// 수치 조절: CanonicalVars의 Damage와 RetainIncrease 및 OnUpgrade 수치를 변경합니다.
/// </summary>
[Pool(typeof(Master_of_MankindCardPool))]
public sealed class WaitingSpearstrike : Master_of_MankindCard
{
    private const string RetainIncreaseKey = "RetainIncrease";
    private const string RetainTriggeredKey = "RetainTriggered";

    public WaitingSpearstrike() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy) { }

    public override HashSet<CardKeyword> CanonicalKeywords => [CardKeyword.Retain];

    protected override bool ShouldGlowGoldInternal => DynamicVars[RetainTriggeredKey].BaseValue > 0m;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(7m, ValueProp.Move),
        new DynamicVar(RetainIncreaseKey, 3m),
        new DynamicVar(RetainTriggeredKey, 0m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await CommonActions.CardAttack(this, cardPlay)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
    }

    public override Task AfterFlush(
        PlayerChoiceContext choiceContext,
        Player player,
        IReadOnlyCollection<CardModel> flushedCards,
        IReadOnlyCollection<CardModel> retainedCards)
    {
        if (retainedCards.Contains(this))
        {
            DynamicVars[RetainTriggeredKey].BaseValue = 1m;
            DynamicVars.Damage.UpgradeValueBy(DynamicVars[RetainIncreaseKey].BaseValue);
        }

        return Task.CompletedTask;
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
        DynamicVars[RetainIncreaseKey].UpgradeValueBy(1m);
    }
}
