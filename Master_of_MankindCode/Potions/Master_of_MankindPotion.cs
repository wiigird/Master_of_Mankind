using BaseLib.Abstracts;
using BaseLib.Utils;
using Master_of_Mankind.Master_of_MankindCode.Character;

namespace Master_of_Mankind.Master_of_MankindCode.Potions;

[Pool(typeof(Master_of_MankindPotionPool))]
public abstract class Master_of_MankindPotion : CustomPotionModel;