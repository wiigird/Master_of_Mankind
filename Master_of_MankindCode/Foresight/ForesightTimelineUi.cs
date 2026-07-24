using Godot;
using System.Runtime.CompilerServices;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace Master_of_Mankind.Master_of_MankindCode.Foresight;

internal static class ForesightTimelineUi
{
    private const string TimelineNodeName = "EmperorForesightTimeline";
    private const float MoveColumnWidth = 52f;
    private const float MoveColumnHeight = 62f;
    private const float MoveColumnSeparation = 2f;
    private sealed class RenderState
    {
        public ICombatState? CombatState { get; set; }
        public int CacheVersion { get; set; } = -1;
        public int Depth { get; set; }
    }

    private static readonly HashSet<ulong> PendingRefreshes = [];
    private static readonly ConditionalWeakTable<NCreature, RenderState> RenderStates = new();

    public static void RefreshDeferred(NCreature creatureNode)
    {
        if (!GodotObject.IsInstanceValid(creatureNode))
            return;

        var instanceId = creatureNode.GetInstanceId();
        if (!PendingRefreshes.Add(instanceId))
            return;

        Callable.From(() =>
        {
            PendingRefreshes.Remove(instanceId);
            Refresh(creatureNode);
        }).CallDeferred();
    }

    public static async Task RefreshAllEnemies(ICombatState combatState)
    {
        ForesightPredictionService.Invalidate();
        var room = MegaCrit.Sts2.Core.Nodes.Rooms.NCombatRoom.Instance;
        if (room == null)
            return;

        foreach (var enemy in combatState.Enemies)
        {
            var node = room.GetCreatureNode(enemy);
            if (node != null)
                await node.RefreshIntents();
        }
    }

    private static void Refresh(NCreature creatureNode)
    {
        if (!GodotObject.IsInstanceValid(creatureNode)
            || creatureNode.Entity.Monster == null
            || creatureNode.Entity.CombatState == null)
            return;

        var combatState = creatureNode.Entity.CombatState;
        var depth = ForesightPredictionService.GetLocalPredictionDepth(combatState);
        var existing = creatureNode.GetNodeOrNull<HBoxContainer>(TimelineNodeName);
        if (depth <= 0)
        {
            existing?.QueueFree();
            RenderStates.Remove(creatureNode);
            return;
        }

        var renderState = RenderStates.GetOrCreateValue(creatureNode);
        if (ReferenceEquals(renderState.CombatState, combatState)
            && renderState.CacheVersion == ForesightPredictionService.CacheVersion
            && renderState.Depth == depth
            && existing != null)
            return;

        var timeline = existing ?? CreateTimeline(creatureNode);
        ClearChildren(timeline);

        var predictions = ForesightPredictionService.GetPredictions(combatState, depth);
        if (!predictions.TryGetValue(creatureNode.Entity, out var moves))
            return;

        var targets = combatState.PlayerCreatures;
        for (var i = 0; i < moves.Count; i++)
        {
            var moveColumn = new VBoxContainer
            {
                CustomMinimumSize = new Vector2(MoveColumnWidth, MoveColumnHeight),
                MouseFilter = Control.MouseFilterEnum.Pass
            };
            moveColumn.AddThemeConstantOverride("separation", 1);

            var turnLabel = new Label
            {
                Text = $"+{i + 1}",
                HorizontalAlignment = HorizontalAlignment.Center,
                MouseFilter = Control.MouseFilterEnum.Ignore
            };
            turnLabel.AddThemeFontSizeOverride("font_size", 13);
            turnLabel.Modulate = new Color("d6ad3c");
            moveColumn.AddChild(turnLabel);

            var intentRow = new HBoxContainer
            {
                MouseFilter = Control.MouseFilterEnum.Pass
            };
            intentRow.AddThemeConstantOverride("separation", 0);
            moveColumn.AddChild(intentRow);
            timeline.AddChild(moveColumn);

            var intents = moves[i].Intents.Count > 0 ? moves[i].Intents : [new UnknownIntent()];
            foreach (var intent in intents)
            {
                var intentNode = NIntent.Create((float)i * 0.15f);
                intentNode.Scale = new Vector2(0.5f, 0.5f);
                intentRow.AddChild(intentNode);
                intentNode.UpdateIntent(intent, targets, creatureNode.Entity);
            }
        }

        PositionBelowStateDisplay(creatureNode, timeline, moves.Count);
        renderState.CombatState = combatState;
        renderState.CacheVersion = ForesightPredictionService.CacheVersion;
        renderState.Depth = depth;
    }

    private static HBoxContainer CreateTimeline(NCreature creatureNode)
    {
        var timeline = new HBoxContainer
        {
            Name = TimelineNodeName,
            MouseFilter = Control.MouseFilterEnum.Pass,
            ZIndex = creatureNode.IntentContainer.ZIndex + 1
        };
        timeline.AddThemeConstantOverride("separation", (int)MoveColumnSeparation);
        creatureNode.AddChild(timeline);
        return timeline;
    }

    private static void PositionBelowStateDisplay(
        NCreature creatureNode,
        HBoxContainer timeline,
        int moveCount)
    {
        var healthBar = creatureNode.GetNode<Control>("%HealthBar");
        float width = moveCount * MoveColumnWidth
                      + Math.Max(0, moveCount - 1) * MoveColumnSeparation;
        timeline.Size = new Vector2(width, MoveColumnHeight);
        timeline.GlobalPosition = new Vector2(
            healthBar.GlobalPosition.X + (healthBar.Size.X - width) * 0.5f,
            healthBar.GlobalPosition.Y + healthBar.Size.Y + 54f);
    }

    private static void ClearChildren(Node node)
    {
        foreach (var child in node.GetChildren())
        {
            node.RemoveChild(child);
            child.QueueFree();
        }
    }
}
