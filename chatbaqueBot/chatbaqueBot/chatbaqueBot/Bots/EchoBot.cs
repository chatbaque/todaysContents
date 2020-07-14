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
                if (ask.Contains("����"))
                {
                    replyText = "�ȳ��ϼ��� \nê���� ���Դϴ� \U0001F64C" +
                                      "\n\n����� �� �ν��� ���� ������ �м��Ͽ� " +
                                      "\n\n å, ��ȭ, ���� ���� ��ȭ �������� ��õ���ִ� ê�� ���񽺸� �����մϴ�. ";
                    await turnContext.SendActivityAsync(MessageFactory.Text(replyText, replyText), cancellationToken);
                    await SendSuggestUploadPicAsync(turnContext, cancellationToken);
                }
                else if (ask.Equals("���� ���ε�"))
                {
                    replyText = "���� ���ε带 �����ϼ̽��ϴ�. ������ ÷�����ּ���.";
                    await turnContext.SendActivityAsync(MessageFactory.Text(replyText, replyText), cancellationToken);
                }
                else if (ask.Equals("���� �Ⱦ��"))
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
            var reply = MessageFactory.Text("\n\n������ �м��ϱ� ���ؼ� �� ������ �ʿ��մϴ�. ������ ����Ͻðڽ��ϱ�?\n\n");

            reply.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
                {
                    new CardAction() { Title = "1. ���� ���ε� \U0001F646", Type = ActionTypes.ImBack, Value = "���� ���ε�" },
                    new CardAction() { Title = "2. ���� �Ⱦ�� \U0001F645", Type = ActionTypes.ImBack, Value = "���� �Ⱦ��" },
                },
            };
            await turnContext.SendActivityAsync(reply, cancellationToken);
        }

        private static async Task SendRequireEmotionAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            await turnContext.SendActivityAsync(MessageFactory.Text("\n\n������ �δ㽺����ôٸ� ������ �������� ���� ��õ������ �� �ֽ��ϴ�.\n\n", "\n\n������ �δ㽺����ôٸ� ������ �������� ���� ��õ������ �� �ֽ��ϴ�.\n\n"), cancellationToken);
            var reply = MessageFactory.Text("���� ������ �������ּ���.\n\n");

            reply.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
                {
                    new CardAction() { Title = "1. ȭ��", Type = ActionTypes.ImBack, Value = "ȭ��" },
                    new CardAction() { Title = "2. �����", Type = ActionTypes.ImBack, Value = "�����" },
                    new CardAction() { Title = "3. ����/��ġ��", Type = ActionTypes.ImBack, Value = "����/��ġ��" },
                    new CardAction() { Title = "4. �ູ��", Type = ActionTypes.ImBack, Value = "�ູ��" },
                    new CardAction() { Title = "5. ��� ��", Type = ActionTypes.ImBack, Value = "��о�" },
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
            var welcomeText1 = "�ȳ��ϼ��� \n\n";
            var welcomeText2 = "'����'�� ������ ������ �Է����ּ���";
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
