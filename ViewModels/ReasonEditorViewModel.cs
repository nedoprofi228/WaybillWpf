using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using WaybillWpf.ViewModels.Common;

namespace WaybillWpf.ViewModels
{
    public class ReasonEditorViewModel : INotifyPropertyChanged
    {
        // Событие для закрытия окна из ViewModel (соблюдая MVVM)
        public Action<bool>? CloseAction { get; set; }

        private string _rejectionReason;
        private string _errorMessage;
        private bool _isLoading;

        public ReasonEditorViewModel()
        {
            RejectCommand = new RelayCommand(async (_) => await RejectAsync(), (_) => !IsLoading);
            CancelCommand = new RelayCommand((_) => Cancel());
        }

        // --- Свойства для привязки (Bindings) ---

        public string RejectionReason
        {
            get => _rejectionReason;
            set
            {
                _rejectionReason = value;
                OnPropertyChanged();
                // Очищаем ошибку, когда пользователь начинает печатать
                if (!string.IsNullOrEmpty(_errorMessage)) 
                    ErrorMessage = string.Empty;
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set 
            { 
                _isLoading = value; 
                OnPropertyChanged(); 
                // Обновляем состояние команд (блокируем кнопку во время загрузки)
                CommandManager.InvalidateRequerySuggested(); 
            }
        }

        // --- Команды ---

        public ICommand RejectCommand { get; }
        public ICommand CancelCommand { get; }

        // --- Логика ---

        private async Task RejectAsync()
        {
            // 1. Валидация
            if (string.IsNullOrWhiteSpace(RejectionReason))
            {
                ErrorMessage = "Пожалуйста, укажите причину отклонения.";
                return;
            }

            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                // 2. Имитация асинхронного запроса к API / Базе данных
                // Здесь должен быть вызов вашего сервиса, например:
                // await _waybillService.RejectWaybillAsync(CurrentWaybillId, RejectionReason);
                
                await Task.Delay(1500); // Имитация задержки сети

                // 3. Если всё ок — закрываем окно с результатом true (DialogResult = true)
                CloseAction?.Invoke(true);
            }
            catch (Exception ex)
            {
                // Обработка ошибок сервера
                ErrorMessage = $"Ошибка при сохранении: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void Cancel()
        {
            // Закрываем окно с результатом false
            CloseAction?.Invoke(false);
        }

        // --- Реализация INotifyPropertyChanged ---
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    
}