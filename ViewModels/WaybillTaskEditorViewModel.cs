using System;
using System.Windows.Input;
using WaybillWpf.Domain.Entities;
using WaybillWpf.Domain.Interfaces;
using WaybillWpf.ViewModels.Base;
using WaybillWpf.ViewModels.Common;

namespace WaybillWpf.ViewModels
{
    public class WaybillTaskEditorViewModel : BaseViewModel
    {
        private readonly IWaybillsRepository _waybillsRepository;
        // Поля для биндинга
        public DateTime Date { get; set; } = DateTime.Now;
        public string DeparturePoint { get; set; } = string.Empty;
        public string ArrivalPoint { get; set; } = string.Empty;
        public float Mileage { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string OtherInfo { get; set; } = string.Empty;

        public event Action<bool>? RequestClose;
        public ICommand SaveCommand { get; }
        
        public WaybillTask? ResultTask { get; private set; }
        private int _waybillId;

        public WaybillTaskEditorViewModel(IWaybillsRepository waybillsRepository)
        {
            _waybillsRepository = waybillsRepository;
            SaveCommand = new RelayCommand(_ => OnSave(), _ => CanSave());
        }

        public void Initialize(int waybillId, WaybillTask? existingTask = null)
        {
            _waybillId = waybillId;
            
            if (existingTask != null)
            {
                Date = existingTask.Date;
                DeparturePoint = existingTask.DeparturePoint;
                ArrivalPoint = existingTask.ArrivalPoint;
                Mileage = existingTask.Mileage;
                CustomerName = existingTask.CustomerName;
                OtherInfo = existingTask.OtherInfo ?? "";
            }
            OnPropertyChanged(string.Empty);
        }

        private bool CanSave()
        {
            return !string.IsNullOrWhiteSpace(DeparturePoint) && 
                   !string.IsNullOrWhiteSpace(ArrivalPoint);
        }

        private async void OnSave()
        {
            ResultTask = new WaybillTask
            {
                Waybill = await _waybillsRepository.GetByIdAsync(_waybillId),
                Date = DateTime.SpecifyKind(Date, DateTimeKind.Utc),
                DeparturePoint = DeparturePoint,
                ArrivalPoint = ArrivalPoint,
                Mileage = Mileage,
                CustomerName = CustomerName,
                OtherInfo = OtherInfo
            };
            RequestClose?.Invoke(true);
        }
    }
}