﻿using Microsoft.Bot.Builder;
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
                    ShowContentsStepAsync,
                    EndStepAsync,
                }));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            InitialDialogId = nameof(WaterfallDialog);
        }

        private static string[] choices = new string[4];
        private static Dictionary<string, List<Movie>> contents = new Dictionary<string, List<Movie>>();
        private static HtmlAgilityPack.HtmlWeb web = new HtmlAgilityPack.HtmlWeb();

        private async Task<DialogTurnResult> SuggestThemeStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            CollectContents();
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

        private static void CollectContents()
        {
            HtmlAgilityPack.HtmlDocument doc = web.Load("https://pedia.watcha.com/ko-KR/");
            int i = 0;
            foreach (var item in doc.DocumentNode.SelectNodes("//div[@class = 'css-gxko42-StyledHomeListContainer ebeya3l2']"))
            {
                foreach (var p_element in item.Descendants("p"))
                {
                    int divCnt = 0;
                    switch (i)
                    {
                        case 0:
                            divCnt = 4;
                            break;
                        case 1:
                        case 2:
                            divCnt = 3;
                            break;
                        default:
                            divCnt = 2;
                            break;
                    }
                    if (p_element.InnerText != "")
                    {
                        choices[i] = p_element.InnerText;
                        contents[p_element.InnerText] = new List<Movie>();
                        Console.WriteLine(p_element.InnerText);
                    }
                    int j = 0;
                    foreach (var li_element in item.Descendants("li"))
                    {
                        var contentList = contents[p_element.InnerText];
                        var a_element = li_element.LastChild;
                        var title = a_element.Attributes["title"].Value;
                        var url = a_element.Attributes["href"].Value;
                        var div_elements = a_element.LastChild.Descendants("div");
                        string sub = "";
                        string info = "";
                        foreach (var div_element in div_elements)
                        {
                            if(j%divCnt == 1)
                            {
                                sub = div_element.InnerText;
                            }
                            else if(j%divCnt != 0)
                            {
                                info += div_element.InnerText + "\n\n";
                            }
                            

                            if (j++ % divCnt == divCnt - 1)
                            {
                                contentList.Add(new Movie(
                                    title = title,
                                    url = url,
                                    sub = sub,
                                    info = info
                                    ));
                            }
                        }
                    }
                    i++;
                }
            }


        }

        private async Task<DialogTurnResult> ShowContentsStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var msg = "\n\n\n데이터를 수집하고 있습니다. 잠시만 기다려주세요. :)\n\n\n";
            await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken);

            var theme = choices[((FoundChoice)stepContext.Result).Index];

            var attachments = new List<Attachment>();
            var reply = MessageFactory.Attachment(attachments);
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            Random random_movie = new Random();

            foreach (Movie item in (List<Movie>)contents[theme])
            {
                string imgUrl = "https://sec.ch9.ms/ch9/7ff5/e07cfef0-aa3b-40bb-9baa-7c9ef8ff7ff5/buildreactionbotframework_960.jpg";

                HtmlAgilityPack.HtmlDocument img_doc = web.Load(item.url);
                var img_div = img_doc.DocumentNode.SelectSingleNode("//div[@class ='css-ds7f62-PosterWithRankingInfoBlock e1svyhwg10']");
                if (img_div.HasAttributes)
                    imgUrl = (img_div.ChildNodes[2].ChildNodes[1].Attributes["src"].Value);

                reply.Attachments.Add(GetHeroCard(item.title, item.sub ,item.info, item.url, imgUrl).ToAttachment());
            }
            await stepContext.Context.SendActivityAsync(reply, cancellationToken);
            return await stepContext.NextAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> EndStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("저희 서비스를 사용해주셔서 감사합니다. 아무 글자를 입력해주시면, 서비스를 다시 시작하실 수 있습니다."), cancellationToken);

            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }


        public static HeroCard GetHeroCard(string title, string sub ,string year, string url, string img)
        {
            var heroCard = new HeroCard
            {
                Title = title,
                Subtitle = sub,
                Text = year,
                Images = new List<CardImage> { new CardImage(img) },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "더보기", value: url) },
            };

            return heroCard;
        }
    }

    public class Movie
    {
        public string title, url, info, sub;
        public Movie(string title, string url, string sub = "", string info = "")
        {
            this.title = title;
            this.url = "https://pedia.watcha.com"+url;
            this.sub = sub;
            this.info = info;
        }
    }
}