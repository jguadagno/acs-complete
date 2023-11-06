using ACSDemo.IdentityHelper;
using Azure.Communication.Sms;

var acsDemoInfo = new DemoAcsInfo(DemoAcsInfo.PhoneAcs);
var smsClient = new SmsClient(acsDemoInfo.ConnectionString);

var sendResult = await smsClient.SendAsync("+18662377109", "+16022936767", "Hello Attendees!");
if (sendResult is null)
{
    await Console.Error.WriteLineAsync("Failed to send message");
    return -1;
}

Console.WriteLine($"Message Id: {sendResult.Value.MessageId}, WasSuccessful:{sendResult.Value.Successful}");
return 0;