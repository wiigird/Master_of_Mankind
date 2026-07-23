using BaseLib.Utils;
using Master_of_Mankind.Master_of_MankindCode.Character;
using Master_of_Mankind.Master_of_MankindCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Master_of_Mankind.Master_of_MankindCode.Cards.Uncommon;

/// <summary>
/// 한국어 이름: 인류의 주인
/// 효과: 매 턴 첫 고비용 카드 사용 시 드로우하고 공격이면 방어도를 얻는 파워입니다.
/// 수치 조절: CanonicalVars의 드로우 및 방어도와 OnUpgrade 수치를 변경합니다.
/// </summary>
[Pool(typeof(Master_of_MankindCardPool))]
public sealed class MasterOfMankind : Master_of_MankindCard
{
    public MasterOfMankind() : base(2, CardType.Power, CardRarity.Uncommon, TargetType.None) { }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<MasterOfMankindPower>(1m),
        new CardsVar(1),
        new DynamicVar("Block", 3m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        MasterOfMankindPower? power = await PowerCmd.Apply<MasterOfMankindPower>(
            choiceContext,
            Owner.Creature,
            1m,
            Owner.Creature,
            this,
            silent: false);
        if (power is not null)
            power.DynamicVars["Block"].UpgradeValueBy(DynamicVars["Block"].BaseValue);
    }

    protected override void OnUpgrade() => DynamicVars["Block"].UpgradeValueBy(2m);
}
