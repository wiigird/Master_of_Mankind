using BaseLib.Utils;
using Master_of_Mankind.Master_of_MankindCode.Character;
using Master_of_Mankind.Master_of_MankindCode.Foresight;
using Master_of_Mankind.Master_of_MankindCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Master_of_Mankind.Master_of_MankindCode.Cards.Common;

/// <summary>
/// 한국어 이름: 계획된 방벽
/// 효과: 방어도를 얻고 예지한 공격이 있으면 다음 턴 방어도를 추가로 얻습니다.
/// 수치 조절: CanonicalVars의 Block과 NextTurnBlock 및 OnUpgrade 수치를 변경합니다.
/// </summary>
[Pool(typeof(Master_of_MankindCardPool))]
public sealed class PreplannedBulwark : Master_of_MankindCard
{
    private const string NextTurnBlockKey = "NextTurnBlock";

    public PreplannedBulwark() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self) { }

    public override bool GainsBlock => true;

    protected override bool ShouldGlowGoldInternal => CombatState is { } state
        && ForesightPredictionService.AnyNextRevealedActionIsAttack(state, Owner.Creature);

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(7m, ValueProp.Move),
        new DynamicVar(NextTurnBlockKey, 4m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        bool hasIncomingAttack = CombatState is { } combatState
                                 && ForesightPredictionService.AnyNextRevealedActionIsAttack(
                                     combatState,
                                     Owner.Creature);

        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        if (hasIncomingAttack)
        {
            await PowerCmd.Apply<NextTurnBulwarkPower>(
                choiceContext,
                Owner.Creature,
                DynamicVars[NextTurnBlockKey].BaseValue,
                Owner.Creature,
                this,
                silent: false);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(2m);
        DynamicVars[NextTurnBlockKey].UpgradeValueBy(2m);
    }
}
