using BaseLib.Patches.Content;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Master_of_Mankind.Master_of_MankindCode.Relics;

/// <summary>한국어 이름: 로자리우스 / 효과: 전투 시작 시 방어도 6을 얻습니다.</summary>
public sealed class Rosarius : Master_of_MankindRelic
{
    public override RelicRarity Rarity => RelicRarity.Common;

    public override async Task BeforeHandDraw(
        Player player,
        PlayerChoiceContext choiceContext,
        ICombatState combatState)
    {
        if (!ReferenceEquals(player, Owner) || player.PlayerCombatState is not { TurnNumber: 1 })
            return;

        Flash();
        await CreatureCmd.GainBlock(Owner.Creature, 6m, ValueProp.Unpowered, null);
    }
}

/// <summary>한국어 이름: 크룩스 터미나투스 / 효과: 전투 시작 시 방어도 12와 인공물 1을 얻습니다.</summary>
public sealed class CruxTerminatus : Master_of_MankindRelic
{
    public override RelicRarity Rarity => RelicRarity.Uncommon;

    public override async Task BeforeHandDraw(
        Player player,
        PlayerChoiceContext choiceContext,
        ICombatState combatState)
    {
        if (!ReferenceEquals(player, Owner) || player.PlayerCombatState is not { TurnNumber: 1 })
            return;

        Flash();
        await CreatureCmd.GainBlock(Owner.Creature, 12m, ValueProp.Unpowered, null);
        await PowerCmd.Apply<ArtifactPower>(
            choiceContext,
            Owner.Creature,
            1m,
            Owner.Creature,
            null);
    }
}

/// <summary>한국어 이름: 아이언 헤일로 / 효과: 전투 시작 시 버퍼 1을 얻습니다.</summary>
public sealed class IronHalo : Master_of_MankindRelic
{
    public override RelicRarity Rarity => RelicRarity.Uncommon;

    public override async Task BeforeHandDraw(
        Player player,
        PlayerChoiceContext choiceContext,
        ICombatState combatState)
    {
        if (!ReferenceEquals(player, Owner) || player.PlayerCombatState is not { TurnNumber: 1 })
            return;

        Flash();
        await PowerCmd.Apply<BufferPower>(
            choiceContext,
            Owner.Creature,
            1m,
            Owner.Creature,
            null);
    }
}

/// <summary>한국어 이름: 아나테임 / 효과: 매 전투 첫 공격이 대상에게 약화와 취약을 각각 2 부여합니다.</summary>
public sealed class Anathame : Master_of_MankindRelic
{
    private static readonly SavedSpireField<Player, bool> TriggeredThisCombat =
        new(() => false, "MasterOfMankind_AnathameTriggered");

    public override RelicRarity Rarity => RelicRarity.Rare;

    public override Task BeforeCombatStart()
    {
        TriggeredThisCombat.Set(Owner, false);
        return Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!ReferenceEquals(cardPlay.Card.Owner, Owner)
            || cardPlay.Card.Type != CardType.Attack
            || cardPlay.Target is null
            || TriggeredThisCombat.Get(Owner))
            return;

        TriggeredThisCombat.Set(Owner, true);
        Flash();
        await PowerCmd.Apply<VulnerablePower>(
            choiceContext,
            cardPlay.Target,
            2m,
            Owner.Creature,
            null);
        await PowerCmd.Apply<WeakPower>(
            choiceContext,
            cardPlay.Target,
            2m,
            Owner.Creature,
            null);
    }
}

/// <summary>한국어 이름: 일곱 망치의 탈리스만 / 효과: 카드 7장마다 에너지 2와 카드 2장을 얻습니다.</summary>
public sealed class TalismanOfSevenHammers : Master_of_MankindRelic
{
    private static readonly SavedSpireField<Player, int> CardsPlayed =
        new(() => 0, "MasterOfMankind_SevenHammersCardsPlayed");

    public override RelicRarity Rarity => RelicRarity.Rare;

    public override Task BeforeCombatStart()
    {
        CardsPlayed.Set(Owner, 0);
        return Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!ReferenceEquals(cardPlay.Card.Owner, Owner))
            return;

        int played = CardsPlayed.Get(Owner) + 1;
        if (played < 7)
        {
            CardsPlayed.Set(Owner, played);
            return;
        }

        CardsPlayed.Set(Owner, 0);
        Flash();
        await PlayerCmd.GainEnergy(2, Owner);
        await CardPileCmd.Draw(choiceContext, 2, Owner);
    }
}

/// <summary>한국어 이름: 호루스의 탈론 / 효과: 공격 카드 5장마다 에너지 1을 얻습니다.</summary>
public sealed class TalonOfHorus : Master_of_MankindRelic
{
    private static readonly SavedSpireField<Player, int> AttacksPlayed =
        new(() => 0, "MasterOfMankind_TalonOfHorusAttacksPlayed");

    public override RelicRarity Rarity => RelicRarity.Rare;

    public override Task BeforeCombatStart()
    {
        AttacksPlayed.Set(Owner, 0);
        return Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!ReferenceEquals(cardPlay.Card.Owner, Owner) || cardPlay.Card.Type != CardType.Attack)
            return;

        int attacks = AttacksPlayed.Get(Owner) + 1;
        if (attacks < 5)
        {
            AttacksPlayed.Set(Owner, attacks);
            return;
        }

        AttacksPlayed.Set(Owner, 0);
        Flash();
        await PlayerCmd.GainEnergy(1, Owner);
    }
}

/// <summary>한국어 이름: 크로노메트론 / 효과: 한 턴 이상 보존된 카드의 비용이 1 감소합니다.</summary>
public sealed class Chronometron : Master_of_MankindRelic
{
    private static readonly SavedSpireField<CardModel, int> RetainedTurns =
        new(() => 0, "MasterOfMankind_ChronometronRetainedTurns");

    public override RelicRarity Rarity => RelicRarity.Shop;

    public override Task AfterFlush(
        PlayerChoiceContext choiceContext,
        Player player,
        IReadOnlyCollection<CardModel> flushedCards,
        IReadOnlyCollection<CardModel> retainedCards)
    {
        if (!ReferenceEquals(player, Owner))
            return Task.CompletedTask;

        foreach (CardModel card in flushedCards)
            RetainedTurns.Set(card, 0);
        foreach (CardModel card in retainedCards)
            RetainedTurns.Set(card, RetainedTurns.Get(card) + 1);

        return Task.CompletedTask;
    }

    public override Task AfterCardChangedPiles(
        CardModel card,
        PileType oldPileType,
        AbstractModel? clonedBy)
    {
        if (ReferenceEquals(card.Owner, Owner) && oldPileType == PileType.Hand)
            RetainedTurns.Set(card, 0);

        return Task.CompletedTask;
    }

    public override bool TryModifyEnergyCostInCombat(
        CardModel card,
        decimal originalCost,
        out decimal modifiedCost)
    {
        modifiedCost = originalCost;
        if (!ReferenceEquals(card.Owner, Owner)
            || card.EnergyCost.CostsX
            || originalCost <= 0m
            || RetainedTurns.Get(card) < 1)
            return false;

        modifiedCost = Math.Max(0m, originalCost - 1m);
        return true;
    }
}
