using Prism.Mvvm;
using Prism.Navigation;

namespace HttpTracer.TestApp.ViewModels
{
    public class ViewModelBase : BindableBase, INavigationAware, IDestructible
    {
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        protected INavigationService NavigationService { get; }

        private string _title;
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public ViewModelBase(INavigationService navigationService)
        {
            NavigationService = navigationService;
        }

        public virtual void OnNavigatedFrom(INavigationParameters parameters)
        {
            
        }

        public virtual void OnNavigatedTo(INavigationParameters parameters)
        {

        }

        public void Destroy()
        {

        }

        public void OnNavigatingTo(INavigationParameters parameters)
        {

        }
    }
}
