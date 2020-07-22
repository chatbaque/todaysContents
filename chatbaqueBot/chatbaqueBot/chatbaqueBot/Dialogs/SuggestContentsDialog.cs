using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;


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
            var msg = $"감정 상태인 {emotion}에 따른 {recommand_msg },,," + //감정에 따른 추천 메세지 case문같은거로 넣기만 하면됨
                "\n\n\n추천 데이터\n\n\n";


            HtmlAgilityPack.HtmlWeb web = new HtmlAgilityPack.HtmlWeb();
            HtmlAgilityPack.HtmlDocument doc = web.Load("https://pedia.watcha.com/ko-KR/" + "search?query=" + emotion.ToString());
            string[] contents_str = new string[4];
            int cnt = 0;

            foreach (var item in doc.DocumentNode.SelectNodes("//ul[@class ='e1m1t8xe4 css-1nweu1w-VisualUl-StyledStackableUl-ResultStackableUl e1xf1y6l1']"))
            { //영화 TV 책 컨텐츠를 배열에 string으로 다 때려ㅕ넣음
                contents_str[cnt] = item.InnerText;
                cnt++;
            }

            //contents[0] = 영화, contents[2] = 책
            string[] movie = contents_str[0].Split(" ・ "); //? 제거 =>length추출 위해서 씀
            string[] tv = contents_str[1].Split(" ・ ");
            string[] book = contents_str[2].Split(" ・ ");

            string[] contents_list = new string[100];
            int cnt_str = 0;
            int book_start_idx = movie.Length + tv.Length - 2;

            foreach (var item in doc.DocumentNode.SelectNodes("//div[@class ='css-1t67tr2-Titles e32z22d4']"))
            {
                contents_list[cnt_str] = item.InnerText;
                if (cnt_str == 100)
                    break;
                cnt_str++;
            }

            //랜덤 뽑기
            Random random_movie = new Random();
            for (int i = 0; i < 2; i++)
            {
                int movie_idx = random_movie.Next(0, movie.Length - 1);
                string[] movie_info = contents_list[movie_idx].Split(" ・ ");
                msg += "영화제목: " + movie_info[0].Substring(0, movie_info[0].Length - 4) + "\n\n";
                msg += "연도: " + movie_info[0].Substring(movie_info[0].Length - 4) + "\n\n";
                msg += "국가: " + movie_info[1] + "\n\n\n\n";

            }
            for (int i = 0; i < 2; i++)
            {
                int book_idx = random_movie.Next(book_start_idx, book_start_idx + book.Length + 1);
                string[] book_info = contents_list[book_idx].Split(" ・ ");

                msg += "책제목: " + book_info[0].Substring(0, book_info[0].Length - 4) + "\n\n";
                msg += "연도: " + book_info[0].Substring(book_info[0].Length - 4) + "\n\n";
                if (book_info.Length > 1)
                    msg += "작가: " + book_info[1] + "\n";
                msg += "\n\n\n\n";
            }


            await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken);
            return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = MessageFactory.Text("서비스가 만족스러우셨나요? 별점은 큰 힘이 됩니다.") }, cancellationToken);
        }



        private async Task<DialogTurnResult> ConfirmStarStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if ((bool)stepContext.Result)
            {
                return await stepContext.NextAsync(null, cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("사용해주셔서 감사합니다. 서비스를 다시 시작하길 원하시면 아무 글자를 입력해주세요."), cancellationToken);
                return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
            }


        }

        private async Task<DialogTurnResult> StarStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("별점 감사합니다. 서비스를 다시 시작하길 원하시면 아무 글자를 입력해주세요."), cancellationToken);

            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }



    }
}
