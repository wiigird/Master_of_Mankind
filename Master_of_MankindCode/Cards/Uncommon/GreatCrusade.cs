using BaseLib.Utils;
using Master_of_Mankind.Master_of_MankindCode.Character;
using Master_of_Mankind.Master_of_MankindCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Master_of_Mankind.Master_of_MankindCode.Cards.Uncommon;

/// <summary>
/// 한국어 이름: 대성전
/// 효과: 턴마다 방어도를 얻고 칙령 집행 시 모든 적에게 피해를 주는 파워입니다.
/// 수치 조절: CanonicalVars의 Block과 Damage 및 OnUpgrade 수치를 변경합니다.
/// </summary>
[Pool(typeof(Master_of_MankindCardPool))]
public sealed class GreatCrusade : Master_of_MankindCard
{
    public GreatCrusade() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.None) { }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<GreatCrusadePower>(1m),
        new DynamicVar("Block", 2m),
        new DynamicVar("Damage", 4m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        GreatCrusadePower? power = await PowerCmd.Apply<GreatCrusadePower>(
            choiceContext,
            Owner.Creature,
            1m,
            Owner.Creature,
            this,
            silent: false);
        if (power is not null)
        {
            power.DynamicVars["Block"].UpgradeValueBy(DynamicVars["Block"].BaseValue);
            power.DynamicVars["Damage"].UpgradeValueBy(DynamicVars["Damage"].BaseValue);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Block"].UpgradeValueBy(1m);
        DynamicVars["Damage"].UpgradeValueBy(1m);
    }
}
