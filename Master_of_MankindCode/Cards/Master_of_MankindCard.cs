using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using Master_of_Mankind.Master_of_MankindCode.Character;
using Master_of_Mankind.Master_of_MankindCode.Extensions;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace Master_of_Mankind.Master_of_MankindCode.Cards;

/// <summary>
/// This is the base class for your mod's cards, which is set up to load the card's images from your mod's resources.
/// When creating a card, right click the Cards folder and create a new file with the Custom Card template.
/// This will generate a class that extends this one.
/// You can also just create the class manually; just make sure to inherit from this class.
/// </summary>
[Pool(typeof(Master_of_MankindCardPool))]
public abstract class Master_of_MankindCard(
    int cost,
    CardType type,
    CardRarity rarity,
    TargetType target,
    bool shouldShowInCardLibrary = true) :
    CustomCardModel(cost, type, rarity, target, shouldShowInCardLibrary)
{
    // 카드 도서관에서 숨긴 선택 전용 카드는 포션과 기타 무작위 생성 후보에서도 제외합니다.
    public override bool CanBeGeneratedInCombat => ShouldShowInCardLibrary;
    public override bool CanBeGeneratedByModifiers => ShouldShowInCardLibrary;

    //Image size:
    //Normal art: 1000x760 (Using 500x380 should also work, it will simply be scaled.)
    //Full art: 606x852
    public override string CustomPortraitPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigCardImagePath();

    //Smaller variants of card images for efficiency:
    //Smaller variant of fullart: 250x350
    //Smaller variant of normalart: 250x190

    //Uses card_portraits/card_name.png as image path. These should be smaller images.
    public override string PortraitPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();
    public override string BetaPortraitPath => $"beta/{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();
}
