﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Schema;
using System.Collections.Generic;

namespace Microsoft.BotBuilderSamples
{
    /// <summary>
    /// This is our application state. Just a regular serializable .NET class.
    /// </summary>
    public class UserProfile
    {
        public string Transport { get; set; }

        public string Name { get; set; }

        public int Age { get; set; }
        public List<string> CompaniesToReview { get; set; } = new List<string>();

        public string Emotion { get; set; }
        public Attachment Picture { get; set; }
    }
}