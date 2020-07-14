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
            if (ask.Contains("����"))
            {
                replyText = "����� �Ǻ�Ÿ�� �� ȭ��ǰ ��õ ���񽺸� ����� �ֽ��ϴ�.";
            }
            else
            {
            }
            await turnContext.SendActivityAsync(MessageFactory.Text(replyText, replyText), cancellationToken);
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var welcomeText = "�ȳ��ϼ���, ê���� ���Դϴ�. ����� �Ǻ�Ÿ�� �� ȭ��ǰ ��õ ���񽺸� ����� �ֽ��ϴ�.";
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text(welcomeText, welcomeText), cancellationToken);
                }
            }
        }
    }
}
