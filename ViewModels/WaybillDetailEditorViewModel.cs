using System;
using System.Windows.Input;
using WaybillWpf.Domain.Entities;
using WaybillWpf.ViewModels.Base;
using WaybillWpf.ViewModels.Common;

namespace WaybillWpf.ViewModels
{
    public class WaybillDetailEditorViewModel : BaseViewModel
    {
        private int _id = 0;
        private float _carFuelRate;

        // --- ВРЕМЯ ---
        private DateTime _departureDateTime;
        public DateTime DepartureDateTime 
        {
            get => _departureDateTime;
            set 
            {
                _departureDateTime = DateTime.SpecifyKind(value, DateTimeKind.Utc);
                OnPropertyChanged();
            }
        }

        private DateTime _arrivalDateTime;
        public DateTime ArrivalDateTime
        {
            get => _arrivalDateTime;
            set
            {
                _arrivalDateTime = DateTime.SpecifyKind(value, DateTimeKind.Utc);
                OnPropertyChanged();
            }
        }

        // --- СПИДОМЕТР (Ввод) ---
        private float _startMealing;
        public float StartMealing
        {
            get => _startMealing;
            set
            {
                _startMealing = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CalculatedDistance));
                OnPropertyChanged(nameof(CalculatedNormConsumption));
            }
        }

        private float _endMealing;
        public float EndMealing
        {
            get => _endMealing;
            set 
            { 
                _endMealing = value;
                OnPropertyChanged(); 
                OnPropertyChanged(nameof(CalculatedDistance)); 
                OnPropertyChanged(nameof(CalculatedNormConsumption)); 
            }
        }

        // --- ОСТАТКИ ТОПЛИВА (Ввод) ---
        private float _startFuel;
        public float StartFuel
        {
            get => _startFuel;
            set { _startFuel = value; OnPropertyChanged(); }
        }

        private float _endFuel;
        public float EndFuel
        {
            get => _endFuel;
            set { _endFuel = value; OnPropertyChanged(); }
        }

        // --- ФАКТИЧЕСКИЙ РАСХОД (Ввод) ---
        private double _fuelConsumedInput;
        public double FuelConsumedInput
        {
            get => _fuelConsumedInput;
            set 
            { 
                _fuelConsumedInput = value; 
                OnPropertyChanged(); 
            }
        }
        public float CalculatedDistance => EndMealing - StartMealing;

        // АВТО-НОРМА = Время * Расход
        public double CalculatedNormConsumption 
        {
            get => CalculatedDistance * _carFuelRate / 100;
        }

        // --- КОМАНДЫ ---
        public event Action<bool>? RequestClose;
        public ICommand SaveCommand { get; }
        public WaybillDetails? ResultDetail { get; private set; }
        private Waybill _waybill;

        public WaybillDetailEditorViewModel()
        {
            SaveCommand = new RelayCommand(_ => OnSave(), _ => CanSave());
            ArrivalDateTime = DateTime.Now.AddHours(3);
            DepartureDateTime = DateTime.Now;;
        }

        public void Initialize(Waybill waybill, float carFuelRate)
        {
            _waybill = waybill;
            _carFuelRate = carFuelRate;
            
            // По умолчанию Факт 0 или можно предложить (Start - End), если хотите
            FuelConsumedInput = 0; 

            OnPropertyChanged(string.Empty); 
        }
        
        public void Initialize(Waybill waybill, WaybillDetails waybillDetail)
        { 
            _waybill = waybill;
            _carFuelRate = waybill.Car!.FuelRate;
            DepartureDateTime = waybillDetail.DepartureDateTime;
            ArrivalDateTime = waybillDetail.ArrivalDateTime;
            StartMealing = waybillDetail.StartMealing;
            EndMealing = waybillDetail.EndMealing;
            StartFuel = waybillDetail.StartRemeaningFuel;
            EndFuel = waybillDetail.EndRemeaningFuel;
            
        }

        private bool CanSave()
        {
            return ArrivalDateTime >= DepartureDateTime && 
                   CalculatedDistance >= 0 &&
                   FuelConsumedInput >= 0;
        }

        private void OnSave()
        {
            ResultDetail = new WaybillDetails
            {
                Id = _id,
                Waybill = _waybill,
                CreatedAt = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc),
                
                DepartureDateTime = DepartureDateTime,
                ArrivalDateTime = ArrivalDateTime,
                
                StartMealing = StartMealing,
                EndMealing = EndMealing,
                
                StartRemeaningFuel = StartFuel,
                EndRemeaningFuel = EndFuel,
                
                // Сохраняем введенный руками факт
                FuelConsumed = (float)FuelConsumedInput
            };

            RequestClose?.Invoke(true);
        }
    }
}