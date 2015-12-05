using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LeagueSharp;
using LeagueSharp.Common;
using ItemData = LeagueSharp.Common.Data.ItemData;
using Color = System.Drawing.Color;
using SharpDX;

namespace Zed_Sharpy.Plugins
{
    public class Zed
    {
        private Obj_AI_Hero Player = ObjectManager.Player;
        private Spell Q, W, E, R;
        private Vector3 rshadowpos;
        private Obj_AI_Minion shadow;
        private Orbwalking.Orbwalker Orbwalker
        {
            get
            {
                return
                    MenuProvider.Orbwalker;
            }
        }

        private enum wCheck
        {
            First,
            Second,
            Cooltime
        }

        private enum rCheck
        {
            First,
            Second,
            Cooltime
        }

        private wCheck wShadow
        {
            get
            {
                if (!W.isReadyPerfectly())
                {
                    return
                        wCheck.Cooltime;
                }
                return
                    (Player.Spellbook.GetSpell(SpellSlot.W).Name == "ZedW" ? wCheck.First : wCheck.Second);
            }
        }

        private rCheck rShadow
        {
            get
            {
                if (!R.isReadyPerfectly())
                {
                    return
                        rCheck.Cooltime;
                }
                return
                    (Player.Spellbook.GetSpell(SpellSlot.R).Name == "ZedR" ? rCheck.First : rCheck.Second);
            }
        }

        private Obj_AI_Minion checkWshadow
        {
            get
            {
                return
                    ObjectManager.Get<Obj_AI_Minion>().FirstOrDefault(x => x.IsVisible && x.IsAlly && x.Position != rshadowpos && x.Name == "Shadow");
            }
        }

        private Obj_AI_Minion checkRshadow
        {
            get
            {
                return
                    ObjectManager.Get<Obj_AI_Minion>().FirstOrDefault(x => x.IsVisible && x.IsAlly && x.Position == rshadowpos && x.Name == "Shadow");
            }
        }

        public Zed()
        {
            Game.PrintChat("<font Color = \"#00D8FF\">Zed Sharpy Loaded");

            Q = new Spell(SpellSlot.Q, 900f, TargetSelector.DamageType.Physical) { MinHitChance = HitChance.High };
            W = new Spell(SpellSlot.W, 700f) { MinHitChance = HitChance.High };
            E = new Spell(SpellSlot.E, 270f, TargetSelector.DamageType.Physical) { MinHitChance = HitChance.High };
            R = new Spell(SpellSlot.R, 650f,TargetSelector.DamageType.Physical);

            Q.SetSkillshot(.5f, 150f, float.MaxValue, false, SkillshotType.SkillshotLine);
            W.SetSkillshot(0f, 0f, 1750f, false, SkillshotType.SkillshotCircle);
            E.SetSkillshot(0f, 0f, float.MaxValue, false, SkillshotType.SkillshotCircle);

            MenuProvider.Champion.Combo.addUseQ();
            MenuProvider.Champion.Combo.addItem("W Use Only Line Combo");
            MenuProvider.Champion.Combo.addItem("Combo Use W is Manual :)");
            MenuProvider.Champion.Combo.addUseE();
            MenuProvider.Champion.Combo.addUseR();
            MenuProvider.Champion.Combo.addItem("Use R only Selected", true);

            MenuProvider.Champion.Combo.addItem("Use Item In Line Combo", true);

            MenuProvider.Champion.Harass.addUseQ();
            MenuProvider.Champion.Harass.addItem("Use W Toggle", new KeyBind('Y', KeyBindType.Toggle));
            MenuProvider.Champion.Harass.addUseE();

            MenuProvider.Champion.Flee.addUseW();

            MenuProvider.Champion.Lasthit.addUseQ();
            MenuProvider.Champion.Lasthit.addUseE();

            MenuProvider.Champion.Laneclear.addUseQ();
            MenuProvider.Champion.Laneclear.addUseE();

            MenuProvider.Champion.Jungleclear.addUseQ();
            MenuProvider.Champion.Jungleclear.addUseE();

            MenuProvider.Champion.Drawings.addDrawQrange(Color.Green, true);
            MenuProvider.Champion.Drawings.addDrawWrange(Color.Green, true);
            MenuProvider.Champion.Drawings.addDrawErange(Color.Green, true);
            MenuProvider.Champion.Drawings.addDrawRrange(Color.Green, true);
            MenuProvider.Champion.Drawings.addItem("Draw QW Range", new Circle(true, Color.Green));
            MenuProvider.Champion.Drawings.addDamageIndicator(getcombodamage);

            Drawing.OnDraw += Drawing_OnDraw;
            Spellbook.OnCastSpell += Spellbook_OnCastSpell;
            Game.OnUpdate += Game_OnUpdate;
            Orbwalking.BeforeAttack += Orbwalking_BeforeAttack;
        }

        private void Orbwalking_BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            if (args.Unit.IsMe)
            {
                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LastHit)
                {
                    var LE = MenuProvider.Champion.Lasthit.UseE;
                    if (LE)
                    {
                        if (Player.Mana >= E.ManaCost)
                        {
                            if (E.isReadyPerfectly())
                            {
                                args.Process = false;
                            }
                        }
                    }
                    var LQ = MenuProvider.Champion.Lasthit.UseQ;
                    if (LQ)
                    {
                        if (Player.Mana >= Q.ManaCost)
                        {
                            if (Q.isReadyPerfectly())
                            {
                                args.Process = false;
                            }
                        }
                    }
                }
            }
        }

        private void Game_OnUpdate(EventArgs args)
        {
            if (Orbwalking.CanMove(20))
            {
                switch (Orbwalker.ActiveMode)
                {
                    case Orbwalking.OrbwalkingMode.LineCombo:
                        {
                            var CR = MenuProvider.Champion.Combo.UseR;
                            if (CR)
                            {
                                if (R.isReadyPerfectly() && rShadow == rCheck.First)
                                {
                                    var CO = MenuProvider.Champion.Combo.getBoolValue("Use R only Selected");
                                    if (!CO)
                                    {
                                        var target = TargetSelector.GetTarget(R.Range, R.DamageType);
                                        if (target != null && !target.IsZombie)
                                        {
                                            R.CastOnUnit(target);
                                        }
                                    }
                                    else
                                    {
                                        var target = TargetSelector.GetSelectedTarget();
                                        if (target != null && !target.IsZombie && target.Distance(Player.Position) <= R.Range)
                                        {
                                            R.CastOnUnit(target);
                                        }
                                    }
                                }
                            }

                            if (E.isReadyPerfectly() && wShadow == wCheck.First && rShadow == rCheck.Second)
                            {
                                var target = TargetSelector.GetTarget(E.Range, E.DamageType);
                                if (target != null)
                                {
                                    E.Cast();
                                }
                            }

                            if (W.isReadyPerfectly() && wShadow == wCheck.First && rShadow == rCheck.Second)
                            {
                                var rtarget = TargetSelector.GetTarget(W.Range, R.DamageType);
                                var wtarget = rtarget.Position.Extend(Player.Position, -650);
                                if (rtarget != null)
                                {
                                    W.Cast(wtarget);
                                }
                            }

                            if (Q.isReadyPerfectly() && wShadow == wCheck.Second && rShadow == rCheck.Second)
                            {
                                var target = TargetSelector.GetTarget(Q.Range, Q.DamageType);
                                if (target != null)
                                {
                                    Q.Cast(target);
                                }
                            }

                            var useItem = MenuProvider.Champion.Combo.getBoolValue("Use Item In Line Combo");
                            if (useItem)
                            {
                                var ytarget = TargetSelector.GetTarget(Q.Range + 300, Q.DamageType);
                                var YOUMUU = ItemData.Youmuus_Ghostblade.GetItem();
                                if (ytarget != null)
                                {
                                    YOUMUU.Cast();
                                }

                                var BILGE = ItemData.Bilgewater_Cutlass.GetItem();
                                if (BILGE.IsReady())
                                {
                                    var target = TargetSelector.GetTarget(BILGE.Range, TargetSelector.DamageType.Physical);
                                    if (target != null)
                                    {
                                        BILGE.Cast(target);
                                    }
                                }
                                else
                                {
                                    var BOTRK = ItemData.Blade_of_the_Ruined_King.GetItem();
                                    if (BOTRK.IsReady())
                                    {
                                        var target = TargetSelector.GetTarget(BOTRK.Range, TargetSelector.DamageType.Physical);
                                        if (target != null)
                                        {
                                            BOTRK.Cast(target);
                                        }
                                    }
                                }

                                var TIAMAT = ItemData.Tiamat_Melee_Only.GetItem();
                                if (TIAMAT.IsReady())
                                {
                                    var target = TargetSelector.GetTarget(TIAMAT.Range, TargetSelector.DamageType.Physical);
                                    if (target != null)
                                    {
                                        TIAMAT.Cast();
                                    }
                                }
                                else
                                {
                                    var HYDRA = ItemData.Ravenous_Hydra_Melee_Only.GetItem();
                                    if (HYDRA.IsReady())
                                    {
                                        var target = TargetSelector.GetTarget(HYDRA.Range, TargetSelector.DamageType.Physical);
                                        if (target != null)
                                        {
                                            HYDRA.Cast();
                                        }
                                    }
                                }
                            }
                        }
                        break;

                    case Orbwalking.OrbwalkingMode.Flee:
                        {
                            var FW = MenuProvider.Champion.Flee.UseW;
                            if (!FW || wShadow == wCheck.Cooltime)
                            {
                                return;
                            }
                            if (FW)
                            {
                                if (W.isReadyPerfectly() && wShadow == wCheck.First)
                                {
                                    W.Cast(Game.CursorPos);
                                }

                                if (wShadow == wCheck.Second)
                                {
                                    W.Cast();
                                }
                            }
                        }
                        break;

                    case Orbwalking.OrbwalkingMode.LastHit:
                        {
                            var LE = MenuProvider.Champion.Lasthit.UseE;
                            if (LE)
                            {
                                if (Player.Mana > E.ManaCost)
                                {
                                    if (E.isReadyPerfectly())
                                    {
                                        var target = MinionManager.GetMinions(E.Range).FirstOrDefault(x => x.isKillableAndValidTarget(E.GetDamage(x, 1), E.DamageType, E.Range));
                                        if (target != null)
                                        {
                                            E.Cast(target);
                                        }
                                    }
                                }
                            }
                            var LQ = MenuProvider.Champion.Lasthit.UseQ;
                            if (LQ)
                            {
                                if (Player.Mana > Q.ManaCost)
                                {
                                    if (Q.isReadyPerfectly())
                                    {
                                        var target = MinionManager.GetMinions(Q.Range).FirstOrDefault(x => x.isKillableAndValidTarget(Q.GetDamage(x, 1), Q.DamageType, Q.Range));
                                        if (target != null)
                                        {
                                            Q.Cast(target);
                                        }
                                    }
                                }
                            }
                        }
                        break;

                    case Orbwalking.OrbwalkingMode.Mixed:
                        {

                            var WEQMana = W.ManaCost + E.ManaCost + Q.ManaCost;
                            var HW = MenuProvider.Champion.Harass.getKeyBindValue("Use W Toggle").Active;
                            if (HW)
                            {

                                if (W.isReadyPerfectly() && wShadow == wCheck.First && Player.Mana >= WEQMana)
                                {
                                    var target = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Physical);
                                    if (target != null)
                                    {
                                        W.Cast(target);
                                    }
                                }
                            }

                            var HE = MenuProvider.Champion.Harass.UseE;
                            if (HE)
                            {
                                if (E.isReadyPerfectly())
                                {
                                    if (wShadow == wCheck.Second || wShadow == wCheck.Cooltime && checkWshadow != null)
                                    {
                                        var target = TargetSelector.GetTarget(Player.Position.Distance(checkWshadow.Position) + E.Range, E.DamageType);
                                        if (target != null)
                                        {
                                            if (checkWshadow.Position.Distance(target.Position) <= E.Range || Player.Position.Distance(target.Position) <= E.Range)
                                            {
                                                E.Cast();
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var target = TargetSelector.GetTarget(E.Range, E.DamageType);
                                        if (Player.Mana < WEQMana && Player.Mana >= E.ManaCost || !HW && wShadow == wCheck.First || wShadow == wCheck.Cooltime)
                                        {
                                            if (target != null)
                                            {
                                                E.Cast();
                                            }
                                        }                  
                                    }
                                }
                            }

                            var CQ = MenuProvider.Champion.Harass.UseQ;
                            if (CQ)
                            {
                                if (Q.isReadyPerfectly())
                                {
                                    if (wShadow == wCheck.Second || wShadow == wCheck.Cooltime && checkWshadow != null)
                                    {
                                        var target = TargetSelector.GetTarget(Player.Position.Distance(checkWshadow.Position) + Q.Range, Q.DamageType);
                                        if (target != null)
                                        {
                                            if (target.Position.Distance(checkWshadow.Position) <= Q.Range)
                                            {
                                                Q.UpdateSourcePosition(checkWshadow.Position, checkWshadow.Position);
                                                Q.Cast(target, false, true);
                                            }
                                            else
                                            {
                                                if (target.Position.Distance(Player.Position) <= Q.Range)
                                                {
                                                    Q.UpdateSourcePosition(Player.Position, Player.Position);
                                                    Q.Cast(target, false, true);
                                                }
                                            }
                                        }
                                    }
                                    else                                    
                                    {
                                        var target = TargetSelector.GetTarget(Q.Range, Q.DamageType);
                                        if (Player.Mana < WEQMana && Player.Mana >= Q.ManaCost || !HW && wShadow == wCheck.First || wShadow == wCheck.Cooltime)
                                        {
                                            if (target != null)
                                            {
                                                Q.Cast(target);
                                            }
                                        }                                        
                                    }
                                }
                            }
                        }
                        break;

                    case Orbwalking.OrbwalkingMode.LaneClear:
                        {
                            var LQ = MenuProvider.Champion.Laneclear.UseQ;
                            if (LQ)
                            {
                                if (Player.Mana >= Q.ManaCost)
                                {
                                    if (Q.isReadyPerfectly())
                                    {
                                        var qLoc = Q.GetLineFarmLocation(MinionManager.GetMinions(Q.Range));
                                        if (qLoc.MinionsHit >= 1)
                                        {
                                            Q.Cast(qLoc.Position);
                                        }
                                    }
                                }
                            }

                            var LE = MenuProvider.Champion.Laneclear.UseE;
                            if (LE)
                            {
                                if (Player.Mana >= E.ManaCost)
                                {
                                    if (E.isReadyPerfectly())
                                    {
                                        if (wShadow == wCheck.Second)
                                        {
                                            var eLoc = E.GetCircularFarmLocation(MinionManager.GetMinions(E.Range + W.Range));
                                            if (eLoc.MinionsHit >= 1)
                                            {
                                                E.Cast();
                                            }
                                        }
                                        else
                                        {
                                            var eLoc = E.GetCircularFarmLocation(MinionManager.GetMinions(E.Range));
                                            if (eLoc.MinionsHit >= 1)
                                            {
                                                E.Cast();
                                            }
                                        }
                                    }
                                }
                            }

                            var JQ = MenuProvider.Champion.Jungleclear.UseQ;
                            if (JQ)
                            {
                                if (Player.Mana >= Q.ManaCost)
                                {
                                    if (Q.isReadyPerfectly())
                                    {
                                        var qMob = MinionManager.GetMinions
                                            (Q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth).FirstOrDefault(x => x.IsValidTarget(Q.Range));
                                        if (qMob != null)
                                        {
                                            Q.Cast(qMob);
                                        }
                                    }
                                }
                            }

                            var JE = MenuProvider.Champion.Jungleclear.UseE;
                            if (JE)
                            {
                                if (Player.Mana >= E.ManaCost)
                                {
                                    if (E.isReadyPerfectly())
                                    {
                                        if (wShadow == wCheck.Second)
                                        {
                                            var eMob = MinionManager.GetMinions
                                                (E.Range + Player.Distance(checkWshadow.Position), MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth).FirstOrDefault(x => x.IsValidTarget(Player.Distance(checkWshadow.Position) + E.Range));
                                            if (eMob != null)
                                            {
                                                if (eMob.Distance(Player.Position) <= E.Range || eMob.Distance(checkWshadow.Position) <= E.Range)
                                                {
                                                    E.Cast();
                                                }
                                            }
                                        }
                                        else
                                        {
                                            var eMob = MinionManager.GetMinions
                                                (E.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth).FirstOrDefault(x => x.IsValidTarget(E.Range));
                                            if (eMob != null)
                                            {
                                                E.Cast();
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        break;

                    case Orbwalking.OrbwalkingMode.Combo:
                        {
                            var CR = MenuProvider.Champion.Combo.UseR;
                            if (CR)
                            {
                                if (R.isReadyPerfectly() && rShadow == rCheck.First)
                                {
                                    var CO = MenuProvider.Champion.Combo.getBoolValue("Use R only Selected");
                                    if (!CO)
                                    {
                                        var target = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical);
                                        if (target != null && !target.IsZombie)
                                        {
                                            R.CastOnUnit(target);
                                        }
                                    }
                                    else
                                    {
                                        var target = TargetSelector.GetSelectedTarget();
                                        if (target != null && !target.IsZombie && target.Distance(Player.Position) <= R.Range)
                                        {
                                            R.CastOnUnit(target);
                                        }
                                    }
                                }
                            }

                            var CE = MenuProvider.Champion.Combo.UseE;
                            if (CE)
                            {
                                if (E.isReadyPerfectly())
                                {
                                    if (rShadow == rCheck.Second || rShadow == rCheck.Cooltime && checkRshadow != null)
                                    {
                                        var target = TargetSelector.GetTarget(E.Range + Player.Position.Distance(checkRshadow.Position), E.DamageType);
                                        if (target != null)
                                        {
                                            if (wShadow == wCheck.Second || wShadow == wCheck.Cooltime && checkWshadow != null)
                                            {
                                                if (checkRshadow.Position.Distance(target.Position) < checkWshadow.Position.Distance(target.Position))
                                                {
                                                    if (checkRshadow.Position.Distance(target.Position) <= E.Range || Player.Position.Distance(target.Position) <= E.Range)
                                                    {
                                                        E.Cast();
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (checkRshadow.Position.Distance(target.Position) <= E.Range || Player.Position.Distance(target.Position) <= E.Range)
                                                {
                                                    E.Cast();
                                                }
                                            }
                                        }
                                    }

                                    if (wShadow == wCheck.Second || wShadow == wCheck.Cooltime && checkWshadow != null)
                                    {
                                        var target = TargetSelector.GetTarget(E.Range + Player.Position.Distance(checkWshadow.Position), E.DamageType);
                                        if (target != null)
                                        {
                                            if (rShadow == rCheck.Second || rShadow == rCheck.Cooltime && checkRshadow != null)
                                            {
                                                if (checkWshadow.Position.Distance(target.Position) < checkRshadow.Position.Distance(target.Position))
                                                {
                                                    if (checkWshadow.Position.Distance(target.Position) <= E.Range || Player.Position.Distance(target.Position) <= E.Range)
                                                    {
                                                        E.Cast();
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (checkWshadow.Position.Distance(target.Position) <= E.Range || Player.Position.Distance(target.Position) <= E.Range)
                                                {
                                                    E.Cast();
                                                }
                                            }
                                        }
                                    }

                                    if (!CR || rShadow == rCheck.Cooltime)
                                    {
                                        var target = TargetSelector.GetTarget(E.Range, E.DamageType);
                                        if (target != null)
                                        {
                                            E.Cast();
                                        }
                                    }
                                }
                            }

                            var CQ = MenuProvider.Champion.Combo.UseQ;
                            if (CQ)
                            {
                                if (Q.isReadyPerfectly())
                                {
                                    if (rShadow == rCheck.Second || rShadow == rCheck.Cooltime && checkRshadow != null)
                                    {

                                        var target = TargetSelector.GetTarget(Player.Position.Distance(checkRshadow.Position) + Q.Range, Q.DamageType);
                                        if (target != null)
                                        {
                                            if (wShadow == wCheck.Second || wShadow == wCheck.Cooltime && checkWshadow != null)
                                            {
                                                if (checkRshadow.Position.Distance(target.Position) < checkWshadow.Position.Distance(target.Position))
                                                {
                                                    if (checkRshadow.Position.Distance(target.Position) <= Q.Range)
                                                    {
                                                        Q.UpdateSourcePosition(checkRshadow.Position, checkRshadow.Position);
                                                        Q.Cast(target, false, true);
                                                    }
                                                    else
                                                    {
                                                        if (Player.Position.Distance(target.Position) <= Q.Range)
                                                        {
                                                            Q.UpdateSourcePosition(Player.Position, Player.Position);
                                                            Q.Cast(target, false, true);
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (checkRshadow.Position.Distance(target.Position) <= Q.Range)
                                                {
                                                    Q.UpdateSourcePosition(checkRshadow.Position, checkRshadow.Position);
                                                    Q.Cast(target, false, true);
                                                }
                                                else
                                                {
                                                    if (Player.Position.Distance(target.Position) <= Q.Range)
                                                    {
                                                        Q.UpdateSourcePosition(Player.Position, Player.Position);
                                                        Q.Cast(target, false, true);
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    if (wShadow == wCheck.Second || wShadow == wCheck.Cooltime && checkWshadow != null)
                                    {
                                        var target = TargetSelector.GetTarget(Player.Position.Distance(checkWshadow.Position) + Q.Range, Q.DamageType);
                                        if (target != null)
                                        {
                                            if (rShadow == rCheck.Second || rShadow == rCheck.Cooltime && checkRshadow != null)
                                            {
                                                if (checkWshadow.Position.Distance(target.Position) < checkRshadow.Position.Distance(target.Position))
                                                {
                                                    if (checkWshadow.Position.Distance(target.Position) <= Q.Range)
                                                    {
                                                        Q.UpdateSourcePosition(checkWshadow.Position, checkWshadow.Position);
                                                        Q.Cast(target, false, true);
                                                    }
                                                    else
                                                    {
                                                        if (Player.Position.Distance(target.Position) <= Q.Range)
                                                        {
                                                            Q.UpdateSourcePosition(Player.Position, Player.Position);
                                                            Q.Cast(target, false, true);
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (checkWshadow.Position.Distance(target.Position) <= Q.Range)
                                                {
                                                    Q.UpdateSourcePosition(checkWshadow.Position, checkWshadow.Position);
                                                    Q.Cast(target, false, true);
                                                }
                                                else
                                                {
                                                    if (Player.Position.Distance(target.Position) <= Q.Range)
                                                    {
                                                        Q.UpdateSourcePosition(Player.Position, Player.Position);
                                                        Q.Cast(target, false, true);
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    if (!CR || rShadow == rCheck.Cooltime)
                                    {
                                        var qtarget = TargetSelector.GetTarget(Q.Range, Q.DamageType);
                                        if (qtarget != null)
                                        {
                                            Q.Cast(qtarget, false, true);
                                        }
                                    }
                                }
                            }
                        }
                        break;

                    case Orbwalking.OrbwalkingMode.None:
                        break;
                }
            }
            RC();
        }
        
        private void RC()
        {
            if (LastCastedSpell.LastCastPacketSent.Slot == SpellSlot.R)
            {
                shadow = ObjectManager.Get<Obj_AI_Minion>().FirstOrDefault(x => x.IsVisible && x.IsAlly && x.Name == "Shadow");

                rshadowpos = shadow.Position;
            }
        }

        private void Spellbook_OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (args.Slot == SpellSlot.W || args.Slot == SpellSlot.R)
            {
                Orbwalking.ResetAutoAttackTimer();
            }
        }

        private void Drawing_OnDraw(EventArgs args)
        {
            var drawQ = MenuProvider.Champion.Drawings.DrawQrange;
            if (Q.isReadyPerfectly() && drawQ.Active)
            {
                Render.Circle.DrawCircle(Player.Position, Q.Range, drawQ.Color,3);
            }

            var drawW = MenuProvider.Champion.Drawings.DrawWrange;
            if (W.isReadyPerfectly() && drawW.Active && wShadow == wCheck.First)
            {
                Render.Circle.DrawCircle(Player.Position, W.Range, drawW.Color,3);
            }

            var drawE = MenuProvider.Champion.Drawings.DrawErange;
            if (E.isReadyPerfectly() && drawE.Active)
            {
                Render.Circle.DrawCircle(Player.Position, E.Range, drawE.Color,3);
            }

            var drawR = MenuProvider.Champion.Drawings.DrawRrange;
            if (R.isReadyPerfectly() && drawR.Active && rShadow == rCheck.First)
            {
                Render.Circle.DrawCircle(Player.Position, R.Range, drawR.Color,3);
            }

            var drawQW = MenuProvider.Champion.Drawings.getCircleValue("Draw QW Range");
            if (Q.isReadyPerfectly() && W.isReadyPerfectly() && wShadow == wCheck.First && drawQW.Active)
            {
                Render.Circle.DrawCircle(Player.Position, Q.Range + W.Range, drawQW.Color, 3);
            }
        }

        private float getcombodamage(Obj_AI_Base enemy)
        {
            float damage = 0;

            var BILGE = ItemData.Bilgewater_Cutlass.GetItem();
            if (BILGE.IsReady())
            {
                damage += (float)Player.GetItemDamage(enemy, Damage.DamageItems.Bilgewater);
            }

            var BOTRK = ItemData.Blade_of_the_Ruined_King.GetItem();
            if (BOTRK.IsReady())
            {
                damage += (float)Player.GetItemDamage(enemy, Damage.DamageItems.Botrk);
            }

            var TIAMAT = ItemData.Tiamat_Melee_Only.GetItem();
            if (TIAMAT.IsReady())
            {
                damage += (float)Player.GetItemDamage(enemy, Damage.DamageItems.Tiamat);
            }

            var HYDRA = ItemData.Ravenous_Hydra_Melee_Only.GetItem();
            if (HYDRA.IsReady())
            {
                damage += (float)Player.GetItemDamage(enemy, Damage.DamageItems.Hydra);
            }

            if (Q.isReadyPerfectly())
            {
                damage += Q.GetDamage(enemy);
            }

            if (W.isReadyPerfectly())
            {
                damage += Q.GetDamage(enemy) / 2;
            }

            if (E.isReadyPerfectly())
            {
                damage += E.GetDamage(enemy);
            }

            if (R.isReadyPerfectly())
            {
                damage += R.GetDamage(enemy);
                damage += (float)(R.Level * 0.15 + 0.05);
            }

            return damage;
        }
    }
}
