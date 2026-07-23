using BaseLib.Utils;
using Master_of_Mankind.Master_of_MankindCode.Character;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Master_of_Mankind.Master_of_MankindCode.Cards.Common;

/// <summary>
/// 한국어 이름: 황금 진형
/// 효과: 방어도를 얻고 손의 카드 1장을 선택해 이번 턴 보존합니다.
/// 수치 조절: BaseBlock과 UpgradeBlock을 변경하면 기본 및 강화 방어도가 바뀝니다.
/// </summary>
[Pool(typeof(Master_of_MankindCardPool))]
public sealed class GoldenFormation : Master_of_MankindCard
{
    // 밸런스 조정: 기본 방어도와 강화 시 추가 방어도입니다.
    private const decimal BaseBlock = 3m;
    private const decimal UpgradeBlock = 3m;

    public GoldenFormation() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self) { }

    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new BlockVar(BaseBlock, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);

        List<CardModel> hand = Owner.PlayerCombatState?.Hand.Cards.ToList() ?? [];
        if (hand.Count == 0)
            return;

        CardSelectorPrefs prefs = new(SelectionScreenPrompt, 1)
        {
            Cancelable = false,
            PretendCardsCanBePlayed = true
        };
        CardModel? selected = (await CardSelectCmd.FromSimpleGrid(
                choiceContext,
                hand,
                Owner,
                prefs))
            .FirstOrDefault();
        selected?.GiveSingleTurnRetain();
    }

    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(UpgradeBlock);
}
