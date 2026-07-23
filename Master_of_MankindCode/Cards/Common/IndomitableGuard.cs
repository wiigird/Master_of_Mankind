using BaseLib.Utils;
using Master_of_Mankind.Master_of_MankindCode.Character;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Master_of_Mankind.Master_of_MankindCode.Cards.Common;

/// <summary>
/// 한국어 이름: 불굴의 수호
/// 효과: 보존한 뒤 사용하면 더 큰 방어도를 얻습니다.
/// 수치 조절: CanonicalVars의 기본 및 보존 방어도와 OnUpgrade 수치를 변경합니다.
/// </summary>
[Pool(typeof(Master_of_MankindCardPool))]
public sealed class IndomitableGuard : Master_of_MankindCard
{
    private const string RetainedBlockKey = "RetainedBlock";
    private const string RetainTriggeredKey = "RetainTriggered";

    public IndomitableGuard() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self) { }

    public override HashSet<CardKeyword> CanonicalKeywords => [CardKeyword.Retain];
    public override bool GainsBlock => true;

    protected override bool ShouldGlowGoldInternal => DynamicVars[RetainTriggeredKey].BaseValue > 0m;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(7m, ValueProp.Move),
        new DynamicVar(RetainedBlockKey, 12m),
        new DynamicVar(RetainTriggeredKey, 0m)
    ];

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) =>
        CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);

    public override Task AfterFlush(
        PlayerChoiceContext choiceContext,
        Player player,
        IReadOnlyCollection<CardModel> flushedCards,
        IReadOnlyCollection<CardModel> retainedCards)
    {
        if (retainedCards.Contains(this))
        {
            DynamicVars[RetainTriggeredKey].BaseValue = 1m;
            decimal retainedBlock = DynamicVars[RetainedBlockKey].BaseValue;
            if (DynamicVars.Block.BaseValue < retainedBlock)
                DynamicVars.Block.UpgradeValueBy(retainedBlock - DynamicVars.Block.BaseValue);
        }

        return Task.CompletedTask;
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(2m);
        DynamicVars[RetainedBlockKey].UpgradeValueBy(3m);
    }
}
