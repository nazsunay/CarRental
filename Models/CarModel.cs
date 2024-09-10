namespace CarRental.Models
{
    public class CarModel
    {
    }
    public class Car
    {
        public int Id { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public decimal DailyRate { get; set; }
        public bool IsAvailable { get; set; }
        public string ImgUrl { get; set; }
    }
    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public int PhoneNumber { get; set; }
    }
    public class Rental
    {
        public int Id { get; set; }
        public int CarId { get; set; }
        public int CustomerId { get; set; }
        public DateTime RentalDate { get; set; }
        public DateTime ReturnDate { get; set; }
        public decimal TotalCost { get; set; }
    }

    public class CustomerLoginModel
    {
        public string Password { get; set; }
        public string Email { get; set; }
    }
}
