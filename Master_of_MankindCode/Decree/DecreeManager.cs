using Master_of_Mankind.Master_of_MankindCode.Cards;
using Master_of_Mankind.Master_of_MankindCode.Powers;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace Master_of_Mankind.Master_of_MankindCode.Decree;

public static class DecreeManager
{
    private sealed record LastExecutedDecree(PreparedDecree Entry, int TurnNumber);

    private static readonly Dictionary<Player, LastExecutedDecree> LastExecutedDecrees = [];

    // 칙령 영역의 사실상 무제한 상한입니다. 이 값을 조절하면 동시에 준비할 수 있는 최대 장수가 바뀝니다.
    public const int MaxPreparedDecrees = 256;

    public static bool CanPrepare(Player player)
    {
        CardPile? pile = DecreeState.GetPile(player);
        return pile is not null && pile.Cards.Count < MaxPreparedDecrees;
    }

    public static IReadOnlyList<PreparedDecree> GetPrepared(Player player)
    {
        CardPile? pile = DecreeState.GetPile(player);
        return pile is null
            ? Array.Empty<PreparedDecree>()
            : pile.Cards
                .OfType<DecreeCard>()
                .Select(DecreeState.ToPrepared)
                .ToList();
    }

    public static async Task Prepare(
        PlayerChoiceContext choiceContext,
        DecreeCard card,
        CardPlay cardPlay)
    {
        if (DecreeState.IsPrepared(card))
            return;

        Player owner = card.Owner;
        PlayerCombatState playerState = owner.PlayerCombatState
            ?? throw new InvalidOperationException("A decree can only be prepared during combat.");
        CardPile decreePile = DecreeState.GetPile(owner)
            ?? throw new InvalidOperationException("The decree pile was not registered.");
        if (decreePile.Cards.Count >= MaxPreparedDecrees)
            throw new InvalidOperationException("The decree area is full.");

        DecreeState.Prepare(card, cardPlay, playerState.TurnNumber);

        if (!owner.Creature.HasPower<DecreeControllerPower>())
        {
            await PowerCmd.Apply<DecreeControllerPower>(
                choiceContext,
                owner.Creature,
                1m,
                owner.Creature,
                card,
                silent: true);
        }

        DecreeZoneUi.RefreshDeferred(owner);

        // 제국의 대명령은 준비 직후 같은 집행 경로를 사용하므로 모든 칙령 연계가 빠짐없이 발동합니다.
        if (owner.Creature.HasPower<ImperialMandatePower>())
            await ExecuteFromPlay(choiceContext, owner, DecreeState.ToPrepared(card));
    }

    public static async Task ResolveReady(PlayerChoiceContext choiceContext, Player player)
    {
        PlayerCombatState? playerState = player.PlayerCombatState;
        if (playerState is null)
            return;

        int currentTurn = playerState.TurnNumber;
        List<PreparedDecree> ready = GetPrepared(player)
            .Where(entry => entry.PreparedTurn < currentTurn)
            .ToList();
        if (ready.Count == 0)
            return;

        LocString prompt = new("static_hover_tips", "MASTER_OF_MANKIND-DECREE_EXECUTION_PROMPT");
        CardSelectorPrefs prefs = new(prompt, 0, ready.Count)
        {
            RequireManualConfirmation = true,
            PretendCardsCanBePlayed = true
        };

        List<CardModel> selected = (await CardSelectCmd.FromSimpleGrid(
                choiceContext,
                ready.Select(entry => (CardModel)entry.Card).ToList(),
                player,
                prefs))
            .ToList();

        bool executedDecree = false;
        foreach (CardModel selectedCard in selected)
        {
            PreparedDecree? entry = ready.FirstOrDefault(candidate =>
                ReferenceEquals(candidate.Card, selectedCard));
            if (entry is null || entry.Card.Pile?.Type != DecreePile.DecreePileType)
                continue;

            await Execute(choiceContext, player, entry);
            executedDecree = true;
            DecreeZoneUi.Refresh(player);

            if (CombatManager.Instance.IsOverOrEnding || player.Creature.IsDead)
                break;
        }

        // 영원의 문 반격은 다음 턴의 칙령 묶음이 끝난 직후, 마지막 집행만 한 번 더 반복합니다.
        if (executedDecree
            && !CombatManager.Instance.IsOverOrEnding
            && !player.Creature.IsDead
            && player.Creature.GetPower<EternityGateCounterstrokePower>() is { } counterstroke)
            await counterstroke.RepeatFinalDecree(choiceContext, player);

        DecreeZoneUi.Refresh(player);
    }

    /// <summary>
    /// 준비된 칙령 중 플레이어가 선택한 수만큼 대기 시간 없이 즉시 집행합니다.
    /// 칙령의 기존 대상, 소멸 여부, 격노 및 대성전 연계는 일반 집행과 동일하게 처리합니다.
    /// </summary>
    public static async Task ResolveImmediately(PlayerChoiceContext choiceContext, Player player)
    {
        List<PreparedDecree> prepared = GetPrepared(player).ToList();
        if (prepared.Count == 0)
            return;

        LocString prompt = new("static_hover_tips", "MASTER_OF_MANKIND-DECREE_IMMEDIATE_PROMPT");
        CardSelectorPrefs prefs = new(prompt, 0, prepared.Count)
        {
            RequireManualConfirmation = true,
            PretendCardsCanBePlayed = true
        };

        List<CardModel> selected = (await CardSelectCmd.FromSimpleGrid(
                choiceContext,
                prepared.Select(entry => (CardModel)entry.Card).ToList(),
                player,
                prefs))
            .ToList();

        foreach (CardModel selectedCard in selected)
        {
            PreparedDecree? entry = prepared.FirstOrDefault(candidate =>
                ReferenceEquals(candidate.Card, selectedCard));
            if (entry is null || entry.Card.Pile?.Type != DecreePile.DecreePileType)
                continue;

            await Execute(choiceContext, player, entry);
            DecreeZoneUi.Refresh(player);

            if (CombatManager.Instance.IsOverOrEnding || player.Creature.IsDead)
                break;
        }
    }

    public static void Clear(Player player)
    {
        LastExecutedDecrees.Remove(player);
        DecreeZoneUi.Refresh(player);
    }

    public static void ClearAll()
    {
        LastExecutedDecrees.Clear();
        DecreeZoneUi.Clear();
    }

    public static async Task RepeatLastExecutedThisTurn(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.PlayerCombatState is not { } state
            || !LastExecutedDecrees.TryGetValue(player, out LastExecutedDecree? last)
            || last.TurnNumber != state.TurnNumber)
            return;

        await Execute(choiceContext, player, last.Entry, recordExecution: false);
    }

    private static async Task Execute(
        PlayerChoiceContext choiceContext,
        Player player,
        PreparedDecree entry,
        bool recordExecution = true)
    {
        if (player.Creature.CombatState is not CombatState combatState
            || player.PlayerCombatState is not { } playerState)
            return;

        DecreeCard card = entry.Card;
        CardPileAddResult moveResult = await CardPileCmd.Add(
            card,
            playerState.PlayPile,
            CardPilePosition.Bottom,
            clonedBy: null,
            skipVisuals: true);
        if (!moveResult.success)
            return;

        DecreeState.Clear(card);
        DecreeZoneUi.Refresh(player);

        Creature? target = ResolveTarget(player, combatState, entry);
        bool hasValidTarget = !RequiresCreatureTarget(card.TargetType) || target is not null;
        bool exhaust = card.Keywords.Contains(CardKeyword.Exhaust) || card.ExhaustOnNextPlay;
        CardPlay execution = new()
        {
            Card = card,
            Target = target,
            ResultPile = exhaust ? PileType.Exhaust : PileType.Discard,
            Resources = entry.Resources,
            IsAutoPlay = true,
            PlayIndex = 0,
            PlayCount = 1
        };

        try
        {
            if (hasValidTarget)
                await card.ExecutePreparedEffect(choiceContext, execution);

            await TriggerExecutionHooks(choiceContext, player, card);
            if (recordExecution)
                RecordExecution(player, entry);
        }
        finally
        {
            if (card.Pile?.Type == PileType.Play && !CombatManager.Instance.IsOverOrEnding)
            {
                if (exhaust)
                {
                    await CardCmd.Exhaust(
                        choiceContext,
                        card,
                        causedByEthereal: false,
                        skipVisuals: true);
                }
                else
                {
                    await CardPileCmd.Add(
                        card,
                        playerState.DiscardPile,
                        CardPilePosition.Bottom,
                        clonedBy: null,
                        skipVisuals: true);
                }
            }
        }
    }

    private static async Task ExecuteFromPlay(
        PlayerChoiceContext choiceContext,
        Player player,
        PreparedDecree entry)
    {
        if (player.Creature.CombatState is not CombatState combatState
            || player.PlayerCombatState is not { } playerState)
            return;

        DecreeCard card = entry.Card;
        DecreeState.Clear(card);
        Creature? target = ResolveTarget(player, combatState, entry);
        bool hasValidTarget = !RequiresCreatureTarget(card.TargetType) || target is not null;
        bool exhaust = card.Keywords.Contains(CardKeyword.Exhaust) || card.ExhaustOnNextPlay;
        CardPlay execution = new()
        {
            Card = card,
            Target = target,
            ResultPile = exhaust ? PileType.Exhaust : PileType.Discard,
            Resources = entry.Resources,
            IsAutoPlay = true,
            PlayIndex = 0,
            PlayCount = 1
        };

        try
        {
            if (hasValidTarget)
                await card.ExecutePreparedEffect(choiceContext, execution);

            await TriggerExecutionHooks(choiceContext, player, card);
            RecordExecution(player, entry);
        }
        finally
        {
            // 결과 더미는 OnPlay 전에 확정되므로, 즉시 집행된 카드는 여기서 플레이 더미를 직접 벗어납니다.
            if (card.Pile?.Type == PileType.Play && !CombatManager.Instance.IsOverOrEnding)
            {
                if (exhaust)
                {
                    await CardCmd.Exhaust(
                        choiceContext,
                        card,
                        causedByEthereal: false,
                        skipVisuals: true);
                }
                else
                {
                    await CardPileCmd.Add(
                        card,
                        playerState.DiscardPile,
                        CardPilePosition.Bottom,
                        clonedBy: null,
                        skipVisuals: true);
                }
            }

            DecreeZoneUi.RefreshDeferred(player);
        }
    }

    private static async Task TriggerExecutionHooks(
        PlayerChoiceContext choiceContext,
        Player player,
        DecreeCard card)
    {
        WrathMarkPower.RecordDecreeExecution(player, card);
        if (player.Creature.GetPower<GreatCrusadePower>() is { } greatCrusade)
            await greatCrusade.AfterDecreeExecuted(choiceContext);
        if (player.Creature.GetPower<EndlessMandatePower>() is { } endlessMandate)
            await endlessMandate.AfterDecreeExecuted(choiceContext);
        if (player.Creature.GetPower<WarInTheWebwayPower>() is { } webwayWar)
            await webwayWar.AfterDecreeExecuted(choiceContext);
    }

    private static void RecordExecution(Player player, PreparedDecree entry)
    {
        if (player.PlayerCombatState is { } state)
            LastExecutedDecrees[player] = new LastExecutedDecree(entry, state.TurnNumber);
    }

    private static Creature? ResolveTarget(
        Player player,
        CombatState combatState,
        PreparedDecree entry)
    {
        if (entry.TargetCombatId >= 0)
        {
            Creature? target = combatState.GetCreature((uint)entry.TargetCombatId);
            if (target is { IsAlive: true } && combatState.ContainsCreature(target))
                return target;
        }

        return entry.Card.TargetType switch
        {
            TargetType.AnyEnemy or TargetType.RandomEnemy => combatState.HittableEnemies.FirstOrDefault(),
            TargetType.Self => player.Creature,
            TargetType.AnyPlayer => combatState.PlayerCreatures.FirstOrDefault(creature => creature.IsAlive),
            TargetType.AnyAlly => combatState.GetTeammatesOf(player.Creature)
                .FirstOrDefault(creature => creature.IsAlive),
            _ => null
        };
    }

    private static bool RequiresCreatureTarget(TargetType targetType) => targetType is
        TargetType.AnyEnemy or
        TargetType.RandomEnemy or
        TargetType.AnyPlayer or
        TargetType.AnyAlly or
        TargetType.Self or
        TargetType.Osty;
}

public sealed record PreparedDecree(
    DecreeCard Card,
    int TargetCombatId,
    ResourceInfo Resources,
    int PreparedTurn);
