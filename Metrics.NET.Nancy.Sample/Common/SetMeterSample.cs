
using System;

namespace Metrics.NET.Nancy.Sample.Common
{
    public class SetMeterSample
    {
        private readonly Meter _errorMeter = Metric.Meter("Errors", Unit.Errors);

        public interface ICommand { }
        public class SendEmail : ICommand { }
        public class ShipProduct : ICommand { }
        public class BillCustomer : ICommand { }
        public class MakeInvoice : ICommand { }
        public class MarkAsPreffered : ICommand { }

        public void Process(ICommand command)
        {
            try
            {
                ActualCommandProcessing(command);
            }
            catch
            {
                _errorMeter.Mark(command.GetType().Name);
            }
        }

        private void ActualCommandProcessing(ICommand command)
        {
            //throw new DivideByZeroException();
        }

        public static void RunSomeRequests()
        {
            for (int i = 0; i < 30; i++)
            {
                var commandIndex = new Random().Next() % 5;
                if (commandIndex == 0) new SetMeterSample().Process(new SendEmail());
                if (commandIndex == 1) new SetMeterSample().Process(new ShipProduct());
                if (commandIndex == 2) new SetMeterSample().Process(new BillCustomer());
                if (commandIndex == 3) new SetMeterSample().Process(new MakeInvoice());
                if (commandIndex == 4) new SetMeterSample().Process(new MarkAsPreffered());
            }
        }
    }
}
