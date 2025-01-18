using System.Collections.Generic;
using System.Linq;
using GDObject = Godot.Object;

namespace Orcinus.Scripts.Core
{
    public static class GlobalSignalBus
    {
        public static readonly Dictionary<string, List<GDObject>> SignalEmitters = new Dictionary<string, List<GDObject>>();
        public static readonly Dictionary<string, List<(GDObject Target, string Method)>> SignalHandlers = new Dictionary<string, List<(GDObject Target, string Method)>>();
        private static readonly object SignalBusLock = new object();

        public static void RegisterEmitter(string signalName, GDObject emitter)
        {
            lock (SignalBusLock)
            {
                if (!SignalEmitters.ContainsKey(signalName))
                {
                    SignalEmitters[signalName] = new List<GDObject>();
                }

                bool isEmitterAlreadyRegistered = SignalEmitters[signalName].Any(existingEmitter => GDObject.IsInstanceValid(existingEmitter) && existingEmitter.GetInstanceId() == emitter.GetInstanceId());
                if (isEmitterAlreadyRegistered)
                {
                    return;
                }
                else
                {
                    SignalEmitters[signalName].Add(emitter);
                    ConnectEmitterToHandlers(signalName, emitter);
                }
            }
        }

        public static void RegisterHandler(string signalName, GDObject handlerTarget, string methodName)
        {
            lock (SignalBusLock)
            {
                if (!SignalHandlers.ContainsKey(signalName))
                {
                    SignalHandlers[signalName] = new List<(GDObject Target, string Method)>();
                }

                bool isHandlerAlreadyRegistered = SignalHandlers[signalName].Any(handler => GDObject.IsInstanceValid(handler.Target) && handler.Target.GetInstanceId() == handlerTarget.GetInstanceId());
                if (isHandlerAlreadyRegistered)
                {
                    return;
                }
                else
                {
                    SignalHandlers[signalName].Add((Target: handlerTarget, Method: methodName));
                    ConnectHandlerToEmitters(signalName, handlerTarget, methodName);
                }
            }
        }

        private static void ConnectHandlerToEmitters(string signalName, GDObject handlerTarget, string methodName)
        {
            if (!SignalEmitters.ContainsKey(signalName))
            {
                return;
            }

            var emitters = SignalEmitters[signalName];
            emitters.RemoveAll(e => !GDObject.IsInstanceValid(e));
            foreach (var emitter in emitters)
            {
                emitter.Connect(signalName, handlerTarget, methodName);
            }
        }

        private static void ConnectEmitterToHandlers(string signalName, GDObject emitter)
        {
            if (!SignalHandlers.ContainsKey(signalName))
            {
                return;
            }

            var handlers = SignalHandlers[signalName];
            handlers.RemoveAll(h => !GDObject.IsInstanceValid(h.Target));
            foreach (var handler in handlers)
            {
                emitter.Connect(signalName, handler.Target, handler.Method);
            }
        }


    }
}
