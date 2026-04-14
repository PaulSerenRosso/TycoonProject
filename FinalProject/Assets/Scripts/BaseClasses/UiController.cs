namespace BaseClasses
{
    public class UiController<TViewModel> where TViewModel : ViewModel
    {
        protected TViewModel viewModel;
    }
}