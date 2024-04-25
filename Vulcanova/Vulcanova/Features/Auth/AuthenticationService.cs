using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Vulcanova.Core.Uonet;
using Vulcanova.Features.Auth.Accounts;
using Vulcanova.Uonet;
using Vulcanova.Uonet.Api.Auth;
using Xamarin.Essentials;

namespace Vulcanova.Features.Auth;

public class AuthenticationService : IAuthenticationService
{
    private readonly IApiClientFactory _apiClientFactory;
    private readonly IMapper _mapper;
    private readonly IAccountRepository _accountRepository;

    public AuthenticationService(
        IApiClientFactory apiClientFactory,
        IMapper mapper,
        IAccountRepository accountRepository)
    {
        _apiClientFactory = apiClientFactory;
        _mapper = mapper;
        _accountRepository = accountRepository;
    }

    public async Task<Account[]> AuthenticateAsync(string token, string pin, string instanceUrl)
    {
        var identity = await ClientIdentityProvider.CreateClientIdentityAsync();

        var x509Certificate2 = identity.Certificate;

        var device = $"Vulcanova – {DeviceInfo.Name}";

        var request = new RegisterClientRequest
        {
            OS = Constants.AppOs,
            DeviceModel = device,
            Certificate = Convert.ToBase64String(x509Certificate2.RawData),
            CertificateType = "X509",
            CertificateThumbprint = x509Certificate2.Thumbprint,
            PIN = pin,
            SecurityToken = token,
            SelfIdentifier = Guid.NewGuid().ToString()
        };

        var client = _apiClientFactory.GetAuthenticated(identity, instanceUrl);

        await client.PostAsync(RegisterClientRequest.ApiEndpoint, request);

        await ClientIdentityStore.SaveIdentityAsync(identity);

        var registerHebeResponse = await client.GetAsync(RegisterHebeClientQuery.ApiEndpoint, new RegisterHebeClientQuery());

        var accounts = registerHebeResponse.Envelope
            .Where(a => a.Login != null && a.Periods is { Length: > 0 })
            .Select(_mapper.Map<Account>)
            .ToArray();

        foreach (var account in accounts)
        {
            // in some rare cases, the data will contain duplicated periods
            account.Periods = account.Periods.GroupBy(p => p.Id).Select(g => g.First()).ToList();
            account.IdentityThumbprint = identity.Certificate.Thumbprint;
            account.Id = await _accountRepository.GetNextAccountIdAsync();
        }

        return accounts;
    }
}