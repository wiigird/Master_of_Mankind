using Godot;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Nodes.Screens.Shops;

namespace Master_of_Mankind.Master_of_MankindCode.Nodes;

[GlobalClass]
public partial class EmperorMerchantCharacter : NMerchantCharacter
{
    public override void _Ready()
    {
        base._Ready();
        CallDeferred(MethodName.StartIdleLoop);
    }

    private void StartIdleLoop()
    {
        var sprite = new MegaSprite((Variant)(GodotObject)GetNode("SpineSprite"));
        MegaAnimationState? state = sprite.TryGetAnimationState();
        state?.SetAnimation("idle", true);
    }
}
