using System;
using System.Collections;
using System.IO;
using UnityEngine;

public partial class NeonRun
{
    // Opt-in standalone validation; never runs in normal play.
    IEnumerator RunSmoke()
    {
        string[] args = Environment.GetCommandLineArgs();
        int arg = Array.IndexOf(args, "--neon-smoke");
        string output = arg + 1 < args.Length ? args[arg + 1] : Application.persistentDataPath;
        Directory.CreateDirectory(output);
        yield return new WaitForSeconds(.5f);
        yield return new WaitForEndOfFrame();
        ScreenCapture.CaptureScreenshot(Path.Combine(output, "01-menu.png"));
        yield return null;
        string result;
        try { result = RunAssertions(); }
        catch (Exception e) { result = "FAIL\n" + e; }
        File.WriteAllText(Path.Combine(output, "runtime-validation.txt"), result);
        Debug.Log(result);
        StartRun();
        player = new Vector2(11, Floor); cameraX = 3;
        enemies.Add(new Foe { p = new Vector2(19, Floor), hp = 65, max = 65, kind = 0, fire = 100 });
        enemies.Add(new Foe { p = new Vector2(23, 5), hp = 65, max = 65, kind = 1, fire = 100 });
        enemies.Add(new Foe { p = new Vector2(26, Floor), hp = 100, max = 100, kind = 2, fire = 100 });
        aim = Vector2.right; FirePlayer(); State = Phase.Paused;
        // Draw gameplay without a pause overlay for the evidence capture.
        smokeCapture = true;
        yield return new WaitForSeconds(.2f);
        yield return new WaitForEndOfFrame();
        ScreenCapture.CaptureScreenshot(Path.Combine(output, "02-gameplay.png"));
        yield return null;
        smokeCapture = false; OpenUpgrade();
        yield return new WaitForSeconds(.2f);
        yield return new WaitForEndOfFrame();
        ScreenCapture.CaptureScreenshot(Path.Combine(output, "03-upgrade.png"));
        yield return null;
        for (int sector = 4; sector <= 8; sector++)
        {
            BeginSector(sector); player = sector == 4 ? new Vector2(12, 18.82f) : player;
            cameraY = Mathf.Max(0, player.y - 5); cameraX = 0;
            State = Phase.Paused; smokeCapture = true;
            yield return new WaitForSeconds(.15f);
            yield return new WaitForEndOfFrame();
            ScreenCapture.CaptureScreenshot(Path.Combine(output, "sector-" + sector + ".png"));
            yield return null;
        }
        StartRun(); BeginSector(8); player = new Vector2(11, Floor); aim = Vector2.right;
        enemies[0].p = player + Vector2.right * 2 + Vector2.up * .65f; enemies[0].hp = 1;
        InteractSwitch(1); InteractSwitch(0); InteractSwitch(2);
        weapon = 2; FirePlayer();
        for (int i = 0; i < 20; i++) UpdateShots(.016f);
        UpdateCampaign(.016f); smokeCapture = false;
        yield return new WaitForSeconds(.15f);
        yield return new WaitForEndOfFrame();
        ScreenCapture.CaptureScreenshot(Path.Combine(output, "09-victory.png"));
        yield return null;
        for (int difficulty = 0; difficulty < 3; difficulty++)
        {
            ReturnToMenu(); setupPage = SetupPage.Difficulty; SelectDifficulty(difficulty);
            yield return new WaitForSeconds(.1f);
            yield return new WaitForEndOfFrame();
            ScreenCapture.CaptureScreenshot(Path.Combine(output, "difficulty-" + difficulty + ".png"));
            yield return null;
        }
        for (int gun = 0; gun < weaponNames.Length; gun++)
        {
            ReturnToMenu(); setupPage = SetupPage.Weapon; SelectDifficulty(1); SelectWeapon(gun);
            yield return new WaitForSeconds(.1f);
            yield return new WaitForEndOfFrame();
            ScreenCapture.CaptureScreenshot(Path.Combine(output, "loadout-" + gun + ".png"));
            yield return null;
        }
        for (int role = 0; role < jobNames.Length; role++)
        {
            ReturnToMenu(); setupPage = SetupPage.Profession; SelectProfession(role);
            yield return new WaitForSeconds(.25f);
            yield return new WaitForEndOfFrame();
            ScreenCapture.CaptureScreenshot(Path.Combine(output, "profession-" + role + ".png"));
            yield return null;
            ReturnToMenu(); SelectProfession(role); SelectWeapon(role); StartRun();
            player = new Vector2(11, Floor); cameraX = 3; aim = new Vector2(1,.12f).normalized;
            enemies.Add(new Foe { p = new Vector2(18,Floor), hp = 200, max = 200, kind = 0, fire = 100 });
            UseSkill(); clock += .18f; UpdateCampaign(.016f); FirePlayer();
            State = Phase.Paused; smokeCapture = true;
            yield return new WaitForSeconds(.15f);
            yield return new WaitForEndOfFrame();
            ScreenCapture.CaptureScreenshot(Path.Combine(output, "skill-" + role + ".png"));
            yield return null;
        }
        ReturnToMenu(); SelectProfession(0); SelectWeapon(0); SelectDifficulty(1); StartRun();
        player=new Vector2(11,Floor); cameraX=3; clock=8; bannerUntil=0; grounded=true; velocity=new Vector2(5,0); aim=new Vector2(1,.08f).normalized;
        enemies.Add(new Foe{p=new Vector2(18,Floor),hp=100,max=100,kind=0,fire=100});
        enemies.Add(new Foe{p=new Vector2(24,Floor),hp=150,max=150,kind=2,fire=100});
        enemies.Add(new Foe{p=new Vector2(21,5.7f),hp=80,max=80,kind=1,fire=100});
        FirePlayer(); State=Phase.Paused; smokeCapture=true;
        yield return new WaitForSeconds(.15f); yield return new WaitForEndOfFrame();
        ScreenCapture.CaptureScreenshot(Path.Combine(output,"slice-01-street.png")); yield return null;
        State=Phase.Playing; UseSkill(); clock+=.2f; shots.Clear(); FirePlayer(); State=Phase.Paused;
        yield return new WaitForSeconds(.15f); yield return new WaitForEndOfFrame();
        ScreenCapture.CaptureScreenshot(Path.Combine(output,"slice-02-dual-wield.png")); yield return null;
        for(int pose=0;pose<4;pose++)
        {
            clock+=.095f;
            yield return new WaitForSeconds(.08f); yield return new WaitForEndOfFrame();
            ScreenCapture.CaptureScreenshot(Path.Combine(output,"slice-run-"+pose+".png")); yield return null;
        }
        enemies.Clear(); shots.Clear(); spawned=quota; player=new Vector2(40,Floor); cameraX=17; skillUntil=0; skillFxUntil=0;
        yield return new WaitForSeconds(.1f); yield return new WaitForEndOfFrame();
        ScreenCapture.CaptureScreenshot(Path.Combine(output,"slice-03-elevator.png")); yield return null;
        for(int sector=2;sector<=8;sector++)
        {
            ReturnToMenu();SelectProfession(sector%4);SelectWeapon(sector%5);SelectDifficulty(1);StartRun();BeginSector(sector);
            clock=9;bannerUntil=0;cameraX=3;player=new Vector2(11,Floor);grounded=true;velocity=new Vector2(4,0);aim=Vector2.right;
            if(sector==4) {player=new Vector2(10,16.32f);cameraX=0;cameraY=11.32f;}
            if(sector==7) {player=new Vector2(15,16.32f);cameraX=0;cameraY=11.32f;}
            if(sector==5) {player=new Vector2(12,Floor);cameraX=2;}
            if(sector==6) {player=new Vector2(14,Floor);cameraX=3;InteractSwitch(1);}
            if(sector==3||sector==8) {cameraX=5;player=new Vector2(14,Floor);enemies[0].p=new Vector2(26,5.5f);if(sector==8)enemies[0].hp=enemies[0].max*.4f;}
            else enemies.Add(new Foe{p=player+new Vector2(8,0),hp=100,max=100,kind=0,fire=100});
            FirePlayer();State=Phase.Paused;smokeCapture=true;
            yield return new WaitForSeconds(.1f);yield return new WaitForEndOfFrame();
            ScreenCapture.CaptureScreenshot(Path.Combine(output,"campaign-"+sector+".png"));yield return null;
            if(sector==4)
            {
                missionItems.Clear();relics=1;player=exit+Vector2.left*2;cameraY=22;aim=Vector2.left;
                yield return new WaitForSeconds(.1f);yield return new WaitForEndOfFrame();
                ScreenCapture.CaptureScreenshot(Path.Combine(output,"campaign-4-top.png"));yield return null;
            }
            if(sector==7)
            {
                missionItems.Clear();relics=2;player=new Vector2(8,Floor);cameraY=0;cameraX=0;
                yield return new WaitForSeconds(.1f);yield return new WaitForEndOfFrame();
                ScreenCapture.CaptureScreenshot(Path.Combine(output,"campaign-7-bottom.png"));yield return null;
            }
            if(sector==6)
            {
                InteractSwitch(0);InteractSwitch(2);cameraX=8;player=new Vector2(20,Floor);
                yield return new WaitForSeconds(.1f);yield return new WaitForEndOfFrame();
                ScreenCapture.CaptureScreenshot(Path.Combine(output,"campaign-6-powered.png"));yield return null;
            }
        }
        Application.Quit(result.StartsWith("PASS") ? 0 : 1);
    }
    bool smokeCapture;
    string RunAssertions()
    {
        string log = "PASS — Neon Breakout runtime validation\n";
        log += ValidateAimTransforms() + "\n";
        Require(nightCity!=null && streetTile!=null && sliceDrone!=null && sliceTorso[0]!=null,"first-sector art resources load");
        Require(foundryScene!=null&&spireScene!=null&&archiveScene!=null&&reactorScene!=null&&sentinelModel!=null,"all campaign backgrounds and sentinel load");
        for(int stage=1;stage<=8;stage++) Require(SectorBackdrop(stage)!=null,"backdrop exists for sector "+stage);
        ReturnToMenu();StartRun();BeginSector(6);
        Require(!TerminalActive(0)&&!TerminalActive(1)&&!TerminalActive(2),"initial terminals disconnected");
        InteractSwitch(1);Require(TerminalActive(1)&&!TerminalActive(0)&&!TerminalActive(2),"only II shows connected after first step");
        InteractSwitch(2);Require(!TerminalActive(1),"wrong order resets terminal lights");
        InteractSwitch(1);InteractSwitch(0);Require(TerminalActive(0)&&TerminalActive(1)&&!TerminalActive(2),"partial circuit highlights I and II");
        InteractSwitch(2);Require(TerminalActive(0)&&TerminalActive(1)&&TerminalActive(2),"completed circuit all connected");
        ReturnToMenu();
        log+="All eight sector backgrounds and new sentinel load; terminal lights match partial progress, error reset and completed circuit.\n";
        for(int gun=0;gun<5;gun++)
        {
            ReturnToMenu(); SelectWeapon(gun); SelectProfession(0); StartRun(); aim=Vector2.right;
            FirePlayer(); int normalCount=shots.Count; float normalInterval=nextShot-clock;
            shots.Clear(); UseSkill(); FirePlayer();
            Require(IsDualWield && shots.Count==normalCount*2,"dual wield doubles simultaneous projectiles for gun "+gun);
            Require(Mathf.Abs(nextShot-clock-normalInterval)<.001f,"dual wield keeps gun cadence instead of quadrupling output");
            bool primary=false,copy=false;
            foreach(Shot shot in shots) {Require(shot.gun==gun,"overload preserves chosen gun"); if(shot.barrel==0)primary=true; if(shot.barrel==1)copy=true;}
            Require(primary&&copy&&Vector2.Distance(GunMuzzle(0),GunMuzzle(1))>.4f,"two distinct gun sockets fire together");
            clock=skillUntil+.01f; shots.Clear(); heat=0; overheated=false; FirePlayer();
            Require(!IsDualWield && shots.Count==normalCount,"overload expiry restores single gun");
        }
        ReturnToMenu(); SelectWeapon(0); SelectProfession(0); StartRun(); aim=Vector2.right; UseSkill();
        enemies.Add(new Foe{p=player+Vector2.right*3+Vector2.up*.15f,hp=1000,max=1000});
        FirePlayer(); for(int i=0;i<30;i++)UpdateShots(.016f);
        Require(enemies[0].hp==968,"both real rifle projectiles deal damage");
        ReturnToMenu(); SelectWeapon(0); SelectProfession(0);
        log+="First-sector resources, five dual-wield variants, separate sockets, paired real damage, normal cadence and expiry pass.\n";
        ReturnToMenu(); AdvanceSetup();
        Require(State == Phase.Menu && setupPage == SetupPage.Difficulty, "start opens difficulty instead of combat");
        SelectDifficulty(2); AdvanceSetup(); Require(setupPage == SetupPage.Profession && State == Phase.Menu, "difficulty advances to profession");
        SelectProfession(3); AdvanceSetup(); Require(setupPage == SetupPage.Weapon && State == Phase.Menu, "profession advances to gun selection");
        SelectWeapon(4); BackSetup(); Require(setupPage == SetupPage.Profession && selectedWeapon == 4 && profession == 3, "back preserves choices");
        BackSetup(); Require(setupPage == SetupPage.Difficulty && selectedDifficulty == 2, "back preserves difficulty");
        AdvanceSetup(); AdvanceSetup(); AdvanceSetup();
        Require(State == Phase.Playing && Sector == 1 && profession == 3 && weapon == 4 && runDifficulty == 2, "only final confirmation deploys selected loadout");
        SelectProfession(0); Require(profession == 3, "profession locked in combat");
        ReturnToMenu(); Require(setupPage == SetupPage.Home, "return menu restores home page");
        BackSetup(); Require(setupPage == SetupPage.Home, "back on home is safe");
        SelectDifficulty(1); SelectProfession(0); SelectWeapon(0);
        var bodyShapes = new System.Collections.Generic.HashSet<string>();
        foreach (Texture2D model in classModels) Require(model != null && bodyShapes.Add(ModelSilhouette(model)), "profession silhouettes are unique");
        var gunShapes = new System.Collections.Generic.HashSet<string>();
        foreach (Texture2D model in gunModels) Require(model != null && gunShapes.Add(ModelSilhouette(model)), "gun silhouettes are unique");
        log += "Deployment wizard: home/difficulty/profession/weapon steps, back retention, final confirmation, locked profession; four distinct bodies and five gun silhouettes pass.\n";
        for (int gun = 0; gun < weaponNames.Length; gun++)
        {
            ReturnToMenu(); SelectWeapon(gun); SelectWeapon(-1); SelectWeapon(weaponNames.Length);
            StartRun(); Require(weapon == gun && selectedWeapon == gun, "menu gun selection latches; invalid values ignored");
            for (int sector = 1; sector <= 8; sector++)
            {
                BeginSector(sector); SelectWeapon((gun + 1) % weaponNames.Length);
                Require(weapon == gun && selectedWeapon == gun, "gun locked in sector " + sector);
                shots.Clear(); FirePlayer(); Require(shots.Count == (gun == 1 ? 7 : 1), "selected gun fires in every sector");
                foreach (Shot shot in shots) Require(shot.gun == gun, "only selected gun projectiles");
                if (sector < 8)
                {
                    State = Phase.Upgrade; SelectWeapon((gun + 1) % weaponNames.Length); choices[0] = 0; ChooseUpgrade(0);
                    Require(weapon == gun && selectedWeapon == gun, "upgrade cannot change selected gun");
                }
            }
            foreach (Phase phase in new[] { Phase.Paused, Phase.Defeat, Phase.Victory })
            {
                State = phase; SelectWeapon((gun + 1) % weaponNames.Length);
                Require(weapon == gun && selectedWeapon == gun, "gun cannot be changed outside menu: " + phase);
            }
            StartRun(); Require(weapon == gun, "restart keeps chosen gun");
            ReturnToMenu(); SelectWeapon((gun + 1) % weaponNames.Length); StartRun();
            Require(weapon == (gun + 1) % weaponNames.Length, "menu allows next-run gun change");
        }
        ReturnToMenu(); SelectWeapon(0);
        log += "Five loadouts: menu selection, all eight sectors fire only chosen gun, upgrade/pause/end locks, restart retention and menu change pass.\n";
        Require(selectedDifficulty == 1, "default medium difficulty");
        float[] expectedHealth = { 45, 60, 81 };
        int[] expectedDamage = { 7, 10, 14 };
        float[] expectedInterval = { 2.25f, 1.8f, 1.44f };
        float[] expectedBossHealth = { 1650, 2200, 2970 };
        int[] expectedBossDamage = { 11, 16, 22 };
        for (int difficulty = 0; difficulty < 3; difficulty++)
        {
            ReturnToMenu(); SelectDifficulty(difficulty); SelectDifficulty(-1); SelectDifficulty(3);
            StartRun(); Require(runDifficulty == difficulty && selectedDifficulty == difficulty, "valid menu difficulty latches");
            clock = spawnAt; SpawnWave(); Require(enemies.Count == 1 && Mathf.Abs(enemies[0].hp - expectedHealth[difficulty]) < .01f, "regular enemy health by difficulty");
            enemies[0].fire = -1; UpdateEnemies(.016f);
            Require(shots.Count == 1 && shots[0].damage == expectedDamage[difficulty], "enemy projectile difficulty damage");
            Require(Mathf.Abs(enemies[0].fire - clock - expectedInterval[difficulty]) < .001f, "enemy difficulty fire interval");
            Hurt(shots[0].damage); Require(Health == 100 - expectedDamage[difficulty], "difficulty damage reaches player");
            SelectDifficulty((difficulty + 1) % 3); Require(runDifficulty == difficulty && selectedDifficulty == difficulty, "difficulty locked during run");
            State = Phase.Upgrade; choices[0] = 0; ChooseUpgrade(0); Require(runDifficulty == difficulty && Sector == 2, "difficulty survives upgrades");
            BeginSector(8); Require(Mathf.Abs(enemies[0].hp - expectedBossHealth[difficulty]) < .01f && enemies[0].hp == enemies[0].max, "final boss difficulty health");
            enemies[0].fire = -1; UpdateEnemies(.016f); Require(shots.Count == 5 && shots[0].damage == expectedBossDamage[difficulty], "boss difficulty damage");
            StartRun(); Require(runDifficulty == difficulty, "restart retains selected difficulty");
        }
        ReturnToMenu(); SelectDifficulty(1);
        log += "Three difficulties: menu selection, invalid input, regular/boss health and damage, attack frequency, run lock, upgrade and restart retention pass.\n";
        StartRun();
        Require(State == Phase.Playing && Sector == 1 && Health == 100, "start/reset");
        Require(SegmentDistance(new Vector2(1, 0), Vector2.zero, new Vector2(2, 0)) < .001f, "swept hit detection");
        player = new Vector2(5, Floor); aim = Vector2.right;
        for (int w = 0; w < 5; w++)
        {
            shots.Clear(); enemies.Clear(); weapon = w;
            var target = new Foe { p = new Vector2(7, Floor + .15f), hp = 1, max = 1 };
            enemies.Add(target); int before = Kills; FirePlayer();
            Require(shots.Count == (w == 1 ? 7 : 1), "weapon " + w + " projectile count");
            for (int t = 0; t < 20; t++) UpdateShots(.016f);
            Require(Kills == before + 1 && enemies.Count == 0, "weapon " + w + " hit/kill");
        }
        log += "All five weapons spawn, sweep-hit and kill targets.\n";
        weapon = 3; heat = 0; shots.Clear();
        for (int i = 0; i < 20; i++) FirePlayer();
        Require(overheated && heat == 1, "machinegun overheats");
        int shotCount = shots.Count; FirePlayer(); Require(shots.Count == shotCount, "overheat blocks shots");
        UpdateCampaign(4); Require(!overheated, "machinegun cools");
        weapon = 2; shots.Clear(); FirePlayer(); Require(shots[0].pierce && shots[0].v.magnitude > 50, "sniper penetration/speed");
        StartRun(); weapon = 2; player = new Vector2(5, Floor); aim = Vector2.right;
        enemies.Add(new Foe { p = new Vector2(7, Floor + .15f), hp = 1, max = 1 });
        enemies.Add(new Foe { p = new Vector2(9, Floor + .15f), hp = 1, max = 1 });
        FirePlayer(); for (int i = 0; i < 20; i++) UpdateShots(.016f);
        Require(Kills == 2, "sniper hits multiple targets");
        for (int job = 0; job < 4; job++)
        {
            profession = job; StartRun(); Health = 40; aim = Vector2.right; Vector2 before = player;
            UseSkill(); Require(skillReady > clock, "skill cooldown");
            Require(skillFxKind == job && skillFxUntil > clock, "profession-specific visual effect activation");
            if (job == 0) Require(skillUntil == clock + 5, "assault overdrive");
            if (job == 1) Require(Health == 80 && immuneUntil == clock + 2, "medic repair/shield");
            if (job == 2)
            {
                Require(turretUntil == clock + 8, "engineer turret"); enemies.Add(new Foe { p = player + Vector2.right * 3, hp = 100, max = 100 });
                shots.Clear(); UpdateCampaign(.016f); Require(shots.Count == 1, "turret tracks/fires");
            }
            if (job == 3) Require(player == before && immuneUntil == 0 && skillUntil == clock+6 && skillReady == clock+14, "scout scan without movement or invulnerability");
            float cooldown = skillReady; UseSkill(); Require(skillReady == cooldown, "skill cannot spam");
        }
        profession = 0;
        log += "Four profession skills, cooldowns, turret targeting, sniper and machinegun heat pass.\n";
        ReturnToMenu();SelectProfession(3);SelectWeapon(0);StartRun();aim=Vector2.right;
        var scanNear=new Foe{p=player+new Vector2(3,.15f),hp=1000,max=1000};
        var scanFar=new Foe{p=player+new Vector2(16,.15f),hp=1000,max=1000};
        enemies.Add(scanNear);enemies.Add(scanFar);Vector2 scanPosition=player;
        velocity=new Vector2(2,3);UseSkill();
        Require(IsScanned(scanNear)&&!IsScanned(scanFar),"scan marks nearby existing enemies only");
        Require(player==scanPosition&&velocity==new Vector2(2,3)&&immuneUntil==0,"scan preserves motion and provides no shield");
        FirePlayer();for(int i=0;i<20;i++)UpdateShots(.016f);
        Require(scanNear.hp==976,"scanned target takes 24 damage from real 16-damage rifle");
        shots.Clear();shots.Add(new Shot{p=scanFar.p+Vector2.up*.5f-Vector2.right*.2f,v=Vector2.right*20,life=1,damage=16,gun=0});UpdateShots(.016f);
        Require(scanFar.hp==984,"unscanned target takes normal damage");
        var scanLate=new Foe{p=player+Vector2.right*5,hp=1000,max=1000};enemies.Add(scanLate);UseSkill();
        Require(!IsScanned(scanLate)&&skillReady==14,"cooldown blocks rescanning newly spawned enemies");
        shots.Clear();shots.Add(new Shot{p=scanNear.p+Vector2.up*.5f-Vector2.right*.2f,v=Vector2.right*20,life=1,damage=16,gun=-1});UpdateShots(.016f);
        Require(scanNear.hp==960,"scan does not amplify automated turret damage");
        clock=6;shots.Clear();FirePlayer();for(int i=0;i<20;i++)UpdateShots(.016f);
        Require(!IsScanned(scanNear)&&scanNear.hp==944,"expiry restores normal damage exactly at six seconds");
        clock=14;UseSkill();Require(IsScanned(scanLate)&&skillReady==28,"scan ready after fourteen seconds");
        BeginSector(3);player=new Vector2(20,Floor);UseSkill();Require(IsScanned(enemies[0]),"boss within radius is scanned");
        float bossBefore=enemies[0].hp;shots.Clear();shots.Add(new Shot{p=enemies[0].p-Vector2.right*.2f,v=Vector2.right*20,life=1,damage=100,gun=2,pierce=true});UpdateShots(.016f);
        Require(enemies[0].hp==bossBefore-150,"marked boss takes amplified sniper damage");
        BeginSector(4);Require(skillUntil==0&&enemies.Count==0,"sector transition clears scan");
        ReturnToMenu();SelectProfession(0);SelectWeapon(0);
        log+="Scout scan: radius, no displacement/shield, real rifle bonus, normal unmarked/turret damage, cooldown, expiry, new-enemy exclusion, boss sniper bonus and sector reset pass.\n";
        StartRun(); Hurt(10); Require(Health == 90, "damage"); Hurt(10); Require(Health == 90, "invulnerability");
        immuneUntil = -1; dashUntil = clock + .1f; immuneUntil = dashUntil; Hurt(20); Require(Health == 90, "dash protection");
        Land(4); Require(grounded && jumps == 0 && velocity.y == 0 && player.y == 4, "platform landing");
        log += "Damage, invulnerability, dash protection and landing reset pass.\n";
        for (int type = 0; type < 5; type++)
        {
            StartRun(); Health = 40; State = Phase.Upgrade; choices[0] = type; ChooseUpgrade(0);
            Require(Sector == 2 && State == Phase.Playing, "upgrade transition");
            Require(type != 0 || damageLevel == 1, "damage module");
            Require(type != 1 || rateLevel == 1, "rate module");
            Require(type != 2 || spreadLevel == 1, "spread module");
            Require(type != 3 || (MaxHealth == 125 && Health == 90), "armor module");
            Require(type != 4 || (dashCooldown < 1.35f && Health == 60), "dash module");
        }
        log += "All five upgrades apply and advance the sector.\n";
        BeginSector(3); Require(enemies.Count == 1 && enemies[0].kind == 3, "boss spawn");
        shots.Clear(); enemies[0].fire = -1; UpdateEnemies(.016f); Require(shots.Count == 5, "boss normal pattern");
        shots.Clear(); enemies[0].hp = 400; enemies[0].fire = -1; UpdateEnemies(.016f); Require(shots.Count == 9, "boss rage pattern");
        log += "Boss spawns; normal and rage patterns produce 5/9 projectiles.\n";
        enemies.Clear(); spawned = quota; player = exit; CheckExit();
        Require(State == Phase.Upgrade, "sector 3 continues campaign");
        StartRun(); spawned = quota; enemies.Clear(); player = exit; CheckExit();
        Require(State == Phase.Upgrade, "clear and exit gate");
        for (int sector = 4; sector <= 8; sector++)
        {
            BeginSector(sector); player = exit; CheckExit(); Require(State == Phase.Playing, "locked objective sector " + sector);
            if (sector == 4 || sector == 7)
            {
                float maxRise = 2 * 10.6f * 10.6f / (2 * 27);
                float previous = Floor;
                foreach (Rect platform in ledges) { Require(platform.yMax - previous < maxRise, "double-jump reachable ledge " + sector); previous = platform.yMax; }
                Require(exit.x > 0 && exit.x < mapWidth && exit.y <= previous, "exit inside reachable map");
            }
            if (sector == 4 || sector == 5 || sector == 7)
            {
                while (missionItems.Count > 0) { player = missionItems[0] - Vector2.up * .5f; UpdateCampaign(.016f); }
                Require(ObjectiveComplete(), "collect objective " + sector);
                Require(enemies.Count > 0 || spawned < quota, "objective independent of kills");
            }
            else
            {
                InteractSwitch(0); Require(sequence == 0, "wrong sequence reset");
                InteractSwitch(1); InteractSwitch(2); Require(sequence == 0, "partial sequence reset");
                InteractSwitch(1); InteractSwitch(0); InteractSwitch(2); Require(sequence == 3, "correct circuit order");
                if (sector == 8) { Require(!ObjectiveComplete(), "final boss required"); enemies.Clear(); spawned = quota; }
            }
            player = exit; CheckExit(); Require(State == (sector == 8 ? Phase.Victory : Phase.Upgrade), "exit advances sector " + sector);
        }
        log += "All eight sectors: locked gates, collection without clearing, wrong/right circuits, tower/left/bottom/central exits pass.\n";
        for (int circuitFirst = 0; circuitFirst < 2; circuitFirst++)
        {
            StartRun(); BeginSector(8); player = new Vector2(4, Floor); aim = Vector2.right;
            Require(Vector2.Distance(player, exit) > 10, "final regression starts far from former gate");
            if (circuitFirst == 1)
            {
                InteractSwitch(1); InteractSwitch(0); InteractSwitch(2); UpdateCampaign(.016f);
                Require(State == Phase.Playing, "circuit alone does not win");
            }
            enemies[0].p = player + Vector2.right * 2 + Vector2.up * .65f; enemies[0].hp = 1;
            weapon = 2; FirePlayer(); for (int i = 0; i < 20; i++) UpdateShots(.016f);
            Require(enemies.Count == 0 && Kills == 1, "final boss killed by projectile");
            if (circuitFirst == 0)
            {
                UpdateCampaign(.016f); Require(State == Phase.Playing, "boss alone does not win");
                InteractSwitch(1); InteractSwitch(0); InteractSwitch(2);
            }
            State = Phase.Paused; CheckExit(); Require(State == Phase.Paused, "pause cannot be replaced by victory");
            State = Phase.Playing; UpdateCampaign(.016f);
            Require(State == Phase.Victory && shots.Count == 0, "automatic victory away from portal, either task order");
            clock += 1; CheckExit(); ChooseUpgrade(0); Require(State == Phase.Victory && Sector == 8, "final cannot enter sector nine");
            ReturnToMenu(); Require(State == Phase.Menu && Sector == 0 && enemies.Count == 0 && shots.Count == 0 && cameraX == 0 && cameraY == 0, "victory returns to clean menu");
            StartRun(); Require(State == Phase.Playing && Sector == 1 && Health == 100 && Kills == 0, "restart after victory");
        }
        BeginSector(8); enemies.Clear(); sequence = 3; State = Phase.Defeat; CheckExit();
        Require(State == Phase.Defeat, "defeat cannot be replaced by victory");
        log += "Final flow: real boss projectile kill, both task orders, instant victory away from portal, pause/defeat guards, menu and restart pass.\n";
        BeginSector(1);
        immuneUntil = -1; Hurt(999); Require(State == Phase.Defeat, "defeat");
        StartRun(); Require(Health == 100 && Kills == 0 && damageLevel == 0, "restart clears run");
        log += "Exit gate, final victory, defeat and full run reset pass.\n";
        log += ValidateEditableRuntime();
        return log;
    }
    string ValidateEditableRuntime()
    {
        for(int i=0;i<5;i++)Require(gunModels[i]==balance.guns[i].artwork,"runtime gun uses editable PNG asset "+i);
        for(int i=0;i<4;i++)Require(sliceTorso[i]==balance.roles[i].torso&&sliceHead[i]==balance.roles[i].head,"runtime role uses editable PNG assets "+i);
        Require(streetTile==campaign.street&&sliceDrone==campaign.drone&&sentinelModel==campaign.boss,"runtime environment/enemies use authored images");
        NeonCampaign originalCampaign=campaign;NeonBalance originalBalance=balance;
        var editedCampaign=Instantiate(campaign);var editedBalance=Instantiate(balance);
        var editedLayout=Instantiate(campaign.levels[0]);var editedCircuit=Instantiate(campaign.levels[5]);
        try
        {
            editedCampaign.balance=editedBalance;editedCampaign.levels[0]=editedLayout;editedCampaign.levels[5]=editedCircuit;
            StageElement platform=Array.Find(editedLayout.Elements,e=>e.kind==StageElement.Kind.Platform);
            platform.transform.position+=new Vector3(1.25f,.45f,0);platform.transform.localScale+=new Vector3(1.5f,.1f,0);Rect expected=platform.PlatformRect;
            StageElement gate=Array.Find(editedLayout.Elements,e=>e.kind==StageElement.Kind.Exit);gate.transform.position+=Vector3.left*5;
            StageElement spawn=Array.Find(editedLayout.Elements,e=>e.kind==StageElement.Kind.PlayerSpawn);spawn.transform.position+=Vector3.right*2;
            editedLayout.goal=StageLayout.Goal.CollectItems;
            for(int i=0;i<2;i++){var item=new GameObject("DesignerAddedItem");item.transform.SetParent(editedLayout.transform,false);item.transform.localPosition=new Vector2(12+i*2,4);item.AddComponent<StageElement>().kind=StageElement.Kind.MissionItem;}
            editedBalance.guns[0].damage=37;editedBalance.guns[0].interval=.3f;editedBalance.guns[0].speed=42;
            editedBalance.roles[3].duration=4;editedBalance.roles[3].cooldown=9;editedBalance.roles[3].scanRadius=5;editedBalance.roles[3].scanMultiplier=2;
            editedBalance.difficulties[1].health=2;
            campaign=editedCampaign;balance=editedBalance;ReturnToMenu();SelectProfession(3);SelectWeapon(0);SelectDifficulty(1);StartRun();
            Require(Vector2.Distance(player,spawn.transform.position)<.001f&&Vector2.Distance(exit,gate.transform.position)<.001f,"authored player/exit transforms drive runtime");
            Require(ledges.Exists(r=>Mathf.Abs(r.x-expected.x)<.001f&&Mathf.Abs(r.y-expected.y)<.001f&&Mathf.Abs(r.width-expected.width)<.001f&&Mathf.Abs(r.height-expected.height)<.001f),"authored moved/resized collider drives platform");
            Require(missionItemTotal==2&&missionItems.Count==2&&!ObjectiveComplete(),"added prefab items drive goal count");
            relics=1;Require(!ObjectiveComplete(),"partial configured goal remains locked");relics=2;Require(ObjectiveComplete(),"configured collection count unlocks goal");
            aim=Vector2.right;var near=new Foe{p=player+new Vector2(3,.15f),hp=1000,max=1000};var far=new Foe{p=player+Vector2.right*6,hp=1000,max=1000};enemies.Add(near);enemies.Add(far);UseSkill();
            Require(skillUntil==4&&skillReady==9&&IsScanned(near)&&!IsScanned(far),"Inspector skill duration/cooldown/radius drive runtime");
            FirePlayer();Require(shots.Count==1&&shots[0].damage==37&&Mathf.Abs(nextShot-.3f)<.001f&&Mathf.Abs(shots[0].v.magnitude-42)<.001f,"Inspector gun damage/rate/speed drive real projectile");
            for(int i=0;i<20;i++)UpdateShots(.016f);Require(near.hp==926,"Inspector scan multiplier affects actual damage");
            editedCircuit.switchOrder=new[]{2,1,0};BeginSector(6);InteractSwitch(2);Require(sequence==1&&TerminalActive(2)&&!TerminalActive(1),"edited circuit sequence and lights drive runtime");
            BeginSector(3);Require(enemies[0].max==2500,"Inspector difficulty health multiplier drives boss");
        }
        finally
        {
            campaign=originalCampaign;balance=originalBalance;editedLayout.gameObject.SetActive(false);editedCircuit.gameObject.SetActive(false);
            Destroy(editedLayout.gameObject);Destroy(editedCircuit.gameObject);Destroy(editedBalance);Destroy(editedCampaign);
            ReturnToMenu();SelectProfession(0);SelectWeapon(0);SelectDifficulty(1);StartRun();
        }
        return "Editable content integration: moved/resized platform, player/exit positions, added collection items, custom circuit order, gun damage/rate/speed, skill radius/duration/cooldown/multiplier and difficulty all change real runtime behavior.\n";
    }
    static void Require(bool value, string name) { if (!value) throw new InvalidOperationException(name); }
    static string ModelSilhouette(Texture2D model)
    {
        var pixels = model.GetPixels32(); var shape = new char[pixels.Length];
        for (int i = 0; i < pixels.Length; i++) shape[i] = pixels[i].a > 0 ? '#' : '.';
        return model.width + "x" + model.height + new string(shape);
    }
}
