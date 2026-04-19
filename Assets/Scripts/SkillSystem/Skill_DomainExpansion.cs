using System.Collections.Generic;
using UnityEngine;

public class Skill_DomainExpansion : Skill_Base
{
    [Header("预制体")]
    [SerializeField] private GameObject domainPrefab;

    [Header("Slowing Down Upgrade")]
    [SerializeField] private float slowDownPercent = 0.8f;
    [SerializeField] private float slowDownDomainDuration = 5;

    [Header("Spell Casting Upgrade")]
    [SerializeField] private int spellsToCast = 10;
    [SerializeField] private float spellCastingDomainSlowDownPercentage = 1;
    [SerializeField] private float spellCastingDomainDuration = 8;
    private float spellCastTimer;
    private float spellsPerSecond;

    [Header("Domain details")]
    public float maxDomainSize = 10;
    public float expandSpeed = 3;

    private List<Enemy> trappedTargets = new List<Enemy>();
    private Transform currentTarget;

    public void CreateDomain()
    {
        spellsPerSecond = spellsToCast / GetDomainDuration();

        GameObject domain = Instantiate(domainPrefab, transform.position, Quaternion.identity);
        domain.GetComponent<SkillObject_DomainExpansion>().SetupDomain(this);
    }

    public void DoSpellCasting()
    {
        spellCastTimer -= Time.deltaTime;

        if (currentTarget == null)
        {
            currentTarget = FindTargetInDomain();
        }

        if (currentTarget != null && spellCastTimer < 0) 
        {
            CastSpell(currentTarget);
            spellCastTimer = 1f / spellsPerSecond;
            currentTarget = null;
        }
    }

    private void CastSpell(Transform target)
    {
        if (upgradeType == SkillUpgradeType.Domain_EchoSpam) 
        {
            Vector3 offset = Random.value < 0.5f ? new Vector2(1, 0) : new Vector2(-1, 0);

            skillManager.timeEcho.CreateTimeEcho(target.position + offset);
        }

        if (upgradeType == SkillUpgradeType.Domain_ShardSpam)
        {
            skillManager.shard.CreateRawShard(target, true);
        }
    }

    private Transform FindTargetInDomain()
    {
        if (trappedTargets.Count == 0)
        {
            return null;
        }

        int randomIndex = Random.Range(0, trappedTargets.Count);
        Transform target = trappedTargets[randomIndex].transform;

        if (target == null)
        {
            trappedTargets.RemoveAt(randomIndex);
            return null;
        }

        return target;
    }

    public float GetDomainDuration()
    {
        if (upgradeType == SkillUpgradeType.Domain_SlowingDown)
        {
            return slowDownDomainDuration;
        }
        else
        {
            return spellCastingDomainDuration;
        }
    }

    public float GetSlowPercentage()
    {
        if (upgradeType == SkillUpgradeType.Domain_SlowingDown)
        {
            return slowDownPercent;
        }
        else
        {
            return spellCastingDomainSlowDownPercentage; 
        }
    }

    public bool InstantDomain()
    {
        return upgradeType != SkillUpgradeType.Domain_EchoSpam
            && upgradeType != SkillUpgradeType.Domain_ShardSpam;
    }

    public void AddTarget(Enemy targetToAdd)
    {
        trappedTargets.Add(targetToAdd);
    }

    public void ClearTargets()
    {
        foreach (Enemy target in trappedTargets)
        {
            target.StopSlowDown();
        }

        trappedTargets = new List<Enemy>();
    }
}
