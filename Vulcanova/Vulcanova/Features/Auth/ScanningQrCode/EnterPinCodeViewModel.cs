using System.Reactive;
using System.Threading.Tasks;
using Prism.Navigation;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Vulcanova.Core.Mvvm;

namespace Vulcanova.Features.Auth.ScanningQrCode
{
    public class EnterPinCodeViewModel : ViewModelBase, INavigatedAware
    {
        public ReactiveCommand<Unit, Unit> RegisterDevice { get; }

        [Reactive] public string Pin { get; set; }

        private string _instanceUrl;
        private string _token;
        
        private readonly IAuthenticationService _authenticationService;

        public EnterPinCodeViewModel(
            INavigationService navigationService,
            IAuthenticationService authenticationService) : base(navigationService)
        {
            _authenticationService = authenticationService;

            RegisterDevice = ReactiveCommand.CreateFromTask(_ => RegisterDeviceAsync(_token, Pin, _instanceUrl));
        }

        public void OnNavigatedFrom(INavigationParameters parameters)
        {
        }

        public void OnNavigatedTo(INavigationParameters parameters)
        {
            _instanceUrl = parameters.GetValue<string>("instanceUrl");
            _token = parameters.GetValue<string>("token");
        }

        private async Task<Unit> RegisterDeviceAsync(string token, string pin, string instanceUrl)
        {
            await _authenticationService.AuthenticateAsync(token, pin, instanceUrl);

            return Unit.Default;
        }
    }
}