using CUButt;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CUButt
{
    public static class ConsoleCommands
    {
        private static readonly List<Command> RegisteredCommands = new List<Command>();

        private const BindingFlags ConsoleMethodFlags =
            BindingFlags.Instance |
            BindingFlags.Static |
            BindingFlags.Public |
            BindingFlags.NonPublic |
            BindingFlags.FlattenHierarchy;

        public static void Register()
        {
            RegisterCommand(
                "intiface",
                "Open CUButt settings window.",
                args =>
                {
                    CUButtSettingsWindow.Open();
                },
                null,
                Args()
            );
        }

        private static void RegisterCommand(string name, string description, Command.Action action, Dictionary<int, List<string>> argAutofill, ValueTuple<string, string>[] argDescription)
        {
            if (string.IsNullOrWhiteSpace(name) || action == null)
            {
                return;
            }

            name = name.Trim();

            if (RegisteredCommands.Any(command => SameCommandName(command.name, name)))
            {
                return;
            }

            Command command = new Command(
                name,
                description ?? string.Empty,
                action,
                argAutofill,
                argDescription ?? new ValueTuple<string, string>[0]
            );

            RegisteredCommands.Add(command);
            if (ConsoleScript.Commands != null && ConsoleScript.Commands.Count > 0)
            {
                InjectSingle(command);
            }
        }

        private static void InjectRegisteredCommands()
        {
            if (ConsoleScript.Commands == null)
            {
                return;
            }

            foreach (Command command in RegisteredCommands)
            {
                InjectSingle(command);
            }
        }

        private static void InjectSingle(Command command)
        {
            if (command == null || ConsoleScript.Commands == null)
            {
                return;
            }

            if (ConsoleScript.Commands.Any(existing => SameCommandName(existing.name, command.name)))
            {
                return;
            }

            ConsoleScript.Commands.Add(command);
        }

        private static bool SameCommandName(string left, string right)
        {
            return string.Equals(left, right, StringComparison.OrdinalIgnoreCase);
        }

        private static ValueTuple<string, string>[] Args(params ValueTuple<string, string>[] args)
        {
            return args ?? new ValueTuple<string, string>[0];
        }

        private static void ConsoleLog(ConsoleScript console, string message)
        {
            InvokeConsoleMethod(console, "LogToConsole", message);
        }

        private static void InvokeConsoleMethod(ConsoleScript console, string methodName, params object[] parameters)
        {
            if (console == null || string.IsNullOrEmpty(methodName))
            {
                return;
            }

            MethodInfo method = console.GetType().GetMethod(methodName, ConsoleMethodFlags);
            if (method == null)
            {
                return;
            }

            try
            {
                method.Invoke(console, parameters);
            }
            catch (TargetInvocationException ex)
            {
                if (ex.InnerException != null && ex.InnerException.Message.Contains("No world is loaded"))
                {
                    return;
                }

                throw;
            }
        }

        [HarmonyPatch(typeof(ConsoleScript), "RegisterAllCommands")]
        private static class ConsoleScriptRegisterAllCommandsPatch
        {
            private static void Postfix()
            {
                InjectRegisteredCommands();
            }
        }

        [HarmonyPatch(typeof(ConsoleScript), "RegisterSpawnEntities")]
        private static class ConsoleScriptRegisterSpawnEntitiesPatch
        {
            private static bool Prefix(ConsoleScript __instance)
            {
                if (__instance == null || ConsoleScript.Commands == null)
                {
                    return true;
                }

                if (ConsoleScript.Commands.Count == 0)
                {
                    __instance.RegisterAllCommands();
                }

                Command spawnCommand = ConsoleScript.Commands.FirstOrDefault(command => SameCommandName(command.name, "spawn"));
                if (spawnCommand == null)
                {
                    return true;
                }

                if (spawnCommand.argAutofill == null)
                {
                    spawnCommand.argAutofill = new Dictionary<int, List<string>>();
                }

                return true;
            }
        }
    }
}
