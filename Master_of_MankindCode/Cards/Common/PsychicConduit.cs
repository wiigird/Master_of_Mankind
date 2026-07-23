using BaseLib.Utils;
using Master_of_Mankind.Master_of_MankindCode.Character;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Master_of_Mankind.Master_of_MankindCode.Cards.Common;

/// <summary>
/// 한국어 이름: 사이킥 도관
/// 효과: 소멸하며 에너지 2를 얻는 일반 에너지 카드입니다. 강화하면 보존을 얻습니다.
/// 수치 조절: EnergyGain을 변경하면 에너지 획득량이 바뀝니다.
/// </summary>
[Pool(typeof(Master_of_MankindCardPool))]
public sealed class PsychicConduit : Master_of_MankindCard
{
    // 밸런스 조정: 카드 사용 시 획득하는 에너지입니다.
    private const int EnergyGain = 2;

    public PsychicConduit() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self) { }

    public override HashSet<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new EnergyVar(EnergyGain)];

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) =>
        PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, Owner);

    protected override void OnUpgrade() => CardCmd.ApplyKeyword(this, CardKeyword.Retain);
}
