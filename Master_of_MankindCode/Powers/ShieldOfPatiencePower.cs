using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Master_of_Mankind.Master_of_MankindCode.Powers;

/// <summary>
/// 한국어 이름: 인내의 방패
/// 효과: 직전 턴에 보존한 카드 수에 비례해 다음 턴 시작 시 방어도를 얻습니다.
/// 수치 조절: 카드 클래스의 BlockPerRetainedCard를 변경하면 카드당 방어도가 바뀝니다.
/// </summary>
public sealed class ShieldOfPatiencePower : Master_of_MankindPower
{
    private static readonly SavedSpireField<Player, int> RetainedCardCount =
        new(() => 0, "MasterOfMankind_ShieldOfPatienceRetainedCount");

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override Task AfterFlush(
        PlayerChoiceContext choiceContext,
        Player player,
        IReadOnlyCollection<CardModel> flushedCards,
        IReadOnlyCollection<CardModel> retainedCards)
    {
        if (ReferenceEquals(Owner.Player, player))
            RetainedCardCount.Set(player, retainedCards.Count);
        return Task.CompletedTask;
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (!ReferenceEquals(Owner.Player, player))
            return;

        int count = RetainedCardCount.Get(player);
        if (count <= 0)
            return;

        Flash();
        await CreatureCmd.GainBlock(
            Owner,
            count * Amount,
            ValueProp.Unpowered,
            null);
    }
}
