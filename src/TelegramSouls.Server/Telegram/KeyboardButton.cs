﻿using Newtonsoft.Json;

namespace TelegramSouls.Server.Telegram
{
    public class KeyboardButton
    {
        [JsonProperty("text")]
        public string Text;

        [JsonProperty("request_contact", NullValueHandling = NullValueHandling.Ignore)]
        public bool? RequestContact;

        [JsonProperty("request_location", NullValueHandling = NullValueHandling.Ignore)]
        public bool? RequestLocation;
    }
}