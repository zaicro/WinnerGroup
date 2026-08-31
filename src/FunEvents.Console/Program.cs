using FunEvents.ConsoleApp.Reservation;

var httpClient = new HttpClient
{
    BaseAddress = new Uri("https://localhost:7023/")
};

var reservationApiClient = new ReservationApiClient(httpClient);

while (true)
{
    Console.Clear();

    Console.WriteLine("=================================");
    Console.WriteLine("          FunEvents");
    Console.WriteLine("=================================");
    Console.WriteLine();
    Console.WriteLine("1. User");
    Console.WriteLine("2. Reservation");
    Console.WriteLine("3. Event");
    Console.WriteLine("0. Exit");
    Console.WriteLine();
    Console.Write("Select an option: ");

    var option = Console.ReadLine();

    switch (option)
    {
        case "1":
            Console.WriteLine("User selected.");
            break;

        case "2":
            await ReservationMenu.ShowAsync(reservationApiClient);
            break;

        case "3":
            Console.WriteLine("Event selected.");
            break;

        case "0":
            return;

        default:
            Console.WriteLine("Invalid option.");
            break;
    }

    Console.WriteLine();
    Console.WriteLine("Press any key to continue...");
    Console.ReadKey();
}