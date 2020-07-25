using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;

namespace Microsoft.BotBuilderSamples
{
    public class HowtoDialog : ComponentDialog
    {
        public HowtoDialog()
            : base(nameof(HowtoDialog))
        {
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
                {
                    SelectStepAsync,
                    ShowMsgStepAsync
                }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> SelectStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var choices = new[] { "서비스 더 알아보기", "사용 방법" };

            return await stepContext.PromptAsync(nameof(ChoicePrompt), new PromptOptions
            {
                Choices = ChoiceFactory.ToChoices(choices),
            }, cancellationToken);
        }

        private async Task<DialogTurnResult> ShowMsgStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var choice = (FoundChoice)stepContext.Result;

            if (choice.Index == 0)
            {
                string[] msg = { "https://user-images.githubusercontent.com/33623107/88454004-17a87e00-cea7-11ea-8978-e251bba00a9f.jpg",
                };
                await stepContext.Context.SendActivityAsync(CreateUsageCards(msg));
            }
            else
            {
                string[] msg = {
                "https://user-images.githubusercontent.com/33623107/88454005-1a0ad800-cea7-11ea-8874-a6e8d912d87e.jpg",
                "https://user-images.githubusercontent.com/33623107/88454006-1bd49b80-cea7-11ea-9cc7-0b702a9951a7.jpg"
                };
                await stepContext.Context.SendActivityAsync(CreateUsageCards(msg));
            }


            await stepContext.Context.SendActivityAsync(MessageFactory.Text("서비스를 다시 시작하시려면 채팅을 입력해주세요. :)"), cancellationToken);

            return await stepContext.EndDialogAsync(null, cancellationToken);
        }

        private static IMessageActivity CreateUsageCards(string[] msg)
        {
            var attachments = new List<Attachment>();
            var reply = MessageFactory.Attachment(attachments);
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;

            foreach (string item in msg)
            {
                reply.Attachments.Add(new HeroCard {
                    Images = new List<CardImage> { new CardImage(item) },
                }.ToAttachment());
            }

            return reply;
        }

    }
}