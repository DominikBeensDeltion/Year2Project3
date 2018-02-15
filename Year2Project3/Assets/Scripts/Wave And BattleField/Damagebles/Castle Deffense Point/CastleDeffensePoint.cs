﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CastleDeffensePoint : Damagebles {
    public bool gateOpen;
    public List<Enemy> attackingMe = new List<Enemy>();
    public CastleGate myGate;
    public Image healthbarFill;

    public override void TakeDamage(float damage){
        myStats.health.currentValue -= damage;
    }
}