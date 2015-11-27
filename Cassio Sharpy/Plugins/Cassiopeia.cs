using System;
using System.Linq;

using LeagueSharp;
using LeagueSharp.Common;

using Color = System.Drawing.Color;

namespace Cassio_Sharpy.Plugins
{
    public class Cassiopeia
    {
        private Menu Menu;
        private Orbwalking.Orbwalker Orbwalker { get { return MenuProvider.Orbwalker; } }
        public SpellSlot Flash = ObjectManager.Player.GetSpellSlot("summonerFlash");
        private Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        private Spell Q, W, E, R;

        public Cassiopeia()
        {
            Q = new Spell(SpellSlot.Q, 887f, TargetSelector.DamageType.Magical);
            W = new Spell(SpellSlot.W, 900f, TargetSelector.DamageType.Magical);
            E = new Spell(SpellSlot.E, 700f, TargetSelector.DamageType.Magical);
            R = new Spell(SpellSlot.R, 850f, TargetSelector.DamageType.Magical);

            Q.SetSkillshot(.75f, 75f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            W.SetSkillshot(.5f, 100f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            E.SetTargetted(0f, float.MaxValue);
            R.SetSkillshot(.3f, (float)(90f + Math.PI / 180), float.MaxValue, false, SkillshotType.SkillshotCone);

            MenuProvider.Champion.Combo.addUseQ();
            MenuProvider.Champion.Combo.addUseW();
            MenuProvider.Champion.Combo.addUseE();
            MenuProvider.Champion.Combo.addUseR();
            //MenuProvider.Champion.Combo.addItem("UltMinHitFacing", new Slider(1,1,5));
            MenuProvider.Champion.Combo.addItem(("R + Flash"), new KeyBind('T', KeyBindType.Press));
            MenuProvider.Champion.Combo.addItem(("Auto Harass Use Q"), new KeyBind('H', KeyBindType.Toggle));
            MenuProvider.Champion.Combo.addItem("UltMinEnemiesHit", new Slider(1, 1, 5));

            MenuProvider.Champion.Harass.addUseQ();
            MenuProvider.Champion.Harass.addUseW();
            MenuProvider.Champion.Harass.addUseE();
            MenuProvider.Champion.Harass.addIfMana(60);

            MenuProvider.Champion.Lasthit.addUseE();
            MenuProvider.Champion.Lasthit.addIfMana(60);

            MenuProvider.Champion.Laneclear.addUseQ();
            MenuProvider.Champion.Laneclear.addUseW();
            MenuProvider.Champion.Laneclear.addUseE();
            MenuProvider.Champion.Laneclear.addIfMana(60);

            MenuProvider.Champion.Jungleclear.addUseQ();
            MenuProvider.Champion.Jungleclear.addUseW();
            MenuProvider.Champion.Jungleclear.addUseE();
            MenuProvider.Champion.Jungleclear.addIfMana(60);

            MenuProvider.Champion.Misc.addHitchanceSelector();
            MenuProvider.Champion.Misc.addItem("UseRAntiGapcloser",true);
            MenuProvider.Champion.Misc.addItem("UseInterrupter",true);

            MenuProvider.Champion.Drawings.addDrawQrange(Color.Green, true);
            MenuProvider.Champion.Drawings.addDrawWrange(Color.Green, true);
            MenuProvider.Champion.Drawings.addDrawErange(Color.Green, true);
            MenuProvider.Champion.Drawings.addDrawRrange(Color.Green, true);
            MenuProvider.Champion.Drawings.addDamageIndicator(getComboDamage);
            MenuProvider.Champion.Drawings.addItem("DrawR + FlashRange", new Circle(true, Color.Green, 3));

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Orbwalking.BeforeAttack += Orbwalking_BeforeAttack;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
        }

        private void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            var Rint = MenuProvider.Champion.Misc.getBoolValue("UseInterrupter");

            if (!MenuProvider.Champion.Misc.getBoolValue("UseInterrupter") || Player.IsDead)
                return;

            if (Rint && args.DangerLevel >= Interrupter2.DangerLevel.High && sender.IsValidTarget(R.Range) && sender.IsFacing(Player))
                R.CastOnBestTarget(0f, false, true);
        }

        private void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            var RAnti = MenuProvider.Champion.Misc.getBoolValue("UseRAntiGapcloser");

            if (!MenuProvider.Champion.Misc.getBoolValue("UseRAntiGapcloser") || Player.IsDead)
                return;

            if (RAnti && gapcloser.Sender.IsValidTarget(R.Range) && R.IsReady() && !gapcloser.Sender.IsInvulnerable)
            {
                R.Cast(gapcloser.Sender, false);
            }
        }

        private void Orbwalking_BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            if (!args.Unit.IsMe)
                return;

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo || (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LastHit && E.IsReady() && Player.ManaPercent > MenuProvider.Champion.Lasthit.IfMana && MenuProvider.Champion.Lasthit.UseE))
                args.Process = false;

        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
                return;

            var drawQ = MenuProvider.Champion.Drawings.DrawQrange;
            var drawW = MenuProvider.Champion.Drawings.DrawWrange;
            var drawE = MenuProvider.Champion.Drawings.DrawErange;
            var drawR = MenuProvider.Champion.Drawings.DrawRrange;
            var drawRF = MenuProvider.Champion.Drawings.getCircleValue("DrawR + FlashRange");

            if (Q.IsReady() && drawQ.Active)
                Render.Circle.DrawCircle(Player.Position, Q.Range, drawQ.Color, 3);

            if (W.IsReady() && drawW.Active)
                Render.Circle.DrawCircle(Player.Position, W.Range, drawW.Color, 3);

            if (E.IsReady() && drawE.Active)
                Render.Circle.DrawCircle(Player.Position, E.Range, drawE.Color, 3);

            if (R.IsReady() && drawR.Active)
                Render.Circle.DrawCircle(Player.Position, R.Range, drawR.Color, 3);

            if (R.IsReady() && Flash.IsReady() && drawRF.Active)
                Render.Circle.DrawCircle(Player.Position, R.Range + 350 , drawRF.Color, 3); 
        }

        private void Game_OnUpdate(EventArgs args)
        {
            var AutoHarass = MenuProvider.Champion.Combo.getKeyBindValue("Auto Harass Use Q");
            var RF = MenuProvider.Champion.Combo.getKeyBindValue("R + Flash");

            if (Player.IsDead)
                return;
            
            Q.MinHitChance = MenuProvider.Champion.Misc.SelectedHitchance;
            W.MinHitChance = MenuProvider.Champion.Misc.SelectedHitchance;
            R.MinHitChance = MenuProvider.Champion.Misc.SelectedHitchance;

            if (Orbwalking.CanMove(10))
            {
                switch (Orbwalker.ActiveMode)
                {
                    case Orbwalking.OrbwalkingMode.Combo:
                        Combo();
                        break;

                    case Orbwalking.OrbwalkingMode.Mixed:
                        Harass();
                        break;

                    case Orbwalking.OrbwalkingMode.LastHit:
                        Lasthit();
                        break;

                    case Orbwalking.OrbwalkingMode.LaneClear:
                        Laneclear();
                        Jungleclear();
                        break;

                    case Orbwalking.OrbwalkingMode.None:
                        break;
                }
                if (RF.Active)
                {
                    ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                    var target = TargetSelector.GetSelectedTarget();
                    if (target != null && target.IsValidTarget() && !target.IsZombie)
                    {
                        if (Flash != SpellSlot.Unknown && Flash.IsReady()
                            && R.IsReady() && (Player.Distance(target.Position) <= 1150))
                        {
                            R.Cast();
                            Utility.DelayAction.Add(5, () => Player.Spellbook.CastSpell(Flash, target.Position));
                        }
                    }
                }

                if (AutoHarass.Active)
                {
                    if (!(Player.ManaPercent > MenuProvider.Champion.Harass.IfMana))
                        return;

                    if (MenuProvider.Orbwalker.ActiveMode !=
                        Orbwalking.OrbwalkingMode.Combo)

                        if (!ObjectManager.Player.IsRecalling())
                        {
                            if (Q.IsReady())

                                if (MenuProvider.Champion.Harass.UseQ)
                                
                                    Q.CastOnBestTarget(0f, false, true);
                        }
                }
            }
        }

        private bool IsPoisoned(Obj_AI_Base unit)
        {
            return
            unit.Buffs.Where(buff => buff.IsActive && buff.Type == BuffType.Poison)
            .Any(buff => buff.EndTime >= (Game.Time + .35 + 700 / 1900));
        }
        private void Combo()
        {
             var UltMinEnemiesHit = MenuProvider.Champion.Combo.getSliderValue("UltMinEnemiesHit").Value;
            //var UltMinHitFacing = MenuProvider.Champion.Combo.getSliderValue("UltMinHitFacing").Value;

            if (MenuProvider.Champion.Combo.UseQ && Q.IsReady())
                Q.CastOnBestTarget(0f, false, true);

            if (MenuProvider.Champion.Combo.UseW && W.IsReady())
                W.CastOnBestTarget(0f, false, true);

            if (MenuProvider.Champion.Combo.UseE && E.IsReady())
            {
                var Etarget = HeroManager.Enemies.Where(x => x.IsValidTarget(E.Range) && IsPoisoned(x) && !TargetSelector.IsInvulnerable(x, E.DamageType)).OrderByDescending(x => E.GetDamage(x)).FirstOrDefault();
                
                if (Etarget != null)
                E.CastOnBestTarget(0f, false, true);
            }

            if (MenuProvider.Champion.Combo.UseR && R.IsReady())
            {
                var rTarget = HeroManager.Enemies.Where(x => x.IsValidTarget(R.Range)).ToList();
                //var rTargetFacing = rTarget.Where(x => x.IsFacing(Player)).ToList();

                if (rTarget.Count() >= UltMinEnemiesHit) //&& rTargetFacing.Count() >= UltMinHitFacing)
                    R.CastOnBestTarget(0f, false, true);
            }

        }
        private void Harass()
        {
            if (!(Player.ManaPercent > MenuProvider.Champion.Harass.IfMana))
                return;

            if (MenuProvider.Champion.Harass.UseQ && Q.IsReady())
                Q.CastOnBestTarget(0f, false, true);

            if (MenuProvider.Champion.Harass.UseW && W.IsReady())
                W.CastOnBestTarget(0f, false, true);

            if (MenuProvider.Champion.Harass.UseE && E.IsReady())
            {
                var Etarget = HeroManager.Enemies.Where(x => x.IsValidTarget(E.Range) && IsPoisoned(x) && !TargetSelector.IsInvulnerable(x, E.DamageType)).OrderByDescending(x => E.GetDamage(x)).FirstOrDefault();

                if (Etarget != null)
                    E.CastOnBestTarget(0f, false, true);
            }
        }
        private void Lasthit()
        {
            if (!(Player.ManaPercent > MenuProvider.Champion.Lasthit.IfMana))
                return;

            var Minions = MinionManager.GetMinions(1000, MinionTypes.All, MinionTeam.Enemy);

            if (Minions.Count <= 0)
                return;

            if (MenuProvider.Champion.Lasthit.UseE && E.IsReady())
            {
                //var eTarget = Minions.FirstOrDefault(x => x.IsValidTarget(E.Range) && HealthPrediction.GetHealthPrediction(x, (int)(E.Delay + 1000)) <= E.GetDamage(x, 1)); 잘못된 로직
                var eTarget = Minions.FirstOrDefault(x => Func.isKillable(x, E, 0) && HealthPrediction.GetHealthPrediction(x, (int)(Player.Distance(x, false) / E.Speed), (int)(E.Delay * 1000 + Game.Ping / 2)) > 0);

                if (eTarget != null)
                    E.Cast(eTarget);
            }
        }
        private void Laneclear()
        {
            if (!(Player.ManaPercent > MenuProvider.Champion.Laneclear.IfMana))
                return;

            var WMinions = MinionManager.GetMinions(1000, MinionTypes.Ranged, MinionTeam.Enemy);
            var Minions = MinionManager.GetMinions(1000, MinionTypes.All, MinionTeam.Enemy);

            if (Minions.Count <= 0)
                return;

            if (MenuProvider.Champion.Laneclear.UseQ && Q.IsReady())
            {
                var qloc = Q.GetCircularFarmLocation(Minions.Where(x => x.IsValidTarget(Q.Range)).ToList());
                
                if (qloc.MinionsHit >= 1)
                    Q.Cast(qloc.Position);
            }
            if (MenuProvider.Champion.Laneclear.UseW && W.IsReady())
            {
                var wloc = W.GetCircularFarmLocation(Minions.Where(x => x.IsValidTarget(W.Range)).ToList());

                if (wloc.MinionsHit >= 1)
                    W.Cast(wloc.Position);
            }
            if (MenuProvider.Champion.Laneclear.UseE && E.IsReady())
            {
                var eTarget = Minions.FirstOrDefault(x => x.IsValidTarget(E.Range) && HealthPrediction.GetHealthPrediction(x, (int)(Player.Distance(x, false) / E.Speed), (int)(E.Delay * 1000 + Game.Ping / 2)) > 0 && IsPoisoned(x));

                if (eTarget != null)
                    E.CastOnUnit(eTarget);
            }
        }
        private void Jungleclear()
        {
            if (!(Player.ManaPercent > MenuProvider.Champion.Jungleclear.IfMana))
                return;

            var Mobs = MinionManager.GetMinions(1000, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (Mobs.Count <= 0)
                return;

            if (MenuProvider.Champion.Jungleclear.UseQ && Q.IsReady())
            {
                var qloc = Q.GetCircularFarmLocation(Mobs.Where(x => x.IsValidTarget(Q.Range)).ToList());

                if (qloc.MinionsHit >= 1)
                    Q.Cast(qloc.Position);
            }

            if (MenuProvider.Champion.Jungleclear.UseW && W.IsReady())
            {
                var wloc = W.GetCircularFarmLocation(Mobs.Where(x => x.IsValidTarget(W.Range)).ToList());

                if (wloc.MinionsHit >= 1)
                    W.Cast(wloc.Position);
            }
            if (MenuProvider.Champion.Jungleclear.UseE && E.IsReady())
            {
                var eTarget = Mobs.FirstOrDefault(x => Func.isKillable(x, E, 0) && HealthPrediction.GetHealthPrediction(x, (int)(Player.Distance(x, false) / E.Speed), (int)(E.Delay * 1000 + Game.Ping / 2)) > 0 && IsPoisoned(x));

                if (eTarget != null)
                    E.CastOnUnit(eTarget);
            }
        }
        private float getComboDamage(Obj_AI_Base enemy)
        {
            float damage = 0;

            if (Q.IsReady())
                damage += Q.GetDamage(enemy);
            if (W.IsReady())
                damage += W.GetDamage(enemy);
            if (E.IsReady())
                damage += E.GetDamage(enemy) * 4;
            if (R.IsReady())
                damage += R.GetDamage(enemy);

            return damage;
        }
    }
}
