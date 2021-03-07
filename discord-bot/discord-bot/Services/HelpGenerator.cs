using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using discord_bot.Utils;

namespace discord_bot.Services
{
    public class HelpGenerator
    {
        private readonly CommandService _service;
        private readonly RequestScope _scope;

        public HelpGenerator(CommandService service, RequestScope scope)
        {
            _service = service;
            _scope = scope;
        }
        public ModuleInfo GetModule(string query)
        {
            var k = _service.Modules;
            foreach(var p in k)
            {
                foreach (var a in p.Aliases)
                {
                    if (a.ToLower() == query.ToLower()) return p;
                }
            }
            return null;
        }
        public async Task<bool> CheckPermissionPrecondition(CommandInfo command)
        {
            bool valid = true;
            foreach (var cond in command.Preconditions)
            {
                if (cond is not RequireBotPermissionAttribute)
                {
                    valid &= (await cond.CheckPermissionsAsync(_scope.Context, command, null)).IsSuccess;
                }
            }

            return valid;
        }
        public async Task<bool> CheckPermissionPrecondition(ModuleInfo module)
        {
            bool valid = true;
            foreach (var cond in module.Preconditions)
            {
                if (cond is not RequireBotPermissionAttribute)
                {
                    valid &= (await cond.CheckPermissionsAsync(_scope.Context, null, null)).IsSuccess;
                }
            }
            return valid;
        }
        public async Task<List<string>> CheckBotPermissionPrecondition(CommandInfo command)
        {
            var required = new List<string>();
            foreach (var cond in command.Preconditions)
            {
                if (cond is RequireBotPermissionAttribute bperm)
                {
                    if (!(await cond.CheckPermissionsAsync(_scope.Context, command, null)).IsSuccess)
                    {
                        if (!(await cond.CheckPermissionsAsync(_scope.Context, command, null)).IsSuccess)
                        {
                            if (bperm.GuildPermission != null)
                            {
                                required.Add(bperm.GuildPermission.ToString());
                            }
                        }
                    }
                }
            }
            return required;
        }
        public CommandInfo GetCommand(string query)
        {
            var k = _service.Commands;
            foreach(var p in k)
            {
                foreach (var a in p.Aliases)
                {
                    if (a.ToLower() == query.ToLower()) return p;
                }
            }
            return null;
        }
        public IEnumerable<string> GetCommandParameters(CommandInfo command)
        {
            var parameters = command.Parameters;
            var optionalTemplate = "`<{0}>`";
            var mandatoryTemplate = "`[{0}]`";
            List<string> parametersFormated = new List<string>();

            foreach (var parameter in parameters)
            {
                if (parameter.IsOptional)
                    parametersFormated.Add(String.Format(optionalTemplate, parameter.Name));
                else
                    parametersFormated.Add(String.Format(mandatoryTemplate, parameter.Name));
            }

            return parametersFormated;
        }
        public async Task<string> GetDetailedCommandInfo(CommandInfo command)
        {
            var aliases = string.Join(", ", command.Aliases);
            var parameters = string.Join(", ", GetCommandParameters(command));
            var name = command.Aliases.First();
            var summary = command.Summary;
            var requiredPerms = await CheckBotPermissionPrecondition(command);
            var sb = new StringBuilder()
                .AppendLine($"**Command name**: {name}")
                .AppendLine($"**Summary**: {summary}")
                .AppendLine($"**Usage**: {name} {parameters}")
                .AppendLine($"**Aliases**: {aliases}");
            if (requiredPerms.Count != 0)
            {
                sb.AppendLine($"**Missing Bot Permissions:** {string.Join(", ", requiredPerms)}");
            }
            return sb.ToString();
        }
        public string GetMiniCommandInfo(CommandInfo command)
        {
            var parameters = string.Join(", ", GetCommandParameters(command));
            var summary = command.Summary;
            var sb = new StringBuilder();
            sb.Append($"{command.Aliases.First()}");
            if (!string.IsNullOrEmpty(parameters))
            {
                sb.Append($" {parameters}");
            }
            if (!string.IsNullOrEmpty(summary))
            {
                sb.Append($" — {summary}");
            }
            return sb.ToString() + '\n';
        }
        public string GetSubModuleInfo(ModuleInfo submodule)
        {
            var summary = submodule.Summary;
            var sb = new StringBuilder();
            sb.Append($"*{submodule.Aliases.First()}*");
            if (!string.IsNullOrEmpty(summary))
            {
                sb.Append($" — {summary}");
            }
            return sb.ToString() + '\n';
        }
        public async Task<string> RenderModuleHelp(ModuleInfo module)
        {
            var str = "";
            foreach (var cmd in await FilterCommands(module.Commands))
            {
                str += GetMiniCommandInfo(cmd);
            }
            
            foreach (var cmd in await FilterModules(module.Submodules))
            {
                str += GetSubModuleInfo(cmd);
            }
            return str;
        }
        public async Task<List<ModuleInfo>> FilterModules(IEnumerable<ModuleInfo> modules)
        {
            var allowed = new List<ModuleInfo>();
            foreach (var module in modules)
            {
                if (await ShouldShowModule(module))
                {
                    allowed.Add(module);
                }
            }
            return allowed;
        }
        public async Task<List<CommandInfo>> FilterCommands(IEnumerable<CommandInfo> commands)
        {
            var allowed = new List<CommandInfo>();
            foreach (var module in commands)
            {
                if (await ShouldShowCommand(module))
                {
                    allowed.Add(module);
                }
            }
            return allowed;
        }
        public async Task<bool> ShouldShowModule(ModuleInfo module)
        {
            if (!Config.PermissionBasedGeneration) return true;
            if (!await CheckPermissionPrecondition(module)) return false;
            foreach (var cmd in module.Commands)
            {
                if (await ShouldShowCommand(cmd))
                {
                    return true;
                }
            }
            foreach (var cmd in module.Submodules)
            {
                if (await ShouldShowModule(cmd))
                {
                    return true;
                }
            }
            return false;
        }
        public async Task<bool> ShouldShowCommand(CommandInfo cmd)
        {
            if (!Config.PermissionBasedGeneration) return true;

            var curModule = cmd.Module;
            while (curModule.IsSubmodule)
            {
                if (!await CheckPermissionPrecondition(curModule))
                {
                    return false;
                }

                curModule = curModule.Parent;
            }
            if (!await CheckPermissionPrecondition(curModule))
            {
                return false;
            }
            
            var result = await CheckPermissionPrecondition(cmd);
            return result;
        }
        public async Task<Embed> GetHelp(string query)
        {
            var builder = new EmbedBuilder()
            {
                Title = $"{Config.BotName} Help",
                Color = new Color(114, 137, 218)
            };
            var footer = new EmbedFooterBuilder();
            if (query.Trim() == "")
            {
                foreach (var module in await FilterModules(_service.Modules))
                {
                    if (!module.IsSubmodule)
                    {
                        builder.AddField(async x =>
                        {
                            var alias = module.Aliases[0];
                            if (string.IsNullOrEmpty(alias))
                            {
                                x.Name = module.Name;
                            }
                            else
                            {
                                x.Name = module.Name + $" (`{alias}`)";
                            }
                            x.Value = await RenderModuleHelp(module);
                            x.IsInline = false;
                        });
                    }
                }
                footer.Text += "Italicised items represent sub-modules.\nFor more detail run help <sub-module/command>";
            }
            else
            {
                var module = GetModule(query);
                if (module != null && await ShouldShowModule(module))
                {
                    footer.Text = $"Showing help for module {query}";
                    builder.Description = await RenderModuleHelp(module);
                }
                else
                {
                    var command = GetCommand(query);
                    if (command != null && await ShouldShowCommand(command))
                    {
                        footer.Text = $"Showing help for command {query}";
                        builder.Description = await GetDetailedCommandInfo(command);
                    }
                    else
                    {
                        builder.Description = "Command or Module not found!";
                    }
                }
            }
            
            builder.Footer = footer;
            return builder.Build();
        }
    }
}