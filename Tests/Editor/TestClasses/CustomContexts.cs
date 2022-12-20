using System.Collections.Generic;

namespace Physalia.AbilityFramework.Tests
{
    public class CustomActivationPayload : IEventContext
    {
        public CustomUnit activator;
    }

    public class CustomUnitTriggerContext : IEventContext
    {
        public CustomUnit unit;
    }

    public class CustomNormalAttackPayload : IEventContext
    {
        public CustomUnit attacker;
        public CustomUnit mainTarget;
    }

    public class CustomDamageEvent : IEventContext
    {
        public CustomUnit instigator;
        public CustomUnit target;
    }
}
