using BaseLib.Utils;
using Master_of_Mankind.Master_of_MankindCode.Character;
using Master_of_Mankind.Master_of_MankindCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Master_of_Mankind.Master_of_MankindCode.Cards.Basic;

/// <summary>
/// 한국어 이름: 황제의 예지
/// 효과: 모든 적의 미래 행동 3개를 공개하며, 강화 시 모든 적에게 약화와 취약을 부여합니다.
/// 수치 조절: 기본 비용은 생성자 첫 번째 인수, 강화 효과 수치는 CanonicalVars의 Power에서 변경합니다.
/// </summary>
[Pool(typeof(Master_of_MankindCardPool))]
public sealed class EmperorsForesight() : Master_of_MankindCard(2, CardType.Power, CardRarity.Basic, TargetType.None)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new PowerVar<EmperorsForesightPower>(3), new DynamicVar("Power", 2m)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CommonActions.ApplySelf<EmperorsForesightPower>(choiceContext, this);

        if (!IsUpgraded || CombatState is not { } combatState)
            return;

        foreach (var enemy in combatState.HittableEnemies.ToList())
        {
            await PowerCmd.Apply<WeakPower>(choiceContext, enemy, DynamicVars["Power"].BaseValue, Owner.Creature, this);
            await PowerCmd.Apply<VulnerablePower>(choiceContext, enemy, DynamicVars["Power"].BaseValue, Owner.Creature, this);
        }
    }
}
