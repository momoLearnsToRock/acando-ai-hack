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
        private static double? avgScore = 0.6;
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
                if (avgScore > 0.45)
                {
                    avgScore = textAnalyser.GetScore(turnContext.Activity.Text);
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

                    var msg = "I'm not sure how i can help you, with that!";

                    await turnContext.SendActivityAsync(msg, cancellationToken: cancellationToken);

                }
                else
                {
                    await turnContext.SendActivityAsync("Hand over to human", cancellationToken: cancellationToken);
                }
            }
            else
            {
                await turnContext.SendActivityAsync($"{turnContext.Activity.Type} event detected");
            }
        }
    }
}
