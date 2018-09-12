using System;
using System.Collections.Immutable;
using Traducir.Core.Helpers;

namespace Traducir.Core.Models.Services
{
    public class PushNotificationMessage
    {
        public PushNotificationMessage(string title, string url, string content, string topic = null, bool requireInteraction = false, ImmutableArray<Action> actions = default(ImmutableArray<Action>))
        {
            if (!title.HasValue() || !content.HasValue())
            {
                throw new ArgumentException("title and content need a value");
            }

            Title = title;
            Url = url;
            Topic = topic;
            Content = content;
            RequireInteraction = requireInteraction;
            Actions = actions.IsDefault ? ImmutableArray<Action>.Empty : actions;
        }

        public string Title { get; }

        public string Topic { get; }

        public string Url { get; }

        public string Content { get; }

        public bool RequireInteraction { get; }

        public ImmutableArray<Action> Actions { get; }

        public class Action
        {
            public Action(string title, string url)
            {
                Title = title;
                Url = url;
            }

            public string Title { get; }

            public string Url { get; }
        }
    }
}