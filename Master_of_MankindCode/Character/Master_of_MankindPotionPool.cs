using BaseLib.Abstracts;
using Master_of_Mankind.Master_of_MankindCode.Extensions;
using Godot;

namespace Master_of_Mankind.Master_of_MankindCode.Character;

public class Master_of_MankindPotionPool : CustomPotionPoolModel
{
    public override Color LabOutlineColor => Emperor.Color;


    public override string BigEnergyIconPath => "charui/big_energy.png".ImagePath();
    public override string TextEnergyIconPath => "charui/text_energy.png".ImagePath();
}
