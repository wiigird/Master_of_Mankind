using BaseLib.Utils;
using Master_of_Mankind.Master_of_MankindCode.Character;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace Master_of_Mankind.Master_of_MankindCode.Cards.Common;

/// <summary>
/// 한국어 이름: 천사의 자비
/// 효과: 카드를 3장 뽑은 뒤 손의 카드 2장을 선택해 버립니다.
/// 수치 조절: DrawCount와 DiscardCount를 변경하면 드로우 및 버리기 수가 바뀝니다.
/// </summary>
[Pool(typeof(Master_of_MankindCardPool))]
public sealed class TacticalInsight : Master_of_MankindCard
{
    // 밸런스 조정: 뽑는 카드 수와 이후 버릴 카드 수입니다.
    private const int DrawCount = 3;
    private const int DiscardCount = 2;

    public TacticalInsight() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self) { }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(DrawCount)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.IntValue, Owner);

        int count = Math.Min(DiscardCount, Owner.PlayerCombatState?.Hand.Cards.Count ?? 0);
        if (count <= 0)
            return;

        CardSelectorPrefs prefs = new(SelectionScreenPrompt, count, count)
        {
            Cancelable = false
        };
        IEnumerable<CardModel> selected = await CardSelectCmd.FromHandForDiscard(
            choiceContext,
            Owner,
            prefs,
            filter: null,
            this);
        await CardCmd.Discard(choiceContext, selected);
    }

    protected override void OnUpgrade() => DynamicVars.Cards.UpgradeValueBy(1);
}
