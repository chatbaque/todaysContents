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
    public class MainDialog : ComponentDialog
    {

        public MainDialog(UserState userState)
    : base(nameof(MainDialog))
        {

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
                {
                    IntroStepAsync,
                    ServiceStepAsync,
                    FinalStepAsync,
                    ConfirmStarStepAsync,
                    StarStepAsync
                }));

            AddDialog(new UserProfileDialog(userState));
            AddDialog(new OtherSuggestDialog());
            AddDialog(new HowtoDialog());

            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            InitialDialogId = nameof(WaterfallDialog);
        }


        private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var choices = new[] { "기분 맞춤형 컨텐츠 추천받기", "요즘 인기있는 컨텐츠 추천받기", "서비스 소개" };
            var heroCard = new HeroCard
            {
                Title = "기분 맞춤형 컨텐츠 추천 서비스",
                Subtitle = "by Chatbaque",
                Text = "안녕하세요 \n챗바퀴 팀입니다 \U0001F64C" +
                        "\n\n저희는 얼굴 인식을 통해 감정을 분석하여 " +
                        "\n\n 책, 영화, 음악 등의 문화 컨텐츠를 추천해주는 챗봇 서비스를 제공합니다.",
                Images = new List<CardImage> { new CardImage("https://user-images.githubusercontent.com/33623107/87881702-f1fa1f80-ca35-11ea-98d4-1b9d7c4d6a5e.png") },
                Buttons = choices.Select(choice => new CardAction(ActionTypes.ImBack, choice, value: choice)).ToList(),

            };

            var reply = MessageFactory.Attachment(heroCard.ToAttachment());
            return await stepContext.PromptAsync(nameof(ChoicePrompt), new PromptOptions
            {
                Prompt = (Activity)MessageFactory.Attachment(heroCard.ToAttachment()),
                Choices = ChoiceFactory.ToChoices(choices),
            }, cancellationToken);
        }

        private static async Task<DialogTurnResult> ServiceStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var choice = (FoundChoice)stepContext.Result;
            if (choice.Index == 0)
            {
                return await stepContext.BeginDialogAsync(nameof(UserProfileDialog), null, cancellationToken);
            }
            else if (choice.Index == 1)
            {
                return await stepContext.BeginDialogAsync(nameof(OtherSuggestDialog), null, cancellationToken);
            }
            else
            {
                return await stepContext.ReplaceDialogAsync(nameof(HowtoDialog), null, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = MessageFactory.Text("추천해드린 컨텐츠가 마음에 드시나요?") }, cancellationToken);
        }

        private async Task<DialogTurnResult> ConfirmStarStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if ((bool)stepContext.Result)
            {
                return await stepContext.NextAsync(null, cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("만족스러운 서비스를 제공하지 못하여 유감이네요... 아무 글자를 입력해주시면 서비스를 다시 시작하실 수 있습니다."), cancellationToken);
                return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
            }


        }

        private async Task<DialogTurnResult> StarStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("저희 서비스를 사용해주셔서 감사합니다. 아무 글자를 입력해주시면, 서비스를 다시 시작하실 수 있습니다."), cancellationToken);

            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }

    }
}