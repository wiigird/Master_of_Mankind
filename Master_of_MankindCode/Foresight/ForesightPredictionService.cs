using System.Reflection;
using HarmonyLib;
using Master_of_Mankind.Master_of_MankindCode.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Random;

namespace Master_of_Mankind.Master_of_MankindCode.Foresight;

internal sealed record PredictedMove(IReadOnlyList<AbstractIntent> Intents, bool IsKnown);

internal static class ForesightPredictionService
{
    private sealed class PreviewMonster
    {
        public required Creature Creature { get; init; }
        public required MonsterMoveStateMachine Machine { get; init; }
        public required MoveState CurrentMove { get; set; }
    }

    private static readonly AccessTools.FieldRef<MegaCrit.Sts2.Core.Models.MonsterModel, MonsterMoveStateMachine?>
        MoveStateMachineRef = AccessTools.FieldRefAccess<MegaCrit.Sts2.Core.Models.MonsterModel, MonsterMoveStateMachine?>(
            "_moveStateMachine");

    private static readonly AccessTools.FieldRef<MegaCrit.Sts2.Core.Models.MonsterModel, MoveState>
        NextMoveRef = AccessTools.FieldRefAccess<MegaCrit.Sts2.Core.Models.MonsterModel, MoveState>(
            "<NextMove>k__BackingField");

    private static readonly AccessTools.FieldRef<MonsterMoveStateMachine, bool>
        PerformedFirstMoveRef = AccessTools.FieldRefAccess<MonsterMoveStateMachine, bool>("_performedFirstMove");

    private static readonly AccessTools.FieldRef<MoveState, bool>
        PerformedMoveRef = AccessTools.FieldRefAccess<MoveState, bool>("_performedAtLeastOnce");

    private static ICombatState? _cachedCombatState;
    private static int _cachedDepth;
    private static IReadOnlyDictionary<Creature, IReadOnlyList<PredictedMove>>? _cachedPredictions;
    private static readonly Dictionary<Type, MethodInfo> GenerateMoveMethods = [];

    public static int CacheVersion { get; private set; }

    public static int GetPredictionDepth(Creature observer)
    {
        if (!observer.IsAlive)
            return 0;

        return observer.Powers
            .OfType<EmperorsForesightPower>()
            .Select(power => power.Amount)
            .DefaultIfEmpty(0)
            .Max();
    }

    public static int GetLocalPredictionDepth(ICombatState combatState)
    {
        Creature? localPlayer = combatState.PlayerCreatures
            .FirstOrDefault(creature => LocalContext.IsMe(creature));
        return localPlayer is null ? 0 : GetPredictionDepth(localPlayer);
    }

    public static IReadOnlyDictionary<Creature, IReadOnlyList<PredictedMove>> GetPredictions(
        ICombatState combatState,
        int depth)
    {
        if (ReferenceEquals(_cachedCombatState, combatState)
            && _cachedDepth == depth
            && _cachedPredictions != null)
            return _cachedPredictions;

        _cachedCombatState = combatState;
        _cachedDepth = depth;
        _cachedPredictions = Predict(combatState, depth);
        return _cachedPredictions;
    }

    public static bool IsNextRevealedActionAttack(Creature creature, Creature observer)
    {
        if (creature.CombatState is not { } combatState)
            return false;

        int depth = GetObserverDepth(combatState, observer);
        if (depth <= 0
            || !GetPredictions(combatState, depth).TryGetValue(creature, out var moves)
            || moves.FirstOrDefault() is not { IsKnown: true } nextMove)
            return false;

        return nextMove.Intents.Any(intent => intent is AttackIntent);
    }

    public static bool IsNextRevealedActionKnown(Creature creature, Creature observer) =>
        TryGetNextRevealedMove(creature, observer, out _);

    public static bool IsNextRevealedActionKnownNonAttack(Creature creature, Creature observer) =>
        TryGetNextRevealedMove(creature, observer, out PredictedMove? move)
        && move!.Intents.All(intent => intent is not AttackIntent);

    public static bool AnyKnownNextAction(ICombatState combatState, Creature observer) =>
        combatState.Enemies.Any(enemy => IsNextRevealedActionKnown(enemy, observer));

    public static bool AnyNextRevealedActionIsAttack(ICombatState combatState, Creature observer) =>
        combatState.Enemies.Any(enemy => IsNextRevealedActionAttack(enemy, observer));

    public static bool AnyNextRevealedActionIsKnownNonAttack(ICombatState combatState, Creature observer) =>
        combatState.Enemies.Any(enemy => IsNextRevealedActionKnownNonAttack(enemy, observer));

    public static bool AnyRevealedActionIsKnown(ICombatState combatState, Creature observer) =>
        combatState.Enemies.Any(creature => CountRevealedActions(creature, observer) > 0);

    public static int CountRevealedActions(Creature creature, Creature observer)
    {
        if (creature.CombatState is not { } combatState)
            return 0;

        int depth = GetObserverDepth(combatState, observer);
        return depth > 0
               && GetPredictions(combatState, depth).TryGetValue(creature, out var moves)
            ? moves.Count(move => move.IsKnown)
            : 0;
    }

    public static void Invalidate()
    {
        _cachedCombatState = null;
        _cachedDepth = 0;
        _cachedPredictions = null;
        unchecked { CacheVersion++; }
    }

    private static bool TryGetNextRevealedMove(
        Creature creature,
        Creature observer,
        out PredictedMove? move)
    {
        move = null;
        if (creature.CombatState is not { } combatState)
            return false;

        int depth = GetObserverDepth(combatState, observer);
        if (depth <= 0
            || !GetPredictions(combatState, depth).TryGetValue(creature, out var moves)
            || moves.FirstOrDefault() is not { IsKnown: true } knownMove)
            return false;

        move = knownMove;
        return true;
    }

    private static int GetObserverDepth(ICombatState combatState, Creature observer) =>
        ReferenceEquals(observer.CombatState, combatState)
            ? GetPredictionDepth(observer)
            : 0;

    private static IReadOnlyDictionary<Creature, IReadOnlyList<PredictedMove>> Predict(
        ICombatState combatState,
        int depth)
    {
        var enemies = combatState.Enemies.Where(enemy => enemy.IsAlive && enemy.Monster != null).ToList();
        var result = enemies.ToDictionary(enemy => enemy, _ => new List<PredictedMove>(depth));
        if (enemies.Count == 0 || depth <= 0)
            return result.ToDictionary(pair => pair.Key, pair => (IReadOnlyList<PredictedMove>)pair.Value);

        var liveRng = enemies[0].Monster!.RunRng.MonsterAi;
        var originalCounter = liveRng.Counter;
        var previewRng = new Rng(liveRng.Seed, liveRng.Counter);

        var previews = new List<PreviewMonster>(enemies.Count);
        var failedCreatures = new HashSet<Creature>();
        foreach (var enemy in enemies)
        {
            try
            {
                previews.Add(CreatePreview(enemy));
            }
            catch (Exception exception)
            {
                MainFile.Logger.Warn(
                    $"Foresight could not clone {enemy.Monster!.Id.Entry}: {exception.Message}");
                for (var step = 0; step < depth; step++)
                    result[enemy].Add(UnknownMove());
            }
        }

        for (var step = 0; step < depth; step++)
        {
            foreach (var preview in previews)
            {
                if (failedCreatures.Contains(preview.Creature))
                {
                    result[preview.Creature].Add(UnknownMove());
                    continue;
                }

                try
                {
                    var move = RollPreviewMove(preview, combatState.PlayerCreatures, previewRng);
                    result[preview.Creature].Add(new PredictedMove(move.Intents, true));
                }
                catch (Exception exception)
                {
                    MainFile.Logger.Warn(
                        $"Foresight could not predict {preview.Creature.Monster!.Id.Entry}: {exception.Message}");
                    result[preview.Creature].Add(UnknownMove());
                    failedCreatures.Add(preview.Creature);
                }
            }
        }

        if (liveRng.Counter != originalCounter)
        {
            MainFile.Logger.Error(
                $"Foresight advanced live MonsterAi RNG from {originalCounter} to {liveRng.Counter}. Predictions discarded.");
            return UnknownPredictions(enemies, depth);
        }

        return result.ToDictionary(pair => pair.Key, pair => (IReadOnlyList<PredictedMove>)pair.Value);
    }

    private static PreviewMonster CreatePreview(Creature creature)
    {
        var monster = creature.Monster
                      ?? throw new InvalidOperationException("Foresight preview requires a monster creature.");
        var liveMachine = monster.MoveStateMachine
                          ?? throw new InvalidOperationException($"{monster.Id.Entry} has no move state machine.");
        var monsterType = monster.GetType();
        if (!GenerateMoveMethods.TryGetValue(monsterType, out var generateMethod))
        {
            generateMethod = AccessTools.Method(monsterType, "GenerateMoveStateMachine")
                             ?? throw new MissingMethodException(monsterType.FullName, "GenerateMoveStateMachine");
            GenerateMoveMethods.Add(monsterType, generateMethod);
        }
        var previewMachine = generateMethod.Invoke(monster, null) as MonsterMoveStateMachine
                             ?? throw new InvalidOperationException($"{monster.Id.Entry} returned no preview state machine.");

        if (!previewMachine.States.TryGetValue(monster.NextMove.Id, out var currentState)
            || currentState is not MoveState currentMove)
            throw new InvalidOperationException($"Current move {monster.NextMove.Id} cannot be cloned.");

        previewMachine.StateLog.Clear();
        foreach (var liveState in liveMachine.StateLog)
        {
            if (!previewMachine.States.TryGetValue(liveState.Id, out var previewState))
                throw new InvalidOperationException($"State {liveState.Id} cannot be cloned.");
            previewMachine.StateLog.Add(previewState);
        }

        previewMachine.ForceCurrentState(currentMove);
        PerformedFirstMoveRef(previewMachine) = true;
        PerformedMoveRef(currentMove) = true;

        return new PreviewMonster
        {
            Creature = creature,
            Machine = previewMachine,
            CurrentMove = currentMove
        };
    }

    private static MoveState RollPreviewMove(
        PreviewMonster preview,
        IReadOnlyList<Creature> targets,
        Rng previewRng)
    {
        var monster = preview.Creature.Monster
                      ?? throw new InvalidOperationException("Foresight preview lost its monster creature.");
        var liveMachine = MoveStateMachineRef(monster);
        var liveNextMove = NextMoveRef(monster);

        try
        {
            MoveStateMachineRef(monster) = preview.Machine;
            NextMoveRef(monster) = preview.CurrentMove;
            PerformedMoveRef(preview.CurrentMove) = true;

            var nextMove = preview.Machine.RollMove(targets, preview.Creature, previewRng);
            preview.Machine.OnMovePerformed(nextMove);
            PerformedMoveRef(nextMove) = true;
            preview.CurrentMove = nextMove;
            return nextMove;
        }
        finally
        {
            MoveStateMachineRef(monster) = liveMachine;
            NextMoveRef(monster) = liveNextMove;
        }
    }

    private static IReadOnlyDictionary<Creature, IReadOnlyList<PredictedMove>> UnknownPredictions(
        IEnumerable<Creature> enemies,
        int depth)
    {
        return enemies.ToDictionary(
            enemy => enemy,
            _ => (IReadOnlyList<PredictedMove>)Enumerable.Range(0, depth).Select(_ => UnknownMove()).ToList());
    }

    private static PredictedMove UnknownMove() => new([new UnknownIntent()], false);

}
