using BaseLib.Abstracts;
using BaseLib.Patches.Content;
using Godot;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;

namespace Master_of_Mankind.Master_of_MankindCode.Decree;

public sealed class DecreePile : CustomPile
{
    [CustomEnum]
    public static PileType DecreePileType;

    public DecreePile() : base(DecreePileType)
    {
    }

    public override bool CardShouldBeVisible(CardModel card) => false;

    public override Vector2 GetTargetPosition(CardModel model, Vector2 size) =>
        DecreeZoneUi.GetTargetPosition(model, size);
}
