namespace zamren_management_system.Areas.Common.Interfaces;

public interface ISmsSender
{
    /// <summary>
    ///     Sends an SMS message to the specified phone number.
    /// </summary>
    /// <param name="phoneNumber"> The phone number to send the message to.</param>
    /// <param name="message"> The message to send.</param>
    /// <param name="currentUserId"></param>
    /// <returns> A task that represents the asynchronous operation.</returns>
    Task SendSmsAsync(string phoneNumber, string message, string? currentUserId);

    /// <summary>
    ///     Sends an SMS message to multiple phone numbers.
    /// </summary>
    /// <param name="phoneNumbers"></param>
    /// <param name="message"></param>
    /// <param name="currentUserId"></param>
    /// <returns> A task that represents the asynchronous operation.</returns>
    Task SendSmsAsync(IEnumerable<string> phoneNumbers, string message, string? currentUserId);
}