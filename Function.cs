using Google.Cloud.Functions.Framework;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using Twilio;
using Twilio.Rest.Conversations.V1.Conversation;

namespace TwilioConversationsAutocreateGCF
{
    public class Function : IHttpFunction
    {
        public async Task HandleAsync(HttpContext context)
        {
            var request = context.Request;
            var response = context.Response;

            if (request.Method != "POST")
            {
                response.StatusCode = StatusCodes.Status405MethodNotAllowed;
                await response.WriteAsync(string.Empty);
                return;
            }

            if (request.Form.TryGetValue("EventType", out var eventType))
            {
                if (eventType != "onConversationAdded")
                {
                    await BadRequest(response, "Expected onConversationAdded webhook");
                    return;
                }
            }
            else
            {
                await BadRequest(response, "Expected event type");
                return;
            }

            if (request.Form.TryGetValue("ConversationSid", out var conversationSid))
            {
                var accountSid = Environment.GetEnvironmentVariable("TWILIO_ACCOUNT_SID");
                var authToken = Environment.GetEnvironmentVariable("TWILIO_AUTH_TOKEN");

                TwilioClient.Init(accountSid, authToken);

                ParticipantResource.Create(
                    conversationSid,
                    messagingBindingAddress: "<Your phone number>",
                    messagingBindingProxyAddress: "<Your Twilio number>"
                );
            }
            else
            {
                await BadRequest(response, "Expected conversation SID");
                return;
            }

            await response.WriteAsync("OK");
        }

        static async Task BadRequest(HttpResponse response, string message)
        {
            response.StatusCode = StatusCodes.Status400BadRequest;
            await response.WriteAsync(message);
        }
    }
}
