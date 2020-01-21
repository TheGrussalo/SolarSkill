using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Newtonsoft.Json;
using System.Management.Automation;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace AlexaPowershell.Controllers
{
    public class AlexaController : ApiController
    {
        private const string ApplicationId = "amzn1.ask.skill.25ef031c-fb92-439d-ac26-82b8a61ccf14";

        [HttpPost, Route("api/alexa/solarskill")]
        public dynamic powershell(AlexaRequest alexaRequest)
        {
            //if (alexaRequest.Session.Application.ApplicationId != ApplicationId)
            //    throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest));
            //AlexaResponse response = null;

            AlexaResponse response = new AlexaResponse(alexaRequest.Request.Type, true);
            try
            {
                if (!string.IsNullOrWhiteSpace(alexaRequest.Request.Type))
                {
                    switch (alexaRequest.Request.Type)
                    {
                        case "LaunchRequest":
                            response = LaunchRequestHandler(alexaRequest);
                            break;
                        case "IntentRequest":
                            response = IntentRequestHandler(alexaRequest);
                            break;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return response;
        }

        private AlexaResponse LaunchRequestHandler(AlexaRequest request)
        {
            Task<int> task = SolarAsync();
            task.Wait();
            var power = task.Result;

            var response = new AlexaResponse(String.Format("Current Power is {0} watts", power), true);


            //var response = new AlexaResponse("The current power being produced is...");
            //response.Response.Card.Title = "Solar Skill";
            //response.Response.Card.Content = "";
            //response.Response.ShouldEndSession = false;

            return response;
        }
        private AlexaResponse IntentRequestHandler(AlexaRequest request)
        {
            AlexaResponse response = null;

            switch (request.Request.Intent.Name)
            {
                case "AMAZON.CancelIntent":
                    response = CancelOrStopIntentHandler(request);
                    break;
                case "AMAZON.StopIntent":
                    response = CancelOrStopIntentHandler(request);
                    break;
                case "AMAZON.HelpIntent":
                    response = HelpIntent(request);
                    break;
                case "DidNotUnderstand":
                    response = DidNotUnderstandIntent(request);
                    break;
                case "CurrentPowerIntent":
                    response = CurrentPowerIntentHandler(request);
                    break;
                default:
                    response = CancelOrStopIntentHandler(request);
                    break;
            }
            return response;
        }

        private AlexaResponse CancelOrStopIntentHandler(AlexaRequest request)
        {
            return new AlexaResponse("Goodbye.", true);
        }
        private AlexaResponse HelpIntent(AlexaRequest request)
        {
            var response = new AlexaResponse("Sorry, help is not available.  Goodbye", true);
            return response;
        }

        private AlexaResponse DidNotUnderstandIntent(AlexaRequest request)
        {
            var response = new AlexaResponse("I'm sorry, I did not understand what you said.  Goodbye", true);
            return response;
        }

        private AlexaResponse CurrentPowerIntentHandler(AlexaRequest request)
        {

            Task<int> task = SolarAsync();
            task.Wait();
            var power = task.Result;
  
            var response = new AlexaResponse(String.Format("Current Power is {0} watts", power), true);

            return response;
        }

        static async Task<int> SolarAsync()
        {
            var userlogin = Properties.Settings.Default.username;
            var userPassword = Properties.Settings.Default.password;

            var SunnyPortal = new Bomblix.SunnyPortal.Core.SunnyPortal();

            var result = await SunnyPortal.Connect(userlogin, userPassword);

            if (!SunnyPortal.IsConnected)
            {
                Console.WriteLine("Not authenticated");
            }
            else
            {
                var power = await SunnyPortal.GetCurrentPower();
                Console.WriteLine("Current power is " + power + " watts");
            }
            return 1;
        }
    }
}
