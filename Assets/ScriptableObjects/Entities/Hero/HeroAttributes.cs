using UnityEngine;
using System;

namespace Momentum
{

[CreateAssetMenu(fileName = "HeroStats", menuName = "ScriptableObjects/HeroStats")]
public class HeroAttributes : ScriptableObject
{
    public MovementAttribute movement       = new();
    public DashAttribute dash               = new();
    public AttackAttribute attack           = new();
}

[Serializable]
public class MovementAttribute
{
    public float speed              = 10f;
    public float groundFriction     = 100f;
    public float groundControlRate  = 5f;
}

[Serializable]
public class DashAttribute
{
    [Header("Dash movement:")]
    public float force              = 10f;
    public float duration           = 1f;

    [Header("Dash configuration:")]
    public float dashCount          = 3f;
    public float dashCooldown       = .5f;
    public float dashComboCooldown  = 2f; // to be implemented? 
}

[Serializable]
public class AttackAttribute
{
    [Header("Attack damage:")]
    public float damage             = 1f;
    public float range              = 10f;

    [Header("Attack movement:")]
    public float force              = 1f;
 
    [Header("Attack configuration:")]
    public float duration           = 1f;
    public float attackCount        = 3f;
    public float attackInterval     = 1f;
    public float attackComboInterval= 2f;
    public float attackComboBreak   = 2f;

}



}
