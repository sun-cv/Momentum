using System;
using UnityEngine;


[Serializable]
public class BaseWeapon : ScriptableObject {}

public class BaseDamagingWeapon : BaseWeapon        {}
public class BasePlayerWeapon   : BaseWeapon        {}
public class BaseUtilityWeapon  : BasePlayerWeapon  {}
public class BaseMovementWeapon : BasePlayerWeapon  {}
public class BaseEnemyWeapon    : BaseWeapon        {}
