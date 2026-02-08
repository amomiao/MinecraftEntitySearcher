using System.ComponentModel;

namespace MoNbtSearcher.Wpf {
    public class ViewModelBase : INotifyPropertyChanged {
        // INotifyPropertyChanged 事件
        public event PropertyChangedEventHandler PropertyChanged;

        // 通用的属性更改通知方法
        protected bool SetProperty<T>(ref T field, T value, string propertyName = null) {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        // 通知属性更改的方法
        protected void OnPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
