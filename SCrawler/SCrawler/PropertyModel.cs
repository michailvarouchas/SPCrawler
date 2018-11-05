using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCrawler
{
    class PropertyModel
    {
        public string Id { get; set; }

        public string PropertyType { get; set; }

        public string Locationp1 { get; set; }
        public string Locationp2 { get; set; }
        public string Locationp3 { get; set; }
        public string Floor { get; set; }
        public string Description { get; set; }
        public int Year { get; set; }
        public int Bedroms { get; set; }
        public int Toilets { get; set; }
        public double SqMeteters { get; set; }
        public string Parking { get; set; }
        public string Fireplace { get; set; }
        public string AutonomousHeat { get; set; }
        public double Price { get; set; }
        public string Phone { get; set; }
        public double PricePerSqMeter { get; set; }

        public override string ToString()
        {
            return $"PropertyType {PropertyType}\n" +
                $"Location {Locationp1}\n" +
                $"Location2 {Locationp2}\n" +
                $"Description {Description}\n" +
                $"Year {Year}\n" +
                $"NumberOfBedroms {Bedroms}\n" +
                $"Toilets {Toilets}\n" +
                $"SqMeteters {SqMeteters}\n" +
                $"Parking {Parking}\n" +
                $"Fireplace {Fireplace}\n" +
                $"AutonomousHeat {AutonomousHeat}\n" +
                $"Price {Price}\n" +
                $"Phone {Phone}\n" +
                $"Price per sq meter {PricePerSqMeter}\n";

        }
    }
}
