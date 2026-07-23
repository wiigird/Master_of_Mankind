using Master_of_Mankind.Master_of_MankindCode.Extensions;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Rooms;
using Master_of_Mankind.Master_of_MankindCode.Rewards;

namespace Master_of_Mankind.Master_of_MankindCode.Powers;

/// <summary>
/// 한국어 이름: 황제의 유산
/// 효과: 전투 종료 시 덱의 카드 1장에 보존을 영구적으로 부여하는 선택형 보상을 추가합니다.
/// </summary>
public sealed class EmperorsLegacyPower : Master_of_MankindPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromKeyword(CardKeyword.Retain)];

    // 전용 아이콘을 제작하기 전까지 템플릿의 기본 파워 이미지를 사용합니다.
    public override string CustomPackedIconPath => "power.png".PowerImagePath();
    public override string CustomBigIconPath => "power.png".BigPowerImagePath();

    public override async Task AfterCombatEnd(CombatRoom room)
    {
        await base.AfterCombatEnd(room);

        if (Owner.Player is not { } player)
            return;

        // 전투 보상으로 남겨 두어 플레이어가 이번 전투에서는 적용하지 않을 수 있게 합니다.
        room.AddExtraReward(player, new EmperorsLegacyReward(player));
    }
}
