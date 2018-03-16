﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBowman : Enemy {
    public Transform bowPos;
    public float fireRate;

    public override void TakeDamage(float damage) {
        if(myStats.health.currentValue >= damage) {
            WaveManager.instance.DecreaseWaveCurrentHealth((int)damage);
        }
        else {
            WaveManager.instance.DecreaseWaveCurrentHealth((int)myStats.health.currentValue);
        }

        myStats.health.currentValue -= damage;

        if(myStats.health.currentValue <= 0) {
            StopAllCoroutines();
            ObjectPooler.instance.AddToPool("Enemy Bowman", gameObject);
            ResourceManager.instance.AddGold(ResourceManager.instance.normalEnemyGoldReward);
        }
    }

    public override IEnumerator Attack() {
        yield return new WaitForSeconds(attackCooldown);
        float distance = Vector3.Distance(bowPos.position, targetTransform.position);
        Transform _currentArrow = ObjectPooler.instance.GrabFromPool("Attacking Arrow", bowPos.position, Quaternion.Euler(new Vector3(0, 0, -45))).transform;
        _currentArrow.LookAt(targetTransform);
        _currentArrow.GetChild(0).GetComponent<Arrow>().distance = distance;
        _currentArrow.position += _currentArrow.forward * distance / 2;
        _currentArrow.GetChild(0).GetComponent<Arrow>().myArrow.position -= new Vector3(0, _currentArrow.GetChild(0).GetComponent<Arrow>().minAmount, 0);
        _currentArrow.GetChild(0).transform.position = bowPos.position;
        _currentArrow.GetChild(0).transform.rotation = Quaternion.Euler(new Vector3(-55, 90, 0));

        if(target == null) {
            if(attackingSoldiers.Count > 0) {
                for(int i = 0; i < attackingSoldiers.Count; i++) {
                    if(attackingSoldiers[i].inFight == true) {

                        target = attackingSoldiers[i];
                        break;
                    }
                }
            }
            StopBattle();

        }
        else {
            StartCoroutine(Attack());
            target.TakeDamage(myStats.damage.currentValue);
        }
    }

    void OnTriggerStay(Collider collision){
        if(collision.tag == "Defense"){
            if(collision.transform.GetComponent<CastleDeffensePoint>().gateOpen == true) {
                FindNewTarget();
                attackingCastle = false;
                StopAllCoroutines();
            }
            else {
                agent.isStopped = true;
            }
        }

    }


    void OnTriggerEnter(Collider collision) {
        if(collision.transform == targetTransform){
            StartBattle(targetTransform.GetComponent<Damagebles>());
            targetTransform.GetComponent<CastleDeffensePoint>().attackingMe.Add(this);
            agent.isStopped = true;
            attackingCastle = true;
            target = collision.gameObject.GetComponent<Damagebles>();
            StartCoroutine(Attack());
        }
        /*if(collision.tag == "Ally" && attackingCastle == false && attackingSoldiers.Count < maxAttacking && target.tag != "Ally"){
            target = collision.GetComponent<Allie>();
            StartBattle(target);
        }*/
    }

    void OnTriggerExit(Collider collision) {
        if(collision.transform == targetTransform && targetTransform.tag == "Defense") {
            StopAllCoroutines();
            FindNewTarget();
            targetTransform.GetComponent<CastleDeffensePoint>().attackingMe.Remove(this);
        }
    }
}
