using DevExpress.DentalClinic.ViewModel;
using DevExpress.Utils.Extensions;
using DevExpress.Utils.MVVM.Services;
using DevExpress.XtraEditors;

namespace DevExpress.DentalClinic.View
{
    public partial class AnalyticsView : XtraUserControl
    {
        public AnalyticsView()
        {
            InitializeComponent();
            if (!mvvmContext.IsDesignMode)
            {
                InitializeNavigation();
            }
        }

        void InitializeNavigation()
        {
            var navigationService = DocumentManagerService.Create(tabPane);
            navigationService.UseDeferredLoading = Utils.DefaultBoolean.True;
            mvvmContext.RegisterService(navigationService);
            var fluentApi = mvvmContext.OfType<AnalyticsViewModel>();
            fluentApi.ViewModel.Load();

            // Ẩn tất cả các tab trừ Analytics (trang đầu tiên)
            if (tabPane != null && tabPane.Pages.Count > 1)
            {
                for (int i = tabPane.Pages.Count - 1; i >= 1; i--)
                {
                    tabPane.Pages.Remove(tabPane.Pages[i]);
                }
            }
        }
    }
}
