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
                replyText = "�ȳ��ϼ��� \nê���� ���Դϴ� \U0001F64C" +
                                  "\n\n����� �� �ν��� ���� ������ �м��Ͽ� " +
                                  "\n\n å, ��ȭ, ���� ���� ��ȭ �������� ��õ���ִ� ê�� ���񽺸� �����մϴ�. " +
                                  "\n\n1. ���� ���ε� \U0001F646 \n\n 2. ���� �Ⱦ�� \U0001F645";
            }
            else
            {
            }
            await turnContext.SendActivityAsync(MessageFactory.Text(replyText, replyText), cancellationToken);
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
