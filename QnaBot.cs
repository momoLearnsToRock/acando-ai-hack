// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

using System.Collections.Generic;
using Microsoft.Bot.Builder.AI.QnA;

namespace Bot3
{
    /// <summary>
    /// Represents a bot that processes incoming activities.
    /// For each user interaction, an instance of this class is created and the OnTurnAsync method is called.
    /// This is a Transient lifetime service. Transient lifetime services are created
    /// each time they're requested. Objects that are expensive to construct, or have a lifetime
    /// beyond a single turn, should be carefully managed.
    /// For example, the <see cref="MemoryStorage"/> object and associated
    /// <see cref="IStatePropertyAccessor{T}"/> object are created with a singleton lifetime.
    /// </summary>
    /// <seealso cref="https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-2.1"/>
    public class QnaBot : IBot
    {
        private readonly List<QnAMaker> _qnaServices;
        private TextAnalyser textAnalyser;
        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>      
        public QnaBot(List<QnAMaker> qnaServices)
        {
            // ...
            _qnaServices = qnaServices;
            textAnalyser = new TextAnalyser();
        }

        /// <summary>
        /// Every conversation turn calls this method.
        /// </summary>
        /// <param name="turnContext">A <see cref="ITurnContext"/> containing all the data needed
        /// for processing this conversation turn. </param>
        /// <param name="cancellationToken">(Optional) A <see cref="CancellationToken"/> that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> that represents the work queued to execute.</returns>
        /// <seealso cref="BotStateSet"/>
        /// <seealso cref="ConversationState"/>
        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            
            
            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                await turnContext.SendActivityAsync(
                    textAnalyser.GetScore(turnContext.Activity.Text).ToString(),
                    cancellationToken: cancellationToken);
                foreach (var qnaService in _qnaServices)
                {
                    var response = await qnaService.GetAnswersAsync(turnContext);
                    if (response != null && response.Length > 0)
                    {
                        await turnContext.SendActivityAsync(
                            response[0].Answer,
                            cancellationToken: cancellationToken);
                        return;
                    }
                }

                var msg = "No QnA Maker answers were found. This example uses a QnA Maker knowledge base that " +
                    "focuses on smart light bulbs. Ask the bot questions like 'Why won't it turn on?' or 'I need help'.";

                await turnContext.SendActivityAsync(msg, cancellationToken: cancellationToken);
            }
            else
            {
                await turnContext.SendActivityAsync($"{turnContext.Activity.Type} event detected");
            }
        }
    }
}
