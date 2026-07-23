using Master_of_Mankind.Master_of_MankindCode.Decree;
using Master_of_Mankind.Master_of_MankindCode.Keywords;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Master_of_Mankind.Master_of_MankindCode.Cards;

public abstract class DecreeCard : Master_of_MankindCard
{
    protected DecreeCard(int cost, CardType type, CardRarity rarity, TargetType target)
        : base(cost, type, rarity, target)
    {
    }

    public override HashSet<CardKeyword> CanonicalKeywords => [EmperorKeywords.Decree];

    protected override bool IsPlayable => DecreeManager.CanPrepare(Owner);

    protected sealed override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) =>
        DecreeManager.Prepare(choiceContext, this, cardPlay);

    public sealed override (PileType, CardPilePosition) ModifyCardPlayResultPileTypeAndPosition(
        CardModel card,
        bool isAutoPlay,
        ResourceInfo resources,
        PileType pileType,
        CardPilePosition position) =>
        ReferenceEquals(card, this)
            ? (DecreePile.DecreePileType, CardPilePosition.Bottom)
            : (pileType, position);

    internal Task ExecutePreparedEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay) =>
        OnExecuteDecree(choiceContext, cardPlay);

    protected abstract Task OnExecuteDecree(PlayerChoiceContext choiceContext, CardPlay cardPlay);
}
