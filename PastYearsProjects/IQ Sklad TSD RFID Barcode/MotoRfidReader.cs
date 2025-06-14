using System;
using IQSoft.Hardware.Abstract;
using Symbol.RFID3;

namespace IQSoft.Hardware.Moto
{
    public class MotoRfidReader : Abstract.RfidReader
    {
        protected RFIDReader RfidDevice;
        
        public MotoRfidReader(Abstract.Device device) : base(device)
        {
            Exists = true;
            RfidDevice = new RFIDReader("localhost", 5084, 2751);
            Initialize();
        }

        private TriggerInfo _triggerInfo;
        private ushort[] _antennas;

        private void Initialize()
        {
            _triggerInfo = new TriggerInfo();
            _triggerInfo.EnableTagEventReport = true;
            _triggerInfo.TagEventReportInfo.ReportNewTagEvent = TAG_EVENT_REPORT_TRIGGER.MODERATED;
            _triggerInfo.TagEventReportInfo.ReportTagInvisibleEvent = TAG_EVENT_REPORT_TRIGGER.MODERATED;
            _triggerInfo.TagEventReportInfo.ReportTagBackToVisibilityEvent = TAG_EVENT_REPORT_TRIGGER.MODERATED;
            _triggerInfo.TagEventReportInfo.NewTagEventModeratedTimeoutMilliseconds = 100;
            _triggerInfo.TagEventReportInfo.TagInvisibleEventModeratedTimeoutMilliseconds = 1000;
            _triggerInfo.TagEventReportInfo.TagBackToVisibilityModeratedTimeoutMilliseconds = 100;
            _triggerInfo.StartTrigger.Type = START_TRIGGER_TYPE.START_TRIGGER_TYPE_HANDHELD;
            _triggerInfo.StartTrigger.Handheld.HandheldEvent = HANDHELD_TRIGGER_EVENT_TYPE.HANDHELD_TRIGGER_PRESSED;
            _triggerInfo.StopTrigger.Type = STOP_TRIGGER_TYPE.STOP_TRIGGER_TYPE_HANDHELD_WITH_TIMEOUT;
            _triggerInfo.StopTrigger.Handheld.HandheldEvent = HANDHELD_TRIGGER_EVENT_TYPE.HANDHELD_TRIGGER_RELEASED;
            _triggerInfo.StopTrigger.Handheld.Timeout = 0;


            try
            {
                RfidDevice.Connect();
            }
            catch (Exception)
            {
                throw new Exception("RFID-ридер недоступен. Работа приложения невозможна.");
            }
            

            //RfidDevice.Events.NotifyHandheldTriggerEvent = true;
            RfidDevice.Events.AttachTagDataWithReadEvent = true;
            RfidDevice.Events.NotifyAntennaEvent = true;
            RfidDevice.Events.NotifyInventoryStartEvent = true;
            RfidDevice.Events.NotifyInventoryStopEvent = true;
            
            RfidDevice.Events.StatusNotify += EventsOnStatusNotify;
            RfidDevice.Events.ReadNotify += EventsOnReadNotify;

            _antennas = RfidDevice.Config.Antennas.AvailableAntennas;

        }

        private void EventsOnReadNotify(object sender, Events.ReadEventArgs e)
        {
            if (e.ReadEventData.TagData.TagEvent == TAG_EVENT.NEW_TAG_VISIBLE)
            {
                var code = e.ReadEventData.TagData.TagID;    
                if (!CachingEnabled || !CachedCodes.Contains(code))
                {
                    CachedCodes.Add(code);
                    RaiseOnCodeReadEvent(code);
                }
            }
        }

        private void EventsOnStatusNotify(object sender, Events.StatusEventArgs e)
        {
            if (e.StatusEventData.StatusEventType == Events.STATUS_EVENT_TYPE.INVENTORY_START_EVENT)
            {
                IsReading = true;
                ((Device) Device).Led.TurnOn();
            }
            else if (e.StatusEventData.StatusEventType == Events.STATUS_EVENT_TYPE.INVENTORY_STOP_EVENT)
            {
                IsReading = false;
                ((Device)Device).Led.TurnOff();
            }
        }

        public override bool TurnOn()
        {
            try
            {
                //RfidDevice.Config.RadioPowerState = RADIO_POWER_STATE.ON;
                SetPower(_power);
                RfidDevice.Actions.PurgeTags();
                RfidDevice.Actions.Inventory.Perform(null, _triggerInfo, null);
                IsTurnedOn = true;
            }
            catch
            {
                IsTurnedOn = false;
            }
            return IsTurnedOn;
        }

        public override void TurnOff()
        {
            IsTurnedOn = false;
            //try
            {
                RfidDevice.Actions.Inventory.Stop();
                RfidDevice.Actions.PurgeTags();
                //RfidDevice.Config.RadioPowerState = RADIO_POWER_STATE.OFF;
                
            } //catch {}
        }


        protected override void SetPower(int value)
        {
            if (value > 100) value = 100; else if (value < 0) value = 0;

            _power = value;
            if (RfidDevice.IsConnected)
            {
                foreach (var antennaId in _antennas)
                {
                    var antenna = RfidDevice.Config.Antennas[antennaId];
                    var cfg = antenna.GetConfig();
                    cfg.ReceiveSensitivityIndex = 0;
                    cfg.TransmitPowerIndex = (ushort)(Math.Round(_power * 2.54));
                    antenna.SetConfig(cfg);
                }
            }
        }

        public override void Dispose()
        {
            try
            {
                TurnOff();
            }
            catch {}
            
            try
            {
                
                if (RfidDevice.IsConnected) RfidDevice.Disconnect();
            }
            catch {}
            base.Dispose();
        }
    }
}