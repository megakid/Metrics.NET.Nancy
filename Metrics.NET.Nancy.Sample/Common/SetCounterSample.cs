
using System;

namespace Metrics.NET.Nancy.Sample.Common
{
    public class SetCounterSample
    {
        private readonly Counter _commandCounter = Metric.Counter("Command Counter", Unit.Custom("Commands"));

        public interface ICommand { }
        public class SendEmail : ICommand { }
        public class ShipProduct : ICommand { }
        public class BillCustomer : ICommand { }
        public class MakeInvoice : ICommand { }
        public class MarkAsPreffered : ICommand { }

        public void Process(ICommand command)
        {
            _commandCounter.Increment(command.GetType().Name);

            // do actual command processing
        }

        public static void RunSomeRequests()
        {
            for (int i = 0; i < 30; i++)
            {

                var commandIndex = new Random().Next() % 5;
                if (commandIndex == 0) new SetCounterSample().Process(new SendEmail());
                if (commandIndex == 1) new SetCounterSample().Process(new ShipProduct());
                if (commandIndex == 2) new SetCounterSample().Process(new BillCustomer());
                if (commandIndex == 3) new SetCounterSample().Process(new MakeInvoice());
                if (commandIndex == 4) new SetCounterSample().Process(new MarkAsPreffered());
            }
        }
    }
}
