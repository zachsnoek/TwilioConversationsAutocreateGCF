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

            if (!HttpMethods.IsPost(request.Method))
            {
                response.StatusCode = StatusCodes.Status405MethodNotAllowed;
                return;
            }

            var form = await request.ReadFormAsync();

            var eventType = form["EventType"];
            if (eventType != "onConversationAdded")
                return;

            var accountSid = Environment.GetEnvironmentVariable("TWILIO_ACCOUNT_SID");
            var authToken = Environment.GetEnvironmentVariable("TWILIO_AUTH_TOKEN");
            var conversationSid = form["ConversationSid"];

            TwilioClient.Init(accountSid, authToken);

            await ParticipantResource.CreateAsync(
                pathConversationSid: conversationSid,
                identity: "<Your identity>"
            );

            await MessageResource.CreateAsync(
                pathConversationSid: conversationSid,
                body: "You are being connected. Please give us a moment to review your message."
            );
        }
    }
}
