using BaseLib.Utils;
using Master_of_Mankind.Master_of_MankindCode.Character;
using Master_of_Mankind.Master_of_MankindCode.Foresight;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Master_of_Mankind.Master_of_MankindCode.Cards.Common;

/// <summary>한국어 이름: 수호자의 미늘창 / 효과: 피해 9 / 수치 조절: Damage.</summary>
[Pool(typeof(Master_of_MankindCardPool))]
public sealed class CustodianHalberd() : Master_of_MankindCard(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(9m, ValueProp.Move)];
    protected override Task OnPlay(PlayerChoiceContext c, CardPlay p) => CommonActions.CardAttack(this, p).Execute(c);
    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3m);
}

/// <summary>한국어 이름: 커스토디안의 맹공 / 효과: 피해 17 / 수치 조절: Damage.</summary>
[Pool(typeof(Master_of_MankindCardPool))]
public sealed class CustodianOnslaught() : Master_of_MankindCard(2, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(17m, ValueProp.Move)];
    protected override Task OnPlay(PlayerChoiceContext c, CardPlay p) => CommonActions.CardAttack(this, p).Execute(c);
    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(5m);
}

/// <summary>한국어 이름: 통일의 볼트 / 효과: 피해를 주고 예지한 비공격 행동이면 1장 드로우 / 수치 조절: Damage.</summary>
[Pool(typeof(Master_of_MankindCardPool))]
public sealed class BoltOfUnification() : Master_of_MankindCard(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    protected override bool ShouldGlowGoldInternal => CombatState is { } state
        && ForesightPredictionService.AnyNextRevealedActionIsKnownNonAttack(state, Owner.Creature);

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(8m, ValueProp.Move), new CardsVar(1)];
    protected override async Task OnPlay(PlayerChoiceContext c, CardPlay p)
    {
        ArgumentNullException.ThrowIfNull(p.Target);
        bool draw = ForesightPredictionService.IsNextRevealedActionKnownNonAttack(p.Target, Owner.Creature);
        await CommonActions.CardAttack(this, p).Execute(c);
        if (draw) await CardPileCmd.Draw(c, DynamicVars.Cards.IntValue, Owner);
    }
    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3m);
}

/// <summary>한국어 이름: 불결한 자를 정화하라 / 효과: 디버프 대상에게 추가 피해 / 수치 조절: Damage와 BonusDamage.</summary>
[Pool(typeof(Master_of_MankindCardPool))]
public sealed class PurgeTheUnclean() : Master_of_MankindCard(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    protected override bool ShouldGlowGoldInternal => CombatState is { } state
        && state.HittableEnemies.Any(enemy => enemy.Powers.Any(power => power.Type == PowerType.Debuff));

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(7m, ValueProp.Move), new DynamicVar("BonusDamage", 4m)];
    protected override async Task OnPlay(PlayerChoiceContext c, CardPlay p)
    {
        ArgumentNullException.ThrowIfNull(p.Target);
        decimal damage = DynamicVars.Damage.BaseValue;
        if (p.Target.Powers.Any(power => power.Type == PowerType.Debuff))
            damage += DynamicVars["BonusDamage"].BaseValue;
        await DamageCmd.Attack(damage).FromCard(this).Targeting(p.Target).Execute(c);
    }
    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
        DynamicVars["BonusDamage"].UpgradeValueBy(2m);
    }
}

/// <summary>한국어 이름: 커스토디안 일제사격 / 효과: 모든 적에게 피해 11 / 수치 조절: Damage.</summary>
[Pool(typeof(Master_of_MankindCardPool))]
public sealed class LegioCustodesVolley() : Master_of_MankindCard(2, CardType.Attack, CardRarity.Common, TargetType.AllEnemies)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(11m, ValueProp.Move)];
    protected override Task OnPlay(PlayerChoiceContext c, CardPlay p) => CommonActions.CardAttack(this, p).Execute(c);
    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(4m);
}

/// <summary>한국어 이름: 진군 칙령 / 효과: 다음 턴부터 피해 16 집행 / 수치 조절: Damage.</summary>
[Pool(typeof(Master_of_MankindCardPool))]
public sealed class EdictOfAdvance() : DecreeCard(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(16m, ValueProp.Move)];
    protected override Task OnExecuteDecree(PlayerChoiceContext c, CardPlay p) => CommonActions.CardAttack(this, p).Execute(c);
    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(5m);
}

/// <summary>한국어 이름: 임페리얼 전술교범 / 효과: 2장 드로우 후 1장을 덱 맨 아래로 이동 / 수치 조절: Cards.</summary>
[Pool(typeof(Master_of_MankindCardPool))]
public sealed class TacticaImperialis() : Master_of_MankindCard(0, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(2)];
    protected override async Task OnPlay(PlayerChoiceContext c, CardPlay p)
    {
        await CardPileCmd.Draw(c, DynamicVars.Cards.IntValue, Owner);
        if (Owner.PlayerCombatState is not { } state || state.Hand.Cards.Count == 0) return;
        CardSelectorPrefs prefs = new(SelectionScreenPrompt, 1) { Cancelable = false };
        CardModel? selected = (await CardSelectCmd.FromSimpleGrid(c, state.Hand.Cards.ToList(), Owner, prefs)).FirstOrDefault();
        if (selected is not null)
            await CardPileCmd.Add(selected, state.DrawPile, CardPilePosition.Bottom, null, false);
    }
    protected override void OnUpgrade() => DynamicVars.Cards.UpgradeValueBy(1);
}

/// <summary>한국어 이름: 테라의 수호 / 효과: 방어도 4, 예지한 공격이 있으면 7 / 수치 조절: Block과AttackBlock.</summary>
[Pool(typeof(Master_of_MankindCardPool))]
public sealed class WardOfTerra() : Master_of_MankindCard(0, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    protected override bool ShouldGlowGoldInternal => CombatState is { } state
        && ForesightPredictionService.AnyNextRevealedActionIsAttack(state, Owner.Creature);

    public override bool GainsBlock => true;
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new BlockVar(4m, ValueProp.Move), new DynamicVar("AttackBlock", 7m)];
    protected override Task OnPlay(PlayerChoiceContext c, CardPlay p)
    {
        decimal block = CombatState is { } state
                        && ForesightPredictionService.AnyNextRevealedActionIsAttack(state, Owner.Creature)
            ? DynamicVars["AttackBlock"].BaseValue : DynamicVars.Block.BaseValue;
        return CreatureCmd.GainBlock(Owner.Creature, block, ValueProp.Move, p);
    }
    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(2m);
        DynamicVars["AttackBlock"].UpgradeValueBy(3m);
    }
}
