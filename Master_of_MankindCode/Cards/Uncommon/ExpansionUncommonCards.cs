using BaseLib.Utils;
using Master_of_Mankind.Master_of_MankindCode.Character;
using Master_of_Mankind.Master_of_MankindCode.Decree;
using Master_of_Mankind.Master_of_MankindCode.Foresight;
using Master_of_Mankind.Master_of_MankindCode.Powers;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Master_of_Mankind.Master_of_MankindCode.Cards.Uncommon;

internal static class ExpansionCardSelection
{
    public static async Task<CardModel?> OneFromHand(PlayerChoiceContext c, CardModel source, Func<CardModel, bool> filter)
    {
        if (source.Owner.PlayerCombatState is not { } state) return null;
        List<CardModel> choices = state.Hand.Cards.Where(filter).ToList();
        if (choices.Count == 0) return null;
        CardSelectorPrefs prefs = new(CardSelectorPrefs.DiscardSelectionPrompt, 1) { Cancelable = false };
        return (await CardSelectCmd.FromSimpleGrid(c, choices, source.Owner, prefs)).FirstOrDefault();
    }
}

/// <summary>한국어 이름: 근위대의 수호창 / 효과: 피해 7, 방어도 6 / 수치 조절: Damage, Block.</summary>
[Pool(typeof(Master_of_MankindCardPool))]
public sealed class GuardianSpear() : Master_of_MankindCard(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    public override bool GainsBlock => true;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(7m, ValueProp.Move), new BlockVar(6m, ValueProp.Move)];
    protected override async Task OnPlay(PlayerChoiceContext c, CardPlay p)
    { await CommonActions.CardAttack(this, p).Execute(c); await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, p); }
    protected override void OnUpgrade() { DynamicVars.Damage.UpgradeValueBy(3); DynamicVars.Block.UpgradeValueBy(2); }
}

/// <summary>한국어 이름: 번개 발톱 / 효과: 피해 4를 2회 / 수치 조절: Damage.</summary>
[Pool(typeof(Master_of_MankindCardPool))]
public sealed class LightningClaw() : Master_of_MankindCard(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(4m, ValueProp.Move), new PowerVar<WeakPower>(1m)];

    protected override async Task OnPlay(PlayerChoiceContext c, CardPlay p)
    {
        ArgumentNullException.ThrowIfNull(p.Target);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).WithHitCount(2).FromCard(this).Targeting(p.Target).Execute(c);
        await PowerCmd.Apply<WeakPower>(c, p.Target, DynamicVars["WeakPower"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(1);
        DynamicVars["WeakPower"].UpgradeValueBy(1);
    }
}

/// <summary>한국어 이름: 사이킥 창 / 효과: 피해 15, 비공격 행동 예지 시 에너지 1 / 수치 조절: Damage.</summary>
[Pool(typeof(Master_of_MankindCardPool))]
public sealed class PsychicLance() : Master_of_MankindCard(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override bool ShouldGlowGoldInternal => CombatState is { } state
        && ForesightPredictionService.AnyNextRevealedActionIsKnownNonAttack(state, Owner.Creature);

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(15m, ValueProp.Move), new EnergyVar(1)];
    protected override async Task OnPlay(PlayerChoiceContext c, CardPlay p)
    { ArgumentNullException.ThrowIfNull(p.Target); bool gain = ForesightPredictionService.IsNextRevealedActionKnownNonAttack(p.Target, Owner.Creature); await CommonActions.CardAttack(this, p).Execute(c); if (gain) await PlayerCmd.GainEnergy(1, Owner); }
    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(5);
}

/// <summary>한국어 이름: 반역자의 파멸 / 효과: 피해 9, 비공격 행동 예지 시 한 번 더 / 수치 조절: Damage.</summary>
[Pool(typeof(Master_of_MankindCardPool))]
public sealed class BaneOfTheTraitor() : Master_of_MankindCard(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override bool ShouldGlowGoldInternal => CombatState is { } state
        && ForesightPredictionService.AnyNextRevealedActionIsKnownNonAttack(state, Owner.Creature);

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(9m, ValueProp.Move)];
    protected override Task OnPlay(PlayerChoiceContext c, CardPlay p)
    { ArgumentNullException.ThrowIfNull(p.Target); int hits = ForesightPredictionService.IsNextRevealedActionKnownNonAttack(p.Target, Owner.Creature) ? 2 : 1; return DamageCmd.Attack(DynamicVars.Damage.BaseValue).WithHitCount(hits).FromCard(this).Targeting(p.Target).Execute(c); }
    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3);
}

/// <summary>한국어 이름: 부케팔루스의 일제사격 / 효과: 모든 적 피해 12, 준비된 칙령이 있으면 +4 / 수치 조절: Damage, BonusDamage.</summary>
[Pool(typeof(Master_of_MankindCardPool))]
public sealed class BucephelusBroadside() : Master_of_MankindCard(2, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies)
{
    protected override bool ShouldGlowGoldInternal => DecreeManager.GetPrepared(Owner).Count > 0;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(12m, ValueProp.Move), new DynamicVar("BonusDamage", 4m)];
    protected override async Task OnPlay(PlayerChoiceContext c, CardPlay p)
    { decimal damage = DynamicVars.Damage.BaseValue + (DecreeManager.GetPrepared(Owner).Count > 0 ? DynamicVars["BonusDamage"].BaseValue : 0); foreach (var enemy in CombatState!.HittableEnemies.ToList()) await DamageCmd.Attack(damage).FromCard(this).Targeting(enemy).Execute(c); }
    protected override void OnUpgrade() { DynamicVars.Damage.UpgradeValueBy(3); DynamicVars["BonusDamage"].UpgradeValueBy(1); }
}

/// <summary>한국어 이름: 아라라트 산의 최후 / 효과: 피해 22, 보존될 때마다 전투 중 비용 1 감소 / 수치 조절: Damage.</summary>
[Pool(typeof(Master_of_MankindCardPool))]
public sealed class MountArarat() : Master_of_MankindCard(4, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    private const string RetainTriggeredKey = "RetainTriggered";

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Retain];
    protected override bool ShouldGlowGoldInternal => DynamicVars[RetainTriggeredKey].BaseValue > 0m;
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(22m, ValueProp.Move), new DynamicVar(RetainTriggeredKey, 0m)];
    protected override Task OnPlay(PlayerChoiceContext c, CardPlay p) => CommonActions.CardAttack(this, p).Execute(c);
    public override Task AfterFlush(PlayerChoiceContext c, Player player, IReadOnlyCollection<CardModel> flushed, IReadOnlyCollection<CardModel> retained)
    { if (retained.Contains(this)) { DynamicVars[RetainTriggeredKey].BaseValue = 1m; EnergyCost.AddThisCombat(-1, reduceOnly: true); } return Task.CompletedTask; }
    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(6);
}

/// <summary>한국어 이름: 참수 칙령 / 효과: 피해 28 / 수치 조절: Damage.</summary>
[Pool(typeof(Master_of_MankindCardPool))]
public sealed class EdictOfDecapitation() : DecreeCard(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    public override HashSet<CardKeyword> CanonicalKeywords => [.. base.CanonicalKeywords, CardKeyword.Exhaust];
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(28m, ValueProp.Move)];
    protected override Task OnExecuteDecree(PlayerChoiceContext c, CardPlay p) => CommonActions.CardAttack(this, p).Execute(c);
    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(8);
}

/// <summary>한국어 이름: 영원의 문 반격 / 효과: 현재 방어도만큼 피해 / 수치 조절: 강화 시 EnergyCost.</summary>
[Pool(typeof(Master_of_MankindCardPool))]
public sealed class EternityGateCounterstroke() : Master_of_MankindCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override Task OnPlay(PlayerChoiceContext c, CardPlay p) =>
        PowerCmd.Apply<EternityGateCounterstrokePower>(c, Owner.Creature, 1, Owner.Creature, this);

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}

/// <summary>한국어 이름: 일만인의 창 / 효과: 피해 7을 3회 / 수치 조절: Damage.</summary>
[Pool(typeof(Master_of_MankindCardPool))]
public sealed class SpearOfTheTenThousand() : Master_of_MankindCard(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(7m, ValueProp.Move)];
    protected override Task OnPlay(PlayerChoiceContext c, CardPlay p)
    { ArgumentNullException.ThrowIfNull(p.Target); return DamageCmd.Attack(DynamicVars.Damage.BaseValue).WithHitCount(3).FromCard(this).Targeting(p.Target).Execute(c); }
    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(2);
}

/// <summary>한국어 이름: 태양의 질책 / 효과: 피해 8, 힘 1 감소 / 수치 조절: Damage, Strength.</summary>
[Pool(typeof(Master_of_MankindCardPool))]
public sealed class SolarRebuke() : Master_of_MankindCard(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(8m, ValueProp.Move), new PowerVar<StrengthPower>(1m)];
    protected override async Task OnPlay(PlayerChoiceContext c, CardPlay p)
    { ArgumentNullException.ThrowIfNull(p.Target); await CommonActions.CardAttack(this, p).Execute(c); await PowerCmd.Apply<StrengthPower>(c, p.Target, -DynamicVars.Strength.BaseValue, Owner.Creature, this); }
    protected override void OnUpgrade() { DynamicVars.Damage.UpgradeValueBy(3); DynamicVars.Strength.UpgradeValueBy(1); }
}

/// <summary>한국어 이름: 니케아 칙령 / 효과: 모든 적에게 약화와 취약 2 / 수치 조절: Power.</summary>
[Pool(typeof(Master_of_MankindCardPool))]
public sealed class EdictOfNikaea() : DecreeCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.AllEnemies)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Power", 2m)];
    protected override async Task OnExecuteDecree(PlayerChoiceContext c, CardPlay p)
    { foreach (var enemy in CombatState!.HittableEnemies.ToList()) { await PowerCmd.Apply<WeakPower>(c, enemy, DynamicVars["Power"].BaseValue, Owner.Creature, this); await PowerCmd.Apply<VulnerablePower>(c, enemy, DynamicVars["Power"].BaseValue, Owner.Creature, this); } }
    protected override void OnUpgrade() => DynamicVars["Power"].UpgradeValueBy(1);
}

/// <summary>한국어 이름: 시길라이트의 회수 / 효과: 버린 카드 1장을 손으로 가져와 보존 / 수치 조절: 강화 시 EnergyCost.</summary>
[Pool(typeof(Master_of_MankindCardPool))]
public sealed class SigillitesRecall() : Master_of_MankindCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    protected override async Task OnPlay(PlayerChoiceContext c, CardPlay p)
    { if (Owner.PlayerCombatState is not { } state || state.DiscardPile.Cards.Count == 0) return; CardModel? card = (await CardSelectCmd.FromCombatPile(c, state.DiscardPile, Owner, new CardSelectorPrefs(SelectionScreenPrompt, 1))).FirstOrDefault(); if (card is null) return; await CardPileCmd.Add(card, state.Hand, CardPilePosition.Bottom, null, false); card.GiveSingleTurnRetain(); }
    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}

/// <summary>한국어 이름: 말하지 않은 제재 / 효과: 손의 다른 카드 1장 소멸, 2장 드로우 / 수치 조절: Cards.</summary>
[Pool(typeof(Master_of_MankindCardPool))]
public sealed class UnspokenSanction() : Master_of_MankindCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(2)];
    protected override async Task OnPlay(PlayerChoiceContext c, CardPlay p)
    { CardModel? card = await ExpansionCardSelection.OneFromHand(c, this, candidate => !ReferenceEquals(candidate, this)); if (card is null) return; await CardCmd.Exhaust(c, card); await CardPileCmd.Draw(c, DynamicVars.Cards.IntValue, Owner); }
    protected override void OnUpgrade() => DynamicVars.Cards.UpgradeValueBy(1);
}

/// <summary>한국어 이름: 커스토디안의 방패 / 효과: 방어도 18, 보존 / 수치 조절: Block.</summary>
[Pool(typeof(Master_of_MankindCardPool))]
public sealed class AegisOfTheCustodes() : Master_of_MankindCard(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    public override bool GainsBlock => true;
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Retain];
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(18m, ValueProp.Move)];
    protected override Task OnPlay(PlayerChoiceContext c, CardPlay p) => CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, p);
    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(6);
}

/// <summary>한국어 이름: 웹웨이의 기만 / 효과: 방어도 8, 공격 예지 시 1장 드로우 / 수치 조절: Block, Cards.</summary>
[Pool(typeof(Master_of_MankindCardPool))]
public sealed class WebwayFeint() : Master_of_MankindCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override bool ShouldGlowGoldInternal => CombatState is { } state
        && ForesightPredictionService.AnyNextRevealedActionIsAttack(state, Owner.Creature);

    public override bool GainsBlock => true;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(8m, ValueProp.Move), new CardsVar(1)];
    protected override async Task OnPlay(PlayerChoiceContext c, CardPlay p)
    { bool draw = CombatState is { } state && ForesightPredictionService.AnyNextRevealedActionIsAttack(state, Owner.Creature); await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, p); if (draw) await CardPileCmd.Draw(c, DynamicVars.Cards.IntValue, Owner); }
    protected override void OnUpgrade() { DynamicVars.Block.UpgradeValueBy(3); DynamicVars.Cards.UpgradeValueBy(1); }
}

/// <summary>한국어 이름: 제국 병참 / 효과: 2장 드로우, 다음 비용 2 이상 카드 비용 1 감소 / 수치 조절: Cards.</summary>
[Pool(typeof(Master_of_MankindCardPool))]
public sealed class ImperialLogistics() : Master_of_MankindCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(2)];
    protected override async Task OnPlay(PlayerChoiceContext c, CardPlay p)
    { await CardPileCmd.Draw(c, DynamicVars.Cards.IntValue, Owner); await PowerCmd.Apply<ImperialLogisticsPower>(c, Owner.Creature, 1, Owner.Creature, this); }
    protected override void OnUpgrade() => DynamicVars.Cards.UpgradeValueBy(1);
}

/// <summary>한국어 이름: 계산된 희생 / 효과: 체력 3 상실, 에너지 1과 카드 1장 / 수치 조절: HpLoss.</summary>
[Pool(typeof(Master_of_MankindCardPool))]
public sealed class CalculatedSacrifice() : Master_of_MankindCard(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    protected override IEnumerable<DynamicVar> CanonicalVars => [new HpLossVar(3m), new EnergyVar(1), new CardsVar(1)];
    protected override async Task OnPlay(PlayerChoiceContext c, CardPlay p)
    { await CreatureCmd.Damage(c, Owner.Creature, DynamicVars.HpLoss.BaseValue, ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move, Owner.Creature, this); await PlayerCmd.GainEnergy(1, Owner); await CardPileCmd.Draw(c, 1, Owner); }
    protected override void OnUpgrade() => DynamicVars.HpLoss.UpgradeValueBy(-2);
}

/// <summary>한국어 이름: 예견된 파멸 / 효과: 대상의 예지 행동에 따라 방어 또는 드로우 / 수치 조절: AttackBlock, Cards, Block.</summary>
[Pool(typeof(Master_of_MankindCardPool))]
public sealed class ForeseenDoom() : Master_of_MankindCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override bool ShouldGlowGoldInternal => CombatState is { } state
        && ForesightPredictionService.AnyKnownNextAction(state, Owner.Creature);

    public override bool GainsBlock => true;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("AttackBlock", 13m), new CardsVar(2), new BlockVar(7m, ValueProp.Move)];
    protected override async Task OnPlay(PlayerChoiceContext c, CardPlay p)
    { ArgumentNullException.ThrowIfNull(p.Target); if (ForesightPredictionService.IsNextRevealedActionAttack(p.Target, Owner.Creature)) await CreatureCmd.GainBlock(Owner.Creature, DynamicVars["AttackBlock"].BaseValue, ValueProp.Move, p); else if (ForesightPredictionService.IsNextRevealedActionKnownNonAttack(p.Target, Owner.Creature)) await CardPileCmd.Draw(c, DynamicVars.Cards.IntValue, Owner); else await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, p); }
    protected override void OnUpgrade() { DynamicVars["AttackBlock"].UpgradeValueBy(4); DynamicVars.Cards.UpgradeValueBy(1); DynamicVars.Block.UpgradeValueBy(2); }
}

/// <summary>한국어 이름: 장인의 무기고 / 효과: 공격 1장에 보존과 피해 +4 / 수치 조절: BonusDamage.</summary>
[Pool(typeof(Master_of_MankindCardPool))]
public sealed class MasterCraftedArsenal() : Master_of_MankindCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("BonusDamage", 4m)];
    protected override Task OnPlay(PlayerChoiceContext c, CardPlay p) => Apply(c);
    private async Task Apply(PlayerChoiceContext c)
    { CardModel? card = await ExpansionCardSelection.OneFromHand(c, this, candidate => candidate.Type == CardType.Attack); if (card is null) return; CardCmd.ApplyKeyword(card, CardKeyword.Retain); if (card.DynamicVars.TryGetValue("Damage", out DynamicVar? damage)) damage.BaseValue += DynamicVars["BonusDamage"].BaseValue; }
    protected override void OnUpgrade() => DynamicVars["BonusDamage"].UpgradeValueBy(2);
}

/// <summary>한국어 이름: 테라의 예비대 / 효과: 방어도 10, 다음 턴 에너지 1 / 수치 조절: Block.</summary>
[Pool(typeof(Master_of_MankindCardPool))]
public sealed class ReservesOfTerra() : Master_of_MankindCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    public override bool GainsBlock => true;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(10m, ValueProp.Move), new EnergyVar(1)];
    protected override async Task OnPlay(PlayerChoiceContext c, CardPlay p)
    { await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, p); await PowerCmd.Apply<EnergyNextTurnPower>(c, Owner.Creature, 1, Owner.Creature, this); }
    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(4);
}

/// <summary>한국어 이름: 황금의 길 / 효과: 예지에 따라 매 턴 방어 또는 드로우 / 수치 조절: Block.</summary>
[Pool(typeof(Master_of_MankindCardPool))]
public sealed class TheGoldenPath() : Master_of_MankindCard(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(4m, ValueProp.Unpowered)];
    protected override Task OnPlay(PlayerChoiceContext c, CardPlay p) => PowerCmd.Apply<TheGoldenPathPower>(c, Owner.Creature, DynamicVars.Block.BaseValue, Owner.Creature, this);
    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(2);
}

/// <summary>한국어 이름: 끝없는 명령 / 효과: 칙령 집행마다 카드 1장 드로우 / 수치 조절: 강화 시 EnergyCost.</summary>
[Pool(typeof(Master_of_MankindCardPool))]
public sealed class EndlessMandate() : Master_of_MankindCard(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
{
    protected override Task OnPlay(PlayerChoiceContext c, CardPlay p) => PowerCmd.Apply<EndlessMandatePower>(c, Owner.Creature, 1, Owner.Creature, this);
    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
