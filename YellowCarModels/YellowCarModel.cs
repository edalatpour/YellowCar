using System;
using System.Collections.Generic;
using System.Linq;

namespace YellowCar.Models
{
    public class YellowCarModel
    {

        public YellowCarModel()
        {
            Who = "";
            When = "";
            Where = "";
            ImageFileName = "";
            IsYellowCar = false;
            IsYellowHummer = false;
        }

        public string Who { get; set; }

        public string When { get; set; }

        public string Where { get; set; }

        public string ImageFileName { get; set; }

        public bool IsYellowCar { get; set; }

        public bool IsYellowHummer { get; set; }

    }

}