﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : Projectile{
    public float damage;
    public Transform myArrow;
    public float distance;
    public int arrowSpeed;
    public float minAmount;

    private void Update()
    {
        if (rb.isKinematic == false){
            myArrow.Rotate(Vector3.right, Time.deltaTime * arrowSpeed);
        }
    }

    private void OnTriggerEnter(Collider other) {
        if(type == Type.Stationary && other.tag == "Enemy"){
            //other.GetComponent<Enemy>().TakeDamage(damage);
        }

        if(hit) {
            return;
        }

        if(targetsHit >= maxTargetsToHit) {
            return;
        }

        if(enemyArrow) {
            Allie allie = other.transform.GetComponent<Allie>();
            if(allie != null) {
                allie.TakeDamage(myDamage);
                ReAddToPool();
                targetsHit++;

                if(targetsHit >= maxTargetsToHit) {
                    hit = true;
                }
            }
        }
        else {
            Enemy enemy = other.transform.GetComponent<Enemy>();
            if(enemy != null) {
                if(type == Type.CatapultProjectile || type == Type.CanonProjectile) {
                    ObjectPooler.instance.GrabFromPool("meteor explode particle", transform.position, Quaternion.identity);
                }
                else if(type == Type.BallistaProjectile) {
                    ObjectPooler.instance.GrabFromPool("ballista hit particle", other.transform.position, transform.rotation);
                }

                enemy.TakeDamage(myDamage);

                ReAddToPool();
                targetsHit++;

                if(targetsHit >= maxTargetsToHit) {
                    hit = true;
                }
            }
        }

    }
}
