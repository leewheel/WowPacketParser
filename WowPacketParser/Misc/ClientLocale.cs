﻿using System;
using System.IO;
using WowPacketParser.Enums;


namespace WowPacketParser.Misc
{
    public static class ClientLocale
    {
        public static string ClientLocaleString;

        public static string PacketLocaleString;

        public static LocaleConstant PacketLocale => (LocaleConstant)Enum.Parse(typeof(LocaleConstant), PacketLocaleString);

        public static void SetLocale(string locale)
        {
            // By leewheel 2026-08-14: ymir 写全零 locale，默认为 enUS
            if (locale == string.Empty || string.IsNullOrWhiteSpace(locale) || locale.Trim('\0') == string.Empty)
            {
                ClientLocaleString = "enUS";
                PacketLocaleString = "enUS";
                return;
            }
            // End By leewheel

            ClientLocaleString = locale;

            // enGB contains same data as enUS
            if (locale == "enGB")
                PacketLocaleString = "enUS";
            else
                PacketLocaleString = locale;

            if (!Enum.TryParse<LocaleConstant>(PacketLocaleString, out var _))
                throw new InvalidDataException($"Invalid locale '{PacketLocaleString}'.");
        }
    }
}
