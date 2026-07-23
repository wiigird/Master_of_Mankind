using Godot;
using Master_of_Mankind.Master_of_MankindCode.Character;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.UI;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace Master_of_Mankind.Master_of_MankindCode.Decree;

public static class DecreeZoneUi
{
    private const string RootNodeName = "EmperorDecreeZone";
    private const string SlotsNodeName = "Slots";
    private const float CardScale = 0.3f;

    private static readonly List<NCard> CardPreviews = [];

    public static void RefreshDeferred(Player player) =>
        Callable.From(() => Refresh(player)).CallDeferred();

    public static void Initialize(CombatState state)
    {
        Clear();
    }

    public static void Refresh(Player player)
    {
        Clear();
    }

    public static void Clear()
    {
        ClearPreviews();

        NCombatRoom? room = NCombatRoom.Instance;
        Control? root = room?.Ui?.GetNodeOrNull<Control>(RootNodeName);
        if (root is null)
            return;

        root.GetParent()?.RemoveChild(root);
        root.QueueFreeSafely();
    }

    public static Vector2 GetTargetPosition(CardModel card, Vector2 cardSize)
    {
        NCombatRoom? room = NCombatRoom.Instance;
        if (room?.Ui is null)
            return Vector2.Zero;

        return (room.Ui.Size - cardSize) * 0.5f;
    }

    private static Control CreateRoot(NCombatRoom room)
    {
        Control root = new()
        {
            Name = RootNodeName,
            MouseFilter = Control.MouseFilterEnum.Ignore,
            ZIndex = 30
        };
        root.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        room.Ui.AddChild(root);

        HBoxContainer slots = new()
        {
            Name = SlotsNodeName,
            AnchorLeft = 0.5f,
            AnchorRight = 0.5f,
            AnchorTop = 1f,
            AnchorBottom = 1f,
            OffsetLeft = -178f,
            OffsetRight = 178f,
            OffsetTop = -510f,
            OffsetBottom = -340f,
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        slots.AddThemeConstantOverride("separation", 10);
        root.AddChild(slots);
        return root;
    }

    private static PanelContainer CreateSlot(
        int index,
        PreparedDecree? entry,
        int currentTurn)
    {
        PanelContainer slot = new()
        {
            CustomMinimumSize = new Vector2(112f, 170f),
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        slot.AddThemeStyleboxOverride("panel", CreateSlotStyle(entry is not null));

        VBoxContainer contents = new()
        {
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        contents.AddThemeConstantOverride("separation", 2);
        slot.AddChild(contents);

        Label status = new()
        {
            Text = GetStatusText(index, entry, currentTurn),
            HorizontalAlignment = HorizontalAlignment.Center,
            CustomMinimumSize = new Vector2(0f, 25f),
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        status.AddThemeFontSizeOverride("font_size", 14);
        status.Modulate = entry is null
            ? new Color(0.72f, 0.68f, 0.56f, 0.72f)
            : new Color("f0c95d");
        contents.AddChild(status);

        Control previewHolder = new()
        {
            CustomMinimumSize = new Vector2(102f, 137f),
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        contents.AddChild(previewHolder);

        if (entry is null)
        {
            Label emptyMark = new()
            {
                Text = "-",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                MouseFilter = Control.MouseFilterEnum.Ignore
            };
            emptyMark.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
            emptyMark.AddThemeFontSizeOverride("font_size", 28);
            emptyMark.Modulate = new Color(0.72f, 0.68f, 0.56f, 0.55f);
            previewHolder.AddChild(emptyMark);
            return slot;
        }

        NCard? card = NCard.Create(entry.Card, ModelVisibility.Visible);
        if (card is null)
            return slot;

        card.Scale = Vector2.One * CardScale;
        card.Position = new Vector2(6f, 4f);
        card.MouseFilter = Control.MouseFilterEnum.Ignore;
        previewHolder.AddChild(card);
        card.UpdateVisuals(PileType.None, CardPreviewMode.Normal);
        SetMouseFilterRecursive(card);
        CardPreviews.Add(card);
        return slot;
    }

    private static StyleBoxFlat CreateSlotStyle(bool occupied) => new()
    {
        BgColor = occupied
            ? new Color(0.105f, 0.09f, 0.045f, 0.9f)
            : new Color(0.035f, 0.035f, 0.04f, 0.72f),
        BorderColor = occupied
            ? new Color(0.84f, 0.68f, 0.24f, 0.95f)
            : new Color(0.42f, 0.4f, 0.35f, 0.65f),
        BorderWidthLeft = 2,
        BorderWidthTop = 2,
        BorderWidthRight = 2,
        BorderWidthBottom = 2,
        CornerRadiusTopLeft = 4,
        CornerRadiusTopRight = 4,
        CornerRadiusBottomLeft = 4,
        CornerRadiusBottomRight = 4,
        ContentMarginLeft = 4f,
        ContentMarginTop = 3f,
        ContentMarginRight = 4f,
        ContentMarginBottom = 3f
    };

    private static string GetStatusText(
        int index,
        PreparedDecree? entry,
        int currentTurn)
    {
        if (entry is null)
            return ToRoman(index + 1);

        string key = entry.PreparedTurn < currentTurn
            ? "MASTER_OF_MANKIND-DECREE_READY"
            : "MASTER_OF_MANKIND-DECREE_WAITING";
        return new LocString("static_hover_tips", key).GetFormattedText();
    }

    private static string ToRoman(int value) => value switch
    {
        1 => "I",
        2 => "II",
        3 => "III",
        _ => value.ToString()
    };

    private static void SetMouseFilterRecursive(Node node)
    {
        if (node is Control control)
            control.MouseFilter = Control.MouseFilterEnum.Ignore;

        foreach (Node child in node.GetChildren())
            SetMouseFilterRecursive(child);
    }

    private static void ClearPreviews()
    {
        foreach (NCard card in CardPreviews)
        {
            if (!GodotObject.IsInstanceValid(card))
                continue;

            card.GetParent()?.RemoveChild(card);
            card.QueueFreeSafely();
        }

        CardPreviews.Clear();
    }

    private static void ClearChildren(Node node)
    {
        foreach (Node child in node.GetChildren())
        {
            node.RemoveChild(child);
            child.QueueFreeSafely();
        }
    }
}
