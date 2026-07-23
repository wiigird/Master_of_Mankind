using BaseLib.Utils;
using Master_of_Mankind.Master_of_MankindCode.Character;
using Master_of_Mankind.Master_of_MankindCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Master_of_Mankind.Master_of_MankindCode.Cards.Rare;

/// <summary>
/// 한국어 이름: 카오스의 축복
/// 효과: 카오스 권능과 영구 표식을 선택하는 희귀 파워입니다.
/// 수치 조절: CanonicalVars의 파워 수치와 OnUpgrade의 비용을 변경합니다.
/// </summary>
[Pool(typeof(Master_of_MankindCardPool))]
public sealed class BlessingsOfChaos : Master_of_MankindCard
{
    public BlessingsOfChaos() : base(3, CardType.Power, CardRarity.Rare, TargetType.None) { }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<ChaosControllerPower>(1m)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ChaosControllerPower? power = await PowerCmd.Apply<ChaosControllerPower>(
            choiceContext,
            Owner.Creature,
            1m,
            Owner.Creature,
            this,
            silent: false);
        if (power is not null)
            await power.ChooseBoons(choiceContext, 1, this);
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
