using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using discord_bot.Services;
using discord_bot.Utils;

namespace discord_bot.Modules
{
    [Name("Post Contest")]
    public class PostContestModule : ModuleBase<SocketCommandContext>
    {
        public RequestScope Scope { get; set; }
        public EventService Events { get; set; }
        [Command("setup"), Summary("Setup Post Contest"), RequireUserPermission(GuildPermission.ManageChannels)]
        public async Task Setup()
        {
            if (!Scope.GetGuildPersistentData().HasSetup)
            {
                await ReplyAsync("Creating Post Contest Channels...");
                var guild = Scope.Context.Guild;
                var data = Scope.GetGuildPersistentData();
                var conf = await guild.CreateCategoryChannelAsync("Confidential");
                var pub = await guild.CreateCategoryChannelAsync("Archived");
                data.ConfidentialCategory = conf.Id;
                data.PublicCategory = pub.Id;
                data.HasSetup = true;
            }
            else
            {
                await ReplyAsync("Already Setup!");
            }
        }

        public string Clean(string input)
        {
            string s = "";
            foreach (var c in input)
            {
                if (char.IsDigit(c) || ('a' <= c && c <= 'z') || c == ' ' || c == '-')
                {
                    s += c;
                }
            }
            return s.ToLower().Trim().Replace(" ", "-");
        }
        [Command("create"), Summary("Create Post Contest"), RequireUserPermission(GuildPermission.ManageChannels)]
        public async Task Create([Remainder] string contestName)
        {
            var data = Scope.GetGuildPersistentData();
            if (!Scope.GetGuildPersistentData().HasSetup)
            {
                await ReplyAsync("Please setup the bot first!");
                return;
            }

            if (data.ActiveChannels.Count >= Config.MaxChannels)
            {
                await ReplyAsync($"Exceeded channel limit of {Config.MaxChannels}");
                return;
            }
            var name = Clean(contestName);
            if (string.IsNullOrEmpty(name))
            {
                await ReplyAsync($"Please enter a contest name that is not empty!");
                return;
            }
            if (data.ActiveChannels.ContainsKey(name))
            {
                await ReplyAsync($"Contest with name `{name}` already exists!");
                return;
            }
            await ReplyAsync($"Creating post-contest channel `{name}`...");
            var channel = await Scope.Context.Guild.CreateTextChannelAsync(name);
            data.ActiveChannels[name] = channel.Id;
            await channel.ModifyAsync(x =>
            {
                x.CategoryId = data.ConfidentialCategory;
            });
            await channel.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole,
                new OverwritePermissions(viewChannel: PermValue.Deny));
        }
        [Command("convert"), Summary("Convert a channel into a post-contest channel"), RequireUserPermission(GuildPermission.Administrator)]
        public async Task Convert(string channelMention, [Remainder] string contestName)
        {
            var data = Scope.GetGuildPersistentData();
            if (!Scope.GetGuildPersistentData().HasSetup)
            {
                await ReplyAsync("Please setup the bot first!");
                return;
            }

            if (!MentionUtils.TryParseChannel(channelMention, out var cid)) return;
            var name = Clean(contestName);
            if (string.IsNullOrEmpty(name))
            {
                await ReplyAsync($"Please enter a contest name that is not empty!");
                return;
            }
            if (data.ActiveChannels.ContainsKey(name))
            {
                await ReplyAsync($"Contest with name `{name}` already exists!");
                return;
            }
            await ReplyAsync($"Converting post-contest channel `{name}`...");
            var channel = Context.Guild.GetChannel(cid);
            data.ActiveChannels[name] = channel.Id;
            await channel.ModifyAsync(x =>
            {
                x.Name = name;
                x.CategoryId = data.ConfidentialCategory;
            });
            await channel.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole,
                new OverwritePermissions(viewChannel: PermValue.Deny));
        }
        
        [Command("grant"), Summary("Grant a user permission to see a post-contest channel"), RequireUserPermission(GuildPermission.ManageRoles)]
        public async Task Grant(string mention, [Remainder] string contestName)
        {
            var data = Scope.GetGuildPersistentData();
            if (!Scope.GetGuildPersistentData().HasSetup)
            {
                await ReplyAsync("Please setup the bot first!");
                return;
            }

            var name = Clean(contestName);
            if (!data.ActiveChannels.ContainsKey(name))
            {
                await ReplyAsync($"Contest with name `{name}` does not exist!");
                return;
            }

            if (!MentionUtils.TryParseUser(mention, out var userId)) return;
            var user = Context.Guild.GetUser(userId);
            await ReplyAsync($"Granting permission to user `{user.Username}` for contest `{name}`");
            var channel = Scope.Context.Guild.GetChannel(data.ActiveChannels[name]);
            await channel.AddPermissionOverwriteAsync(user,
                new OverwritePermissions(viewChannel: PermValue.Allow));
        }
        [Command("revoke"), Summary("Revoke a user's permission to see a post-contest channel"), RequireUserPermission(GuildPermission.ManageRoles)]
        public async Task Revoke(string mention, [Remainder] string contestName)
        {
            var data = Scope.GetGuildPersistentData();
            if (!Scope.GetGuildPersistentData().HasSetup)
            {
                await ReplyAsync("Please setup the bot first!");
                return;
            }

            var name = Clean(contestName);
            if (!data.ActiveChannels.ContainsKey(name))
            {
                await ReplyAsync($"Contest with name `{name}` does not exist!");
                return;
            }

            if (!MentionUtils.TryParseUser(mention, out var userId)) return;
            var user = Context.Guild.GetUser(userId);
            await ReplyAsync($"Revoking `{user.Username}`'s permission for contest `{name}`");
            var channel = Scope.Context.Guild.GetChannel(data.ActiveChannels[name]);
            await channel.AddPermissionOverwriteAsync(user,
                new OverwritePermissions(viewChannel: PermValue.Inherit));
        }
        [Command("list"), Summary("List current post-contests")]
        public async Task List()
        {
            var data = Scope.GetGuildPersistentData();
            if (!Scope.GetGuildPersistentData().HasSetup)
            {
                await ReplyAsync("Please setup the bot first!");
                return;
            }

            var eb = new EmbedBuilder();
            eb.Title = "Current Contests:";
            var values = new List<string>();
            int len = 0;
            foreach (var channels in data.ActiveChannels)
            {
                if (len + channels.Key.Length + 10 < 2000)
                {
                    values.Add(channels.Key);
                    len += channels.Key.Length + 10;
                }
                else
                {
                    eb.Title += " (truncated)";
                    break;
                }
            }

            eb.Description = string.Join(", ", values);
            await ReplyAsync(embed:eb.Build());
        }
        [Command("publish"), Summary("Make a post-contest channel public"), RequireUserPermission(GuildPermission.ManageRoles)]
        public async Task Publish([Remainder] string contestName)
        {
            var data = Scope.GetGuildPersistentData();
            if (!Scope.GetGuildPersistentData().HasSetup)
            {
                await ReplyAsync("Please setup the bot first!");
                return;
            }

            var name = Clean(contestName);
            if (!data.ActiveChannels.ContainsKey(name))
            {
                await ReplyAsync($"Contest with name `{name}` does not exist!");
                return;
            }
            
            await ReplyAsync($"{MentionUtils.MentionUser(Context.User.Id)} Are you sure you want to publish `{name}`? Say `yes` in 5 seconds to confirm!");

            EventService.UserMessageDelegate del = null;
            del = delegate(SocketCommandContext context)
            {
                if (context.User.Id == Context.User.Id)
                {
                    Events.OnUserMessage -= del;
                    del = null;
                    if (context.Message.Content == "yes")
                    {
                        ReplyAsync("Publishing Channel...").Wait();
                        var channel = Scope.Context.Guild.GetChannel(data.ActiveChannels[name]);
                        channel.RemovePermissionOverwriteAsync(Context.Guild.EveryoneRole).Wait();
                        channel.ModifyAsync(x =>
                        {
                            x.CategoryId = data.PublicCategory;
                        }).Wait();
                        data.ActiveChannels.Remove(name);
                    }
                    else
                    {
                        ReplyAsync("Cancelled");
                    }
                }
                return true;
            };
            Task.Run(async () =>
            {
                await Task.Delay(5000);
                if (del != null)
                {
                    Events.OnUserMessage -= del;
                    await ReplyAsync("Cancelled");
                }
            });
            Events.OnUserMessage += del;
        }
        [Command("delete"), Summary("Delete a post contest"), RequireOwner]
        public async Task Delete([Remainder] string contestName)
        {
            var data = Scope.GetGuildPersistentData();
            if (!Scope.GetGuildPersistentData().HasSetup)
            {
                await ReplyAsync("Please setup the bot first!");
                return;
            }

            var name = Clean(contestName);
            if (!data.ActiveChannels.ContainsKey(name))
            {
                await ReplyAsync($"Contest with name `{name}` does not exist!");
                return;
            }
            
            await ReplyAsync($"{MentionUtils.MentionUser(Context.User.Id)} Are you sure you want to **delete** `{name}`? Say `yes` in 5 seconds to confirm!");

            EventService.UserMessageDelegate del = null;
            del = delegate(SocketCommandContext context)
            {
                Task.Run(async () =>
                {
                    if (context.User.Id == Context.User.Id)
                    {
                        Events.OnUserMessage -= del;
                        del = null;
                        if (context.Message.Content == "yes")
                        {
                            await ReplyAsync("Deleting Channel...");
                            try
                            {
                                var k = data.ActiveChannels[name];
                                data.ActiveChannels.Remove(name);
                                var channel = Scope.Context.Guild.GetChannel(k);
                                await channel.DeleteAsync();
                            }
                            catch
                            {
                                
                            }
                        }
                        else
                        {
                            await ReplyAsync("Cancelled");
                        }
                    }
                });
                return true;
            };
            Task.Run(async () =>
            {
                await Task.Delay(5000);
                if (del != null)
                {
                    Events.OnUserMessage -= del;
                    await ReplyAsync("Cancelled");
                }
            });
            Events.OnUserMessage += del;
        }
        [Command("deletebatch"), Summary("Delete a bunch of post contests, comma separated"), RequireOwner]
        public async Task DeleteBatch([Remainder] string contestName)
        {
            var data = Scope.GetGuildPersistentData();
            if (!Scope.GetGuildPersistentData().HasSetup)
            {
                await ReplyAsync("Please setup the bot first!");
                return;
            }

            var val = new List<string>();
            var spl = contestName.Split(",");
            foreach (var contest in spl)
            {
                var name = Clean(contest);
                if (data.ActiveChannels.ContainsKey(name))
                {
                    val.Add(name);
                }
            }

            if (val.Count == 0)
            {
                await ReplyAsync("No contests removed");
                return;
            }

            await ReplyAsync($"{MentionUtils.MentionUser(Context.User.Id)} Are you sure you want to **delete** `{string.Join(", ", val)}`? Say `yes` in 25 seconds to confirm!");

            EventService.UserMessageDelegate del = null;
            del = delegate(SocketCommandContext context)
            {
                Task.Run(async () =>
                {
                    if (context.User.Id == Context.User.Id)
                    {
                        Events.OnUserMessage -= del;
                        del = null;
                        if (context.Message.Content == "yes")
                        {
                            await ReplyAsync("Deleting Channels...");
                            foreach (var name in val)
                            {
                                try
                                {
                                    var k = data.ActiveChannels[name];
                                    data.ActiveChannels.Remove(name);
                                    var channel = Scope.Context.Guild.GetChannel(k);
                                    await channel.DeleteAsync();
                                }
                                catch
                                {
                                
                                }

                                await Task.Delay(500);
                            }
                        }
                        else
                        {
                            await ReplyAsync("Cancelled");
                        }
                    }
                });
                return true;
            };
            Task.Run(async () =>
            {
                await Task.Delay(25000);
                if (del != null)
                {
                    Events.OnUserMessage -= del;
                    await ReplyAsync("Cancelled");
                }
            });
            Events.OnUserMessage += del;
        }
    }
}