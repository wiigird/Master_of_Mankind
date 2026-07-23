using BaseLib.Utils;
using Master_of_Mankind.Master_of_MankindCode.Character;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Master_of_Mankind.Master_of_MankindCode.Cards.Uncommon;

/// <summary>
/// 한국어 이름: 정복 칙령
/// 효과: 집행 시 힘을 얻는 1코스트 고급 칙령입니다.
/// 수치 조절: CanonicalVars와 OnUpgrade의 힘 수치를 변경합니다.
/// </summary>
[Pool(typeof(Master_of_MankindCardPool))]
public sealed class ConquestDecree : DecreeCard
{
    public ConquestDecree() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self) { }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<StrengthPower>(2)];

    protected override async Task OnExecuteDecree(PlayerChoiceContext choiceContext, CardPlay cardPlay) =>
        await CommonActions.Apply<StrengthPower>(choiceContext, Owner.Creature, this);

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromPower<StrengthPower>()];

    protected override void OnUpgrade() => DynamicVars["StrengthPower"].UpgradeValueBy(1);
}
