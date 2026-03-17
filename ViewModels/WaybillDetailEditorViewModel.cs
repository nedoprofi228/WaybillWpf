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
        private WaybillDetails? _originalDetail;

        // --- ВРЕМЯ ---
        private DateTime _departureDateTime;
        public DateTime DepartureDateTime
        {
            get => _departureDateTime;
            set
            {
                _departureDateTime = DateTime.SpecifyKind(value, DateTimeKind.Utc);
                OnPropertyChanged();
                OnPropertyChanged(nameof(CalculatedTimeInWay));
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
                OnPropertyChanged(nameof(CalculatedTimeInWay));
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
            get => MathF.Round(_startFuel, 1);
            set
            {
                _startFuel = MathF.Round(value, 1);
                OnPropertyChanged();
            }
        }

        private float _endFuel;
        public float EndFuel
        {
            get => MathF.Round(_endFuel, 1);
            set
            {
                _endFuel = MathF.Round(value, 1);
                OnPropertyChanged();
            }
        }

        // --- ФАКТИЧЕСКИЙ РАСХОД (Ввод) ---
        private float _fuelConsumedInput;
        public float FuelConsumedInput
        {
            get => MathF.Round(_fuelConsumedInput, 1);
            set
            {
                _fuelConsumedInput = MathF.Round(value, 1);
                OnPropertyChanged();
            }
        }

        // --- ЗАПРАВКА (Ввод) ---
        private float _refueledAmount;
        public float RefueledAmount
        {
            get => MathF.Round(_refueledAmount, 1);
            set
            {
                _refueledAmount = MathF.Round(value, 1);
                OnPropertyChanged();
                OnPropertyChanged(nameof(TotalRefuelingCost));
            }
        }

        // Цена топлива на момент заправки (берем из машины)
        public decimal FuelPriceAtRefueling { get; private set; }

        public TimeSpan CalculatedTimeInWay => ArrivalDateTime >= DepartureDateTime ? ArrivalDateTime - DepartureDateTime : TimeSpan.Zero;
        public decimal TotalRefuelingCost => (decimal)_refueledAmount * FuelPriceAtRefueling;

        public float CalculatedDistance => EndMealing - StartMealing;

        // АВТО-НОРМА = Время * Расход
        public float CalculatedNormConsumption
        {
            get => CalculatedDistance > 0 ? MathF.Round(CalculatedDistance * _carFuelRate / 100, 1) : 0;
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
            DepartureDateTime = DateTime.Now; ;
        }

        public void Initialize(Waybill waybill, float carFuelRate, decimal fuelPrice)
        {
            _id = 0;
            _originalDetail = null;
            _waybill = waybill;
            _carFuelRate = carFuelRate;
            FuelPriceAtRefueling = fuelPrice;

            // По умолчанию Факт 0 или можно предложить (Start - End), если хотите
            FuelConsumedInput = 0;
            RefueledAmount = 0;

            OnPropertyChanged(string.Empty);
            OnPropertyChanged(nameof(CalculatedTimeInWay));
            OnPropertyChanged(nameof(TotalRefuelingCost));
        }

        public void Initialize(Waybill waybill, WaybillDetails waybillDetail)
        {
            _id = waybillDetail.Id;
            _originalDetail = waybillDetail;
            _waybill = waybill;
            _carFuelRate = waybill.Car!.FuelRate;
            FuelPriceAtRefueling = waybill.Car!.FuelType?.Price ?? 0m;

            DepartureDateTime = waybillDetail.DepartureDateTime;
            ArrivalDateTime = waybillDetail.ArrivalDateTime;
            StartMealing = waybillDetail.StartMealing;
            EndMealing = waybillDetail.EndMealing;
            StartFuel = waybillDetail.StartRemeaningFuel;
            EndFuel = waybillDetail.EndRemeaningFuel;
            RefueledAmount = waybillDetail.RefueledAmount;
            FuelConsumedInput = waybillDetail.FuelConsumed;

            OnPropertyChanged(nameof(CalculatedTimeInWay));
            OnPropertyChanged(nameof(TotalRefuelingCost));
        }

        private bool CanSave()
        {
            return ArrivalDateTime >= DepartureDateTime &&
                   CalculatedDistance >= 0 &&
                   FuelConsumedInput >= 0;
        }

        private void OnSave()
        {
            if (_originalDetail != null)
            {
                // Редактирование: обновляем существующий объект, чтобы EF Core не ругался на tracking
                _originalDetail.DepartureDateTime = DepartureDateTime;
                _originalDetail.ArrivalDateTime = ArrivalDateTime;
                _originalDetail.StartMealing = StartMealing;
                _originalDetail.EndMealing = EndMealing;
                _originalDetail.StartRemeaningFuel = StartFuel;
                _originalDetail.EndRemeaningFuel = EndFuel;
                _originalDetail.RefueledAmount = RefueledAmount;
                _originalDetail.FuelPriceAtRefueling = FuelPriceAtRefueling;
                _originalDetail.FuelConsumed = FuelConsumedInput;

                ResultDetail = _originalDetail;
            }
            else
            {
                // Создание нового
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

                    RefueledAmount = RefueledAmount,
                    FuelPriceAtRefueling = FuelPriceAtRefueling,

                    // Сохраняем введенный руками факт
                    FuelConsumed = FuelConsumedInput
                };
            }

            RequestClose?.Invoke(true);
        }
    }
}