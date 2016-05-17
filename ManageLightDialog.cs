using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace LightManagerBot
{
    [Serializable]
    [LuisModel("Replace with your ModelId", "Replace with your subscriptionKey")]
    public class ManageLightDialog : LuisDialog<object>
    {
        [LuisIntent("")]
        public async Task noIntent(IDialogContext context, LuisResult result)
        {
            string text = "Sorry, didn't understand.  Try again";

            await context.PostAsync(text);
            context.Wait(MessageReceived);
        }

        [LuisIntent("OnLight")]
        public async Task Onlight(IDialogContext context, LuisResult result)
        {
            string location;
            string color;

            if (TryFindType(result, out location, out color))
            {
                string message = $"Searching for {location} in {color}....";

                context.ConversationData.SetValue<QueryItem>("userQuery", new QueryItem() { location = location, color = color });

                await context.PostAsync(message);
                context.Wait(MessageReceived);
            }
            else if (location == null && color == null)
            {
                string message = $"I think you want to switch on, please try again.\n\n" +
                    "Try a phrase like \"Switch on light in the kitchen with red color\"";
                await context.PostAsync(message);
                context.Wait(MessageReceived);
            }
            else if (location == null)
            {
                string message = $"Ok, you want to light on with {color} color - But what location?";
                context.ConversationData.SetValue("color", color);
                await context.PostAsync(message);
                context.Wait(GetOtherField);
            }
            else
            {
                string message = $"Ok, you want to light on the light in the {location} - but what color ?";
                context.ConversationData.SetValue("location", location);
                await context.PostAsync(message);
                context.Wait(GetOtherField);

            }
        }

        public async Task GetOtherField(IDialogContext context, IAwaitable<Message> argument)
        {
            var incomingMessage = await argument;
            string location = null;
            string color = null;
            if (context.ConversationData.TryGetValue("location", out location))
                color = incomingMessage.Text;
            else if (context.ConversationData.TryGetValue("color", out color))
                location = incomingMessage.Text;

            string message = $"Switch on light in the {location} with {color} color ....";

            context.ConversationData.SetValue<QueryItem>("userQuery", new QueryItem() { location = location, color = color });

            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }

        private bool TryFindType(LuisResult result, out string location, out string color)
        {
            location = null;
            color = null;

            EntityRecommendation LocationEntity, ColorEntity;
            if (result.TryFindEntity("Location", out LocationEntity))
                location = LocationEntity.Entity;

            if (result.TryFindEntity("Color", out ColorEntity))
                color = ColorEntity.Entity;

            if (location != null && color != null)
                return true;

            return false;
        }
    }

    

   
}





