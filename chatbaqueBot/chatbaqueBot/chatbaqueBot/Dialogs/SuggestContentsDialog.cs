using HtmlAgilityPack;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;


namespace Microsoft.BotBuilderSamples
{
    public class SuggestContentsDialog : ComponentDialog
    {
        public SuggestContentsDialog()
    : base(nameof(SuggestContentsDialog))
        {
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
                {
                    SuggestStepAsync,
                    ShowContentCardsStepAsync,
                    ConfirmStarStepAsync,
                    StarStepAsync
                }));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> SuggestStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userInfoList = stepContext.Options as Dictionary<string, object>;
            foreach (KeyValuePair<string, object> item in userInfoList)
            {
                stepContext.Values[item.Key] = item.Value;
            }

            var emotion = stepContext.Values["emotion"];
            var recommand_msg = "";
            var msg = $"감정 상태인 {emotion}에 따른 {recommand_msg }한 컨텐츠를 추천해드릴게요!" + //감정에 따른 추천 메세지 case문같은거로 넣기만 하면됨
                "\n\n\n데이터를 수집하고 있습니다. 잠시만 기다려주세요. :)\n\n\n";

            await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken);
            return await stepContext.NextAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> ShowContentCardsStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync(createContentsCard((string)stepContext.Values["emotion"]), cancellationToken);

            return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = MessageFactory.Text("추천해드린 컨텐츠가 마음에 드시나요?") }, cancellationToken);
        }


        private static IMessageActivity createContentsCard(string emotion)
        {
            HtmlAgilityPack.HtmlWeb web = new HtmlAgilityPack.HtmlWeb();
            HtmlAgilityPack.HtmlDocument doc = web.Load("https://pedia.watcha.com/ko-KR/" + "search?query=" + emotion.ToString());
            string baseURL = "https://pedia.watcha.com";
            List<Contents> contentsList = new List<Contents>();
            int i = 0;
            foreach (HtmlNode item in doc.DocumentNode.SelectNodes("//a[@class ='css-oeirkd-LinkSelf e32z22d1']")) //이미지 파일 링크 가져오기
            {
                if (i++ > 26)
                {
                    break;
                }
                var title = item.Attributes["title"].Value;
                var year = item.InnerText.Replace(title, "");
                var urll = baseURL + item.Attributes["href"].Value;
                contentsList.Add(new Contents(title, year, urll));

/*                HtmlAgilityPack.HtmlDocument img_doc = web.Load(baseURL + (urll));

                var img_div = img_doc.DocumentNode.SelectSingleNode("//div[@class ='css-ds7f62-PosterWithRankingInfoBlock e1svyhwg10']");
                if (img_div.HasAttributes)
                    Console.WriteLine(img_div.ChildNodes[2].ChildNodes[1].Attributes["src"].Value);*/

            }

            var attachments = new List<Attachment>();
            var reply = MessageFactory.Attachment(attachments);
            reply.Text = $"감정 상태인 {emotion}에 따른 {emotion},,,";
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            Random random_movie = new Random();

            foreach(Contents item in contentsList)
            {
                HtmlAgilityPack.HtmlDocument img_doc = web.Load(item.url);
                string imgUrl = "https://sec.ch9.ms/ch9/7ff5/e07cfef0-aa3b-40bb-9baa-7c9ef8ff7ff5/buildreactionbotframework_960.jpg";
                var img_div = img_doc.DocumentNode.SelectSingleNode("//div[@class ='css-ds7f62-PosterWithRankingInfoBlock e1svyhwg10']");
                if (img_div.HasAttributes)
                    imgUrl = (img_div.ChildNodes[2].ChildNodes[1].Attributes["src"].Value);
                reply.Attachments.Add(GetHeroCard(item.title, item.year, item.url, imgUrl).ToAttachment());
            }

            return reply;
        }

        public static HeroCard GetHeroCard(string title, string year, string url, string img)
        {
            var heroCard = new HeroCard
            {
                Title = title,
                Subtitle = year,
                Images = new List<CardImage> { new CardImage(img) },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "더보기", value: url) },
            };

            return heroCard;
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

    class Contents
    {
        public string title, url, year;
        public Contents(string title, string year, string url)
        {
            this.title = title;
            this.year = year;
            this.url = url;
        }
    }



}