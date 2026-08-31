namespace FunEvents.ConsoleApp.Reservation;

internal static class ReservationMenu
{
    public static async Task ShowAsync(
        ReservationApiClient apiClient,
        CancellationToken cancellationToken = default)
    {
        while (true)
        {
            Console.Clear();

            Console.WriteLine("=================================");
            Console.WriteLine("          Reservation");
            Console.WriteLine("=================================");
            Console.WriteLine();
            Console.WriteLine("1. Create");
            Console.WriteLine("2. Update");
            Console.WriteLine("3. Get All");
            Console.WriteLine("0. Back");
            Console.WriteLine();
            Console.Write("Select an option: ");

            var option = Console.ReadLine();

            switch (option)
            {
                case "1":
                    await CreateAsync(apiClient, cancellationToken);
                    break;

                case "2":
                    await UpdateAsync(apiClient, cancellationToken);
                    break;

                case "3":
                    await GetAllAsync(apiClient, cancellationToken);
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
    }

    private static async Task CreateAsync(
        ReservationApiClient apiClient,
        CancellationToken cancellationToken)
    {
        Console.Clear();

        Console.WriteLine("=================================");
        Console.WriteLine("       Create Reservation");
        Console.WriteLine("=================================");
        Console.WriteLine();

        Console.Write("Code: ");
        var code = Console.ReadLine() ?? string.Empty;

        Console.Write("Event Code: ");
        var eventCode = Console.ReadLine() ?? string.Empty;

        Console.Write("User Name: ");
        var userName = Console.ReadLine() ?? string.Empty;

        Console.Write("Quantity: ");
        if (!int.TryParse(Console.ReadLine(), out var quantity))
        {
            Console.WriteLine("Invalid quantity.");
            return;
        }

        Console.Write("Channel: ");
        if (!int.TryParse(Console.ReadLine(), out var channel))
        {
            Console.WriteLine("Invalid channel.");
            return;
        }

        var request = new CreateReservationRequest(
            code,
            eventCode,
            userName,
            quantity,
            channel);

        try
        {
            var result = await apiClient.CreateAsync(
                request,
                cancellationToken);

            Console.WriteLine();
            Console.WriteLine("Reservation created successfully.");
            Console.WriteLine();
            Console.WriteLine(result);
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine();
            Console.WriteLine("Error creating reservation.");
            Console.WriteLine(ex.Message);
        }
    }

    private static async Task UpdateAsync(
        ReservationApiClient apiClient,
        CancellationToken cancellationToken)
    {
        Console.Clear();

        Console.WriteLine("=================================");
        Console.WriteLine("       Update Reservation");
        Console.WriteLine("=================================");
        Console.WriteLine();

        Console.Write("Code: ");
        var code = Console.ReadLine() ?? string.Empty;

        Console.Write("Quantity: ");
        if (!int.TryParse(Console.ReadLine(), out var quantity))
        {
            Console.WriteLine("Invalid quantity.");
            return;
        }

        Console.Write("Status Code: ");
        if (!int.TryParse(Console.ReadLine(), out var statusCode))
        {
            Console.WriteLine("Invalid status code.");
            return;
        }

        Console.Write("Status Name: ");
        var statusName = Console.ReadLine() ?? string.Empty;

        var request = new UpdateReservationRequest(
            code,
            quantity,
            new ReservationStatusRequest(
                statusCode,
                statusName));

        try
        {
            var result = await apiClient.UpdateAsync(
                request,
                cancellationToken);

            Console.WriteLine();
            Console.WriteLine("Reservation updated successfully.");
            Console.WriteLine();
            Console.WriteLine(result);
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine();
            Console.WriteLine("Error updating reservation.");
            Console.WriteLine(ex.Message);
        }
    }

    private static async Task GetAllAsync(
        ReservationApiClient apiClient,
        CancellationToken cancellationToken)
    {
        Console.Clear();

        Console.WriteLine("=================================");
        Console.WriteLine("         Reservations");
        Console.WriteLine("=================================");
        Console.WriteLine();

        try
        {
            var result = await apiClient.GetAllAsync(cancellationToken);

            Console.WriteLine(result);
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine();
            Console.WriteLine("Error getting reservations.");
            Console.WriteLine(ex.Message);
        }
    }
}