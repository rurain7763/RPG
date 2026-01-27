using System;
using UnityEngine;

public class ThrowSwordParameters : Arguments
{
    public Vector2 ThrowDirection;
}

[Serializable]
public class ThrowSword : RPGSkill
{
    class AimedThrowSequence : SkillSequence
    {
        private ThrowSword skill;

        private GameObject[] trajectoryDots;

        public AimedThrowSequence(ThrowSword skill)
        {
            this.skill = skill;
        }

        private void SetTrajectoryDotPositions()
        {
            Vector2 startPosition = skill.entity.CenterPosition;
            Vector2[] relativeTrajectoryPoints = Helper.CalcTrajectoryPoints2D(skill.lastThrowDirection, skill.Data.ThrowForce, Physics2D.gravity.y * skill.swordGravityScale, skill.Data.TrajectoryDotCount, 0.1f);
        
            for (int i = 0; i < trajectoryDots.Length; i++)
            {
                trajectoryDots[i].transform.position = startPosition + relativeTrajectoryPoints[i];
            }
        }

        public override void Begin()
        {
            // Initialize trajectory dots
            trajectoryDots = new GameObject[skill.Data.TrajectoryDotCount];
            for (int i = 0; i < trajectoryDots.Length; i++)
            {
                trajectoryDots[i] = GameObject.Instantiate(skill.Data.TrajectoryDotPrefab);
            }
            SetTrajectoryDotPositions();
        }

        public override void UpdateParameters(Arguments parameters)
        {
            ThrowSwordParameters throwParams = parameters as ThrowSwordParameters;
            if (throwParams == null)
            {
                Debug.LogError("Invalid parameters for ThrowSword skill.");
                return;
            }

            skill.lastThrowDirection = throwParams.ThrowDirection;

            SetTrajectoryDotPositions();
        }

        public override void End()
        {
            for (int i = 0; i < trajectoryDots.Length; i++)
            {
                GameObject.Destroy(trajectoryDots[i]);
            }
        }
    }

    class DefaultThrowSequence : SkillSequence
    {
        private ThrowSword skill;

        public DefaultThrowSequence(ThrowSword skill)
        {
            this.skill = skill;
        }

        public override void Begin()
        {
            Vector2 startPosition = skill.entity.CenterPosition;
            skill.currentSwordInstance = GameObject.Instantiate(skill.Data.SwordPrefab, startPosition, Quaternion.identity);
            skill.currentSwordInstance.Owner = skill.entity;
            skill.currentSwordInstance.Throw(skill.lastThrowDirection, skill.Data.ThrowForce);
            skill.NextSequence();
        }
    }

    class PierceThrowSequence : SkillSequence
    {
        private ThrowSword skill;

        public PierceThrowSequence(ThrowSword skill)
        {
            this.skill = skill;
        }

        public override void Begin()
        {
            Vector2 startPosition = skill.entity.CenterPosition;
            skill.currentSwordInstance = GameObject.Instantiate(skill.Data.SwordPrefab, startPosition, Quaternion.identity);
            skill.currentSwordInstance.Owner = skill.entity;
            skill.currentSwordInstance.PierceCount = skill.Data.PierceCount;
            skill.currentSwordInstance.Throw(skill.lastThrowDirection, skill.Data.ThrowForce);
            skill.NextSequence();
        }
    }

    class SpinThrowSequence : SkillSequence
    {
        private ThrowSword skill;

        public SpinThrowSequence(ThrowSword skill)
        {
            this.skill = skill;
        }

        public override void Begin()
        {
            Vector2 startPosition = skill.entity.CenterPosition;
            skill.currentSwordInstance = GameObject.Instantiate(skill.Data.SwordPrefab, startPosition, Quaternion.identity);
            skill.currentSwordInstance.Owner = skill.entity;
            skill.currentSwordInstance.PierceCount = skill.Data.PierceCount;
            skill.currentSwordInstance.MaxDistanceFromOrigin = skill.Data.MaxThrowDistance;
            skill.currentSwordInstance.OnReachedMaxDistance += () => skill.currentSwordInstance.Spin(skill.Data.SpinDuration, skill.NextSequence);
            skill.currentSwordInstance.Throw(skill.lastThrowDirection, skill.Data.ThrowForce);
        }
    }

    class BounceThrowSequence : SkillSequence
    {
        private ThrowSword skill;
        
        public BounceThrowSequence(ThrowSword skill)
        {
            this.skill = skill;
        }

        public override void Begin()
        {
            Vector2 startPosition = skill.entity.CenterPosition;
            skill.currentSwordInstance = GameObject.Instantiate(skill.Data.SwordPrefab, startPosition, Quaternion.identity);
            skill.currentSwordInstance.Owner = skill.entity;
            skill.currentSwordInstance.BounceCount = skill.Data.BounceCount;
            skill.currentSwordInstance.OnStuck += () => skill.currentSwordInstance.Bounce(skill.NextSequence);
            skill.currentSwordInstance.Throw(skill.lastThrowDirection, skill.Data.ThrowForce);
        }
    }

    class ThrowBackSequence : SkillSequence
    {
        private ThrowSword skill;

        public ThrowBackSequence(ThrowSword skill)
        {
            this.skill = skill;
        }

        public override void Use(GameObject user, Arguments parameters)
        {
            if (user != skill.entity.gameObject)
            {
                throw new InvalidOperationException("Different user when first use something is wrong.");
            }

            skill.currentSwordInstance.BackToOwner(skill.NextSequence);
        }

        public override void End()
        {
            GameObject.Destroy(skill.currentSwordInstance.gameObject);
        }
    }

    public new ThrowSwordData Data => base.Data as ThrowSwordData;

    private float swordGravityScale;

    private Vector2 lastThrowDirection;
    private ThrowingSword currentSwordInstance;

    public ThrowSword(ThrowSwordData data) 
        : base(data)
    {
        swordGravityScale = Data.SwordPrefab.GetComponent<Rigidbody2D>().gravityScale;

        RegisterEventHandler("Throw", (data) => NextSequence());
    }

    protected override void StartUse(GameObject user, Arguments parameters = null)
    {
        base.StartUse(user, parameters);

        var throwParams = parameters as ThrowSwordParameters;
        if (throwParams == null)
        {
            Debug.LogError("Invalid parameters for ThrowSword skill.");
            return;
        }

        lastThrowDirection = throwParams.ThrowDirection;

        AddSequence(new AimedThrowSequence(this));

        if (HasUpgrade(ThrowSwordUpgradeFlag.Spin))
        {
            AddSequence(new SpinThrowSequence(this));
        }
        else if (HasUpgrade(ThrowSwordUpgradeFlag.Pierce))
        {
            AddSequence(new PierceThrowSequence(this));
        }
        else if (HasUpgrade(ThrowSwordUpgradeFlag.Bounce))
        {
            AddSequence(new BounceThrowSequence(this));
        }
        else
        {
            AddSequence(new DefaultThrowSequence(this));
        }

        AddSequence(new ThrowBackSequence(this));

        StartSequences();
    }
}