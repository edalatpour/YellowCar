using System;
using System.Collections.Generic;
using System.Linq;

namespace YellowCar.Models
{
    public class YellowCarResponse
    {

        public YellowCarResponse()
        {
            IsYellowCar = false;
            IsYellowHummer = false;
            Points = 0;
        }

        public bool IsYellowCar { get; set; }

        public bool IsYellowHummer { get; set; }

        public int Points { get; set; }

    }

}