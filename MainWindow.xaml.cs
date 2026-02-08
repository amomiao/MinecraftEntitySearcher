using MoNbt;
using MoNbtSearcher.Wpf;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MoNbtSearcher {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        int pageNum = 0;
        List<EntityPoiData> dpl = new List<EntityPoiData>();
        List<string> uuidList = new List<string>();

        public MainWindow() {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            // 设置初始值
            ThreadNumTextBox.Text = MoNbtSearcherHelper.ConfigData.ThreadNum.ToString();
            OwnerStateComboBox.SelectedIndex = 0;
            SetUserComboBox();
        }

        private void EnterKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                Keyboard.ClearFocus();
            }
        }

        #region MainFunction
        private void ThreadNumTextBox_LostFocus(object sender, RoutedEventArgs e) {
            // 获取文本框内容
            string inputText = ThreadNumTextBox.Text;
            // 假设我们要检查输入的内容是否是一个有效的数字，如果不是则回写一个默认值
            if (int.TryParse(inputText, out int result)) {
                MoNbtSearcherHelper.ChangeThreadNum(result);
            }
            ThreadNumTextBox.Text = MoNbtSearcherHelper.ConfigData.ThreadNum.ToString();
        }

        private void ChooseLevelButton_Click(object sender, RoutedEventArgs e) {
            var viewModel = (WindowViewModel)DataContext;
            if (!viewModel.TryChoosingLevel()) {
                return;
            }
            SetUserComboBox();
            // 允许读取.mca
            MCAButton.IsEnabled = true;
            // 设置选框
            DimComboBox.Items.Clear();
            DimComboBox.Items.Add(new ComboBoxItem() {
                Content = "ALL",
                HorizontalContentAlignment = HorizontalAlignment.Right,
                VerticalContentAlignment = VerticalAlignment.Center,
            });
            foreach (var item in viewModel.DimNames) {
                DimComboBox.Items.Add(new ComboBoxItem() {
                    Content = item,
                    HorizontalContentAlignment = HorizontalAlignment.Right,
                    VerticalContentAlignment = VerticalAlignment.Center,
                });
            }
            DimComboBox.SelectedIndex = 0;
        }

        private async void StartButton_Click(object sender, RoutedEventArgs e) {
            var viewModel = (WindowViewModel)DataContext;
            if (viewModel.DimPaths.Count == 0) {
                Logger.LogUserWarning("未选择存档路径!");
                return;
            }
            // 获取搜索维度
            int si = DimComboBox.SelectedIndex - 1; // 第一个索引是ALL
            List<string> paths = (si == -1) ? viewModel.DimPaths : new List<string> { viewModel.DimPaths[si] };
            if (paths.Count > 1 &&
                MessageBoxResult.No == MessageBox.Show($"可以分维度加载,确定要加载全部吗?", "警告", MessageBoxButton.YesNo, MessageBoxImage.Question)) {
                return;
            }

            await viewModel.LoadDimFile(paths);

            // 不再允许读取.mca
            MCAButton.IsEnabled = false;
            // 设置选框
            DimComboBox.Items.Clear();
            DimComboBox.Items.Add(new ComboBoxItem() {
                Content = "ALL",
                HorizontalContentAlignment = HorizontalAlignment.Right,
                VerticalContentAlignment = VerticalAlignment.Center,
            });
            foreach (var item in viewModel.LoadedDimNames) {
                DimComboBox.Items.Add(new ComboBoxItem() {
                    Content = item,
                    HorizontalContentAlignment = HorizontalAlignment.Right,
                    VerticalContentAlignment = VerticalAlignment.Center,
                });
            }
            DimComboBox.SelectedIndex = 0;
        }

        private async void SearchButton_Click(object sender, RoutedEventArgs e) {
            var viewModel = (WindowViewModel)DataContext;
            if (viewModel.LoadedPathList.Count == 0) {
                Logger.LogUserWarning("未加载.mca文件");
                return;
            }
            // 获取搜索维度
            int si = DimComboBox.SelectedIndex - 1; // 第一个索引是ALL
            string[] paths = (si == -1) ? viewModel.LoadedPathList.ToArray() : new string[] { viewModel.LoadedPathList[si] };
            if (paths.Length > 1 && 
                MessageBoxResult.No == MessageBox.Show($"可以分维度加载,确定要加载全部吗?", "警告", MessageBoxButton.YesNo, MessageBoxImage.Question)) {
                return;
            }

            // 自定义过滤调节
            var kvList = GetFilters();
            // 准备执行
            if (kvList.Count == 0) {
                Logger.LogUserWarning("未设置过滤条件!");
                return;
            }
            // 执行前做
            await viewModel.SearchEnitiyNbt(kvList.ToArray(), paths);

            ChangeResultShowPanel(1);
        }
        #endregion MainFunction

        #region Filter
        void SetUserComboBox() {
            ConfigOwnerComboBox.Items.Clear();
            foreach (var pd in MoNbtSearcherHelper.ConfigData.playerDataList) {
                foreach (var uuid in pd.uuidEnList) {
                    uuidList.Add(uuid.ToString());
                    ConfigOwnerComboBox.Items.Add(new ComboBoxItem() {
                        Content = $"{pd.player_name}({uuid})"
                    });
                }
            }
        }

        // 自定义过滤调节
        List<TagKV> GetFilters() {
            TagKV kv;
            List<TagKV> kvList = new List<TagKV>();
            if (OwnerStateComboBox.SelectedIndex != 0) {
                kvList.Add(new TagKV("Owner", UUIDTextBox.Text));
            }
            kv = new TagKV(FilterNbtIDTextBox.Text, FilterNbtValueTextBox.Text);
            if (!string.IsNullOrEmpty(kv.key) && !string.IsNullOrEmpty(kv.value)) {
                kvList.Add(kv);
            }
            kv = new TagKV(FilterNbtIDTextBox1.Text, FilterNbtValueTextBox1.Text);
            if (!string.IsNullOrEmpty(kv.key) && !string.IsNullOrEmpty(kv.value)) {
                kvList.Add(kv);
            }
            kv = new TagKV(FilterNbtIDTextBox2.Text, FilterNbtValueTextBox2.Text);
            if (!string.IsNullOrEmpty(kv.key) && !string.IsNullOrEmpty(kv.value)) {
                kvList.Add(kv);
            }
            return kvList;
        }

        private void OwnerStateComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            int si = OwnerStateComboBox.SelectedIndex;
            // 确保SelectedIndex有效
            if (si == -1) 
                return;
            if (si == 0) {
                ConfigOwnerComboBox.SelectedIndex = -1;
                ConfigOwnerComboBox.IsEnabled = false;
                UUIDTextBox.Text = string.Empty;
                UUIDTextBox.IsReadOnly = true;
            }
            else if (si == 1) {
                if (uuidList.Count > 0) {
                    ConfigOwnerComboBox.SelectedIndex = 0;
                }
                ConfigOwnerComboBox.IsEnabled = true;
                UUIDTextBox.IsReadOnly = true;
            }
            else if (si == 2) {
                ConfigOwnerComboBox.SelectedIndex = -1;
                ConfigOwnerComboBox.IsEnabled = false;
                UUIDTextBox.IsReadOnly = false;
            }
        }

        private void ConfigOwnerComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            int si = ConfigOwnerComboBox.SelectedIndex;
            if (si >= 0 && si < uuidList.Count) {
                UUIDTextBox.Text = uuidList[si];
            }
        }

        #endregion Filter

        #region ShowData
        bool ChangeResultShowPanel(int pageUserNum) {
            const int PAGE_SIZE = 10;

            (int userNum, int index) page = new(pageUserNum, pageUserNum - 1);

            var viewModel = (WindowViewModel)DataContext;
            if (!viewModel.TryGetPageDatas(page, PAGE_SIZE, ref dpl)) {
                return false;
            }
            ResultShowPanel.Children.Clear();
            Button btn;
            for (int i = 0; i < dpl.Count; i++) {
                int j = page.index * PAGE_SIZE + i;
                btn = new Button {
                    Content = j.ToString(),
                };
                btn.Click += (s, e) => {
                    ShowPoiTextBox.Text = viewModel.ResultList[j].ToString();
                    ShowTagTextBox.Text = viewModel.ResultList[j].RawTag.ToString();
                };
                ResultShowPanel.Children.Add(btn);
            }
            PageNumTextBox.Text = viewModel.NowDataPageNum.ToString();
            return true;
        }

        private void PageNumTextBox_LostFocus(object sender, RoutedEventArgs e) {
            var viewModel = (WindowViewModel)DataContext;
            if (!(int.TryParse(PageNumTextBox.Text, out pageNum) && ChangeResultShowPanel(pageNum))) {
                PageNumTextBox.Text = viewModel.NowDataPageNum.ToString();
            }
        }
        #endregion ShowData
    }
}