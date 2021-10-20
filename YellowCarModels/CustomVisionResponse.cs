using System;
using System.Collections.Generic;
using System.Linq;

namespace YellowCar.Models
{
    public class CustomVisionResponse
    {

        public string Id { get; set; }
        public string Project { get; set; }
        public string Iteration { get; set; }
        public string Created { get; set; }

        public Prediction[] Predictions { get; set; }

    }

    public class Prediction
    {

        public string TagId { get; set; }
        public string Tag { get; set; }
        public float Probability { get; set; }

    }

}