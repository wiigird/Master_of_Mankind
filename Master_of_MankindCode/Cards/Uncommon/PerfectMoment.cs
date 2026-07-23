using BaseLib.Utils;
using Master_of_Mankind.Master_of_MankindCode.Character;
using Master_of_Mankind.Master_of_MankindCode.Powers;
using Master_of_Mankind.Master_of_MankindCode.Retain;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Master_of_Mankind.Master_of_MankindCode.Cards.Uncommon;

/// <summary>
/// 한국어 이름: 완벽한 순간
/// 효과: 손의 카드에 임시 보존과 사용할 때까지 비용 감소를 부여합니다.
/// 수치 조절: TemporaryRetainState의 비용 감소량과 OnUpgrade 비용을 변경합니다.
/// </summary>
[Pool(typeof(Master_of_MankindCardPool))]
public sealed class PerfectMoment : Master_of_MankindCard
{
    public PerfectMoment() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self) { }

    public override HashSet<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        List<CardModel> choices = Owner.PlayerCombatState?.Hand.Cards
            .Where(card => !ReferenceEquals(card, this))
            .ToList() ?? [];
        if (choices.Count == 0)
            return;

        CardSelectorPrefs prefs = new(SelectionScreenPrompt, 1);
        CardModel? selected = (await CardSelectCmd.FromSimpleGrid(
                choiceContext,
                choices,
                Owner,
                prefs))
            .FirstOrDefault();
        if (selected is null)
            return;

        TemporaryRetainState.Apply(selected);
        if (!Owner.Creature.HasPower<PerfectMomentControllerPower>())
        {
            await PowerCmd.Apply<PerfectMomentControllerPower>(
                choiceContext,
                Owner.Creature,
                1m,
                Owner.Creature,
                this,
                silent: true);
        }
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
