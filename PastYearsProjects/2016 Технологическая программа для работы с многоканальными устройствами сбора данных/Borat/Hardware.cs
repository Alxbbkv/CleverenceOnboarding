using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Borat
{

    public class BoratOneChanDevice : Device
    {
        public BoratOneChanDevice()
        {
            _hardwareSamplingInterval = 33;
            Interval = 33;
            SgrLevelTracker.TonicSgrChannelIndex = 0;
        }

        public int Bias { get; set; } = 200;
        public byte Channel { get; set; } = 0;

        protected override void TheWork()
        {
            { PutBytes(0xC4, Channel, Bias/256, Bias & 0xFF); }
        }

        private void CmdVersion()
        {
            PutBytes(0xC2);
        }

        private void CmdStop()
        {
            PutBytes(0xC0);
        }

        private void CmdReset()
        {
            PutBytes(0xC0);
        }

        private void CmdAdjust()
        {
            PutBytes(0xC3);
        }

        private void CmdStart()
        {
            PutBytes(0xC0);
        }

        private void PutBytes(params int[] bytes)
        {
            var len = bytes.Length;
            if (len > 0)
            {
                var bts = new byte[len];
                for (int i = 0; i < len; i++)
                {
                    bts[i] = (byte)bytes[i];
                }
                try
                {
                    _port.Write(bts, 0, len);
                }
                catch (InvalidOperationException e)
                {
                    PortCrashProcedure();
                }

            }
        }

        private const int HardwareDatapackLength = 3;
        private Thread readThread;

        public override void Start()
        {
            // base.Start();
            // return;

            if (readThread != null)
            {
                IsStarted = false;
                readThread.Join();
            }

            var deviceFound = TryFindDevice();
            if (deviceFound)
            {
                if (_port.IsOpen) CmdStart();
                base.Start();
                readThread = new Thread(ReadThreadBody);
                readThread.Start();
            }
        }

        private Queue<byte> inBuffer = new Queue<byte>();

        public float AdjustingRatio { get; private set; }

        private void ReadThreadBody()
        {
            while (IsStarted)
            {
                //while (_port.BytesToRead > 0)
                {
                    try
                    {
                        var b = _port.ReadByte();
                        if (b >= 0) inBuffer.Enqueue((byte)b);
                    }
                    catch (TimeoutException e)
                    {
                    }
                    catch (InvalidOperationException e)
                    {
                        PortCrashProcedure();
                    }
                    catch (IOException e)
                    {
                        PortCrashProcedure();
                    }
                }

                if (inBuffer.Count >= HardwareDatapackLength) CheckInBufferForDatapack();
            }
        }

        private void CheckInBufferForDatapack()
        {

            if (inBuffer.Count < HardwareDatapackLength) return;

            
                if ((inBuffer.ElementAt(0)) != Channel)
                {
                    inBuffer.Dequeue();
                    CheckInBufferForDatapack();
                    return;
                }
           


                var chn = inBuffer.Dequeue();

                var hi = inBuffer.Dequeue();

                var lo = inBuffer.Dequeue();

                var adc = (int)(((hi * 256 + lo)/2.48));

              

            TryRaiseOnDatapack(new DatapackEventArgs { Datapack = new int[]{ adc } });


        }

        public override void Stop()
        {
            try
            {
                if (_port != null) if (_port.IsOpen) CmdStop();
            }
            catch (Exception e)
            {
            }

            base.Stop();
            if (readThread != null) readThread.Abort();
            
        }

        /// <summary>
        /// Когда пропадает связь
        /// </summary>
        private void PortCrashProcedure()
        {
            _port = null;
            IsStarted = false;
            base.Stop();
            TryRaiseOnError();
        }

        protected override bool _tryFindDevice()
        {
            var _found = false;
            var portnames = (_port != null && !string.IsNullOrEmpty(_port.PortName)) ? new List<string> { _port.PortName } : new List<string>();
            portnames.AddRange(SerialPort.GetPortNames());
            foreach (var portname in portnames)
            {
                if (RecreatePort(portname))
                {
                    try
                    {
                        CmdReset();
                        CmdReset();
                        _port.DiscardInBuffer();
                        CmdVersion();
                        Thread.Sleep(100);
                        if (_port.BytesToRead > 0)
                        {
                            var str = _port.ReadExisting();
                            if (str.ToLower().Contains("polygraph")) _found = true;
                        }
                        _port.DiscardInBuffer();
                    }
                    catch (System.Exception e)
                    {
                    }
                }
                if (_found)
                {
                    _port.DiscardInBuffer();

                    AdjustingRatio = 20;

                    _port.ReceivedBytesThreshold = HardwareDatapackLength;
                    //_port.DataReceived += port_DataReceived;
                    break;
                }
            }

            if (!_found && (_port != null))
            {
                _port.Close();
                _port.Dispose();
                _port = null;
            }

            return _found;
        }
    }

    public class BoratDevice : Device
    {
        public enum VendorType {Aqiqat, Triumph};

        public VendorType Vendor = VendorType.Triumph;

        public BoratDevice()
        {
            _hardwareSamplingInterval = 30;
            Interval = 5678;
            SgrLevelTracker.TonicSgrChannelIndex = 3;
        }

        protected override void TheWork()
        {
            { }
        }

        private void CmdVersion()
        {
            PutBytes(0xC2);
        }

        private void CmdStop()
        {
            PutBytes(0xC0);
        }

        private void CmdReset()
        {
            PutBytes(0xC0);
        }

        private void CmdAdjust()
        {
            PutBytes(0xC3);
        }

        private void CmdStart()
        {
            PutBytes(0xC1);
        }

        private void PutBytes(params int[] bytes)
        {
            var len = bytes.Length;
            if (len > 0)
            {
                var bts = new byte[len];
                for (int i = 0; i < len; i++)
                {
                    bts[i] = (byte)bytes[i];
                }
                try
                {
                    _port.Write(bts, 0, len);
                }
                catch (InvalidOperationException e)
                {
                    PortCrashProcedure();
                }

            }
        }

        private int HardwareDatapackLength = 35;
        private Thread readThread;

        public override void Start()
        {
            // base.Start();
            // return;

            if (readThread != null)
            {
                IsStarted = false;
                readThread.Join();
            }

            var deviceFound = TryFindDevice();
            if (deviceFound)
            {
                if (_port.IsOpen) CmdStart();
                base.Start();
                readThread = new Thread(ReadThreadBody);
                readThread.Start();
            }
        }

        private Queue<byte> inBuffer = new Queue<byte>();

        public float AdjustingRatio { get; private set; } = 1;

        private void ReadThreadBody()
        {
            while (IsStarted)
            {
                //while (_port.BytesToRead > 0)
                {
                    try
                    {
                        var b = _port.ReadByte();
                        if (b >= 0) inBuffer.Enqueue((byte)b);
                    }
                    catch (TimeoutException e)
                    {
                    }
                    catch (InvalidOperationException e)
                    {
                        PortCrashProcedure();
                    }
                    catch (IOException e)
                    {
                        PortCrashProcedure();
                    }
                }

                if (inBuffer.Count >= HardwareDatapackLength) CheckInBufferForDatapack();
            }
        }

        private void CheckInBufferForDatapack()
        {

            if (inBuffer.Count < HardwareDatapackLength) return;

            for (int i = 0; i < 7; i++)
            {

                if ((inBuffer.ElementAt(i * 4 + 7) & 0xF0) != i*16)
                {
                    inBuffer.Dequeue();
                    CheckInBufferForDatapack();
                    return;
                }
            }

            var koeff = (Vendor == VendorType.Aqiqat) ? AdjustingRatio : AdjustingRatio + 6.28;
            var hcc = (Vendor == VendorType.Aqiqat) ? 10 : 7;

            var dp = new int[hcc];

            inBuffer.Dequeue();
            inBuffer.Dequeue();
            inBuffer.Dequeue();

            var timestamp = inBuffer.Dequeue() + 0x100 * inBuffer.Dequeue() + 0x10000 * inBuffer.Dequeue() + 0x1000000 * inBuffer.Dequeue();

            if (DatapacksCounter % 100 == 0)
            {
          //      System.Diagnostics.Debug.WriteLine(string.Format("{1}", DateTime.Now.ToLongTimeString(), timestamp));
                System.Diagnostics.Debug.WriteLine(string.Format("{0}", timestamp));

            }


            for (var i = 0; i < hcc; i++)
            {


                var dac_hi = inBuffer.Dequeue();

                var dac_lo = inBuffer.Dequeue();

                var hi = inBuffer.Dequeue();

                var lo = inBuffer.Dequeue();

                //var chn = (byte)(dac_hi / 0x10);

                var bs = (dac_hi & 0x0F) * 0x100 + dac_lo;
                //Debug.Assert(chn != i, "что-то не так в port_DataReceived()");

                

                dp[i] = (int)(bs * koeff + (hi * 0x100 + lo));
                //dp[i] = bs;
            }


            TryRaiseOnDatapack(new DatapackEventArgs { Datapack = dp });


        }

        public override void Stop()
        {
            try
            {
                if (_port != null) if (_port.IsOpen) CmdStop();
            }
            catch (Exception e)
            {
            }

            base.Stop();
            if (readThread != null) readThread.Join();
        }

        /// <summary>
        /// Когда пропадает связь
        /// </summary>
        private void PortCrashProcedure()
        {
            _port = null;
            IsStarted = false;
            base.Stop();
            TryRaiseOnError();
        }

        protected override bool _tryFindDevice()
        {
            var _found = false;
            var portnames = (_port != null && !string.IsNullOrEmpty(_port.PortName)) ? new List<string> { _port.PortName } : new List<string>();
            portnames.AddRange(SerialPort.GetPortNames());
            foreach (var portname in portnames)
            {
                if (RecreatePort(portname))
                {
                    try
                    {
                        CmdReset();
                        CmdReset();
                        _port.DiscardInBuffer();
                        CmdVersion();
                        Thread.Sleep(100);
                        if (_port.BytesToRead > 0)
                        {
                            var str = _port.ReadExisting();
                            if (str.ToLower().Contains("polygraph"))
                            {
                                _found = true;
                                var prms = str.Split(@" ".ToCharArray());
                                this.AdjustingRatio = Convert.ToSingle(prms[4], new System.Globalization.CultureInfo("en-US"));
                            }
                        }
                        _port.DiscardInBuffer();
                    }
                    catch (System.Exception e)
                    {
                    }
                }
                if (_found)
                {
                    _port.DiscardInBuffer();

                    _port.ReceivedBytesThreshold = HardwareDatapackLength;
                    //_port.DataReceived += port_DataReceived;
                    break;
                }
            }

            if (!_found && (_port != null))
            {
                _port.Close();
                _port.Dispose();
                _port = null;
            }

            return _found;
        }
    }

    public class SgrCalcer
    {
        public enum SgrDeltaState
        {
            Red,
            Green
        }

        private Device _parent;

        private AmplitudeCalcer amplitudeCalcer;
        private Meaner amplitudeMeaner;
        private Medianer sgrDataMedianer;

        public int TonicSgrChannelIndex { get; set; }

        public int SgrDeltaValue { get; private set; }

        public int SgrMedValue { get; private set; }

        public SgrDeltaState State
        {
            get
            {
                return (Math.Abs(SgrDeltaValue) > Math.Abs(DeltaTreshold)) ? SgrDeltaState.Red : SgrDeltaState.Green;
            }
        }

        public SgrCalcer(Device parentDevice, int tonicSgrChannelIndex)
        {
            const int sgrAmplCalcInterval = 2500;
            DeltaTreshold = 5;
            _parent = parentDevice;

            amplitudeCalcer = new AmplitudeCalcer { Capacity = sgrAmplCalcInterval / _parent.HardwareSamplingInterval };
            amplitudeMeaner = new Meaner { Capacity = 3 };
            sgrDataMedianer = new Medianer { Capacity = 25 };

            TonicSgrChannelIndex = tonicSgrChannelIndex;
            _parent.OnDatapack += _parent_OnDatapack;
        }

        private SgrDeltaState _predState;

        public int DeltaTreshold { get; set; }

        /// <summary>
        /// Возникает, когда значение КС пересекает пороговый уровень
        /// </summary>
        public event EventHandler OnDeltaStateChanged;

        /// <summary>
        /// Возникает, когда значение КС изменяется
        /// </summary>
        public event EventHandler OnSgrValueChanged;

        void _parent_OnDatapack(object sender, DatapackEventArgs e)
        {
            if (_parent.DatapacksCounter % 15 == 0)
            {
                SgrDeltaValue = amplitudeMeaner.PutGet(amplitudeCalcer.PutGet(e.Datapack[TonicSgrChannelIndex]));
                if (State != _predState)
                {
                    _predState = State;
                    var h = OnDeltaStateChanged;
                    if (h != null) h(this, null);
                }

                var newMedValue = sgrDataMedianer.PutGet(e.Datapack[TonicSgrChannelIndex]);
                if (newMedValue != SgrMedValue)
                {
                    var h2 = OnSgrValueChanged;
                    if (h2 != null) h2(this, null);
                    SgrMedValue = newMedValue;
                }
            }
            else
            {
                amplitudeCalcer.Put(e.Datapack[TonicSgrChannelIndex]);
                sgrDataMedianer.Put(e.Datapack[TonicSgrChannelIndex]);
            }
        }
    }

    public class DatapackEventArgs : EventArgs
    {
        public int[] Datapack;
    }

    public abstract class Device : TimeredWorker, IDisposable
    {
        protected SerialPort _port;
        public event EventHandler OnDeviceNotFound;

        protected Device()
        {
            _port = new SerialPort();
            _hardwareSamplingInterval = 20;
            DatapacksCounter = 0;
            SgrLevelTracker = new SgrCalcer(this, 1);
        }

        public bool RecreatePort(string portName)
        {
            bool ok = true;
            if (_port != null)
            {
                if (_port.IsOpen) _port.Close();
                _port.Dispose();
            }

            _port = new SerialPort
            {
                BaudRate = 57600,
                DataBits = 8,
                Parity = Parity.None,
                StopBits = StopBits.One,
                ReceivedBytesThreshold = 20,
                PortName = portName,
                ReadTimeout = 60,
                WriteTimeout = 60,
                ReadBufferSize = 32000,
                WriteBufferSize = 4000
            };

            try
            {
                _port.Open();
            }
            catch (System.Exception e)
            {
                ok = false;
            }

            return ok;
        }

        protected int _hardwareSamplingInterval;

        /// <summary>
        /// Возвращает ПРИБЛИЗИТЕЛЬНОЕ кол-во миллисекунд между соседними датапаками
        /// </summary>
        public int HardwareSamplingInterval { get { return _hardwareSamplingInterval; } }

        public SgrCalcer SgrLevelTracker { get; private set; }

        protected abstract bool _tryFindDevice();

        public bool TryFindDevice()
        {
            var found = _tryFindDevice();
            if (!found) if (OnDeviceNotFound != null) OnDeviceNotFound(this, new EventArgs());
            return found;
        }

        public int DatapacksCounter { get; private set; }

        public event EventHandler<DatapackEventArgs> OnDatapack;

        public event EventHandler OnError;

        /// <summary>
        ///     Поднимает событие OnDatapack, если есть подписанные обработчики
        /// </summary>
        protected void TryRaiseOnDatapack(DatapackEventArgs e)
        {
            DatapacksCounter++;
            var handler = OnDatapack;
            if (handler != null) handler(this, e);
        }

        protected void TryRaiseOnError()
        {
            if (OnError != null) OnError(this, new EventArgs());
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            this.OnDatapack = null;
            Stop();
            Thread.Sleep(100);
            if (_port != null)
            {
                _port.Close();
                _port.Dispose();
            }
        }

        #endregion
    }



    public abstract class TimeredWorker
    {
        private Timer _timer;
        private bool _workStillActive = false;
        public bool RaiseOnTimerEvent { get; set; }
        public int Interval { get; set; }

        public bool IsStarted { get; protected set; }

        public event EventHandler OnTimer;
        public event EventHandler OnStart;
        public event EventHandler OnStop;

        protected abstract void TheWork();

        public virtual void Start()
        {
            CloseTimer();
            _timer = new Timer(OnTimerHandler, null, 100, Interval);
            IsStarted = true;
            if (IsStarted) if (OnStart != null) OnStart(this, new EventArgs());
        }

        public virtual void Start(int interval)
        {
            Interval = interval;
            Start();
        }

        public virtual void Stop()
        {
            CloseTimer();
            IsStarted = false;
            if (!IsStarted) if (OnStop != null) OnStop(this, new EventArgs());
        }

        private void CloseTimer()
        {
            if (_timer != null)
            {
                _timer.Change(Timeout.Infinite, Timeout.Infinite);
                _timer.Dispose();
                _timer = null;
            }
        }

        private void OnTimerHandler(object s)
        {
            if (!IsStarted) return;
            if (_timer == null) return;
            if (_workStillActive) return; //заменить эту пакость на монитор или мьютекс
            if (RaiseOnTimerEvent && (OnTimer != null)) OnTimer(this, new EventArgs());
            _workStillActive = true;
            TheWork();
            _workStillActive = false;
        }
    }

}
