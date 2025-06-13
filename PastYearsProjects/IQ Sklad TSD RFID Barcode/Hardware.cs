using System.Linq;
using System.Collections.Generic;
using System.Text;
using IQSoft.Hardware.Abstract;
using IQSoft.Hardware.Moto;
using Symbol.ResourceCoordination;

namespace IQSoft.Hardware
{
    public class MotoNoRfidReaders : Abstract.Readers
    {
        public MotoNoRfidReaders(Abstract.Device device)
            : base(device)
        {
        }

        protected override void CreateReaders()
        {
            BarcodeReader = new MotoBarcodeReader(this.Device);

            var tinfo = new TerminalInfo();
            bool IsRfidExists = (tinfo.ConfigData.UHF_RFID > (int) UHFRFIDTypes.NONE);
            if (IsRfidExists)
            {
                RfidReader = new MotoRfidReader(this.Device);
            }
            else
            {
                RfidReader = new Abstract.AbsentRfidReader(this.Device);
            }
        }
    }

    public class Device : Abstract.Device
    {
        public ALED Led;
        public MotoTrigger Trigger;
        public Device()
        {
            Readers = new MotoNoRfidReaders(this);
            Trigger = new MotoTrigger(this);
            Led = new ALED();
        }

        public override string GetSerialNumber()
        {
            var tinfo = new TerminalInfo();
            return tinfo.ESN;
        }

        public override string GetUid()
        {
            var tinfo = new TerminalInfo();
            var uid = new StringBuilder();
            if (tinfo.UniqueUnitID != null)
            {
                foreach (byte b in tinfo.UniqueUnitID)
                    uid.Append(b.ToString("X2"));
            }
            return uid.ToString();
        }

        public override string GetSignature(string parameter)
        {
            var i = GetUid().GetHashCode() + GetSerialNumber().GetHashCode() + parameter.GetHashCode();
            return i.ToString("X");
        }

        public override void Start()
        {
            Readers.BarcodeReader.TurnOff();
            Readers.RfidReader.TurnOff();
            Readers.BarcodeReader.UseGlobalHandler = true;
            Readers.RfidReader.UseGlobalHandler = true;
        }

        public override void Stop()
        {
            Readers.BarcodeReader.TurnOff();
            Readers.RfidReader.TurnOff();
            Dispose();
        }
    }
}
