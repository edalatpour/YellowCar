
namespace YellowCar.Models
{
    public class Image
    {
        /// <summary>
        /// The ID of the contact.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The Contact's full name.
        /// </summary>
        public string SubmittedByUserName { get; set; }

        public string FileName { get; set; }

        public byte[] ImageBytes { get; set; }

        /// <summary>
        /// The Contact's email address. 
        /// </summary>
        public bool IsCar { get; set; }
        public bool IsYellowCar { get; set; }
        public bool IsYellowHummer { get; set; }
    }
}