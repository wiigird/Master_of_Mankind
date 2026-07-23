using BaseLib.Utils;
using Master_of_Mankind.Master_of_MankindCode.Character;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace Master_of_Mankind.Master_of_MankindCode.Cards.Uncommon;

/// <summary>
/// 한국어 이름: 좁혀진 가능성
/// 효과: 카드를 뽑고 손의 카드 1장을 뽑을 카드 더미 위에 놓습니다.
/// 수치 조절: CanonicalVars와 OnUpgrade의 드로우 수를 변경합니다.
/// </summary>
[Pool(typeof(Master_of_MankindCardPool))]
public sealed class NarrowPossibilities : Master_of_MankindCard
{
    public NarrowPossibilities() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self) { }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(2)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.IntValue, Owner);

        List<CardModel> hand = Owner.PlayerCombatState?.Hand.Cards.ToList() ?? [];
        if (hand.Count == 0 || Owner.PlayerCombatState is not { } playerState)
            return;

        CardSelectorPrefs prefs = new(SelectionScreenPrompt, 1);
        CardModel? selected = (await CardSelectCmd.FromSimpleGrid(
                choiceContext,
                hand,
                Owner,
                prefs))
            .FirstOrDefault();
        if (selected is not null)
        {
            await CardPileCmd.Add(
                selected,
                playerState.DrawPile,
                CardPilePosition.Top,
                clonedBy: null,
                skipVisuals: false);
        }
    }

    protected override void OnUpgrade() => DynamicVars.Cards.UpgradeValueBy(1);
}
