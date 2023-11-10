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
        static readonly Dictionary<string, RaiserMaker?> raiserMakersMap = new();
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
                raiserMakersMap[handlerInterface.Name] = RaiserMaker;
            }
        }

        public static Dictionary<string, Handler>? CreateHandlerMap(Server server)
        {
            Handler[]? handlers = HandlerMaker?.GetInvocationList().Select(handlerMaker => ((HandlerMaker)handlerMaker)()).ToArray();

            foreach (Handler handler in handlers ?? Enumerable.Empty<Handler>())
                handler.SetServer(server);

            return handlers?.ToDictionary(handler => handler.Name);
        }

        public static Dictionary<string, IRaiser>? CreateRaiserMap(Handler handler)
        {
            IEnumerable<IRaiser>? raisers = raiserMakersMap[handler.Name]?.GetInvocationList().Select(raiserMaker => ((RaiserMaker)raiserMaker)(handler));
            return raisers?.ToDictionary(raiser => raiser.Name);
        }
    }
}
