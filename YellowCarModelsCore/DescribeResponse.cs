using System;
using System.Collections.Generic;
using System.Linq;

namespace YellowCar.Models
{
    public class DescribeResponse
    {

        public Description description { get; set; }

    }

    public class Description
    {

        public string[] tags { get; set; }
        public Caption[] captions { get; set; }

    }

    public class Caption
    {

        public string text { get; set; }
        public double confidence { get; set; }

    }

}