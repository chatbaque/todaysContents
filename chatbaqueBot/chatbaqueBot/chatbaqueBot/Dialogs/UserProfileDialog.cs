// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

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
    public class UserProfileDialog : ComponentDialog
    {
        private readonly IStatePropertyAccessor<UserProfile> _userProfileAccessor;

        public UserProfileDialog(UserState userState)
            : base(nameof(UserProfileDialog))
        {
            _userProfileAccessor = userState.CreateProperty<UserProfile>("UserProfile");

            // This array defines how the Waterfall will execute.
            var waterfallSteps = new WaterfallStep[]
            {
                StartStepAsync,
                NameStepAsync,
                SuggestUploadStepAsync,
                PictureStepAsync,
                PresumeEmotionStepAsync,
                ConfirmPresumeStepAsync
            };

            // Add named dialogs to the DialogSet. These names are saved in the dialog state.
            AddDialog(new UserStateDialog());
            AddDialog(new SuggestContentsDialog());
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new NumberPrompt<int>(nameof(NumberPrompt<int>), AgePromptValidatorAsync));
            AddDialog(new AttachmentPrompt(nameof(AttachmentPrompt), PicturePromptValidatorAsync));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> StartStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = MessageFactory.Text("추천 서비스를 시작하시겠습니까?") }, cancellationToken);
        }

        private static async Task<DialogTurnResult> NameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if ((bool)stepContext.Result)
            {
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("안녕하세요. 이름을 입력해주세요.") }, cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("추천 서비스를 시작하길 원하시면 아무 글자를 입력해주세요."), cancellationToken);
                return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
            }
        }

        private static async Task<DialogTurnResult> SuggestUploadStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["name"] = (string)stepContext.Result;
            return await stepContext.PromptAsync(nameof(ChoicePrompt),
                    new PromptOptions
                    {
                        Prompt = MessageFactory.Text("\n\n" +
                        $"환영합니다. {(string)stepContext.Result}님.\n" +
                        "감정을 분석하기 위해서 얼굴 사진이 필요합니다. 사진을 등록하시겠습니까?\n\n"),
                        Choices = ChoiceFactory.ToChoices(new List<string> { "1. 사진 업로드", "2. 사진 싫어요" }),
                    }, cancellationToken);
        }

        private static async Task<DialogTurnResult> PictureStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var choice = (FoundChoice)stepContext.Result;
            if (choice.Index == 1)
            {
                stepContext.Values["picture"] = null;
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("사진 업로드를 원하지 않으신다면, 직접 상태를 입력해주세요!"), cancellationToken);
                return await stepContext.ReplaceDialogAsync(nameof(UserStateDialog), stepContext.Values, cancellationToken);
            }

            if (stepContext.Context.Activity.ChannelId == Channels.Msteams)
            {
                // This attachment prompt example is not designed to work for Teams attachments, so skip it in this case
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Skipping attachment prompt in Teams channel..."), cancellationToken);
                return await stepContext.NextAsync(null, cancellationToken);
            }
            else
            {
                // WaterfallStep always finishes with the end of the Waterfall or with another dialog; here it is a Prompt Dialog.
                var promptOptions = new PromptOptions
                {
                    Prompt = MessageFactory.Text("업로드할 사진을 입력해주세요."),
                    RetryPrompt = MessageFactory.Text("입력파일은 jpeg/png 형식의 파일이어야 합니다."),
                };

                return await stepContext.PromptAsync(nameof(AttachmentPrompt), promptOptions, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> ConfirmStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["picture"] = ((IList<Attachment>)stepContext.Result)?.FirstOrDefault();

            // WaterfallStep always finishes with the end of the Waterfall or with another dialog; here it is a Prompt Dialog.
            return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = MessageFactory.Text("이 사진이 맞나요?") }, cancellationToken);
        }

        private async Task<DialogTurnResult> PresumeEmotionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("분석중..."), cancellationToken);

            stepContext.Values["picture"] = ((IList<Attachment>)stepContext.Result)?.FirstOrDefault();
            stepContext.Values["emotion"] = callClovaApi(((IList<Attachment>)stepContext.Result)?.FirstOrDefault())[0];

            stepContext.Values["age"] = callClovaApi(((IList<Attachment>)stepContext.Result)?.FirstOrDefault())[1];
            string msg = $"{stepContext.Values["name"]}님의 나이는 {stepContext.Values["age"]}세로 추정되며, 현재의 감정상태는 \"{stepContext.Values["emotion"]}\"입니다.";
            await stepContext.Context.SendActivityAsync(
                MessageFactory.Text(msg), cancellationToken);

            return await stepContext.PromptAsync(nameof(ConfirmPrompt),
                                new PromptOptions
                                {
                                    Prompt = MessageFactory.Text("\n\n해당 예측이 정확하다고 생각되면 계속 진행해주세요."),
                                }, cancellationToken);

        }

        private async Task<DialogTurnResult> ConfirmPresumeStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if ((bool)stepContext.Result)
            {
                return await stepContext.ReplaceDialogAsync(nameof(SuggestContentsDialog), stepContext.Values, cancellationToken);
            }
            else
            {
                return await stepContext.ReplaceDialogAsync(nameof(UserStateDialog), stepContext.Values, cancellationToken);
            }
        }


        private static async Task<bool> PicturePromptValidatorAsync(PromptValidatorContext<IList<Attachment>> promptContext, CancellationToken cancellationToken)
        {
            if (promptContext.Recognized.Succeeded)
            {
                var attachments = promptContext.Recognized.Value;
                var validImages = new List<Attachment>();

                foreach (var attachment in attachments)
                {
                    if (attachment.ContentType == "image/jpeg" || attachment.ContentType == "image/png")
                    {
                        validImages.Add(attachment);
                    }
                }

                promptContext.Recognized.Value = validImages;

                // If none of the attachments are valid images, the retry prompt should be sent.
                return validImages.Any();
            }
            else
            {
                await promptContext.Context.SendActivityAsync("No attachments received. Proceeding without a profile picture...");

                // We can return true from a validator function even if Recognized.Succeeded is false.
                return true;
            }
        }

        private static Task<bool> AgePromptValidatorAsync(PromptValidatorContext<int> promptContext, CancellationToken cancellationToken)
        {
            // This condition is our validation rule. You can also change the value at this point.
            return Task.FromResult(promptContext.Recognized.Succeeded && promptContext.Recognized.Value > 0 && promptContext.Recognized.Value < 150);
        }

        private static string[] callClovaApi(Attachment img)
        {
            string boundary = "----------------------------" + DateTime.Now.Ticks.ToString("x");
            byte[] fileData = new System.Net.WebClient().DownloadData(img.ContentUrl);
            string FilePath = img.ContentType;
            string CRLF = "\r\n";
            string postData = "--" + boundary + CRLF + "Content-Disposition: form-data; name=\"image\"; filename=\"";
            postData += Path.GetFileName(FilePath) + "\"" + CRLF + "Content-Type: image/jpeg" + CRLF + CRLF;
            string footer = CRLF + "--" + boundary + "--" + CRLF;

            Stream DataStream = new MemoryStream();
            DataStream.Write(Encoding.UTF8.GetBytes(postData), 0, Encoding.UTF8.GetByteCount(postData));
            DataStream.Write(fileData, 0, fileData.Length);
            DataStream.Write(Encoding.UTF8.GetBytes("\r\n"), 0, 2);
            DataStream.Write(Encoding.UTF8.GetBytes(footer), 0, Encoding.UTF8.GetByteCount(footer));
            DataStream.Position = 0;
            byte[] formData = new byte[DataStream.Length];
            DataStream.Read(formData, 0, formData.Length);
            DataStream.Close();

            //string url = "https://openapi.naver.com/v1/vision/celebrity"; // 유명인 얼굴 인식
            string url = "https://openapi.naver.com/v1/vision/face"; // 얼굴 감지
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Headers.Add("X-Naver-Client-Id", "epY6owt321XZfTOurfMC");
            request.Headers.Add("X-Naver-Client-Secret", "VrkRwSv4g8");
            request.Method = "POST";
            request.ContentType = "multipart/form-data; boundary=" + boundary;
            request.ContentLength = formData.Length;

            using (Stream requestStream = request.GetRequestStream())
            {
                requestStream.Write(formData, 0, formData.Length);
                requestStream.Close();
            }
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream stream = response.GetResponseStream();
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            string text = reader.ReadToEnd();
            stream.Close();
            response.Close();
            reader.Close();


            string[] json_result = new string[2];
            JObject json_data = JObject.Parse(text);

            json_result[0] = json_data["faces"][0]["emotion"]["value"].ToString();
            if (json_result[0].Equals("angry") || json_result[0].Equals("disgust"))
                json_result[0] = "화남";
            else if (json_result[0].Equals("fear") || json_result[0].Equals("sad"))
                json_result[0] = "슬픔/센치함";
            else if (json_result[0].Equals("laugh") || json_result[0].Equals("smile"))
                json_result[0] = "행복함";
            else if (json_result[0].Equals("neutral") || json_result[0].Equals("talking"))
                json_result[0] = "기분업";
            else if (json_result[0].Equals("surprise"))
                json_result[0] = "우울함";

            json_result[1] = json_data["faces"][0]["age"]["value"].ToString();


            return json_result;
        }
    }
}