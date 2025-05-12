using System.Text;
using DependencyModules.Runtime.Attributes;
using DependencyModules.Runtime.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.ObjectPool;

namespace SimpleRequest.Client.Impl;

[DependencyModule(GenerateFactories = true)]
public partial class SimpleRequestClientImpl : IServiceCollectionConfiguration {
    public void ConfigureServices(IServiceCollection services) {
        services.TryAddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();
        
        services.TryAddSingleton<ObjectPool<MemoryStream>>(provider => {
            var poolProvider = provider.GetRequiredService<ObjectPoolProvider>();

            return poolProvider.Create<MemoryStream>(new MemoryStreamPolicy());
        });
        
        services.TryAddSingleton<ObjectPool<StringBuilder>>(provider => {
            var poolProvider = provider.GetRequiredService<ObjectPoolProvider>();
            return poolProvider.Create(new StringBuilderPolicy());
        });
    }

    private class StringBuilderPolicy : PooledObjectPolicy<StringBuilder> {

        public override StringBuilder Create() {
            return new StringBuilder();
        }

        public override bool Return(StringBuilder obj) {
            obj.Clear();
            return true;
        }
    }

    private class MemoryStreamPolicy : PooledObjectPolicy<MemoryStream> {
        public override MemoryStream Create() {
            return new MemoryStream();
        }

        public override bool Return(MemoryStream obj) {
            obj.SetLength(0);
            return true;
        }
    }
}