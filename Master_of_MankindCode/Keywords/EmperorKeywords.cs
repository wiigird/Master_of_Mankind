using BaseLib.Patches.Content;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace Master_of_Mankind.Master_of_MankindCode.Keywords;

public static class EmperorKeywords
{
    [CustomEnum]
    [KeywordProperties(AutoKeywordPosition.After)]
    public static CardKeyword Decree;
}
