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
            string title;
            string body;
            if (choice.Index == 0)
            {
                title = "서비스 더 알아보기";
                body = "" +
                "저희는 사용자의 사진을 이용해서 감정을 분석하고, 그 감정에 어울리는 컨텐츠를 추천하는 서비스입니다!" +
                "\n\n" +
                "사진은 감정을 분석하기 위해서 일회적으로 사용되기 때문에 사진을 수집하지 않습니다." +
                "\n\n" +
                "하지만 사진을 등록하길 원하지 않는다면, 자신의 직접 입력할 수 있으니 걱정마세요. ;)";
            }
            else
            {
                title = "사용 방법";
                body = "사용 방법 구구절절";
            }

            var thumbnailCard = new ThumbnailCard
            {
                Title = title,
                Text = body,
            };
            await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(thumbnailCard.ToAttachment()));
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("아무 글자를 입력해서 서비스를 시작해보세요 :)"));

            return await stepContext.EndDialogAsync(null, cancellationToken);
        }

    }
}