using System;
using CommandSystem;
using Exiled.Permissions.Extensions;

namespace CleanMaster
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public class CleanCommand : ICommand
    {
        public string Command => "clean";
        public string[] Aliases => Array.Empty<string>();
        public string Description => "Toggles CleanMaster cleanup functionality";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            // Проверка прав
            if (!sender.CheckPermission(CleanMaster.Singleton.Config.CleanCommandPermission))
            {
                response = "You don't have permission to use this command!";
                return false;
            }

            // Проверка аргументов
            if (arguments.Count == 0)
            {
                response = $"Current cleanup status: {(CleanMaster.Singleton.IsCleanupEnabled ? "ENABLED" : "DISABLED")}\n" +
                          "Usage: clean <enable|disable|toggle>";
                return false;
            }

            // Обработка команды (совместимая с C# 7.3)
            string action = arguments.At(0).ToLower();
            bool? newState = null;

            if (action == "enable" || action == "true" || action == "on")
            {
                newState = true;
            }
            else if (action == "disable" || action == "false" || action == "off")
            {
                newState = false;
            }
            else if (action == "toggle")
            {
                newState = !CleanMaster.Singleton.IsCleanupEnabled;
            }

            if (!newState.HasValue)
            {
                response = "Invalid argument. Use: enable, disable, toggle, true, false, on, off";
                return false;
            }

            CleanMaster.Singleton.IsCleanupEnabled = newState.Value;
            response = $"CleanMaster cleanup has been {(newState.Value ? "ENABLED" : "DISABLED")}";
            return true;
        }
    }
}