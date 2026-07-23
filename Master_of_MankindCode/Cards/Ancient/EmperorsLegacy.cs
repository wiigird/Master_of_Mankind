using BaseLib.Abstracts;
using BaseLib.Utils;
using Master_of_Mankind.Master_of_MankindCode.Character;
using Master_of_Mankind.Master_of_MankindCode.Extensions;
using Master_of_Mankind.Master_of_MankindCode.Powers;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace Master_of_Mankind.Master_of_MankindCode.Cards.Ancient;

/// <summary>
/// 한국어 이름: 황제의 유산
/// 효과: 전투 종료 시 덱의 카드 1장을 선택하여 보존을 영구적으로 인챈트하는 다브 전용 고대 카드입니다.
/// 수치 조절: CardCost와 UpgradeCostReduction을 변경하면 기본 및 강화 비용이 바뀝니다.
/// </summary>
[Pool(typeof(Master_of_MankindCardPool))]
public sealed class EmperorsLegacy()
    : Master_of_MankindCard(CardCost, CardType.Power, CardRarity.Ancient, TargetType.None), ITomeCard
{
    // 밸런스 조정: 기본 비용은 2이며, 다브에게 받을 때 자동 강화되어 1이 됩니다.
    private const int CardCost = 2;
    private const int UpgradeCostReduction = 1;

    public CharacterModel TomeCharacter => ModelDb.Character<Emperor>();

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new PowerVar<EmperorsLegacyPower>(1m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromKeyword(CardKeyword.Retain)];

    // 전용 일러스트를 제작하기 전까지 템플릿의 기본 카드 이미지를 사용합니다.
    public override string CustomPortraitPath => "emperors_legacy.png".BigCardImagePath();
    public override string PortraitPath => "emperors_legacy.png".BigCardImagePath();

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) =>
        await CommonActions.ApplySelf<EmperorsLegacyPower>(choiceContext, this);

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-UpgradeCostReduction);
}
