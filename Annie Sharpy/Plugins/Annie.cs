using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace Annie_Sharpy.Plugins
{
    class Annie
    {
        public Menu Menu;
        public Orbwalking.Orbwalker Orbwalker
        {
            get
            {
                return MenuProvider.Orbwalker;
            }
        }
        public Obj_AI_Hero Player = ObjectManager.Player;
        public GameObject Tibbers;
        public SpellSlot Flash = ObjectManager.Player.GetSpellSlot("summonerFlash");
        public float FlashRange = 450f;
        public Spell Q, W, E, R;

        public bool HaveTibbers
        {
            get
            {
                return Player.HasBuff("infernalguardintimer");
            }
        }

        public Annie()
        {
            Console.WriteLine("Annie Sharpy Loaded");

            Q = new Spell(SpellSlot.Q, 625f, TargetSelector.DamageType.Magical);
            W = new Spell(SpellSlot.W, 620f, TargetSelector.DamageType.Magical) { MinHitChance = HitChance.High };
            E = new Spell(SpellSlot.E);
            R = new Spell(SpellSlot.R, 700f, TargetSelector.DamageType.Magical) { MinHitChance = HitChance.High };

            Q.SetTargetted(.25f, float.MaxValue);
            W.SetSkillshot(.25f, (float)(90f + Math.PI / 180), float.MaxValue, false, SkillshotType.SkillshotCone);
            R.SetSkillshot(0f, 150f, float.MaxValue, false, SkillshotType.SkillshotCircle);

            MenuProvider.Champion.Combo.addUseQ();
            MenuProvider.Champion.Combo.addUseW();
            MenuProvider.Champion.Combo.addUseR();
            MenuProvider.Champion.Combo.addItem("R Min Enimies Hit", new Slider(1, 1, 5));
            MenuProvider.Champion.Combo.addItem("R Only Passive", true);
            MenuProvider.Champion.Combo.addItem("Flash + R", new KeyBind('T', KeyBindType.Press));
            MenuProvider.Champion.Combo.addItem("Flash + R Enimies Count", new Slider(2, 1, 5));

            MenuProvider.Champion.Harass.addUseQ();
            MenuProvider.Champion.Harass.addUseW();
            MenuProvider.Champion.Harass.addIfMana();

            MenuProvider.Champion.Lasthit.addUseQ();
            MenuProvider.Champion.Lasthit.addIfMana(0);

            MenuProvider.Champion.Flee.addUseE();
            MenuProvider.Champion.Flee.addIfMana(60);

            MenuProvider.Champion.Laneclear.addUseQ();
            MenuProvider.Champion.Laneclear.addUseW();
            MenuProvider.Champion.Laneclear.addIfMana(60);

            MenuProvider.Champion.Jungleclear.addUseQ();
            MenuProvider.Champion.Jungleclear.addUseW();
            MenuProvider.Champion.Jungleclear.addIfMana(60);

            MenuProvider.Champion.Misc.addUseAntiGapcloser();
            MenuProvider.Champion.Misc.addUseInterrupter();

            MenuProvider.Champion.Drawings.addDrawQrange(Color.Green, true);
            MenuProvider.Champion.Drawings.addDrawWrange(Color.Green, true);
            MenuProvider.Champion.Drawings.addDrawRrange(Color.Green, true);
            MenuProvider.Champion.Drawings.addItem("Draw Flash + R Range", new Circle(true, Color.Green));
            MenuProvider.Champion.Drawings.addDamageIndicator(getcombodamage);

            Game.OnUpdate += Game_OnUpdate;
            Orbwalking.BeforeAttack += Orbwalking_BeforeAttack;
            Drawing.OnDraw += Drawing_OnDraw;
            GameObject.OnCreate += GameObject_OnCreate;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
        }

        private void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            var interrupter = MenuProvider.Champion.Misc.UseInterrupter;

            if (!interrupter || Player.IsDead)
            {
                return;
            }

            if (interrupter && args.DangerLevel >= Interrupter2.DangerLevel.High && W.IsReady() && sender.IsValidTarget(W.Range) && Player.HasBuff("pyromania_particle"))
            {
                W.CastOnBestTarget(0f, false, true);
            }

            else if (interrupter && args.DangerLevel >= Interrupter2.DangerLevel.High && Q.IsReady() && sender.IsValidTarget(Q.Range) && Player.HasBuff("pyromania_particle"))
            {
                Q.CastOnBestTarget(0f, false, true);
            }

            else if (interrupter && args.DangerLevel >= Interrupter2.DangerLevel.High && R.IsReady() && sender.IsValidTarget(R.Range) && HaveTibbers && Player.HasBuff("pyromania_particle"))
            {
                R.CastOnBestTarget(0f, false, true);
            }
        }

        private void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            var antigapcloser = MenuProvider.Champion.Misc.UseAntiGapcloser;

            if (!antigapcloser || Player.IsDead)
            {
                return;
            }

            if (antigapcloser && gapcloser.Sender.IsValidTarget(W.Range) && W.IsReady() && !gapcloser.Sender.IsInvulnerable && Player.HasBuff("pyromania_particle"))
            {
                W.Cast(gapcloser.Sender);
            }

            else if (antigapcloser && gapcloser.Sender.IsValidTarget(Q.Range) && Q.IsReady() && !gapcloser.Sender.IsInvulnerable && Player.HasBuff("pyromania_particle"))
            {
                Q.Cast(gapcloser.Sender);
            }

            else if (antigapcloser && gapcloser.Sender.IsValidTarget(R.Range) && R.IsReady() && !gapcloser.Sender.IsInvulnerable && Player.HasBuff("pyromania_particle") && !HaveTibbers)
            {
                R.Cast(gapcloser.Sender);
            }
        }

        private void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            if (sender.IsValid && sender.Name == "Tibbers")
            {
                Tibbers = sender;
            }
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
            {
                return;
            }

            var drawQ = MenuProvider.Champion.Drawings.DrawQrange;
            var drawW = MenuProvider.Champion.Drawings.DrawWrange;
            var drawR = MenuProvider.Champion.Drawings.DrawRrange;
            var drawFR = MenuProvider.Champion.Drawings.getCircleValue("Draw Flash + R Range");

            if (Q.IsReady() && drawQ.Active)
            {
                Render.Circle.DrawCircle(Player.Position, Q.Range, drawQ.Color, 3);
            }

            if (W.IsReady() && drawW.Active)
            {
                Render.Circle.DrawCircle(Player.Position, W.Range, drawW.Color, 3);
            }

            if (R.IsReady() && drawR.Active && !HaveTibbers)
            {
                Render.Circle.DrawCircle(Player.Position, R.Range, drawR.Color, 3);
            }

            if (R.IsReady() && drawFR.Active && Flash.IsReady() && !HaveTibbers)
            {
                Render.Circle.DrawCircle(Player.Position, R.Range + 350, drawFR.Color, 3);
            }
        }

        private void Orbwalking_BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            var LHM = MenuProvider.Champion.Lasthit.IfMana;
            var LHQ = MenuProvider.Champion.Lasthit.UseQ;

            if (!args.Unit.IsMe)
            {
                return;
            }

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LastHit && Q.IsReady() && Player.ManaPercent > LHM && LHQ)
            {
                args.Process = false;
            }
        }

        private void Game_OnUpdate(EventArgs args)
        {
            if (Orbwalking.CanMove(20))
            {
                var CFR = MenuProvider.Champion.Combo.getKeyBindValue("Flash + R").Active;
                var CFRU = MenuProvider.Champion.Combo.getSliderValue("Flash + R Enimies Count").Value;

                switch (Orbwalker.ActiveMode)
                {
                    case Orbwalking.OrbwalkingMode.Flee:
                        Flee();
                        break;
                    case Orbwalking.OrbwalkingMode.LastHit:
                        LastHit();
                        break;
                    case Orbwalking.OrbwalkingMode.Mixed:
                        Harass();
                        break;
                    case Orbwalking.OrbwalkingMode.LaneClear:
                        LaneClear();
                        JungleClear();
                        break;
                    case Orbwalking.OrbwalkingMode.Combo:
                        Combo();
                        break;
                    case Orbwalking.OrbwalkingMode.None:
                        break;
                }

                if (CFR)
                {
                    Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);

                    var cfrTarget = TargetSelector.GetTarget(R.Range + FlashRange, TargetSelector.DamageType.Magical);
                    var cfrPos = cfrTarget.Position.Extend(Prediction.GetPrediction(cfrTarget, 1).UnitPosition, FlashRange);
                    var cfrCount = HeroManager.Enemies.Where(x => x.IsValidTarget(R.Range + FlashRange)).ToList();

                    if (cfrTarget == null)
                    {
                        return;
                    }

                    if (HaveTibbers)
                    {
                        Combo();
                    }

                    if (Player.HasBuff("pyromania_particle") && !HaveTibbers && R.IsReady() && !cfrTarget.IsZombie)
                    {
                        if (cfrCount.Count >= CFRU)
                        {
                            Player.Spellbook.CastSpell(Flash, cfrPos);
                            R.Cast(R.GetPrediction(cfrTarget).CastPosition);
                        }
                    }
                }

                if (HaveTibbers)
                {
                    var tTarget = TargetSelector.GetTarget(2200, TargetSelector.DamageType.Magical);

                    if (tTarget.IsValidTarget(2200))
                    {
                        Player.IssueOrder(GameObjectOrder.AutoAttackPet, tTarget.Position);
                        R.Cast(tTarget);
                    }
                }
            }
        }

        private void Flee()
        {
            var FE = MenuProvider.Champion.Flee.UseE;
            var FM = MenuProvider.Champion.Flee.IfMana;

            if (!(Player.ManaPercent > FM))
            {
                return;
            }

            if (FE && E.IsReady() && !(Player.HasBuff("pyromania_particle")))
            {
                E.Cast();
            }
        }

        private void LastHit()
        {
            var LHM = MenuProvider.Champion.Lasthit.IfMana;
            var LHQ = MenuProvider.Champion.Lasthit.UseQ;

            if (!(Player.ManaPercent > LHM))
            {
                return;
            }

            var Minions = MinionManager.GetMinions(1000, MinionTypes.All, MinionTeam.Enemy);

            if (Minions.Count <= 0)
            {
                return;
            }

            if (LHQ && Q.IsReady())
            {
                var qTarget = Minions.FirstOrDefault(x => Func.isKillable(x, Q, 0) && HealthPrediction.GetHealthPrediction(x, (int)(Player.Distance(x, false) / Q.Speed), (int)(Q.Delay * 1000 + Game.Ping / 2)) > 0);

                if (qTarget != null)
                {
                    Q.Cast(qTarget);
                }
            }
        }

        private void Combo()
        {
            var CQ = MenuProvider.Champion.Combo.UseQ;
            var CW = MenuProvider.Champion.Combo.UseW;
            var CR = MenuProvider.Champion.Combo.UseR;
            var CU = MenuProvider.Champion.Combo.getSliderValue("R Min Enimies Hit").Value;
            var CO = MenuProvider.Champion.Combo.getBoolValue("R Only Passive");

            if (CO && CR && R.IsReady() && !HaveTibbers && Player.HasBuff("pyromania_particle"))
            {
                var rTarget = HeroManager.Enemies.Where(x => x.IsValidTarget(R.Range)).ToList();

                if (rTarget.Count() >= CU)
                {
                    R.CastOnBestTarget(0f, false, true);
                }
            }

            else if (CR && R.IsReady() && !CO && R.IsReady() && !HaveTibbers)
            {
                var rTarget = HeroManager.Enemies.Where(x => x.IsValidTarget(R.Range)).ToList();

                if (rTarget.Count() >= CU)
                {
                    R.CastOnBestTarget(0f, false, true);
                }
            }

            if (CW && W.IsReady())
            {
                W.CastOnBestTarget(0f, false, true);
            }

            if (CQ && Q.IsReady())
            {
                var qTarget = HeroManager.Enemies.Where(x => x.IsValidTarget(Q.Range) && !TargetSelector.IsInvulnerable(x, Q.DamageType)).OrderByDescending(x => Q.GetDamage(x)).FirstOrDefault();

                if (qTarget != null)
                {
                    Q.Cast(qTarget);
                }
            }            
        }
        
        private void Harass()
        {
            var HQ = MenuProvider.Champion.Harass.UseQ;
            var HW = MenuProvider.Champion.Harass.UseW;
            var HM = MenuProvider.Champion.Harass.IfMana;

            if (!(Player.ManaPercent > HM))
            {
                return;
            }

            if (HW && W.IsReady())
            {
                W.CastOnBestTarget(0f, false, true);
            }

            if (HQ && Q.IsReady())
            {
                var qTarget = HeroManager.Enemies.Where(x => x.IsValidTarget(Q.Range) && !TargetSelector.IsInvulnerable(x, Q.DamageType)).OrderByDescending(x => Q.GetDamage(x)).FirstOrDefault();

                if (qTarget != null)
                {
                    Q.Cast(qTarget);
                }
            }
        }

        private void LaneClear()
        {
            var Minions = MinionManager.GetMinions(1000, MinionTypes.All, MinionTeam.Enemy);
            var LCM = MenuProvider.Champion.Laneclear.IfMana;
            var LCQ = MenuProvider.Champion.Laneclear.UseQ;
            var LCW = MenuProvider.Champion.Laneclear.UseW;

            if (!(Player.ManaPercent > LCM))
            {
                return;
            }

            if (Minions.Count <= 0)
            {
                return;
            }

            if (LCW && W.IsReady())
            {
                var wLoc = W.GetLineFarmLocation(Minions.Where(x => x.IsValidTarget(W.Range)).ToList());

                if (wLoc.MinionsHit >= 1)
                {
                    W.Cast(wLoc.Position);
                }
            }

            if (LCQ && Q.IsReady())
            {
                var qTarget = Minions.FirstOrDefault(x => x.IsValidTarget(Q.Range) && HealthPrediction.GetHealthPrediction(x, (int)(Player.Distance(x, false) / Q.Speed), (int)(Q.Delay * 1000 + Game.Ping / 2)) > 0);

                if (qTarget != null)
                {
                    Q.Cast(qTarget);
                }
            }
        }

        private void JungleClear()
        {
            var Mobs = MinionManager.GetMinions(1000, MinionTypes.All, MinionTeam.Neutral);
            var JCQ = MenuProvider.Champion.Jungleclear.UseQ;
            var JCW = MenuProvider.Champion.Jungleclear.UseW;
            var JCM = MenuProvider.Champion.Jungleclear.IfMana;

            if (!(Player.ManaPercent > JCM))
            {
                return;
            }

            if (Mobs.Count <= 0)
            {
                return;
            }

            if (JCW && W.IsReady())
            {
                var wLoc = W.GetLineFarmLocation(Mobs.Where(x => x.IsValidTarget(W.Range)).ToList());

                if (wLoc.MinionsHit >= 1)
                {
                    W.Cast(wLoc.Position);
                }
            }

            if (JCQ && Q.IsReady())
            {
                var qTarget = Mobs.FirstOrDefault(x => x.IsValidTarget(Q.Range) && HealthPrediction.GetHealthPrediction(x, (int)(Player.Distance(x, false) / Q.Speed), (int)(Q.Delay * 1000 + Game.Ping / 2)) > 0);

                if (qTarget != null)
                {
                    Q.Cast(qTarget);
                }
            }
        }

        private float getcombodamage(Obj_AI_Base enemy)
        {
            float damage = 0;

            if (Q.IsReady())
            {
                damage += Q.GetDamage(enemy);
            }

            if (W.IsReady())
            {
                damage += W.GetDamage(enemy);
            }

            if (R.IsReady())
            {
                damage += R.GetDamage(enemy);
            }

            return damage;
        }
    }
}
