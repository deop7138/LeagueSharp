using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LeagueSharp;
using LeagueSharp.Common;

using Color = System.Drawing.Color;
using SharpDX;

namespace Zed_Sharpy.Plugins
{
    public class Zed
    {
        public Menu Menu;
        public Obj_AI_Hero Player = ObjectManager.Player;
        public Spell Q, W, E, R;
        public Obj_AI_Minion Shadow;
        public Orbwalking.Orbwalker Orbwalker
        {
            get
            {
                return
                    MenuProvider.Orbwalker;
            }
        }

        public Zed()
        {
            Console.WriteLine("Zed Sharpy Loaded");

            Q = new Spell(SpellSlot.Q, 900f, TargetSelector.DamageType.Physical) { MinHitChance = HitChance.High };
            W = new Spell(SpellSlot.W, 700f) { MinHitChance = HitChance.High };
            E = new Spell(SpellSlot.E, 270f, TargetSelector.DamageType.Physical) { MinHitChance = HitChance.High };
            R = new Spell(SpellSlot.R, 650f);

            Q.SetSkillshot(.25f, 50f, float.MaxValue, false, SkillshotType.SkillshotLine);
            W.SetSkillshot(0f, 270f, float.MaxValue, false, SkillshotType.SkillshotCircle);

            MenuProvider.Champion.Combo.addUseQ();
            MenuProvider.Champion.Combo.addItem("W Use Only Line Combo");
            MenuProvider.Champion.Combo.addItem("Combo Use W is Manual :)");
            MenuProvider.Champion.Combo.addUseE();
            MenuProvider.Champion.Combo.addItem("Use R is Manual :)");

            MenuProvider.Champion.Harass.addUseQ();
            MenuProvider.Champion.Harass.addUseW();
            MenuProvider.Champion.Harass.addUseE();

            MenuProvider.Champion.Flee.addUseW();

            MenuProvider.Champion.Lasthit.addUseE();

            MenuProvider.Champion.Laneclear.addUseQ();
            MenuProvider.Champion.Laneclear.addUseE();

            MenuProvider.Champion.Jungleclear.addUseQ();
            MenuProvider.Champion.Jungleclear.addUseE();

            MenuProvider.Champion.Drawings.addDrawQrange(Color.Green,true);
            MenuProvider.Champion.Drawings.addDrawWrange(Color.Green, true);
            MenuProvider.Champion.Drawings.addDrawErange(Color.Green, true);
            MenuProvider.Champion.Drawings.addDrawRrange(Color.Green, true);
            MenuProvider.Champion.Drawings.addDamageIndicator(getcombodamage);            

            Game.OnUpdate += Game_OnUpdate;
            Orbwalking.BeforeAttack += Orbwalking_BeforeAttack;
            Spellbook.OnCastSpell += Spellbook_OnCastSpell;
            Drawing.OnDraw += Drawing_OnDraw;

        }

        private void Game_OnUpdate(EventArgs args)
        {
            if (Orbwalking.CanMove(20))
            {

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
                    case Orbwalking.OrbwalkingMode.LineCombo:
                        LineCombo();
                        break;
                    case Orbwalking.OrbwalkingMode.None:
                        break;
                }
            }
        }

        private void LineCombo()
        {
            var lcTarget = HeroManager.Enemies.Where(x => x.IsValidTarget(W.Range)).FirstOrDefault();
            var wPos = lcTarget.Position.Extend(Player.ServerPosition, -500);

            if (E.IsReady() && rlocation == CastR.Second)
            {
                E.Cast();
            }

            if (W.IsReady() && wlocation == CastW.First && rlocation == CastR.Second)
            {
                W.Cast(wPos);
            }

            if (Q.IsReady() && wlocation == CastW.Second && rlocation == CastR.Second)
            {
                Q.CastOnBestTarget(0f, false, true);
            }
        }

        private void Combo()
        {
            var CQ = MenuProvider.Champion.Combo.UseQ;
            var CE = MenuProvider.Champion.Combo.UseE;

            if (CE && E.IsReady())
            {
                E.CastOnBestTarget(0f, false, true);
            }

            if (CQ && Q.IsReady())
            {
                Q.CastOnBestTarget(0f, false, true);
            }
            
        }

        private void Harass()
        {
            var CQ = MenuProvider.Champion.Harass.UseQ;
            var CW = MenuProvider.Champion.Harass.UseW;
            var CE = MenuProvider.Champion.Harass.UseE;

            if (CW && wlocation == CastW.First && W.IsReady() && Player.Mana > W.ManaCost + E.ManaCost + Q.ManaCost)
            {
                W.CastOnBestTarget(0f, false, false);
            }

            if (CE && wlocation == CastW.Second && E.IsReady())
            {
                E.Cast();
            }

           if (CE && !CW && E.IsReady() || CE && !W.IsReady() && E.IsReady() || CE && E.IsReady() && !(Player.Mana > W.ManaCost + W.ManaCost + Q.ManaCost))
            {
                E.CastOnBestTarget(0f, false, true);
            }

            if (CQ && wlocation == CastW.Second && Q.IsReady())
            {
                Q.CastOnBestTarget(0f, false, true);
            }

           if (CQ && !CW && Q.IsReady() || CQ && !W.IsReady() && Q.IsReady() || CQ && Q.IsReady() && !(Player.Mana > W.ManaCost + W.ManaCost + Q.ManaCost))
            {
                Q.CastOnBestTarget(0f, false, true);
            }
        }

        private void LastHit()
        {
            var LHE = MenuProvider.Champion.Lasthit.UseE;
            var Minions = MinionManager.GetMinions(E.Range, MinionTypes.All, MinionTeam.Enemy);
            var eTarget = Minions.FirstOrDefault(x => Func.isKillable(x, E, 0) && HealthPrediction.GetHealthPrediction(x, (int)(Player.Distance(x, false) / E.Speed), (int)(E.Delay * 1000 * Game.Ping / 2)) > 0);

            if (!(Player.Mana > E.ManaCost))
            {
                return;
            }

            if (Minions.Count <= 0)
            {
                return;
            }

            if (LHE && E.IsReady())
            {
                if (eTarget != null)
                {
                    E.Cast(eTarget);
                }
            }
        }

        private void Flee()
        {
            var FW = MenuProvider.Champion.Flee.UseW;

            if (FW && W.IsReady() && wlocation == CastW.First && Player.Mana > W.ManaCost)
            {
                W.Cast(Game.CursorPos);
            }

            if (FW && W.IsReady() && wlocation == CastW.Second)
            {
                W.Cast();
            }
        }

        private void LaneClear()
        {
            var Minions = MinionManager.GetMinions(1000, MinionTypes.All, MinionTeam.Enemy);
            var QMinions = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.Enemy);
            var EMinions = MinionManager.GetMinions(E.Range, MinionTypes.All, MinionTeam.Enemy);
            var qLoc = Q.GetLineFarmLocation(QMinions.Where(x => x.IsValidTarget(Q.Range)).ToList());
            var LQ = MenuProvider.Champion.Laneclear.UseQ;
            var LE = MenuProvider.Champion.Laneclear.UseE;

            if (Minions.Count <= 0)
            {
                return;
            }

            if (LQ && Q.IsReady() && Player.Mana > Q.ManaCost)
            {
                if (qLoc.MinionsHit >= 1)
                {
                    Q.Cast(qLoc.Position);
                }
            }

            if (LE && E.IsReady() && Player.Mana > E.ManaCost)
            {
                if (EMinions.Count >= 1)
                {
                    E.Cast();
                }
            }
        }

        private void JungleClear()
        {
            var Mobs = MinionManager.GetMinions(1000, MinionTypes.All, MinionTeam.Neutral);
            var qLoc = Q.GetLineFarmLocation(Mobs.Where(x => x.IsValidTarget(Q.Range)).ToList());
            var JQ = MenuProvider.Champion.Jungleclear.UseQ;
            var JE = MenuProvider.Champion.Jungleclear.UseE;

            if (Mobs.Count <= 0)
            {
                return;
            }

            if (JQ && Q.IsReady() && Player.Mana > Q.ManaCost)
            {
                Q.Cast(qLoc.Position);
            }

            if (JE && E.IsReady() && Player.Mana > E.ManaCost)
            {
                E.Cast();
            }
        }

        private void Spellbook_OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (wlocation == CastW.Second || rlocation == CastR.First)
            {
                Orbwalking.ResetAutoAttackTimer();
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
            var drawE = MenuProvider.Champion.Drawings.DrawErange;
            var drawR = MenuProvider.Champion.Drawings.DrawRrange;

            if (Q.IsReady() && drawQ.Active)
            {
                Render.Circle.DrawCircle(Player.Position, Q.Range, drawQ.Color, 3);
            }

            if (W.IsReady() && drawW.Active && wlocation == CastW.First)
            {
                Render.Circle.DrawCircle(Player.Position, W.Range, drawW.Color, 3);
            }

            if (E.IsReady() && drawE.Active)
            {
                Render.Circle.DrawCircle(Player.Position, E.Range, drawE.Color, 3);
            }

            if (R.IsReady() && drawR.Active && rlocation == CastR.First)
            {
                Render.Circle.DrawCircle(Player.Position, R.Range, drawR.Color, 3);
            }
        }

        private void Orbwalking_BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            var LE = MenuProvider.Champion.Lasthit.UseE;

            if(!args.Unit.IsMe)
            {
                return;
            }

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LastHit && E.IsReady() && Player.Mana > E.ManaCost)
            {
                args.Process = false;
            }
        }

        private float getcombodamage(Obj_AI_Base enemy)
        {
            float damage = 0;

            if (Q.IsReady())
            {
                if (wlocation == CastW.Second)
                {
                    damage += Q.GetDamage(enemy) * 2;
                }

                if (wlocation == CastW.First || !W.IsReady())
                {
                    damage += Q.GetDamage(enemy);
                }
            }

            if (E.IsReady())
            {
                damage += E.GetDamage(enemy);
            }

            if (R.IsReady())
            {
                damage += R.GetDamage(enemy);
            }

            return damage;
        }

        enum CastR
        {
            First,
            Second,
            Cooltime
        }

        private CastR rlocation
        {
            get
            {
                if (!R.IsReady())
                {
                    return
                        CastR.Cooltime;
                }
                return
                    (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.R).Name == "ZedR" ? CastR.First : CastR.Second);
            }
        }

        enum CastW
        {
            First,
            Second,
            Cooltime
        }

        private CastW wlocation
        {
            get
            {
                if (!W.IsReady())
                {
                    return
                        CastW.Cooltime;
                }
                return
                    (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).Name == "ZedW" ? CastW.First : CastW.Second);
            }
        }

    }
}
