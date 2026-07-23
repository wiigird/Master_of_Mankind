using BaseLib.Abstracts;
using BaseLib.Utils.NodeFactories;
using Master_of_Mankind.Master_of_MankindCode.Cards.Basic;
using Master_of_Mankind.Master_of_MankindCode.Extensions;
using Master_of_Mankind.Master_of_MankindCode.Relics;
using Godot;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Relics;

namespace Master_of_Mankind.Master_of_MankindCode.Character;

public class Emperor : PlaceholderCharacterModel
{
    public const string CharacterId = "Emperor";

    public static readonly Color Color = new("d6ad3c");

    public override Color NameColor => Color;
    public override CharacterGender Gender => CharacterGender.Neutral;
    public override int StartingHp => 75;

    public override IEnumerable<CardModel> StartingDeck =>
    [
        ModelDb.Card<EmperorStrike>(),
        ModelDb.Card<EmperorStrike>(),
        ModelDb.Card<EmperorStrike>(),
        ModelDb.Card<EmperorStrike>(),
        ModelDb.Card<EmperorDefend>(),
        ModelDb.Card<EmperorDefend>(),
        ModelDb.Card<EmperorDefend>(),
        ModelDb.Card<EmperorDefend>(),
        ModelDb.Card<EmperorsForesight>(),
        ModelDb.Card<ComplianceDecree>()
    ];

    public override IReadOnlyList<RelicModel> StartingRelics =>
    [
        ModelDb.Relic<BurningBlade>()
    ];

    public override CardPoolModel CardPool => ModelDb.CardPool<Master_of_MankindCardPool>();
    public override RelicPoolModel RelicPool => ModelDb.RelicPool<Master_of_MankindRelicPool>();
    public override PotionPoolModel PotionPool => ModelDb.PotionPool<Master_of_MankindPotionPool>();

    /*  PlaceholderCharacterModel will utilize placeholder basegame assets for most of your character assets until you
        override all the other methods that define those assets.
        These are just some of the simplest assets, given some placeholders to differentiate your character with.
        You don't have to, but you're suggested to rename these images. */
    public override Control CustomIcon
    {
        get
        {
            var icon = NodeFactory<Control>.CreateFromResource(CustomIconTexturePath);
            icon.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
            return icon;
        }
    }

    public override string CustomIconTexturePath => "character_icon_char_name.png".CharacterUiPath();
    public override string CustomCharacterSelectBg =>
        $"{MainFile.ResPath}/scenes/emperor/char_select_bg_emperor.tscn";
    public override string CustomCharacterSelectIconPath => "char_select_char_name.png".CharacterUiPath();
    public override string CustomCharacterSelectLockedIconPath => "char_select_char_name_locked.png".CharacterUiPath();
    public override string CustomMapMarkerPath => "map_marker_char_name.png".CharacterUiPath();
    public override string CustomVisualPath => $"{MainFile.ResPath}/scenes/emperor/emperor.tscn";
    public override string CustomRestSiteAnimPath =>
        $"{MainFile.ResPath}/scenes/emperor/emperor_rest_site.tscn";
    public override string CustomMerchantAnimPath =>
        $"{MainFile.ResPath}/scenes/emperor/emperor_merchant.tscn";

    public override CreatureAnimator SetupCustomAnimationStates(MegaSprite controller)
    {
        // The current LoongBones rig only defines "idle". Map every combat trigger to that
        // animation so the base character animator never requests missing vanilla animations.
        return SetupAnimationState(
            controller,
            idleName: "idle",
            deadName: "defeat", deadLoop: false,
            hitName: "hit", hitLoop: false,
            attackName: "attack", attackLoop: false,
            castName: "idle", castLoop: true,
            relaxedName: "idle", relaxedLoop: true);
    }
}
