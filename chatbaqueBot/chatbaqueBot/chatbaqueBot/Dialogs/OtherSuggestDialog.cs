using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace Microsoft.BotBuilderSamples
{
    public class OtherSuggestDialog : ComponentDialog
    {
        public OtherSuggestDialog()
    : base(nameof(OtherSuggestDialog))
        {
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
                {
                    SuggestThemeStepAsync,
                    StarStepAsync
                }));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            InitialDialogId = nameof(WaterfallDialog);
        }

        private static string[] choices = new[] { "박스오피스 인기 작품", "왓챠 인기 작품", "넷플릭스 인기 작품", "평균 별점이 높은 작품" };

        private async Task<DialogTurnResult> StarStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("저희 서비스를 사용해주셔서 감사합니다. 아무 글자를 입력해주시면, 서비스를 다시 시작하실 수 있습니다."), cancellationToken);

            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> SuggestThemeStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var heroCard = new HeroCard
            {
                Title = "원하는 추천 테마를 선택하세요.",
                Buttons = choices.Select(choice => new CardAction(ActionTypes.ImBack, choice, value: choice)).ToList(),

            };

            var reply = MessageFactory.Attachment(heroCard.ToAttachment());
            return await stepContext.PromptAsync(nameof(ChoicePrompt), new PromptOptions
            {
                Prompt = (Activity)MessageFactory.Attachment(heroCard.ToAttachment()),
                Choices = ChoiceFactory.ToChoices(choices),
            }, cancellationToken);
        }

        private async Task<DialogTurnResult> ShowContentsStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var theme = choices[((FoundChoice)stepContext.Result).Index];
            await stepContext.Context.SendActivityAsync(MessageFactory.Text((string)theme), cancellationToken);
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }
    }
}