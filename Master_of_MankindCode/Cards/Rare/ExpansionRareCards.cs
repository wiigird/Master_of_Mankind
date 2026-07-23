using BaseLib.Utils;
using Master_of_Mankind.Master_of_MankindCode.Character;
using Master_of_Mankind.Master_of_MankindCode.Decree;
using Master_of_Mankind.Master_of_MankindCode.Foresight;
using Master_of_Mankind.Master_of_MankindCode.Powers;
using Master_of_Mankind.Master_of_MankindCode.Retain;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Master_of_Mankind.Master_of_MankindCode.Cards.Rare;

/// <summary>한국어 이름: 황제의 검 / 효과: 방어도를 무시하는 피해 20, 보존 / 수치 조절: Damage.</summary>
[Pool(typeof(Master_of_MankindCardPool))]
public sealed class TheEmperorsSword() : Master_of_MankindCard(2, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Retain];
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(20m, ValueProp.Move | ValueProp.Unblockable)];
    protected override Task OnPlay(PlayerChoiceContext c, CardPlay p)
    { ArgumentNullException.ThrowIfNull(p.Target); return DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(p.Target).Execute(c); }
    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(8);
}

/// <summary>한국어 이름: 호루스의 파멸 / 효과: 피해 24, 공격 예지 시 한 번 더 / 수치 조절: Damage.</summary>
[Pool(typeof(Master_of_MankindCardPool))]
public sealed class BaneOfHorus() : Master_of_MankindCard(3, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
{
    protected override bool ShouldGlowGoldInternal => CombatState is { } state
        && ForesightPredictionService.AnyNextRevealedActionIsAttack(state);

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(24m, ValueProp.Move)];
    protected override Task OnPlay(PlayerChoiceContext c, CardPlay p)
    { ArgumentNullException.ThrowIfNull(p.Target); int hits = ForesightPredictionService.IsNextRevealedActionAttack(p.Target) ? 2 : 1; return DamageCmd.Attack(DynamicVars.Damage.BaseValue).WithHitCount(hits).FromCard(this).Targeting(p.Target).Execute(c); }
    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(6);
}

/// <summary>한국어 이름: 울라노르의 개선 / 효과: 피해 9를 3회, 처치 시 힘 1 / 수치 조절: Damage, Strength.</summary>
[Pool(typeof(Master_of_MankindCardPool))]
public sealed class TriumphAtUllanor() : Master_of_MankindCard(2, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(9m, ValueProp.Move), new PowerVar<StrengthPower>(1m)];
    protected override async Task OnPlay(PlayerChoiceContext c, CardPlay p)
    { ArgumentNullException.ThrowIfNull(p.Target); var target = p.Target; await DamageCmd.Attack(DynamicVars.Damage.BaseValue).WithHitCount(3).FromCard(this).Targeting(target).Execute(c); if (target.IsDead) await PowerCmd.Apply<StrengthPower>(c, Owner.Creature, DynamicVars.Strength.BaseValue, Owner.Creature, this); }
    protected override void OnUpgrade() { DynamicVars.Damage.UpgradeValueBy(2); DynamicVars.Strength.UpgradeValueBy(1); }
}

/// <summary>한국어 이름: 임페라토르 솜니움의 포격 / 효과: 모든 적 피해 24, 다음 턴 2장 추가 드로우 / 수치 조절: Damage, Cards.</summary>
[Pool(typeof(Master_of_MankindCardPool))]
public sealed class ImperatorSomniumBroadside() : Master_of_MankindCard(3, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(24m, ValueProp.Move)];
    protected override Task OnPlay(PlayerChoiceContext c, CardPlay p) => CommonActions.CardAttack(this, p).Execute(c);
    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}

/// <summary>한국어 이름: 아나테마의 손길 / 효과: 피해 12, 약화와 취약 2 / 수치 조절: Damage, Power.</summary>
[Pool(typeof(Master_of_MankindCardPool))]
public sealed class AnathemasTouch() : Master_of_MankindCard(1, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(12m, ValueProp.Move), new DynamicVar("Power", 2m)];
    protected override async Task OnPlay(PlayerChoiceContext c, CardPlay p)
    { ArgumentNullException.ThrowIfNull(p.Target); await CommonActions.CardAttack(this, p).Execute(c); await PowerCmd.Apply<WeakPower>(c, p.Target, DynamicVars["Power"].BaseValue, Owner.Creature, this); await PowerCmd.Apply<VulnerablePower>(c, p.Target, DynamicVars["Power"].BaseValue, Owner.Creature, this); }
    protected override void OnUpgrade() { DynamicVars.Damage.UpgradeValueBy(4); DynamicVars["Power"].UpgradeValueBy(1); }
}

/// <summary>한국어 이름: 최후의 칙령 / 효과: 피해 30, 다른 준비 칙령마다 +6 / 수치 조절: Damage, PerDecree.</summary>
[Pool(typeof(Master_of_MankindCardPool))]
public sealed class FinalDecree() : DecreeCard(2, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
{
    protected override bool ShouldGlowGoldInternal => DecreeManager.GetPrepared(Owner).Count > 0;

    public override HashSet<CardKeyword> CanonicalKeywords => [.. base.CanonicalKeywords, CardKeyword.Exhaust];
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(30m, ValueProp.Move), new DynamicVar("PerDecree", 6m)];
    protected override Task OnExecuteDecree(PlayerChoiceContext c, CardPlay p)
    { ArgumentNullException.ThrowIfNull(p.Target); int others = DecreeManager.GetPrepared(Owner).Count; decimal damage = DynamicVars.Damage.BaseValue + others * DynamicVars["PerDecree"].BaseValue; return DamageCmd.Attack(damage).FromCard(this).Targeting(p.Target).Execute(c); }
    protected override void OnUpgrade() { DynamicVars.Damage.UpgradeValueBy(8); DynamicVars["PerDecree"].UpgradeValueBy(2); }
}

/// <summary>한국어 이름: 말카도르의 희생 / 효과: 체력 6 상실, 에너지 2, 드로우 3 / 수치 조절: HpLoss.</summary>
[Pool(typeof(Master_of_MankindCardPool))]
public sealed class MalcadorsSacrifice() : Master_of_MankindCard(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    protected override IEnumerable<DynamicVar> CanonicalVars => [new HpLossVar(6m), new EnergyVar(2), new CardsVar(3)];
    protected override async Task OnPlay(PlayerChoiceContext c, CardPlay p)
    { await CreatureCmd.Damage(c, Owner.Creature, DynamicVars.HpLoss.BaseValue, ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move, Owner.Creature, this); await PlayerCmd.GainEnergy(2, Owner); await CardPileCmd.Draw(c, 3, Owner); }
    protected override void OnUpgrade() => DynamicVars.HpLoss.UpgradeValueBy(-3);
}

/// <summary>한국어 이름: 황제께서 보호하신다 / 효과: 방어도 30 / 수치 조절: Block.</summary>
[Pool(typeof(Master_of_MankindCardPool))]
public sealed class TheEmperorProtects() : Master_of_MankindCard(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    public override bool GainsBlock => true;
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust, CardKeyword.Retain];
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(25m, ValueProp.Move)];
    protected override Task OnPlay(PlayerChoiceContext c, CardPlay p) => CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, p);
    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(5);
}

/// <summary>한국어 이름: 웹웨이의 지배자 / 효과: 버린 카드 최대 3장을 손으로 회수하고 보존 / 수치 조절: 강화 시 EnergyCost.</summary>
[Pool(typeof(Master_of_MankindCardPool))]
public sealed class MasterTheWebway() : Master_of_MankindCard(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    protected override async Task OnPlay(PlayerChoiceContext c, CardPlay p)
    { if (Owner.PlayerCombatState is not { } state || state.DiscardPile.Cards.Count == 0) return; int max = Math.Min(3, state.DiscardPile.Cards.Count); CardSelectorPrefs prefs = new(SelectionScreenPrompt, 0, max) { RequireManualConfirmation = true }; foreach (CardModel card in (await CardSelectCmd.FromCombatPile(c, state.DiscardPile, Owner, prefs)).ToList()) { await CardPileCmd.Add(card, state.Hand, CardPilePosition.Bottom, null, false); card.GiveSingleTurnRetain(); } }
    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}

/// <summary>한국어 이름: 짐의 의지로 / 효과: 손의 다른 모든 카드에 보존과 사용할 때까지 비용 -1 / 수치 조절: 강화 시 EnergyCost.</summary>
[Pool(typeof(Master_of_MankindCardPool))]
public sealed class ByMyWill() : Master_of_MankindCard(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    protected override async Task OnPlay(PlayerChoiceContext c, CardPlay p)
    {
        if (!Owner.Creature.HasPower<PerfectMomentControllerPower>())
            await PowerCmd.Apply<PerfectMomentControllerPower>(c, Owner.Creature, 1, Owner.Creature, this, silent: true);
        if (Owner.PlayerCombatState is { } state)
            foreach (CardModel card in state.Hand.Cards.Where(card => !ReferenceEquals(card, this)).ToList())
                TemporaryRetainState.Apply(card);
    }
    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}

/// <summary>한국어 이름: 전쟁의 완성 / 효과: 전투 중 모든 전투 카드 강화 / 수치 조절: 강화 시 EnergyCost.</summary>
[Pool(typeof(Master_of_MankindCardPool))]
public sealed class ApotheosisOfWar() : Master_of_MankindCard(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    protected override async Task OnPlay(PlayerChoiceContext c, CardPlay p)
    {
        await PowerCmd.Apply<ApotheosisOfWarPower>(c, Owner.Creature, 1, Owner.Creature, this);
        if (IsUpgraded && Owner.PlayerCombatState is { } state)
            foreach (CardModel card in state.Hand.Cards.Where(card => !ReferenceEquals(card, this) && card.IsUpgradable))
                CardCmd.Upgrade(card);
    }

    protected override void OnUpgrade() { }
}

/// <summary>한국어 이름: 완전한 예지 / 효과: 4장 드로우, 이 효과로 뽑은 카드 이번 턴 보존 / 수치 조절: Cards.</summary>
[Pool(typeof(Master_of_MankindCardPool))]
public sealed class PerfectPrescience() : Master_of_MankindCard(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(4)];
    protected override async Task OnPlay(PlayerChoiceContext c, CardPlay p)
    { if (Owner.PlayerCombatState is not { } state) return; HashSet<CardModel> before = state.Hand.Cards.ToHashSet(); await CardPileCmd.Draw(c, DynamicVars.Cards.IntValue, Owner); foreach (CardModel card in state.Hand.Cards.Where(card => !before.Contains(card))) card.GiveSingleTurnRetain(); }
    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}

/// <summary>한국어 이름: 시길라이트의 인장 / 효과: 모든 디버프 제거, 인공물 2 / 수치 조절: Artifact.</summary>
[Pool(typeof(Master_of_MankindCardPool))]
public sealed class SealOfTheSigillite() : Master_of_MankindCard(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<ArtifactPower>(2)];
    protected override async Task OnPlay(PlayerChoiceContext c, CardPlay p)
    { foreach (var power in Owner.Creature.Powers.Where(power => power.Type == MegaCrit.Sts2.Core.Entities.Powers.PowerType.Debuff).ToList()) await PowerCmd.Remove(power); await PowerCmd.Apply<ArtifactPower>(c, Owner.Creature, DynamicVars["ArtifactPower"].BaseValue, Owner.Creature, this); }
    protected override void OnUpgrade() => DynamicVars["ArtifactPower"].UpgradeValueBy(1);
}

/// <summary>한국어 이름: 군단을 부르라 / 효과: 뽑을 더미 공격 최대 2장을 손으로 가져오고 보존 / 수치 조절: 강화 시 EnergyCost.</summary>
[Pool(typeof(Master_of_MankindCardPool))]
public sealed class CallTheLegiones() : Master_of_MankindCard(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    protected override async Task OnPlay(PlayerChoiceContext c, CardPlay p)
    { if (Owner.PlayerCombatState is not { } state) return; List<CardModel> attacks = state.DrawPile.Cards.Where(card => card.Type == CardType.Attack).ToList(); if (attacks.Count == 0) return; int max = Math.Min(2, attacks.Count); CardSelectorPrefs prefs = new(SelectionScreenPrompt, 0, max) { RequireManualConfirmation = true }; foreach (CardModel card in (await CardSelectCmd.FromSimpleGrid(c, attacks, Owner, prefs)).ToList()) { await CardPileCmd.Add(card, state.Hand, CardPilePosition.Bottom, null, false); card.GiveSingleTurnRetain(); } }
    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}

/// <summary>한국어 이름: 아스트로노미칸 / 효과: 직전 턴 보존 수만큼 최대 2장 추가 드로우 / 수치 조절: Cap.</summary>
[Pool(typeof(Master_of_MankindCardPool))]
public sealed class TheAstronomican() : Master_of_MankindCard(2, CardType.Power, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Cap", 2)];
    protected override Task OnPlay(PlayerChoiceContext c, CardPlay p) => PowerCmd.Apply<TheAstronomicanPower>(c, Owner.Creature, DynamicVars["Cap"].BaseValue, Owner.Creature, this);
    protected override void OnUpgrade() => DynamicVars["Cap"].UpgradeValueBy(1);
}

/// <summary>한국어 이름: 제국의 대명령 / 효과: 준비하는 모든 칙령 즉시 집행 / 수치 조절: 강화 시 EnergyCost.</summary>
[Pool(typeof(Master_of_MankindCardPool))]
public sealed class ImperialMandate() : Master_of_MankindCard(3, CardType.Power, CardRarity.Rare, TargetType.Self)
{
    protected override Task OnPlay(PlayerChoiceContext c, CardPlay p) => PowerCmd.Apply<ImperialMandatePower>(c, Owner.Creature, 1, Owner.Creature, this);
    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}

/// <summary>한국어 이름: 현현한 예지 / 효과: 예지에 따라 방어 10 또는 에너지와 드로우 / 수치 조절: Block.</summary>
[Pool(typeof(Master_of_MankindCardPool))]
public sealed class PrescienceIncarnate() : Master_of_MankindCard(2, CardType.Power, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(10m, ValueProp.Unpowered)];
    protected override Task OnPlay(PlayerChoiceContext c, CardPlay p) => PowerCmd.Apply<PrescienceIncarnatePower>(c, Owner.Creature, DynamicVars.Block.BaseValue, Owner.Creature, this);
    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(4);
}

/// <summary>한국어 이름: 황궁 성소 / 효과: 방어도가 턴 시작 시 사라지지 않음 / 수치 조절: 강화 시 EnergyCost.</summary>
[Pool(typeof(Master_of_MankindCardPool))]
public sealed class SanctumImperialis() : Master_of_MankindCard(2, CardType.Power, CardRarity.Rare, TargetType.Self)
{
    protected override Task OnPlay(PlayerChoiceContext c, CardPlay p) => PowerCmd.Apply<SanctumImperialisPower>(c, Owner.Creature, 1, Owner.Creature, this);
    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}

/// <summary>한국어 이름: 인류의 신격화 / 효과: 매 턴 힘과 민첩 1 / 수치 조절: 강화 시 EnergyCost.</summary>
[Pool(typeof(Master_of_MankindCardPool))]
public sealed class ApotheosisOfMan() : Master_of_MankindCard(3, CardType.Power, CardRarity.Rare, TargetType.Self)
{
    protected override Task OnPlay(PlayerChoiceContext c, CardPlay p) => PowerCmd.Apply<ApotheosisOfManPower>(c, Owner.Creature, 1, Owner.Creature, this);
    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}

/// <summary>한국어 이름: 마지막 교회 / 효과: 매 턴 처음 인공물 획득 시 1장 드로우 / 수치 조절: 강화 시 시작 Artifact.</summary>
[Pool(typeof(Master_of_MankindCardPool))]
public sealed class TheLastChurch() : Master_of_MankindCard(1, CardType.Power, CardRarity.Rare, TargetType.Self)
{
    protected override async Task OnPlay(PlayerChoiceContext c, CardPlay p)
    { await PowerCmd.Apply<TheLastChurchPower>(c, Owner.Creature, 1, Owner.Creature, this); await PowerCmd.Apply<ArtifactPower>(c, Owner.Creature, 1, Owner.Creature, this); }
    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}

/// <summary>한국어 이름: 웹웨이 전쟁 / 효과: 칙령 집행마다 에너지 1, 강화 시 드로우 1 / 수치 조절: Cards.</summary>
[Pool(typeof(Master_of_MankindCardPool))]
public sealed class WarInTheWebway() : Master_of_MankindCard(2, CardType.Power, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(0)];
    protected override async Task OnPlay(PlayerChoiceContext c, CardPlay p)
    { await PowerCmd.Apply<WarInTheWebwayPower>(c, Owner.Creature, 1, Owner.Creature, this); if (Owner.Creature.GetPower<WarInTheWebwayPower>() is { } power) power.DynamicVars.Cards.BaseValue = Math.Max(power.DynamicVars.Cards.BaseValue, DynamicVars.Cards.BaseValue); }
    protected override void OnUpgrade() => DynamicVars.Cards.UpgradeValueBy(1);
}

/// <summary>한국어 이름: 기계신의 계시 / 효과: 매 턴 처음 비용 2 이상 카드 사용 시 에너지 1 / 수치 조절: 강화 시 EnergyCost.</summary>
[Pool(typeof(Master_of_MankindCardPool))]
public sealed class MachineGodsRevelation() : Master_of_MankindCard(2, CardType.Power, CardRarity.Rare, TargetType.Self)
{
    protected override Task OnPlay(PlayerChoiceContext c, CardPlay p) => PowerCmd.Apply<MachineGodsRevelationPower>(c, Owner.Creature, 1, Owner.Creature, this);
    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
