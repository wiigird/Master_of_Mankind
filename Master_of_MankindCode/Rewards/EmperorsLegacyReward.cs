using BaseLib.Abstracts;
using BaseLib.Patches.Content;
using Master_of_Mankind.Master_of_MankindCode.Extensions;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace Master_of_Mankind.Master_of_MankindCode.Rewards;

/// <summary>
/// 황제의 유산 보상: 전투 보상에서 선택했을 때만 덱의 카드 1장에 보존을 영구적으로 부여합니다.
/// </summary>
public sealed class EmperorsLegacyReward : CustomReward
{
    [CustomEnum]
    public static RewardType EmperorsLegacy;

    protected override RewardType RewardType => EmperorsLegacy;

    public override LocString Description => GetLoc();

    public override bool IsPopulated => true;

    // 금지된 마도서와 같은 기본 카드 조작 보상 아이콘을 사용합니다.
    protected override string IconPath => ImageHelper.GetImagePath("ui/reward_screen/reward_icon_card_removal.png");

    public override CreateRewardFromSave<CustomReward> DeserializeMethod => CreateFromSerializable;

    public EmperorsLegacyReward(Player player)
        : base(player)
    {
    }

    public override SerializableReward ToSerializable() => new()
    {
        RewardType = EmperorsLegacy
    };

    public static EmperorsLegacyReward CreateFromSerializable(SerializableReward save, Player player) => new(player);

    public override void Populate()
    {
    }

    public override void MarkContentAsSeen()
    {
    }

    protected override async Task<bool> OnSelect()
    {
        // 보존 인챈트는 덱에 남아 이후 전투에도 유지됩니다.
        // 선택 화면은 미리보기 복제본을 자체 생성하므로 반드시 원본 모델을 받아야 합니다.
        EnchantmentModel retainEnchantment = ModelDb.Enchantment<Steady>();
        CardSelectorPrefs prefs = new(CardSelectorPrefs.EnchantSelectionPrompt, 1)
        {
            Cancelable = false
        };
        CardModel? selected = (await CardSelectCmd.FromDeckForEnchantment(
                Player,
                retainEnchantment,
                amount: 1,
                prefs))
            .FirstOrDefault();
        if (selected is null)
            return false;

        // 확인이 끝난 뒤 실제 적용에 사용할 가변 복제본을 새로 만듭니다.
        CardCmd.Enchant(retainEnchantment.ToMutable(), selected, amount: 1);
        return true;
    }
}
