﻿using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace LINGYUN.Abp.Notifications
{
    public class NotificationDefinitionManager : INotificationDefinitionManager, ISingletonDependency
    {
        protected Lazy<IDictionary<string, NotificationDefinition>> NotificationDefinitions { get; }

        protected AbpNotificationOptions Options { get; }

        protected IServiceProvider ServiceProvider { get; }

        public NotificationDefinitionManager(
            IOptions<AbpNotificationOptions> options,
            IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
            Options = options.Value;

            NotificationDefinitions = new Lazy<IDictionary<string, NotificationDefinition>>(CreateNotificationDefinitions, true);
        }

        public virtual NotificationDefinition Get([NotNull] string name)
        {
            Check.NotNull(name, nameof(name));

            var notification = GetOrNull(name);

            if (notification == null)
            {
                throw new AbpException("Undefined notification: " + name);
            }

            return notification;
        }

        public virtual IReadOnlyList<NotificationDefinition> GetAll()
        {
            return NotificationDefinitions.Value.Values.ToImmutableList();
        }

        public virtual NotificationDefinition GetOrNull(string name)
        {
            return NotificationDefinitions.Value.GetOrDefault(name);
        }

        protected virtual IDictionary<string, NotificationDefinition> CreateNotificationDefinitions()
        {
            var notifications = new Dictionary<string, NotificationDefinition>();

            using (var scope = ServiceProvider.CreateScope())
            {
                var providers = Options
                    .DefinitionProviders
                    .Select(p => scope.ServiceProvider.GetRequiredService(p) as INotificationDefinitionProvider)
                    .ToList();

                foreach (var provider in providers)
                {
                    provider.Define(new NotificationDefinitionContext(notifications));
                }
            }

            return notifications;
        }
    }
}
