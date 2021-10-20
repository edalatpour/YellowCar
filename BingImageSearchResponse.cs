using System;

namespace CustomVisionImageUploader
{
    public class BingImageSearchResponse
    {
		public BingImageSearchResponse()
		{
		}

		public Image[] value { get; set; }

	}

    public class Image
    {
        public Image() {}

		public string name { get; set; }
		public string contentUrl { get; set; }

	}

}
