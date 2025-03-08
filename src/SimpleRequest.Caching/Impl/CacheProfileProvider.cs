using DependencyModules.Runtime.Attributes;

namespace SimpleRequest.Caching.Impl;

public interface ICacheProfileProvider {
    ICacheProfile GetProfile(string profileName);
}

[SingletonService]
public class CacheProfileProvider(IEnumerable<ICacheProfile> profiles) : ICacheProfileProvider {
    private Dictionary<string, ICacheProfile> _profiles = GenerateProfilesByName(profiles);

    private static Dictionary<string, ICacheProfile> GenerateProfilesByName(IEnumerable<ICacheProfile> cacheProfiles) {
        return new Dictionary<string, ICacheProfile>(
            cacheProfiles.Select(p => new KeyValuePair<string, ICacheProfile>(p.Name, p)));
    }

    public ICacheProfile GetProfile(string profileName) {
        if (_profiles.TryGetValue(profileName, out var profile)) {
            return profile;
        }

        throw new KeyNotFoundException($"CacheProfile {profileName} not found");
    }
}