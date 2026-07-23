using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace Master_of_Mankind.Master_of_MankindCode.Powers;

public sealed class TenThousandFuturesPower : EmperorsForesightPower
{
    public override async Task BeforeFlush(PlayerChoiceContext choiceContext, Player player)
    {
        if (!ReferenceEquals(Owner.Player, player) || player.PlayerCombatState is not { } playerState)
            return;

        List<CardModel> hand = playerState.Hand.Cards.ToList();
        int maxSelection = Math.Min(2, hand.Count);
        if (maxSelection <= 0)
            return;

        LocString prompt = new("static_hover_tips", "MASTER_OF_MANKIND-TEN_THOUSAND_FUTURES_PROMPT");
        CardSelectorPrefs prefs = new(prompt, 0, maxSelection)
        {
            PretendCardsCanBePlayed = true
        };
        IEnumerable<CardModel> selected = await CardSelectCmd.FromSimpleGrid(
            choiceContext,
            hand,
            player,
            prefs);
        foreach (CardModel card in selected)
            card.GiveSingleTurnRetain();
    }
}
