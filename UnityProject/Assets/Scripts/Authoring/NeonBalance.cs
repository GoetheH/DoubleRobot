using System;
using UnityEngine;

[CreateAssetMenu(menuName="霓虹突围/战斗配置",fileName="Balance")]
public class NeonBalance : ScriptableObject
{
    [Serializable] public class Gun
    {
        public string name;
        [Min(1)] public int damage=16, pellets=1;
        [Min(.01f)] public float interval=.145f, speed=28, lifetime=1.5f;
        [Range(0,45)] public float pelletAngle=4, randomSpread;
        public bool penetration, distanceFalloff, usesHeat;
        [Range(0,1)] public float heatPerShot=.065f;
        [Min(.1f)] public float muzzleDistance=.95f, displayWidth=1.35f;
        public Color color=Color.cyan;
        [Tooltip("拖入自己的透明枪械图片；留空使用内置像素外观。图片应朝右，握持点位于宽度30%附近。")]
        public Texture2D artwork;
    }
    [Serializable] public class Role
    {
        public string name;
        [Min(.1f)] public float duration=5, cooldown=12, moveSpeed=7.4f;
        [Min(0)] public int healing=40, turretDamage=22;
        [Min(.01f)] public float turretInterval=.22f, scanRadius=14, scanMultiplier=1.5f;
        public Color color=Color.white;
        [Tooltip("可拖入新的透明躯干/头部图片；留空保留内置外观。")]
        public Texture2D torso, head;
    }
    [Serializable] public class Difficulty
    {
        public string name;
        [Min(.05f)] public float health=1, damage=1, attackInterval=1;
    }
    [Header("五类枪械（顺序：步枪、霰弹、狙击、机枪、冲锋枪）")]
    public Gun[] guns=new Gun[5];
    [Header("四职业（顺序：突击、医疗、工程、侦察；技能类型固定，参数可编辑）")]
    public Role[] roles=new Role[4];
    [Header("三档难度（简单、中等、困难）")]
    public Difficulty[] difficulties=new Difficulty[3];
    [Header("玩家运动")]
    [Min(1)] public int startingHealth=100;
    [Min(.1f)] public float jumpVelocity=10.6f, gravity=27, dashSpeed=23, dashDuration=.17f, dashCooldown=1.35f;
    [Range(.1f,1)] public float machinegunMoveScale=6.3f/7.4f;

    public void ValidateData()
    {
        if(guns==null||guns.Length!=5||roles==null||roles.Length!=4||difficulties==null||difficulties.Length!=3) throw new InvalidOperationException("战斗配置需要5把枪、4个职业和3档难度。");
        foreach(var g in guns) if(g==null||g.damage<1||g.interval<=0||g.speed<=0||g.lifetime<=0||g.pellets<1) throw new InvalidOperationException("枪械配置无效。");
        foreach(var r in roles) if(r==null||r.duration<=0||r.cooldown<=0||r.scanRadius<=0||r.scanMultiplier<1) throw new InvalidOperationException("职业配置无效。");
        foreach(var d in difficulties) if(d==null||d.health<=0||d.damage<=0||d.attackInterval<=0) throw new InvalidOperationException("难度配置无效。");
    }
}
