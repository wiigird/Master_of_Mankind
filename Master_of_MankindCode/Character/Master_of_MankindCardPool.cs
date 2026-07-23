using BaseLib.Abstracts;
using Master_of_Mankind.Master_of_MankindCode.Extensions;
using Godot;

namespace Master_of_Mankind.Master_of_MankindCode.Character;

public class Master_of_MankindCardPool : CustomCardPoolModel
{
    public override string Title => Emperor.CharacterId; //This is not a display name.

    public override string BigEnergyIconPath => "charui/big_energy.png".ImagePath();
    public override string TextEnergyIconPath => "charui/text_energy.png".ImagePath();


    /* These HSV values will determine the color of your card back.
    They are applied as a shader onto an already colored image,
    so it may take some experimentation to find a color you like.
    Generally they should be values between 0 and 1. */
    public override float H => 0.12f; //Golden yellow hue.
    public override float S => 0.82f; //Rich metallic saturation.
    public override float V => 1f; //Brightness

    //Alternatively, leave these values at 1 and provide a custom frame image.
    /*public override Texture2D CustomFrame(CustomCardModel card)
    {
        //This will attempt to load Master_of_Mankind/images/cards/frame.png
        return PreloadManager.Cache.GetTexture2D("cards/frame.png".ImagePath());
    }*/

    //Color of small card icons
    public override Color DeckEntryCardColor => Emperor.Color;

    public override bool IsColorless => false;
}
