using NUnit.Framework;
using RAW.Contracts.Skills;

namespace RAW.Contracts.Tests.EditMode
{
    public class SkillExecutionContractTests
    {
        [Test]
        public void Rejected_ReturnsFailedResult()
        {
            SkillExecutionResult result =
                SkillExecutionResult.Rejected(
                    requestId: 10,
                    failureReason: SkillFailureReason.OutOfRange,
                    targetEntityId: 20
                );

            Assert.That(result.Succeeded, Is.False);
            Assert.That(
                result.FailureReason,
                Is.EqualTo(SkillFailureReason.OutOfRange)
            );

            Assert.That(result.RequestId, Is.EqualTo(10UL));
            Assert.That(result.TargetEntityId, Is.EqualTo(20UL));
        }

        [Test]
        public void Rejected_ClearsEffectAndCosts()
        {
            SkillExecutionResult result =
                SkillExecutionResult.Rejected(
                    requestId: 10,
                    failureReason: SkillFailureReason.InvalidTarget,
                    targetEntityId: 20
                );

            Assert.That(
                result.EffectType,
                Is.EqualTo(SkillEffectType.None)
            );

            Assert.That(result.Amount, Is.Zero);
            Assert.That(result.ManaCost, Is.Zero);
            Assert.That(result.CooldownSeconds, Is.Zero);
        }

        [Test]
        public void Rejected_WithNoneReason_UsesInvalidRequest()
        {
            SkillExecutionResult result =
                SkillExecutionResult.Rejected(
                    requestId: 10,
                    failureReason: SkillFailureReason.None
                );

            Assert.That(result.Succeeded, Is.False);

            Assert.That(
                result.FailureReason,
                Is.EqualTo(SkillFailureReason.InvalidRequest)
            );
        }

        [Test]
        public void Success_PreservesValidValues()
        {
            SkillExecutionResult result =
                SkillExecutionResult.Success(
                    requestId: 10,
                    targetEntityId: 20,
                    effectType: SkillEffectType.Damage,
                    amount: 30,
                    manaCost: 10,
                    cooldownSeconds: 2f
                );

            Assert.That(result.Succeeded, Is.True);

            Assert.That(
                result.FailureReason,
                Is.EqualTo(SkillFailureReason.None)
            );

            Assert.That(result.RequestId, Is.EqualTo(10UL));
            Assert.That(result.TargetEntityId, Is.EqualTo(20UL));

            Assert.That(
                result.EffectType,
                Is.EqualTo(SkillEffectType.Damage)
            );

            Assert.That(result.Amount, Is.EqualTo(30));
            Assert.That(result.ManaCost, Is.EqualTo(10));
            Assert.That(result.CooldownSeconds, Is.EqualTo(2f));
        }

        [Test]
        public void Success_ClampsNegativeValuesToZero()
        {
            SkillExecutionResult result =
                SkillExecutionResult.Success(
                    requestId: 10,
                    targetEntityId: 20,
                    effectType: SkillEffectType.Damage,
                    amount: -30,
                    manaCost: -10,
                    cooldownSeconds: -2f
                );

            Assert.That(result.Succeeded, Is.True);
            Assert.That(result.Amount, Is.Zero);
            Assert.That(result.ManaCost, Is.Zero);
            Assert.That(result.CooldownSeconds, Is.Zero);
        }

        [Test]
        public void FailureReason_UsesStableNumericValues()
        {
            Assert.That(
                (int)SkillFailureReason.None,
                Is.EqualTo(0)
            );

            Assert.That(
                (int)SkillFailureReason.InvalidRequest,
                Is.EqualTo(1)
            );

            Assert.That(
                (int)SkillFailureReason.DuplicateRequest,
                Is.EqualTo(2)
            );

            Assert.That(
                (int)SkillFailureReason.NotOwner,
                Is.EqualTo(3)
            );

            Assert.That(
                (int)SkillFailureReason.SkillNotFound,
                Is.EqualTo(10)
            );

            Assert.That(
                (int)SkillFailureReason.InsufficientMana,
                Is.EqualTo(12)
            );

            Assert.That(
                (int)SkillFailureReason.InvalidTarget,
                Is.EqualTo(20)
            );

            Assert.That(
                (int)SkillFailureReason.OutOfRange,
                Is.EqualTo(22)
            );
        }

        [Test]
        public void EffectType_UsesStableNumericValues()
        {
            Assert.That(
                (int)SkillEffectType.None,
                Is.EqualTo(0)
            );

            Assert.That(
                (int)SkillEffectType.Damage,
                Is.EqualTo(1)
            );

            Assert.That(
                (int)SkillEffectType.Heal,
                Is.EqualTo(2)
            );
        }
    }
}