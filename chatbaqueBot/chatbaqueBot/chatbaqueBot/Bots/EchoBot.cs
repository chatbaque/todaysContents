// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.9.2

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace chatbaqueBot.Bots
{
    public class EchoBot : ActivityHandler
    {
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            string ask = turnContext.Activity.Text;
            var replyText = $"echo: {ask}";
            bool messageWithFileDownloadInfo = turnContext.Activity.Attachments?[0].ContentType != null;

            if (ask != null)
            {
                if (ask.Contains("목적"))
                {
                    replyText = "안녕하세요 \n챗바퀴 팀입니다 \U0001F64C" +
                                      "\n\n저희는 얼굴 인식을 통해 감정을 분석하여 " +
                                      "\n\n 책, 영화, 음악 등의 문화 컨텐츠를 추천해주는 챗봇 서비스를 제공합니다. ";
                    await turnContext.SendActivityAsync(MessageFactory.Text(replyText, replyText), cancellationToken);
                    await SendSuggestUploadPicAsync(turnContext, cancellationToken);
                }
                else if (ask.Equals("사진 업로드"))
                {
                    replyText = "사진 업로드를 선택하셨습니다. 사진을 첨부해주세요.";
                    await turnContext.SendActivityAsync(MessageFactory.Text(replyText, replyText), cancellationToken);
                }
                else if (ask.Equals("사진 싫어요"))
                {
                    await SendRequireEmotionAsync(turnContext, cancellationToken);
                }
                else
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text(replyText, replyText), cancellationToken);
                }
            }
            else if (messageWithFileDownloadInfo)
            {
                var inputMsg = ProcessInput(turnContext);
                inputMsg.Attachments = new List<Attachment>() { GetInternetAttachment(turnContext) };

                await turnContext.SendActivityAsync(inputMsg, cancellationToken);

            }

        }

        private static async Task SendSuggestUploadPicAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var reply = MessageFactory.Text("\n\n감정을 분석하기 위해서 얼굴 사진이 필요합니다. 사진을 등록하시겠습니까?\n\n");

            reply.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
                {
                    new CardAction() { Title = "1. 사진 업로드 \U0001F646", Type = ActionTypes.ImBack, Value = "사진 업로드" },
                    new CardAction() { Title = "2. 사진 싫어요 \U0001F645", Type = ActionTypes.ImBack, Value = "사진 싫어요" },
                },
            };
            await turnContext.SendActivityAsync(reply, cancellationToken);
        }

        private static async Task SendRequireEmotionAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            await turnContext.SendActivityAsync(MessageFactory.Text("\n\n사진이 부담스러우시다면 간단한 선택지를 통해 추천받으실 수 있습니다.\n\n", "\n\n사진이 부담스러우시다면 간단한 선택지를 통해 추천받으실 수 있습니다.\n\n"), cancellationToken);
            var reply = MessageFactory.Text("현재 감정을 선택해주세요.\n\n");

            reply.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
                {
                    new CardAction() { Title = "1. 화남", Type = ActionTypes.ImBack, Value = "화남" },
                    new CardAction() { Title = "2. 우울함", Type = ActionTypes.ImBack, Value = "우울함" },
                    new CardAction() { Title = "3. 슬픔/센치함", Type = ActionTypes.ImBack, Value = "슬픔/센치함" },
                    new CardAction() { Title = "4. 행복함", Type = ActionTypes.ImBack, Value = "행복함" },
                    new CardAction() { Title = "5. 기분 업", Type = ActionTypes.ImBack, Value = "기분업" },
                },
            };
            await turnContext.SendActivityAsync(reply, cancellationToken);
        }

        private static Activity ProcessInput(ITurnContext turnContext)
        {
            var activity = turnContext.Activity;
            var reply = activity.CreateReply();

            return reply;
        }

        private static Attachment GetInternetAttachment(ITurnContext turnContext)
        {
            // ContentUrl must be HTTPS.
            var contenturl = turnContext.Activity.Attachments[0].ContentUrl;
            return new Attachment
            {
                Name = @"Resources\architecture-resize.png",
                ContentType = "image/png",
                ContentUrl = contenturl,
            };
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var welcomeText1 = "안녕하세요 \n\n";
            var welcomeText2 = "'목적'을 포함한 질문을 입력해주세요";
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text(welcomeText1, welcomeText1), cancellationToken);
                    await turnContext.SendActivityAsync(MessageFactory.Text(welcomeText2, welcomeText2), cancellationToken);
                }
            }
        }
    }
}
