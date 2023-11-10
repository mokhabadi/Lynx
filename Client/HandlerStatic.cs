using System.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lynx.Client
{
    using HandlerMaker = Func<Handler>;
    using RaiserMaker = Func<Handler, IRaiser>;

    public abstract partial class Handler
    {
        static readonly BindingFlags flags = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        static readonly HandlerMaker? HandlerMaker;
        static readonly Dictionary<Type, RaiserMaker?> raiserMakersMap = new();
        static readonly Type typeofIHandler = typeof(IHandler);
        static readonly Type typeofRaiser = typeof(Raiser<>);
        static readonly Type[] raiserMakerTypes = new[] { typeof(Handler), typeof(FieldInfo) };

        static Handler()
        {
            Type[] types = Assembly.GetExecutingAssembly().GetTypes();
            IEnumerable<Type> handlerInterfaces = types.Where(type => type.IsInterface && type != typeofIHandler && typeofIHandler.IsAssignableFrom(type));

            foreach (Type handlerInterface in handlerInterfaces)
            {
                Type handlerType = types.Single(type => type.GetInterfaces().Contains(handlerInterface));
                RaiserMaker? RaiserMaker = null;

                foreach (EventInfo eventInfo in handlerInterface.GetEvents())
                {
                    Type raiserType = typeofRaiser.MakeGenericType(eventInfo.EventHandlerType!.GetGenericArguments()[0]);
                    FieldInfo fieldInfo = handlerType.GetField(eventInfo.Name, flags)!;
                    ConstructorInfo raiserMaker = raiserType.GetConstructor(flags, raiserMakerTypes)!;
                    RaiserMaker += handler => (IRaiser)raiserMaker.Invoke(new object[] { handler, fieldInfo });
                }

                HandlerMaker += () => (Handler)Activator.CreateInstance(handlerType, flags, null, null, null)!;
                raiserMakersMap[handlerType] = RaiserMaker;
            }
        }

        public static Handler[]? MakeHandlers(Server server)
        {
            Handler[]? handlers = HandlerMaker?.GetInvocationList().Select(handlerMaker => ((HandlerMaker)handlerMaker)()).ToArray();

            foreach (Handler handler in handlers ?? Enumerable.Empty<Handler>())
                handler.SetServer(server);

            return handlers;
        }

        public static IEnumerable<IRaiser>? MakeRaisers(Handler handler)
        {
            return raiserMakersMap[handler.GetType()]?.GetInvocationList().Select(raiserMaker => ((RaiserMaker)raiserMaker)(handler));
        }
    }
}
