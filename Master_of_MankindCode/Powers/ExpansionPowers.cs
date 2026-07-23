using BaseLib.Utils;
using Master_of_Mankind.Master_of_MankindCode.Decree;
using Master_of_Mankind.Master_of_MankindCode.Foresight;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Master_of_Mankind.Master_of_MankindCode.Powers;

/// <summary>한국어 이름: 황금의 길 / 효과: 예지한 행동에 따라 방어 또는 드로우를 얻습니다.</summary>
public sealed class TheGoldenPathPower : Master_of_MankindPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext c, Player player)
    {
        if (!ReferenceEquals(Owner.Player, player) || Owner.CombatState is not { } state) return;
        if (ForesightPredictionService.AnyNextRevealedActionIsAttack(state))
        {
            Flash();
            await CreatureCmd.GainBlock(Owner, Amount, ValueProp.Unpowered, null);
        }
        else if (ForesightPredictionService.AnyKnownNextAction(state))
        {
            Flash();
            await CardPileCmd.Draw(c, 1, player);
        }
    }
}

/// <summary>한국어 이름: 끝없는 명령 / 효과: 칙령이 집행될 때마다 카드를 뽑습니다.</summary>
public sealed class EndlessMandatePower : Master_of_MankindPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public async Task AfterDecreeExecuted(PlayerChoiceContext c)
    {
        if (Owner.Player is null) return;
        Flash();
        await CardPileCmd.Draw(c, Amount, Owner.Player);
    }
}

/// <summary>한국어 이름: 제국 병참 / 효과: 다음 비용 2 이상 카드의 비용을 1 낮춥니다.</summary>
public sealed class ImperialLogisticsPower : Master_of_MankindPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        modifiedCost = originalCost;
        if (!ReferenceEquals(card.Owner.Creature, Owner) || originalCost < 2 || card.EnergyCost.CostsX) return false;
        modifiedCost = Math.Max(0, originalCost - Amount);
        return true;
    }

    public override async Task BeforeCardPlayed(CardPlay cardPlay)
    {
        if (ReferenceEquals(cardPlay.Card.Owner.Creature, Owner)
            && !cardPlay.Card.EnergyCost.CostsX
            && cardPlay.Card.EnergyCost.GetWithModifiers(CostModifiers.Local) >= 2)
            await PowerCmd.Decrement(this);
    }
}

/// <summary>한국어 이름: 아스트로노미칸 / 효과: 직전 턴에 보존한 카드 수만큼 제한 내에서 드로우합니다.</summary>
public sealed class TheAstronomicanPower : Master_of_MankindPower
{
    private static readonly SavedSpireField<Player, int> RetainedCount =
        new(() => 0, "MasterOfMankind_AstronomicanRetainedCount");

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override Task AfterFlush(PlayerChoiceContext c, Player player,
        IReadOnlyCollection<CardModel> flushedCards, IReadOnlyCollection<CardModel> retainedCards)
    {
        if (ReferenceEquals(Owner.Player, player)) RetainedCount.Set(player, retainedCards.Count);
        return Task.CompletedTask;
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext c, Player player)
    {
        if (!ReferenceEquals(Owner.Player, player)) return;
        int count = Math.Min(Amount, RetainedCount.Get(player));
        if (count <= 0) return;
        Flash();
        await CardPileCmd.Draw(c, count, player);
    }
}

/// <summary>한국어 이름: 제국의 대명령 / 효과: 준비한 모든 칙령을 즉시 집행합니다.</summary>
public sealed class ImperialMandatePower : Master_of_MankindPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
}

/// <summary>한국어 이름: 현현한 예지 / 효과: 예지한 행동에 따라 방어 또는 에너지와 드로우를 얻습니다.</summary>
public sealed class PrescienceIncarnatePower : Master_of_MankindPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext c, Player player)
    {
        if (!ReferenceEquals(Owner.Player, player) || Owner.CombatState is not { } state) return;
        if (ForesightPredictionService.AnyNextRevealedActionIsAttack(state))
        {
            Flash();
            await CreatureCmd.GainBlock(Owner, Amount, ValueProp.Unpowered, null);
        }
        else if (ForesightPredictionService.AnyKnownNextAction(state))
        {
            Flash();
            await PlayerCmd.GainEnergy(1, player);
            await CardPileCmd.Draw(c, 1, player);
        }
    }
}

/// <summary>한국어 이름: 황궁 성소 / 효과: 턴 시작 시 방어도를 잃지 않습니다.</summary>
public sealed class SanctumImperialisPower : Master_of_MankindPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    public override bool ShouldClearBlock(MegaCrit.Sts2.Core.Entities.Creatures.Creature creature) =>
        !ReferenceEquals(Owner, creature) || !Owner.HasPower<EmperorsForesightPower>();
}

public sealed class ApotheosisOfWarPower : Master_of_MankindPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override Task AfterFlush(
        PlayerChoiceContext choiceContext,
        Player player,
        IReadOnlyCollection<CardModel> flushedCards,
        IReadOnlyCollection<CardModel> retainedCards)
    {
        if (ReferenceEquals(Owner.Player, player))
            foreach (CardModel card in retainedCards.Where(card => card.IsUpgradable))
                CardCmd.Upgrade(card);

        return Task.CompletedTask;
    }
}

public sealed class EternityGateCounterstrokePower : Master_of_MankindPower
{
    private bool _isArmed;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (ReferenceEquals(Owner.Player, player))
            _isArmed = true;

        return Task.CompletedTask;
    }

    public async Task RepeatFinalDecree(PlayerChoiceContext choiceContext, Player player)
    {
        // 칙령 제어 파워가 턴 시작 훅에서 먼저 실행되므로, 여기서는 무장 순서에 의존하지 않습니다.
        if (!ReferenceEquals(Owner.Player, player))
            return;

        await DecreeManager.RepeatLastExecutedThisTurn(choiceContext, player);
        await PowerCmd.Remove(this);
    }

    public override async Task AfterFlush(
        PlayerChoiceContext choiceContext,
        Player player,
        IReadOnlyCollection<CardModel> flushedCards,
        IReadOnlyCollection<CardModel> retainedCards)
    {
        if (!_isArmed || !ReferenceEquals(Owner.Player, player))
            return;

        // 다음 턴에 칙령을 실행하지 않았다면, 이전 턴의 기록을 재사용하지 않고 효과만 만료합니다.
        await PowerCmd.Remove(this);
    }
}

/// <summary>한국어 이름: 인류의 신격화 / 효과: 매 턴 힘과 민첩을 얻습니다.</summary>
public sealed class ApotheosisOfManPower : Master_of_MankindPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext c, Player player)
    {
        if (!ReferenceEquals(Owner.Player, player)) return;
        Flash();
        await PowerCmd.Apply<StrengthPower>(c, Owner, Amount, Owner, null);
        await PowerCmd.Apply<DexterityPower>(c, Owner, Amount, Owner, null);
    }
}

/// <summary>한국어 이름: 마지막 교회 / 효과: 매 턴 처음 인공물을 얻을 때 카드를 뽑습니다.</summary>
public sealed class TheLastChurchPower : Master_of_MankindPower
{
    private static readonly SavedSpireField<Player, int> TriggeredTurn =
        new(() => -1, "MasterOfMankind_LastChurchTriggeredTurn");

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterPowerAmountChanged(PlayerChoiceContext c, PowerModel power,
        decimal amount, MegaCrit.Sts2.Core.Entities.Creatures.Creature? applier, CardModel? cardSource)
    {
        if (power is not ArtifactPower || !ReferenceEquals(power.Owner, Owner) || amount <= 0
            || Owner.Player?.PlayerCombatState is not { } state
            || TriggeredTurn.Get(Owner.Player) == state.TurnNumber) return;
        TriggeredTurn.Set(Owner.Player, state.TurnNumber);
        Flash();
        await CardPileCmd.Draw(c, Amount, Owner.Player);
    }
}

/// <summary>한국어 이름: 웹웨이 전쟁 / 효과: 칙령 집행마다 에너지를 얻고, 강화 시 카드도 뽑습니다.</summary>
public sealed class WarInTheWebwayPower : Master_of_MankindPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(0)];

    public async Task AfterDecreeExecuted(PlayerChoiceContext c)
    {
        if (Owner.Player is null) return;
        Flash();
        await PlayerCmd.GainEnergy(Amount, Owner.Player);
        if (DynamicVars.Cards.IntValue > 0) await CardPileCmd.Draw(c, DynamicVars.Cards.IntValue, Owner.Player);
    }
}

/// <summary>한국어 이름: 기계신의 계시 / 효과: 매 턴 처음 비용 2 이상 카드를 쓰면 에너지 1을 얻습니다.</summary>
public sealed class MachineGodsRevelationPower : Master_of_MankindPower
{
    private static readonly SavedSpireField<Player, int> TriggeredTurn =
        new(() => -1, "MasterOfMankind_MachineGodTriggeredTurn");

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override async Task AfterCardPlayed(PlayerChoiceContext c, CardPlay cardPlay)
    {
        if (!ReferenceEquals(cardPlay.Card.Owner, Owner.Player) || cardPlay.Resources.EnergySpent < 2
            || Owner.Player?.PlayerCombatState is not { } state
            || TriggeredTurn.Get(Owner.Player) == state.TurnNumber) return;
        TriggeredTurn.Set(Owner.Player, state.TurnNumber);
        Flash();
        await PlayerCmd.GainEnergy(1, Owner.Player);
    }
}
