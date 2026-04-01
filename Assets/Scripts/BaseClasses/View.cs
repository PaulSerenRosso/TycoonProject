using UnityEngine.UIElements;

namespace BaseClasses
{
    public class View<TViewModel> : VisualElement where TViewModel : ViewModel
    {
        protected ViewModel viewModel;
    }
}