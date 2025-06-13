using System;
using System.Linq;
using System.Collections.Generic;

namespace IQSoft.Hardware.Abstract
{

    public enum ReaderType
    {
        None,
        Laser,
        Rfid
    }

    public abstract class DeviceElement : IDisposable
    {
        protected DeviceElement(Device device)
        {
            Device = device;
        }

        public Device Device { get; protected internal set; }
        public bool Exists { get; protected internal set; }

        public virtual void Dispose()
        {
        }
    }

    public abstract class Reader : DeviceElement
    {
        public delegate void OnReadEventHandler(object sender, string code);

        protected Reader(Device device)
            : base(device)
        {
            IsTurnedOn = false;
            UseGlobalHandler = false;
            ConvertUpc12ToEan13 = true;
        }

        /// <summary>
        /// Указывает, какие обработчики должны вызываться при прочтении кода: ассоциированные с данным ридером (OnCodeRead) или глобальный (переданный в Device.Readers.PushHandler)
        /// </summary>
        public bool UseGlobalHandler { get; set; }

        public bool IsTurnedOn { get; protected set; }

        public abstract bool TurnOn();
        public abstract void TurnOff();

        public bool ConvertUpc12ToEan13 { get; set; }

        protected void RaiseOnCodeReadEvent(string code)
        {
            if (ConvertUpc12ToEan13) code = TryConvertUpc12ToEan13(code);
            OnReadEventHandler handler = UseGlobalHandler ? OnCodeRead_GlobalHandler : OnCodeRead;
            if (handler != null) handler(this, code);
        }

        private string TryConvertUpc12ToEan13(string code)
        {
            if (code.Length == 12)
                if (code.All(char.IsDigit))
                {
                    int cs_desired = 0;
                    for (int i = 0; i < 11; i++)
                    {
                        var d = int.Parse(code[i].ToString());
                        cs_desired += d;
                        if (i%2 == 0) cs_desired += d + d;
                    }
                    cs_desired = (10 - (cs_desired%10))%10;

                    int cs_actual = int.Parse(code[11].ToString());

                    if (cs_desired == cs_actual) code = "0" + code;
                }
            return code;
        }

        internal event OnReadEventHandler OnCodeRead_GlobalHandler;

        /// <summary>
        /// Событие "прочитан код". Поднимается при условии UseGlobalHandler == false (при UseGlobalHandler == true в прочтении кода вызывается глобальный обработчик (переданный в Device.Readers.PushHandler)
        /// </summary>
        public event OnReadEventHandler OnCodeRead;
    }

    /// <summary>
    /// Абстрактный RFID-ридер
    /// </summary>
    public abstract class RfidReader : Reader
    {
        protected RfidReader(Device device)
            : base(device)
        {
            CachingEnabled = true;
        }

        protected int _power = 50;

        /// <summary>
        /// Возвращает true, если идёт процесс обнаружения и чтения меток (ридер включен и курок зажат)
        /// </summary>
        public bool IsReading { get; protected set; }

        public int Power
        {
            get { return _power; }
            set { SetPower(value); }
        }

        /// <summary>
        /// Устанавливает мощность антенн: value = 0..100 (проценты от макс мощности)
        /// </summary>
        /// <param name="value"></param>
        protected abstract void SetPower(int value);

        /// <summary>
        /// Кэш считанных кодов. 
        /// </summary>
        public readonly List<string> CachedCodes = new List<string>();

        /// <summary>
        /// Определяет, кэшируются ли считываемые коды
        /// </summary>
        public bool CachingEnabled { get; set; }
    }

    public class AbsentRfidReader : RfidReader
    {
        public AbsentRfidReader(Device device) : base(device)
        {
            this.Exists = false;
        }

        public override bool TurnOn()
        {
            this.IsTurnedOn = true;
            return true;
        }

        public override void TurnOff()
        {
            this.IsTurnedOn = false;
        }

        protected override void SetPower(int value)
        {
            _power = value;
        }
    }

    /// <summary>
    /// Абстракция набора ридеров с возможностью централизованной обработки события "код прочитан"
    /// </summary>
    public abstract class Readers : DeviceElement
    {
        /// <summary>
        /// Сканер штрихкодов
        /// </summary>
        public Reader BarcodeReader;

        /// <summary>
        /// Сканер RFID-меток
        /// </summary>
        public RfidReader RfidReader;

        public List<Reader> AllReaders = new List<Reader>();

        protected Readers(Device device)
            : base(device)
        {
            CreateReaders();
            AllReaders.Add(BarcodeReader);
            AllReaders.Add(RfidReader);
            BarcodeReader.OnCodeRead_GlobalHandler += OnCodeReadGlobalHandler;
            RfidReader.OnCodeRead_GlobalHandler += OnCodeReadGlobalHandler;
        }

        /// <summary>
        /// Метод должен создавать экземпляры ридеров
        /// </summary>
        protected abstract void CreateReaders();

        /// <summary>
        /// Приватный обработчик событий всех ридеров, вызывающий пользовательский обработчик из вершины стека.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="s"></param>
        private void OnCodeReadGlobalHandler(object sender, string s)
        {
            if (_handlers.Count > 0)
            {
                _handlers.First().Invoke(this, s);
            }
        }

        /// <summary>
        /// стек пользовательских обработчиков
        /// </summary>
        private Stack<Reader.OnReadEventHandler> _handlers = new Stack<Reader.OnReadEventHandler>();

        /// <summary>
        /// Помещает в стек пользовательский обработчик
        /// </summary>
        /// <param name="handler"></param>
        public void PushGlobalHandler(Reader.OnReadEventHandler handler)
        {
            _handlers.Push(handler);
        }

        /// <summary>
        /// Извлекает из стека пользовательский обработчик
        /// </summary>
        /// <returns></returns>
        public Reader.OnReadEventHandler PopGlobalHandler()
        {
            return (_handlers.Count > 0) ? _handlers.Pop() : null;
        }

        /// <summary>
        /// Выключает все ридеры, а затем включает один указанный (если указан)
        /// </summary>
        /// <param name="whichReader"></param>
        public void ActivateOnly(ReaderType whichReader)
        {
            bool rfid = false;
            bool laser = false;

            switch (whichReader)
            {
                case ReaderType.None:
                    break;
                case ReaderType.Laser:
                    laser = true;
                    break;
                case ReaderType.Rfid:
                    rfid = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("whichReader");
            }

            foreach (var reader in AllReaders)
            {
                reader.TurnOff();
            }

            if (laser) this.BarcodeReader.TurnOn();
            if (rfid) this.RfidReader.TurnOn();

        }

        protected List<Reader> _wasActiveBeforePause = new List<Reader>();

        /// <summary>
        /// Временно выключает все ридеры, но запоминает, какие из них были включены. Включить заново можно методом Unpause
        /// </summary>
        public void Pause()
        {
            foreach (var reader in AllReaders)
            {
                if (reader.IsTurnedOn) _wasActiveBeforePause.Add(reader);
            }
            ActivateOnly(ReaderType.None);
        }

        /// <summary>
        /// Включает ридеры, которые были включены в момент вызова метода Pause
        /// </summary>
        public void Unpause()
        {
            foreach (var reader in _wasActiveBeforePause)
            {
                reader.TurnOn();
            }
            _wasActiveBeforePause.Clear();
        }

        public override void Dispose()
        {
            BarcodeReader.Dispose();
            RfidReader.Dispose();
            base.Dispose();
        }
    }


    /// <summary>
    /// Тип сообщения, выводимого пользователю
    /// </summary>
    public enum MessageKind
    {
        Ok,
        Info,
        Alarm,
        Error,
        Question
    }

    /// <summary>
    /// Абстракция зуммера
    /// </summary>
    public abstract class Beeper : DeviceElement
    {

        protected Beeper(Device device)
            : base(device)
        {
        }

        /// <summary>
        /// Издает писк заданной громкости, частоты и продолжительности
        /// </summary>
        /// <param name="volume">Громкость в процентах от максимальной</param>
        /// <param name="frequency">Частота в Гц</param>
        /// <param name="duration">Продолжительность звука в мс</param>
        public abstract void Beep(int volume, int frequency, int duration);

        /// <summary>
        /// Издает писк, соответствующий типу сообщения
        /// </summary>
        /// <param name="messageKind"></param>
        public void Beep(MessageKind messageKind)
        {
            switch (messageKind)
            {
                case MessageKind.Ok:
                    Beep(50, 1000, 100);
                    Beep(0, 1000, 100);
                    Beep(50, 1500, 200);
                    break;
                case MessageKind.Info:
                    Beep(50, 1000, 200);
                    break;
                case MessageKind.Alarm:
                    Beep(50, 1000, 300);
                    Beep(0, 1000, 100);
                    Beep(50, 2000, 300);
                    Beep(0, 1000, 100);
                    Beep(50, 3000, 400);
                    Beep(0, 1000, 100);
                    break;
                case MessageKind.Error:
                    Beep(50, 2000, 500);
                    break;
                case MessageKind.Question:
                    Beep(50, 1500, 100);
                    Beep(0, 1000, 100);
                    Beep(50, 1000, 200);
                    break;
                default:
                    Beep(50, 1000, 1000);
                    break;
            }
        }
    }


    /// <summary>
    /// Абстракция терминала
    /// </summary>
    public abstract class Device : IDisposable 
    {
        /*
        /// <summary>
        /// Зуммер терминала
        /// </summary>
        public Beeper Beeper;
        */

        /// <summary>
        /// Набор ридеров с возможностью централизованной обработки событий
        /// </summary>
        public Readers Readers;

        /// <summary>
        /// Возвращает заводской серийный номер устройства
        /// </summary>
        /// <returns></returns>
        public abstract string GetSerialNumber();

        /// <summary>
        /// Возвращает [почти]уникальный идентификатор устройства
        /// </summary>
        /// <returns></returns>
        public abstract string GetUid();

        public abstract string GetSignature(string parameter);

        /// <summary>
        /// Инициализирует оборудование
        /// </summary>
        public abstract void Start();

        /// <summary>
        /// Выключает оборудование (нужно при закрытии приложения)
        /// </summary>
        public abstract void Stop();

        public void Dispose()
        {
            Readers.Dispose();
        }
    }

}