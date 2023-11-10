﻿using System.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lynx.Server
{
    using HandlerMaker = Func<Handler>;
    using ExecuterMaker = Func<Handler, IExecuter>;
    using RaiserMaker = Action<Handler>;

    public abstract partial class Handler
    {
        static readonly BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        static readonly HandlerMaker? HandlerMaker;
        static readonly Dictionary<string, ExecuterMaker?> executerMakerMap = new();
        static readonly Dictionary<string, RaiserMaker?> raiserMakerMap = new();
        static readonly Type typeofIHandler = typeof(IHandler);
        static readonly Type executerGeneric = typeof(Executer<,>);
        static readonly Type[] executerMakerTypes = new[] { typeof(Handler), typeof(MethodInfo) };
        static readonly Type[] raiserMakerTypes = new[] { typeof(Handler), typeof(EventInfo) };
        static readonly Type typeofRaiser = typeof(Raiser<>);

        static Handler()
        {
            Type[] types = Assembly.GetExecutingAssembly().GetTypes();
            IEnumerable<Type> handlerInterfaces = types.Where(type => type.IsInterface && type != typeofIHandler && typeofIHandler.IsAssignableFrom(type));

            foreach (Type handlerInterface in handlerInterfaces)
            {
                Type handlerType = types.Single(type => type.GetInterfaces().Contains(handlerInterface));
                ExecuterMaker? ExecuterMaker = null;
                RaiserMaker? raiserMaker = null;
                IEnumerable<MethodInfo> methods = handlerInterface.GetMethods().Where(method => !method.IsSpecialName);

                foreach (MethodInfo method in methods)
                {
                    Type resultType = method.ReturnParameter.ParameterType.GetGenericArguments()[0];
                    Type parameterType = method.GetParameters()[0].ParameterType;
                    Type executerType = executerGeneric.MakeGenericType(parameterType, resultType);
                    ConstructorInfo executerMaker = executerType.GetConstructor(flags, executerMakerTypes)!;
                    ExecuterMaker += handler => (IExecuter)executerMaker.Invoke(new object[] { handler, method });
                }

                foreach (EventInfo eventInfo in handlerInterface.GetEvents())
                {
                    Type parameterType = eventInfo.EventHandlerType!.GetGenericArguments()[0];
                    Type raiserType = typeofRaiser.MakeGenericType(parameterType);
                    ConstructorInfo raiserConstructor = raiserType.GetConstructor(flags, raiserMakerTypes)!;
                    raiserMaker += handler => raiserConstructor.Invoke(new object[] { handler, eventInfo });
                }

                HandlerMaker += () => (Handler)Activator.CreateInstance(handlerType)!;
                executerMakerMap[handlerInterface.Name] = ExecuterMaker;
                raiserMakerMap[handlerInterface.Name] = raiserMaker;
            }
        }

        public static Dictionary<string, Handler>? CreateHandlerMap(Client client)
        {
            Handler[]? handlers = HandlerMaker?.GetInvocationList().Select(handlerMaker => ((HandlerMaker)handlerMaker)()).ToArray();

            foreach (Handler handler in handlers ?? Enumerable.Empty<Handler>())
                handler.SetClient(client);

            return handlers?.ToDictionary(handler => handler.Name);
        }

        public static Dictionary<string, IExecuter>? CreateExecuterMap(Handler handler)
        {
            IEnumerable<IExecuter>? executers = executerMakerMap[handler.Name]?.GetInvocationList().Select(executerMaker => ((ExecuterMaker)executerMaker)(handler));
            return executers?.ToDictionary(executer => executer.Name);
        }

        public static void CreateRaisers(Handler handler)
        {
            raiserMakerMap[handler.Name]?.Invoke(handler);
        }
    }
}