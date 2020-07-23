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
            var msg = $"감정 상태인 {emotion}에 어울리는 컨텐츠를 추천해드릴게요!" + //감정에 따른 추천 메세지 case문같은거로 넣기만 하면됨
                "\n\n\n데이터를 수집하고 있습니다. 잠시만 기다려주세요. :)\n\n\n";

            await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken);

            await stepContext.Context.SendActivityAsync(createContentsCard((string)stepContext.Values["emotion"]), cancellationToken);

            return await stepContext.EndDialogAsync(null, cancellationToken);
        }




        private static IMessageActivity createContentsCard(string emotion)
        {
            HtmlAgilityPack.HtmlWeb web = new HtmlAgilityPack.HtmlWeb();
            HtmlAgilityPack.HtmlDocument doc = web.Load("https://pedia.watcha.com/ko-KR/" + "search?query=" + emotion.ToString());
            string baseURL = "https://pedia.watcha.com";
            List<Contents> contentsList = new List<Contents>();


            foreach (var item in doc.DocumentNode.SelectNodes("//li[@class = 'css-106b4k6-Self e3fgkal0']"))
            {
                var a_element = item.FirstChild;
                string url="";
                if (a_element.Attributes["href"] != null)
                {
                    url = baseURL + a_element.Attributes["href"].Value;
                }
                string img = "";
                foreach (var img_element in a_element.Descendants("img"))
                {
                    if (img_element.Attributes["src"] != null)
                    {
                        img = img_element.Attributes["src"].Value;
                    }
                }
                string title = "", info = "", type = "";
                var divParent = a_element.LastChild;
                int cnt = 0;
                foreach (var div in divParent.Descendants("div"))
                {
                    switch (cnt++)
                    {
                        case 0:
                            title = div.InnerText;
                            break;
                        case 1:
                            info = div.InnerText;
                            break;
                        default:
                            type = div.InnerText;
                            break;
                    }
                }
                contentsList.Add(new Contents(
                        title = title,
                        info = info,
                        url = url,
                        type = type,
                        img = img
                    ));
            }
            var attachments = new List<Attachment>();
            var reply = MessageFactory.Attachment(attachments);
            reply.Text = $"감정 상태인 {emotion}에 어울리는 컨텐츠입니다.";
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            Random random_movie = new Random();

            foreach(Contents item in contentsList)
            {
                reply.Attachments.Add(GetHeroCard(item.title, $"{item.info}  {item.type}", item.url, item.img).ToAttachment());
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




    }

    class Contents
    {
        public string title, url, info, type, img;
        public Contents(string title, string info, string url, string type, string img)
        {
            this.title = title;
            this.info = info;
            this.url = url;
            this.type = type;
            this.img = img;
        }
    }



}