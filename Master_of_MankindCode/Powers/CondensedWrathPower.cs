using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Master_of_Mankind.Master_of_MankindCode.Powers;

/// <summary>
/// 한국어 이름: 응축된 분노
/// 효과: 공격 카드가 보존된 채 턴을 넘길 때마다 그 카드의 피해량을 Amount만큼 누적 증가시킵니다.
/// 수치 조절: 카드 클래스의 DamageBonus를 변경하면 기본 보너스가 바뀝니다.
/// </summary>
public sealed class CondensedWrathPower : Master_of_MankindPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override Task AfterFlush(
        PlayerChoiceContext choiceContext,
        Player player,
        IReadOnlyCollection<CardModel> flushedCards,
        IReadOnlyCollection<CardModel> retainedCards)
    {
        if (!ReferenceEquals(Owner.Player, player))
            return Task.CompletedTask;

        foreach (CardModel card in retainedCards.Where(card => card.Type == CardType.Attack))
            card.DynamicVars.Damage.UpgradeValueBy(Amount);

        return Task.CompletedTask;
    }
}
