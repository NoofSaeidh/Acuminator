﻿using PX.Common;

namespace Acuminator.Tests.Sources
{
    [PXLocalizable]
    public class MyMessages
    {
        public const string CommasInUserName = "Usernames cannot contain commas.";
        public const string SomeString = "Some string";
        public const string StringToFormat = "Text with placeholder {0}";
    }

    public static class NonLocalizableMessages
    {
        public const string CommasInUserName = "Usernames cannot contain commas.";
        public const string StringToFormat = "Text with placeholder {0}";
    }
}

namespace InnerNamespace
{
    public static class NonLocalizableMessagesInNamespace
    {
        public const string CommasInUserName = "Usernames cannot contain commas.";
        public const string StringToFormat = "Text with placeholder {0}";
    }
}