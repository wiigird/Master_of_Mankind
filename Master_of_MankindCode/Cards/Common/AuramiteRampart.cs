using BaseLib.Utils;
using Master_of_Mankind.Master_of_MankindCode.Character;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Master_of_Mankind.Master_of_MankindCode.Cards.Common;

/// <summary>
/// 한국어 이름: 오라마이트 성벽
/// 효과: 에너지 2로 큰 방어도를 얻는 일반 고방어 카드입니다.
/// 수치 조절: BaseBlock과 UpgradeBlock을 변경하면 기본 및 강화 방어도가 바뀝니다.
/// </summary>
[Pool(typeof(Master_of_MankindCardPool))]
public sealed class AuramiteRampart : Master_of_MankindCard
{
    // 밸런스 조정: 기본 방어도와 강화 시 추가 방어도입니다.
    private const decimal BaseBlock = 16m;
    private const decimal UpgradeBlock = 5m;

    public AuramiteRampart() : base(2, CardType.Skill, CardRarity.Common, TargetType.Self) { }

    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new BlockVar(BaseBlock, ValueProp.Move)];

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) =>
        CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);

    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(UpgradeBlock);
}
