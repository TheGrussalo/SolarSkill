using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Newtonsoft.Json;
using System.Management.Automation;
using System.Collections.ObjectModel;

namespace AlexaPowershell.Controllers
{
    public class AlexaController : ApiController
    {
        private const string ApplicationId = "amzn1.ask.skill.25ef031c-fb92-439d-ac26-82b8a61ccf14";

        [HttpPost, Route("api/alexa/powershell")]
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
            var response = new AlexaResponse("Hey, say Shutdown servers or reboot plex");
            response.Response.Card.Title = "Paddys Powershell";
            response.Response.Card.Content = "Reboot tool";
            response.Response.Reprompt.OutputSpeech.Text = "Say Shutdown servers or reboot plex";
            response.Response.ShouldEndSession = false;

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
                case "ShutdownIntent":
                    response = ShutdownIntentHandler(request);
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

        private AlexaResponse ShutdownIntentHandler(AlexaRequest request)
        {
            var response = new AlexaResponse("Shutting down server", true);

            using (PowerShell PowerShellInstance = PowerShell.Create())
            {
//                PowerShellInstance.AddScript(@"C:\Paddy\Powershell\Shutdown.ps1");
                PowerShellInstance.AddScript("get-service");

                Collection<PSObject> PSOutput = PowerShellInstance.Invoke();

                // loop through each output object item
                foreach (PSObject outputItem in PSOutput)
                {
                    // if null object was dumped to the pipeline during the script then a null
                    // object may be present here. check for null to prevent potential NRE.
                    if (outputItem != null)
                    {
                        //TODO: do something with the output item 
                        // outputItem.BaseOBject
                        //MessageBox.Show(outputItem.Properties.ToString());
                        response.Response.OutputSpeech.Text = outputItem.Properties.ToString();
                    }
                }
            }
            return response;
        }
    }
}
