

namespace Momentum
{


    public static class Registry
    {
        private static IGlobalRegistry registry;

        public static void Initialize(IGlobalRegistry instance) => registry = instance;

        public static T Get<T>() => registry.Get<T>();

        public static ServiceRegistry Service   => registry.Get<ServiceRegistry>();
        public static ConfigRegistry Config     => registry.Get<ConfigRegistry>();
    }


    public static class Predicate
    {
        static IPredicateRegistry predicateRegistry;

        public static void Initialize(IPredicateRegistry registry) => predicateRegistry = registry;

        public static T Get<T>()    where T : IPredicate    => predicateRegistry.Get<T>();
        public static bool Is<T>()  where T : IPredicate    => predicateRegistry.Get<T>().Evaluate();
        public static T Resolve<T>()                        => predicateRegistry.Resolve<T>();
        public static T Resolve<R, T>() where R : IRegistry => Registry.Get<R>().Resolve<T>();
    }


    public static class Config
    {
        private static IConfigRegistry configRegistry;

        public static void Initialize(IConfigRegistry registry)
        {
            configRegistry = registry;
        }

        public static T Resolve<T>()
        {
            return configRegistry.Resolve<T>();
        }
    }

    
    public static class Service
    {
        private static IServiceRegistry serviceRegistry;

        public static void Initialize(IServiceRegistry registry)
        {
            serviceRegistry = registry;
        }

        public static T Resolve<T>()
        {
            return serviceRegistry.Resolve<T>();
        }
    }
}