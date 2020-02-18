﻿using System.Diagnostics;
using System.Linq;
using Common;
using Microsoft.Extensions.Logging;

namespace Commands
{
    public class UserCommand : CommandBase
    {
        private string[] arguments;

        public UserCommand()
            : base(new CommandSettings
            {
                LogPerfix = "USER-COMMAND",
                LogFileName = null,
                MeasureElapsedTime = false,
                Location = CommandLocation.Any
            })
        {
        }

        protected override int Execute()
        {
            var cmd = CementSettings.Get().UserCommands[arguments[0]];
            Log.LogDebug("Run command " + arguments[0] + ": '" + cmd + "'");
            if (arguments.Length > 1)
            {
                arguments = arguments.Skip(1).ToArray();
                cmd = string.Format(cmd, arguments);
            }

            return Run(cmd);
        }

        protected override void ParseArgs(string[] args)
        {
            arguments = args;
        }

        private static int Run(string cmd)
        {
            ConsoleWriter.WriteInfo("Running command '" + cmd + "'");

            var startInfo = new ProcessStartInfo
            {
                FileName = Helper.OsIsUnix() ? "/bin/bash" : "cmd",
                Arguments = Helper.OsIsUnix() ? " -lc " : " /c ",
                UseShellExecute = false
            };
            startInfo.Arguments = startInfo.Arguments + "\"" + cmd + "\"";

            var process = Process.Start(startInfo);
            process.WaitForExit();
            return process.ExitCode;
        }

        public override string HelpMessage => @"";
    }
}