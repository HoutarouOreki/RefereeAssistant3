﻿using System;

namespace RefereeAssistant3.IRC.Events
{
    public class UpdateUsersEventArgs : EventArgs
    {
        public string Channel { get; internal set; }
        public string[] UserList { get; internal set; }

        public UpdateUsersEventArgs(string channel, string[] userList)
        {
            Channel = channel;
            UserList = userList;
        }
    }
}
