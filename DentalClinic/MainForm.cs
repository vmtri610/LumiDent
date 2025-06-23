using DevExpress.DentalClinic.Services;
using DevExpress.DentalClinic.ViewModel;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.Utils;
using DevExpress.Utils.MVVM;
using DevExpress.Utils.MVVM.Services;
using DevExpress.XtraBars.Docking2010.Customization;
using DevExpress.XtraBars.Docking2010.Views.WindowsUI;
using DevExpress.XtraBars.FluentDesignSystem;
using DevExpress.XtraBars.Navigation;
using DevExpress.XtraEditors;
using DevExpress.XtraSplashScreen;
using System;
using System.Windows.Forms;
using System.Threading.Tasks;
using DevExpress.DentalClinic.Model;
using DevExpress.ExpressApp.Security.ClientServer;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp;

namespace DevExpress.DentalClinic
{
    public partial class MainForm : FluentDesignForm
    {
        static MainForm()
        {
            MVVMContext.RegisterFlyoutDialogService();
            ServiceContainer.Default.RegisterService(new LoginService());
            ServiceContainer.Default.RegisterService(new PrintReportService());
        }

        public MainForm()
        {
            InitializeComponent();
            if (!mvvmContext.IsDesignMode)
            {
                InitializeNavigation();
                InitializeBindings();
            }
        }

        public MainForm(Task rescheduleAppointments) : this()
        {
            this.rescheduleAppointments = rescheduleAppointments;
        }

        protected override FormShowMode ShowMode
        {
            get { return FormShowMode.AfterInitialization; }
        }

        void InitializeNavigation()
        {
            var navigationService = NavigationService.Create(navigationFrame);
            mvvmContext.RegisterDefaultService(navigationService);

            var flyoutService = WindowedDocumentManagerService.CreateFlyoutFormService();
            flyoutService.FormStyle = (form) => {
                var flyout = form as FlyoutDialog;
                flyout.Properties.Style = FlyoutStyle.Popup;
            };
            mvvmContext.RegisterDefaultService("Flyout", flyoutService);
        }

        void InitializeBindings()
        {
            var fluentApi = mvvmContext.OfType<NavigationViewModel>();


            fluentApi.BindCommand(darkThemeBBI, x => x.ChangeTheme);
            fluentApi.BindCommand(loginButtonItem, x => x.OpenLoginView);
            fluentApi.SetBinding(usersViewItem, x => x.Visible, x => x.ShowUserCollectionView);

            fluentApi.WithEvent<ElementClickEventArgs>(accordionControl, nameof(accordionControl.ElementClick))
                .EventToCommand(x => x.NavigateTo, args => CreateNavigateArgs(args));

            fluentApi.SetBinding(breadCrumbEdit1, x => x.SelectedNode, x => x.SelectedNode);
            fluentApi.SetBinding(breadCrumbEdit1, x => x.EditValue, x => x.NavigationPath);
            fluentApi.SetBinding(pnlNavigationBar, x => x.Visible, x => x.NavigationBarVisible);

            var navigationContext = new NavigationContext(breadCrumbEdit1.Properties.Nodes);
            fluentApi.WithEvent(this, nameof(Load))
                .EventToCommand(x => x.Load, (EventArgs args) => navigationContext);

            fluentApi.WithEvent<BreadCrumbNodeClickEventArgs>(breadCrumbEdit1.Properties, nameof(breadCrumbEdit1.Properties.NodeClick))
                .EventToCommand(x => x.NavigateTo, args => CreateNavigateArgs(args));

            accordionControl.SelectedElement = patientsViewItem;
            fluentApi.SetTrigger(x => x.CurrentViewType, viewType => {
                if (viewType == nameof(View.PatientCollectionView))
                {
                    accordionControl.SelectedElement = patientsViewItem;
                }
            });
            fluentApi.SetTrigger(x => x.OverlayFormTrigger, showOverlay => {
                if (showOverlay)
                {
                    overlaySplashScreenHandle = SplashScreenManager.ShowOverlayForm(mainPanel, new OverlayWindowOptions()
                    {
                        Opacity = 1f,
                        BackColor = mainPanel.BackColor,
                        FadeIn = false,
                        FadeOut = true
                    });
                }
                else
                {
                    DisposeHelper.Dispose(ref overlaySplashScreenHandle);
                }
            });

        }

        IOverlaySplashScreenHandle overlaySplashScreenHandle;
        Task rescheduleAppointments;

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            SplashScreenManager.CloseForm(false);

            var loginService = ServiceContainer.Default.GetService<ILoginService>();
            bool isAuthenticated = false;

            // Lặp cho đến khi đăng nhập thành công hoặc người dùng đóng form
            while (!isAuthenticated)
            {
                // Hiển thị dialog đăng nhập, không truyền username mặc định
                isAuthenticated = loginService.Login("", true);

                if (!isAuthenticated)
                {
                    var result = XtraMessageBox.Show(
                        "Đăng nhập thất bại. Bạn có muốn thử lại không?",
                        "Lỗi đăng nhập",
                        MessageBoxButtons.RetryCancel,
                        MessageBoxIcon.Error
                    );
                    if (result != DialogResult.Retry)
                    {
                        Close();
                        return;
                    }
                }
            }

            // Đăng nhập thành công, tiếp tục khởi tạo bảo mật và các dịch vụ khác nếu cần
            // (Nếu LoginService đã set SecuritySystem thì không cần đoạn dưới)
            // rescheduleAppointments?.Wait();
        }

        NavigateArgs CreateNavigateArgs(EventArgs eventArgs)
        {
            string path;
            Action cancelAction;

            if (eventArgs is ElementClickEventArgs args1)
            {
                path = args1.Element.Tag as string;
                cancelAction = () => args1.Handled = true;
            }
            else
            {
                var args2 = eventArgs as BreadCrumbNodeClickEventArgs;
                path = args2.Node.Path;
                cancelAction = () => args2.Handled = true;
            }

            bool showOverlay = (path == nameof(View.AnalyticsView)) || (path == nameof(View.SchedulerView));
            return new NavigateArgs(path, cancelAction, showOverlay);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }

        private void navigationFrame_Click(object sender, EventArgs e)
        {

        }
    }
}
