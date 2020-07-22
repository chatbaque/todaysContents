using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;

namespace Microsoft.BotBuilderSamples
{
    public class UserStateDialog : ComponentDialog
    {
        private readonly string[] _emotionOptions = new string[]
        {
            "슬픔", "행복", "기분업", "우울",
        };

        public UserStateDialog()
            : base(nameof(UserStateDialog))
        {
            AddDialog(new SuggestContentsDialog());
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new NumberPrompt<int>(nameof(NumberPrompt<int>), AgePromptValidatorAsync));

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
                {
                    AgeStepAsync,
                    EmotionStepAsync,
                    ConfirmStepAsync
                }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> AgeStepAsync(WaterfallStepContext stepContext,CancellationToken cancellationToken)
        {
            var userinfoList = stepContext.Options as Dictionary<string,object>;
            stepContext.Values["name"] = userinfoList["name"];
            stepContext.Values["picture"] = userinfoList["picture"];

            var promptOptions = new PromptOptions
            {
                Prompt = MessageFactory.Text("나이를 입력해주세요."),
                RetryPrompt = MessageFactory.Text("The value entered must be greater than 0 and less than 150."),
            };
            return await stepContext.PromptAsync(nameof(NumberPrompt<int>), promptOptions, cancellationToken);
        }

        private async Task<DialogTurnResult> EmotionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["age"] = stepContext.Result;

            var options = _emotionOptions.ToList();

            var promptOptions = new PromptOptions
            {
                Prompt = MessageFactory.Text("현재 감정을 선택해주세요."),
                RetryPrompt = MessageFactory.Text("Please choose an option from the list."),
                Choices = ChoiceFactory.ToChoices(options),
            };

            // Prompt the user for a choice.
            return await stepContext.PromptAsync(nameof(ChoicePrompt), promptOptions, cancellationToken);
        }

        private async Task<DialogTurnResult> ConfirmStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var choice = (FoundChoice)stepContext.Result;
            stepContext.Values["emotion"] = choice.Value;

            string msg = $"{stepContext.Values["name"]}님은 현재 {stepContext.Values["emotion"]}을 느끼고 계시고, {stepContext.Values["age"]}살 입니다.\n\n\n분석중...\n\n";
            await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken);
            return await stepContext.ReplaceDialogAsync(nameof(SuggestContentsDialog), stepContext.Values, cancellationToken);
        }

        private static Task<bool> AgePromptValidatorAsync(PromptValidatorContext<int> promptContext, CancellationToken cancellationToken)
        {
            // This condition is our validation rule. You can also change the value at this point.
            return Task.FromResult(promptContext.Recognized.Succeeded && promptContext.Recognized.Value > 0 && promptContext.Recognized.Value < 150);
        }
    }
}