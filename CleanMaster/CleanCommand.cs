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
        public string Description => "Controls CleanMaster functionality";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission(CleanMaster.Singleton.Config.CleanCommandPermission))
            {
                response = "No permission! Required: " + CleanMaster.Singleton.Config.CleanCommandPermission;
                return false;
            }

            if (arguments.Count == 0)
            {
                response = $"Current status: {(CleanMaster.Singleton.IsCleanupEnabled ? "ENABLED" : "DISABLED")}";
                return false;
            }

            switch (arguments.At(0).ToLower())
            {
                case "enable":
                case "on":
                    CleanMaster.Singleton.IsCleanupEnabled = true;
                    response = "Cleanup ENABLED";
                    return true;

                case "disable":
                case "off":
                    CleanMaster.Singleton.IsCleanupEnabled = false;
                    response = "Cleanup DISABLED";
                    return true;

                case "toggle":
                    CleanMaster.Singleton.IsCleanupEnabled = !CleanMaster.Singleton.IsCleanupEnabled;
                    response = $"Cleanup {(CleanMaster.Singleton.IsCleanupEnabled ? "ENABLED" : "DISABLED")}";
                    return true;

                default:
                    response = "Invalid argument. Use: enable, disable, toggle";
                    return false;
            }
        }
    }
}